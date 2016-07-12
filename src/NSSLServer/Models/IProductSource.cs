using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Models
{
    public interface IProductSource
    {
        bool islocal { get; }
        Task<BasicProduct> FindProductByCode(string code);
        Task<List<BasicProduct>> FindProductsByName(string name);
    }
}
