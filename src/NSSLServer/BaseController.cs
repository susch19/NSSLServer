using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Npgsql;
using NSSLServer.Shared;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NSSLServer
{

    public class NsslEnvironment
    {
        public static string ConnectionString = File.ReadAllText("connectionstring") + "Connection Idle Lifetime=5;Maximum Pool Size=1024"; 

        public static async Task<DbConnection> OpenConnectionAsync()
        { 
                        
            var con = new NpgsqlConnection(ConnectionString);
            await con.OpenAsync();//..ConfigureAwait(false);
            
            return con;
        }
    }


    [ExtractJwtSessionFilter]
    public class BaseController : ControllerBase
    {
        public static readonly JsonSerializerSettings DefaultJsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter> { new EmptyToNullConverter() },
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        public NsslSession Session;

        protected JsonResult Json<T>(T obj) where T : class
        {
            
            return new JsonResult(obj, DefaultJsonSettings);
        }

        public DBContext Context;

       
    }


    public class EmptyToNullConverter : JsonConverter
    {
        private readonly JsonSerializer _stringSerializer = new JsonSerializer();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = _stringSerializer.Deserialize<string>(reader);

            if (string.IsNullOrEmpty(value))
            {
                value = null;
            }

            return value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            _stringSerializer.Serialize(writer, value);
        }
    }


    public class ExtractJwtSessionFilterAttribute : Attribute, IAsyncResourceFilter
    {
        private const string ItemDicKey = "jwt_extract";
        public static readonly byte[] JwtKeyBytes;

        static ExtractJwtSessionFilterAttribute()
        {
            JwtKeyBytes = UserManager.SecretKey;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (context.HttpContext.Request.Headers.TryGetValue("X-Token", out StringValues sv) && sv.Count > 0)
            {
                if (JsonWebToken.Decode<NsslSession>(sv[0], JwtKeyBytes, true, out NsslSession s))
                    context.HttpContext.Items[ItemDicKey] = s;
            }
            
            await next.Invoke();
        }

        public static NsslSession FromItems(HttpContext context)
        {
            NsslSession session = null;
            if (context.Items.TryGetValue(ItemDicKey, out object s))
            {
                session = (NsslSession)s;
            }
            return session;
        }
    }

    public class AuthRequiredAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var session = ExtractJwtSessionFilterAttribute.FromItems(context.HttpContext);

            if (session == null || session.Expires < DateTime.Now)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            ((BaseController)context.Controller).Session = session;

            await next.Invoke();
        }
    }

    public class WithDbContextAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            var controller = (BaseController)context.Controller;
            using (controller.Context = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                await next.Invoke();
                controller.Context.Connection.Close(); 
            }
            
            //await controller.Context.SaveChangesAsync();
        }
    }

    [AuthRequired]
    public class AuthenticatingController : BaseController
    {

    }
}
