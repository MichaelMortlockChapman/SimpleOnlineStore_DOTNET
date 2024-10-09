using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SimpleOnlineStore_Dotnet.Data {
    public class Roles {
        public const string CUSTOMER_ROLE = "CUSTOMER";
        public const string ADMIN_ROLE = "ADMIN";
        public static string[] ROLES = [CUSTOMER_ROLE, ADMIN_ROLE];
    }
}
