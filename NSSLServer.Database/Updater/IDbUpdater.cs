using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Database.Updater
{
    public interface IDbUpdater
    {
        string Name { get; }
        int Priority { get; }
        bool UpToDate { get; } 
        Version CurrentVersion { get; }
        Version DesiredVersion { get; }

        public Task LoadCurrentVersion();
        public void LoadDesiredVersion();

        public Task RunUpdates();

        public void RegisterTypes();

    }
}
