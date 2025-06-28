using NSSLServer.Database.Attributes;

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
