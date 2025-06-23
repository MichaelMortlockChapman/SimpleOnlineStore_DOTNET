using SimpleOnlineStore_DOTNET_BlazorWASM.Identity.Models;

namespace SimpleOnlineStore_DOTNET_BlazorWASM.Identity {
    public interface ICustomAccountManagement : IAccountManagement {
        public Task<FormResult> RegisterAsync2(string email, string password, string name, string address, string city, string postalCode, string country);
    }
}
