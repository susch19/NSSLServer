using System;
using System.Collections.Generic;
using System.Text;

namespace NSSLServer.Core.Database
{
    public interface IDbUpdater
    {
        string Name { get; }
        int Priority { get; }
        bool UpToDate { get; }
        Version CurrentVersion { get; }
        Version DesiredVersion { get; }

    }
}
