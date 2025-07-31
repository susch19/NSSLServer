using Deviax.QueryBuilder;
using Microsoft.Extensions.Logging;
using NSSLServer.Database.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NSSLServer.Database.Updater
{
    public abstract class DbUpdater : IDbUpdater
    {
        public string Name { get; }
        public Version CurrentVersion { get; private set; }
        public Version DesiredVersion { get; private set; }
        public abstract int Priority { get; }
        public bool UpToDate => CurrentVersion == DesiredVersion;

        private ILogger logger;
        private List<(Version version, string path)> updateScriptPathes;

        public DbUpdater(ILogger logger)
        {
            var type = GetType();// "MyCompany.MyProduct.MyFile.txt";
            this.logger = logger;

            updateScriptPathes = new List<(Version, string)>();

            Name = type.Name;


            //using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            //using (StreamReader reader = new StreamReader(stream))
            //{
            //    string result = reader.ReadToEnd();
            //}

            //Version.TryParse();
        }

        public virtual async Task LoadCurrentVersion()
        {

            DbVersion dbVersion = null;
            try
            {
                using (var conn = await NsslEnvironment.OpenConnectionAsync())
                    dbVersion = await Q.From(DbVersion.T).Where(x => x.Name.Like(Name)).FirstOrDefault<DbVersion>(conn);

            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error loading current db version for {name}, setting {currentVersion} to default", Name, nameof(CurrentVersion));
            }
            if (dbVersion is null)
            {
                CurrentVersion = new Version(0, 0, 0, 0);
                return;
            }

            CurrentVersion = Version.Parse(dbVersion.Version);
        }

        public void LoadDesiredVersion()
        {
            var type = GetType();
            var assembly = Assembly.GetAssembly(type);
            var scriptPath = type.FullName + "_Scripts";
            foreach (var res in assembly.GetManifestResourceNames())
            {
                if (res.StartsWith(scriptPath))
                {
                    if (Version.TryParse(res[(res.IndexOf("_Scripts") + 9)..^4], out var ver))
                    {
                        updateScriptPathes.Add((ver, res));
                        if (DesiredVersion == null || DesiredVersion < ver)
                            DesiredVersion = ver;
                    }
                }
            }
        }

        public async Task RunUpdates()
        {
            if (UpToDate)
                return;
            bool isNew = CurrentVersion.Major == 0 && CurrentVersion.Minor == 0 && CurrentVersion.Build == 0 && CurrentVersion.Revision == 0;
            var ass = Assembly.GetAssembly(GetType());
            using (var conn = await NsslEnvironment.OpenConnectionAsync())
            {
                DbVersion dbVersion;

                foreach (var updateScript in updateScriptPathes.OrderBy(x=>x.version))
                {
                    if (updateScript.version <= CurrentVersion)
                        continue;

                    var transTask = conn.BeginTransactionAsync();
                    var command = conn.CreateCommand();

                    using (var reader = new StreamReader(ass.GetManifestResourceStream(updateScript.path)))
                    {
                        command.CommandText = reader.ReadToEnd();
                    }

                    using (var trans = await transTask)
                    {
                        try
                        {
                            await command.ExecuteNonQueryAsync();
                            if (isNew)
                            {
                                dbVersion = new DbVersion() { Name = Name, Version = updateScript.version.ToString() };
                                await Q.InsertOne(trans.Connection, trans, dbVersion);
                            }
                            else
                            {
                                await Q.Update(DbVersion.T).Set(x => x.Version.SetV(updateScript.version.ToString())).Where(x => x.Name.EqV(Name)).Execute(trans.Connection, trans);
                            }
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            logger.LogError(ex, "Error in {name} for {version}", Name, updateScript.version);
                            break;
                        }
                    }
                }
            }
        }

        public abstract void RegisterTypes();
    }
}
