using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;

namespace NSSLServer.Core.HelperMethods
{
    public class PluginCreator<T>
    {
        public static T GetInstance(Type pluginType)
        {
            var body = Expression.New(pluginType);
            return Expression.Lambda<Func<T>>(body).Compile().Invoke();
        }

        public static T GetInstance(Type pluginType, ILogger logger)
        {
            return (T)Activator.CreateInstance(pluginType, [logger]);
        }
    }
}
