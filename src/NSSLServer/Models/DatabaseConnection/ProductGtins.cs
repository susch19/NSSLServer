using Deviax.QueryBuilder;
using Deviax.QueryBuilder.Parts;
using NSSLServer.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Models.DatabaseConnection
{
    class ProductGtins
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Gtins { get; set; }

    }
}
