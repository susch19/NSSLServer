using Microsoft.EntityFrameworkCore;
using NLog;

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

        private Logger logger;
        private List<(Version version, string path)> updateScriptPathes;

        public DbUpdater()
        {
            var type = GetType();// "MyCompany.MyProduct.MyFile.txt";
            logger = LogManager.GetCurrentClassLogger();

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
                using var ctx = new DBContext();
                dbVersion = await ctx.Set<DbVersion>().FirstOrDefaultAsync(x => EF.Functions.ILike(x.Name, Name));
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Error loading current db version for {Name}, setting {nameof(CurrentVersion)} to default");
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
            using var ctx = new DBContext();
            DbVersion dbVersion;

            foreach (var updateScript in updateScriptPathes.OrderBy(x => x.version))
            {
                if (updateScript.version <= CurrentVersion)
                    continue;

                using var trans = ctx.Connection.BeginTransaction();

                using (var reader = new StreamReader(ass.GetManifestResourceStream(updateScript.path)))
                {

                    try
                    {
                        ctx.Database.ExecuteSqlRaw(reader.ReadToEnd());
                        var dbSet = ctx.Set<DbVersion>();
                        if (isNew)
                        {
                            dbVersion = new DbVersion() { Name = Name, Version = updateScript.version.ToString() };
                            dbSet.Add(dbVersion);
                        }
                        else
                        {
                            dbVersion = dbSet.FirstOrDefault(x => EF.Functions.ILike(x.Name, Name));

                            dbVersion.Version = updateScript.version.ToString();
                        }
                        ctx.SaveChanges();
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        logger.Error(ex, $"Error in {Name} for {updateScript.version}");
                        break;
                    }
                    //command.CommandText = reader.ReadToEnd();
                }

            }

        }

        public abstract void RegisterTypes();
    }
}
