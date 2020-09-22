using NSSLServer.Database.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Database
{
    [WithDbContext]
    [AuthRequired]
    public class AuthenticatingDbContextController : BaseDbContextController
    {

    }

    [WithDbContext]
    public class BaseDbContextController : BaseController
    {

        public DBContext Context { get; set; }
    }
}
