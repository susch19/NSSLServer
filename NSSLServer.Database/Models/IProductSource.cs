using NSSLServer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Models
{
    public interface IProductSource
    {
        bool Islocal { get; }
        Task<IDatabaseProduct> FindProductByCode(string code);
        Task<Paged<IDatabaseProduct>> FindProductsByName(string name, int page = 1);
    }
}
