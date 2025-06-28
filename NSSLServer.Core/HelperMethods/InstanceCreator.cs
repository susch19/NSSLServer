using System;
using System.Linq.Expressions;

namespace NSSLServer.Core.HelperMethods
{
    public class InstanceCreator<T>
    {
        public static Func<T> GetInstance;

        static InstanceCreator()
        {
            var body = Expression.New(typeof(T));
            GetInstance = Expression.Lambda<Func<T>>(body).Compile();
        }
    }
}
