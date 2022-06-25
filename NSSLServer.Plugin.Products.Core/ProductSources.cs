using NSSLServer.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Plugin.Products.Core
{
    public class ProductSources
    {
        public static ProductSources Instance = new();

        private readonly SortedList<int, IProductSource> sources = new();

        int i = 0;
        public void AddNewSource(IProductSource source)
        {
            if (sources.Any(x => x.Value == source))
                return;

            sources.Add(i++ + (source.Islocal ? 0 : 0xFFFF), source); // source.Priority
        }

        public IEnumerator<IProductSource> GetEnumerator() => sources.Values.GetEnumerator();
    }
}
