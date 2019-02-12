using System.Threading;
using Crossroads.Utilities.Models;

namespace Crossroads.Utilities.Services
{
    public class ImpersonatedUserGuid
    {
        private static ThreadLocal<ImpersonatedData> _instance = new ThreadLocal<ImpersonatedData>();

        public static readonly string NewAuthImpersonateToken = "NewAuthTokenString";

        public static void Set(string guid, string token)
        {
            _instance.Value = new ImpersonatedData() {Guid = guid, Token = token};

        }

        public static string Get()
        {
            return (_instance.Value.Guid);
        }

        public static string GetToken()
        {
            return (_instance.Value.Token);
        }

        public static bool HasValue()
        {
            return (_instance.IsValueCreated && !string.IsNullOrWhiteSpace(_instance.Value.Guid));
        }

        public static void Clear()
        {
            _instance = new ThreadLocal<ImpersonatedData>();
        }
    }
}
