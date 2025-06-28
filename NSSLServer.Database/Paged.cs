using System.Collections.Generic;
using System.Linq;

namespace NSSLServer.Database
{
    public static class PagedExt
    {
        public static Paged<T> Paged<T>(this List<T> l, long total, int page, int perPage) => new Paged<T>(l, total, page, perPage);
        public static Paged<T2> PagedAs<T, T2>(this List<T> l, long total, int page, int perPage) where T : T2 => new Paged<T2>(l.Cast<T2>().ToList(), total, page, perPage);
    }

    public interface IPaged<T>
    {
        IList<T> Items { get; set; }
        long Pages { get; set; }
        long Total { get; set; }
        int PerPage { get; set; }
        int CurrentPage { get; set; }
    }
    public class Paged<T> : IPaged<T>
    {
        public IList<T> Items { get; set; }
        public long Pages { get; set; }
        public long Total { get; set; }
        public int PerPage { get; set; }
        public int CurrentPage { get; set; }


        public Paged(IList<T> items, long total, int page, int perPage)
        {
            Items = items;
            Total = total;
            CurrentPage = page;
            PerPage = perPage;

            Pages = total / perPage;
            if (total % perPage > 0)
                Pages++;
        }

        public Paged() { }
    }
}
