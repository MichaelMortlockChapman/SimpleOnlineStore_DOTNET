using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SimpleOnlineStore_Dotnet.Data {
    public class Roles {
        public static string CUSTOMER_ROLE = "CUSTOMER";
        public static string ADMIN_ROLE = "ADMIN";
        public static string[] ROLES = [CUSTOMER_ROLE, ADMIN_ROLE];
    }
}
