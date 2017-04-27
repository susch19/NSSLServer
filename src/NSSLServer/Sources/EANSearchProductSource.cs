using NSSLServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSSLServer.Sources
{
    public class EANSearchProductSource : IProductSource //TODO Biggest bullshit ever!
    {
        public bool islocal => false;

        public Task<BasicProduct> FindProductByCode(string code)
        {
            throw new NotImplementedException();
        }

        public Task<Paged<BasicProduct>> FindProductsByName(string name, int page = 1)
        {
            throw new NotImplementedException();
        }
    }
}
