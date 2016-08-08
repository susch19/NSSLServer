using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace NSSLServer
{
    public class NsslEnvironment
    {
#if DEBUG
        public const string ConnectionString = "User Id=shoppinglist;Server=localhost;Port=5432;Password=shoppinglist;Database=shoppinglist;";
#else
        public const string ConnectionString = "User Id=shoppinglist;Server=localhost;Port=110;Password=shoppinglist;Database=shoppinglist;";
#endif
        public static async Task<DbConnection> OpenConnectionAsync()
        {
            var con = new NpgsqlConnection(ConnectionString);
            await con.OpenAsync().ConfigureAwait(false);
            return con;
        }
    }

   public class NsslSession {
        public int Id;
        public DateTime Expires;
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

        

        protected async Task<DbConnection> OpenConnection() => await NsslEnvironment.OpenConnectionAsync();
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
            JwtKeyBytes = UserManager.SECRETKEY;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            StringValues sv;
            if (context.HttpContext.Request.Headers.TryGetValue("X-Token", out sv) && sv.Count > 0)
            {
                NsslSession s;
                if (JsonWebToken.Decode<NsslSession>(sv[0], JwtKeyBytes, true, out s))
                    context.HttpContext.Items[ItemDicKey] = s;
            }

            

            await next.Invoke();
        }

        public static NsslSession FromItems(HttpContext context)
        {
            object s;
            NsslSession session = null;
            if (context.Items.TryGetValue(ItemDicKey, out s))
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
            using (controller.Context = new DBContext(await NsslEnvironment.OpenConnectionAsync(),true))
                await next.Invoke();
            
            //await controller.Context.SaveChangesAsync();
        }
    }

    [AuthRequired]
    public class AuthenticatingController : BaseController
    {

    }
}
