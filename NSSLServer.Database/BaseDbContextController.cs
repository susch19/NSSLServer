using NSSLServer.Database.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Database
{
    [WithDbContext]
    public class AuthenticatingDbContextController : AuthenticatingController
    {

        public DBContext Context { get; set; }
    }

    [WithDbContext]
    public class BaseDbContextController : BaseController
    {

        public DBContext Context { get; set; }
    }
}
