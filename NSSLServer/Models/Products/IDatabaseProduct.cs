using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Models
{
    public interface IDatabaseProduct
    {
        string Name { get; set; }
        decimal? Quantity { get; set; }
        string Unit { get; set; }
    }
}
