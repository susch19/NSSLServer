using System.Collections.Generic;

namespace NSSLServer.Models.DatabaseConnection
{
    class ProductGtins
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Gtins { get; set; }

    }
}
