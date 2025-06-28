using System.IO;
using System.Text;

namespace NSSLServer.Core.Authentication
{
    public static class JwtKeyProvider
    {
        /// <summary>
        /// WARNING RADIOACTIVE HAZARD
        /// <para>string = SEKRETKEY</para>
        /// <seealso cref="SecretKey"/>
        /// </summary>
#if true//youareadmin - lookinside
        #region DO NOT LOOK INSIDE HERE IS NOTHING TOO SEA
        public static byte[] SecretKey { get; } //VERY VERY SECRET!
        #endregion  
#endif

        static JwtKeyProvider()
        {
            SecretKey = Encoding.UTF8.GetBytes(File.ReadAllText("external/secretkey"));
        }
    }
}
