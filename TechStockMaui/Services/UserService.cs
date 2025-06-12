using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TechStockMaui.Models;

namespace TechStockMaui.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        private static string BaseUrl
        {
            get
            {
#if ANDROID
                return "http://10.0.2.2:7236/api/User";
#else
                return "https://localhost:7237/api/User";
#endif
            }
        }

        public UserService()
        {
            var handler = new HttpClientHandler();

#if ANDROID
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            System.Diagnostics.Debug.WriteLine($"UserService using: {BaseUrl}");
        }

        private async Task ConfigureAuthAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    System.Diagnostics.Debug.WriteLine("Token added to HTTP headers");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No token found in SecureStorage");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auth configuration error UserService: {ex.Message}");
            }
        }

        public async Task<List<UserRolesViewModel>> GetAllUsersAsync()
        {
            try
            {
                await ConfigureAuthAsync();

                System.Diagnostics.Debug.WriteLine($"GetAllUsers URL: {BaseUrl}");

                var response = await _httpClient.GetAsync(BaseUrl);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var users = System.Text.Json.JsonSerializer.Deserialize<List<UserRolesViewModel>>(content, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    System.Diagnostics.Debug.WriteLine($"{users?.Count ?? 0} users retrieved");
                    return users ?? new List<UserRolesViewModel>();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"API error: {response.StatusCode} - {content}");
                    return new List<UserRolesViewModel>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception GetAllUsersAsync: {ex.Message}");
                return new List<UserRolesViewModel>();
            }
        }

        public async Task<List<RoleItem>> GetRolesAsync(string userName)
        {
            try
            {
                await ConfigureAuthAsync();

                var fullUrl = $"{BaseUrl}/{userName}";
                System.Diagnostics.Debug.WriteLine($"GetRoles URL: {fullUrl}");

                HttpClient httpClientWithTimeout;

#if ANDROID
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                httpClientWithTimeout = new HttpClient(handler);
#else
                httpClientWithTimeout = new HttpClient();
#endif

                using (httpClientWithTimeout)
                {
                    httpClientWithTimeout.Timeout = TimeSpan.FromSeconds(10);

                    var token = await SecureStorage.GetAsync("auth_token");
                    if (!string.IsNullOrEmpty(token))
                    {
                        httpClientWithTimeout.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }

                    System.Diagnostics.Debug.WriteLine("Starting HTTP request...");

                    var response = await httpClientWithTimeout.GetAsync(fullUrl);

                    System.Diagnostics.Debug.WriteLine($"Response received - Status: {response.StatusCode}");

                    var content = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Response: {content}");

                    if (response.IsSuccessStatusCode)
                    {
                        var userViewModel = System.Text.Json.JsonSerializer.Deserialize<UserRolesViewModel>(content, new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (userViewModel != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"User received: {userViewModel.UserName} - Roles: {string.Join(", ", userViewModel.Roles)}");

                            var allRoles = new List<string> { "Admin", "Support", "User" };
                            var roleItems = allRoles.Select(role => new RoleItem
                            {
                                RoleName = role,
                                IsSelected = userViewModel.Roles.Contains(role)
                            }).ToList();

                            System.Diagnostics.Debug.WriteLine($"{roleItems.Count} roles transformed");
                            return roleItems;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"HTTP error: {response.StatusCode} - {content}");
                    }
                }

                return new List<RoleItem>();
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Request timeout: {ex.Message}");
                return new List<RoleItem>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception GetRolesAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Type: {ex.GetType().Name}");
                return new List<RoleItem>();
            }
        }

        public async Task<bool> UpdateUserRolesAsync(string userName, List<string> roles)
        {
            try
            {
                await ConfigureAuthAsync();
                System.Diagnostics.Debug.WriteLine($"UpdateUserRoles URL: {BaseUrl}/ManageRoles");

                var body = new { UserName = userName, Roles = roles };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/ManageRoles", body);

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("User roles updated successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Role update error: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error UpdateUserRolesAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> GetAvailableRolesAsync()
        {
            try
            {
                await ConfigureAuthAsync();
                System.Diagnostics.Debug.WriteLine("GetAvailableRoles - Returning default roles");
                return new List<string> { "Admin", "Support", "User" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetAvailableRolesAsync: {ex.Message}");
                return new List<string> { "Admin", "Support", "User" };
            }
        }
    }
}