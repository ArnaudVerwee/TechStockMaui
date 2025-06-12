using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using TechStockMaui.Models.Auth;
using System.IdentityModel.Tokens.Jwt;
using TechStockMaui.Models;

namespace TechStockMaui.Services
{
    public class AuthService : IDisposable
    {
        private readonly HttpClient _httpClient;

        private static string BaseUrl
        {
            get
            {
#if ANDROID
                return "http://10.0.2.2:7236/api/Auth";
#else
                return "https://localhost:7237/api/Auth"; 
#endif
            }
        }

        public AuthService()
        {
            var handler = new HttpClientHandler();

#if ANDROID
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            System.Diagnostics.Debug.WriteLine($"AuthService use: {BaseUrl}");
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(GetStoredToken());

        public async Task<AuthResult> LoginAsync(string email, string password, bool rememberMe = false)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Try connection {email}");
                System.Diagnostics.Debug.WriteLine($"Used URL : {BaseUrl}/Login");

                var loginRequest = new LoginRequest
                {
                    Email = email,
                    Password = password
                };

                var jsonData = JsonSerializer.Serialize(loginRequest);
                System.Diagnostics.Debug.WriteLine($"Send data {jsonData}");

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Login", loginRequest);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        System.Diagnostics.Debug.WriteLine("Token received, save...");

                        await SecureStorage.SetAsync("auth_token", result.Token);
                        await SecureStorage.SetAsync("user_email", email);
                        await SecureStorage.SetAsync("user_id", result.UserId ?? "");

                        SetAuthorizationHeader(result.Token);

                        
                        if (rememberMe)
                        {
                            await SecureStorage.SetAsync("saved_email", email);
                            await SecureStorage.SetAsync("saved_password", password);
                            await SecureStorage.SetAsync("remember_me", "true");
                            System.Diagnostics.Debug.WriteLine("Credentials saved for auto-reconnection");
                        }

                        return new AuthResult { Success = true, Message = "Connection ok" };
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No token");
                        return new AuthResult { Success = false, Message = "Response not ok from server" };
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"HTTP error: {response.StatusCode}");

                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<JsonElement>(content);
                        var errorMessage = errorResponse.TryGetProperty("message", out var msg) ?
                            msg.GetString() : "Connection error";
                        return new AuthResult { Success = false, Message = errorMessage };
                    }
                    catch
                    {
                        return new AuthResult { Success = false, Message = $"Error {response.StatusCode}: {content}" };
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connection error: {ex.Message}");
                return new AuthResult { Success = false, Message = $"Can't contact server: {ex.Message}" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error {ex.Message}");
                return new AuthResult { Success = false, Message = $"Unexpected error: {ex.Message}" };
            }
        }

        public async Task<string> TestApiConnectionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Connection test : {BaseUrl}");

                var response = await _httpClient.GetAsync(BaseUrl);
                var content = await response.Content.ReadAsStringAsync();

                return $"Status: {response.StatusCode}\nContent: {content.Substring(0, Math.Min(200, content.Length))}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Start disconnecting...");

                _httpClient.DefaultRequestHeaders.Authorization = null;
                System.Diagnostics.Debug.WriteLine("Headers clean");

               
                var rememberMeEnabled = await IsRememberMeEnabledAsync();

               
                SecureStorage.Remove("auth_token");
                SecureStorage.Remove("user_id");
                SecureStorage.Remove("user_email");
                SecureStorage.Remove("user_name");

             
                if (!rememberMeEnabled)
                {
                    System.Diagnostics.Debug.WriteLine("Remember me disabled, clearing saved credentials");
                    await ClearSavedCredentialsAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Remember me enabled, keeping saved credentials");
                }

                System.Diagnostics.Debug.WriteLine("Disconnection done");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logout: {ex.Message}");
            }
        }

        public string GetStoredToken()
        {
            try
            {
                return SecureStorage.GetAsync("auth_token").Result;
            }
            catch
            {
                return null;
            }
        }

        public string GetUserEmail()
        {
            try
            {
                return SecureStorage.GetAsync("user_email").Result;
            }
            catch
            {
                return null;
            }
        }

        public void SetAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<bool> TryRestoreAuthenticationAsync()
        {
            var token = GetStoredToken();
            if (!string.IsNullOrEmpty(token))
            {
                SetAuthorizationHeader(token);

                try
                {
                    var response = await _httpClient.GetAsync($"{BaseUrl}/ValidateToken");
                    if (response.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine("Valid token - automatic connection");
                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Expired token - removal");
                        await LogoutAsync();
                        return false;
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Token validation error - removal");
                    await LogoutAsync();
                    return false;
                }
            }
            return false;
        }

        public async Task<User> GetCurrentUserAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("No token found");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine("Analyzing JWT token...");

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);

                var emailClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                var roleClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

                var user = new User
                {
                    UserName = emailClaim?.Value ?? string.Empty,
                    Roles = roleClaim?.Value != null ? new List<string> { roleClaim.Value } : new List<string>()
                };

                System.Diagnostics.Debug.WriteLine($"User: {user.Email}, Roles: {string.Join(", ", user.Roles)}");

                return user;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCurrentUserAsync error: {ex.Message}");
                return null;
            }
        }
      

        public async Task<bool> IsRememberMeEnabledAsync()
        {
            var rememberMe = await SecureStorage.GetAsync("remember_me");
            return rememberMe == "true";
        }

        public async Task<bool> TryAutoLoginAsync()
        {
            try
            {
                if (!await IsRememberMeEnabledAsync())
                {
                    System.Diagnostics.Debug.WriteLine("Remember me not enabled");
                    return false;
                }

                var savedEmail = await SecureStorage.GetAsync("saved_email");
                var savedPassword = await SecureStorage.GetAsync("saved_password");

                if (string.IsNullOrEmpty(savedEmail) || string.IsNullOrEmpty(savedPassword))
                {
                    System.Diagnostics.Debug.WriteLine("No saved credentials found");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Attempting auto-login for: {savedEmail}");
                var result = await LoginAsync(savedEmail, savedPassword, true);

                if (result.Success)
                {
                    System.Diagnostics.Debug.WriteLine("Auto-login successful");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Auto-login failed, clearing saved credentials");
                    await ClearSavedCredentialsAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-login error: {ex.Message}");
                return false;
            }
        }

        public async Task ClearSavedCredentialsAsync()
        {
            try
            {
                SecureStorage.Remove("saved_email");
                SecureStorage.Remove("saved_password");
                SecureStorage.Remove("remember_me");
                System.Diagnostics.Debug.WriteLine("Saved credentials cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing credentials: {ex.Message}");
            }
        }

      
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }

        ~AuthService()
        {
            Dispose(false);
        }
    }
}