using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using SimpleOnlineStore_DOTNET_BlazorWASM.Identity.Models;
using System.Text;
using System.Net;

namespace SimpleOnlineStore_DOTNET_BlazorWASM.Identity {
    /// <summary>
    /// Handles state for cookie-based auth.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the auth provider.
    /// </remarks>
    /// <param name="httpClientFactory">Factory to retrieve auth client.</param>
    public class CookieAuthenticationStateProvider(IHttpClientFactory httpClientFactory, ILogger<CookieAuthenticationStateProvider> logger)
        : AuthenticationStateProvider, ICustomAccountManagement {
        /// <summary>
        /// Map the JavaScript-formatted properties to C#-formatted classes.
        /// </summary>
        private readonly JsonSerializerOptions jsonSerializerOptions =
            new() {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

        /// <summary>
        /// Special auth client.
        /// </summary>
        private readonly HttpClient httpClient = httpClientFactory.CreateClient("Auth"); 

        /// <summary>
        /// Authentication state.
        /// </summary>
        private bool authenticated = false;

        /// <summary>
        /// Default principal for anonymous (not authenticated) users.
        /// </summary>
        private readonly ClaimsPrincipal unauthenticated = new(new ClaimsIdentity());

        public async Task<FormResult> RegisterAsync(string email, string password) { return new FormResult { }; }

        public async Task<FormResult> RegisterAsync2(string email, string password, string name, string address, string city, string postalCode, string country) {
            string[] defaultDetail = ["An unknown error prevented registration from succeeding."];

            try {
                // make the request
                var result = await httpClient.PostAsJsonAsync(
                    "api/v1/Auth/Register", new {
                        email,
                        password,
                        name,
                        address,
                        city,
                        postalCode,
                        country
                    });

                // successful?
                if (result.IsSuccessStatusCode) {
                    return new FormResult { Succeeded = true };
                }

                // body should contain details about why it failed
                var details = await result.Content.ReadAsStringAsync();
                var problemDetails = JsonDocument.Parse(details);
                var errors = new List<string>();
                var errorList = problemDetails.RootElement.GetProperty("errors");

                foreach (var errorEntry in errorList.EnumerateObject()) {
                    if (errorEntry.Value.ValueKind == JsonValueKind.String) {
                        errors.Add(errorEntry.Value.GetString()!);
                    } else if (errorEntry.Value.ValueKind == JsonValueKind.Array) {
                        errors.AddRange(
                            errorEntry.Value.EnumerateArray().Select(
                                e => e.GetString() ?? string.Empty)
                            .Where(e => !string.IsNullOrEmpty(e)));
                    }
                }

                // return the error list
                return new FormResult {
                    Succeeded = false,
                    ErrorList = problemDetails == null ? defaultDetail : [.. errors]
                };
            } catch (Exception ex) {
                logger.LogError(ex, "App error");
            }

            // unknown error
            return new FormResult {
                Succeeded = false,
                ErrorList = defaultDetail
            };
        }

        /// <summary>
        /// User login.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The result of the login request serialized to a <see cref="FormResult"/>.</returns>
        public async Task<FormResult> LoginAsync(string email, string password) {
            try {
                // login with cookies
                var result = await httpClient.PostAsJsonAsync(
                    "api/v1/Auth/Login", new {
                        email,
                        password
                    });

                // success?
                if (result.IsSuccessStatusCode) {
                    // need to refresh auth state
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                    // success!
                    return new FormResult { Succeeded = true };
                }
            } catch (Exception ex) {
                logger.LogError(ex, "App error");
            }

            // unknown error
            return new FormResult {
                Succeeded = false,
                ErrorList = ["Invalid email and/or password."]
            };
        }

        /// <summary>
        /// Get authentication state.
        /// </summary>
        /// <remarks>
        /// Called by Blazor anytime and authentication-based decision needs to be made, then cached
        /// until the changed state notification is raised.
        /// </remarks>
        /// <returns>The authentication state asynchronous request.</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
            authenticated = false;

            // default to not authenticated
            var user = unauthenticated;

            try {
                // the user info endpoint is secured, so if the user isn't logged in this will fail
                using var userResponse = await httpClient.GetAsync("/api/v1/Auth/Info");

                // throw if user info wasn't retrieved
                userResponse.EnsureSuccessStatusCode();

                // user is authenticated,so let's build their authenticated identity
                var userJson = await userResponse.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<UserInfo>(userJson, jsonSerializerOptions);

                if (userInfo != null) {
                    // in this example app, name and email are the same
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, userInfo.Email),
                        new(ClaimTypes.Email, userInfo.Email),
                    };

                    // add any additional claims
                    claims.AddRange(
                        userInfo.Claims.Select(c => new Claim(ClaimTypes.Role, c))
                    );

                    // set the principal
                    var id = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
                    user = new ClaimsPrincipal(id);
                    authenticated = true;
                }
            } catch (Exception ex) when (ex is HttpRequestException exception) {
                if (exception.StatusCode != HttpStatusCode.Unauthorized) {
                    logger.LogError(ex, "App error");
                }
            } catch (Exception ex) {
                logger.LogError(ex, "App error");
            }

            // return the state
            return new AuthenticationState(user);
        }

        public async Task LogoutAsync() {
            const string Empty = "{}";
            var emptyContent = new StringContent(Empty, Encoding.UTF8, "application/json");
            await httpClient.PostAsync("/api/v1/Auth/Logout", emptyContent);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<bool> CheckAuthenticatedAsync() {
            await GetAuthenticationStateAsync();
            return authenticated;
        }
    }
}