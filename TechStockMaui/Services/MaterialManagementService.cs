using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TechStockMaui.Models;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace TechStockMaui.Services
{
    public class MaterialManagementService
    {
        private readonly HttpClient _httpClient;

        private static string BaseUrl
        {
            get
            {
#if ANDROID
                return "http://10.0.2.2:7236/api";
#else
                return "https://localhost:7237/api";
#endif
            }
        }

        private static string MaterialManagementUrl => $"{BaseUrl}/MaterialManagement";
        private static string UsersUrl => $"{BaseUrl}/User";
        private static string StatesUrl => $"{BaseUrl}/States";

        public MaterialManagementService()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MaterialManagementService constructor - START");

                var handler = new HttpClientHandler();

#if ANDROID
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

                _httpClient = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };

                System.Diagnostics.Debug.WriteLine($"MaterialManagementService uses: {BaseUrl}");
                System.Diagnostics.Debug.WriteLine("HttpClient created");
                System.Diagnostics.Debug.WriteLine("MaterialManagementService constructor - END");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MaterialManagementService constructor ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auth configuration error: {ex.Message}");
            }
        }

        private async Task<string> GetUserIdFromToken()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("Empty token");
                    return "";
                }

                var parts = token.Split('.');
                if (parts.Length != 3)
                {
                    System.Diagnostics.Debug.WriteLine("Invalid JWT token");
                    return "";
                }

                var payload = parts[1];
                while (payload.Length % 4 != 0)
                    payload += "=";

                var bytes = Convert.FromBase64String(payload);
                var json = System.Text.Encoding.UTF8.GetString(bytes);

                System.Diagnostics.Debug.WriteLine($"Token payload: {json}");

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out var element))
                {
                    var userId = element.GetString();
                    System.Diagnostics.Debug.WriteLine($"Extracted user ID: '{userId}'");
                    return userId ?? "";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("nameidentifier property not found in token");
                    return "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUserIdFromToken error: {ex.Message}");
                return "";
            }
        }

        public async Task<List<MaterialManagement>> GetMyAssignmentsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GetMyAssignmentsAsync - START - FILTERED VERSION");

                await ConfigureAuthAsync();

                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("No authentication token");
                    return new List<MaterialManagement>();
                }
                System.Diagnostics.Debug.WriteLine($"Token present: {token.Substring(0, Math.Min(20, token.Length))}...");

                var currentUserId = await GetUserIdFromToken();
                System.Diagnostics.Debug.WriteLine($"Connected user ID: '{currentUserId}'");

                var url = $"{MaterialManagementUrl}";
                System.Diagnostics.Debug.WriteLine($"Called URL: {url}");

                var response = await _httpClient.GetAsync(url);
                System.Diagnostics.Debug.WriteLine($"Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Raw content received: {content.Substring(0, Math.Min(200, content.Length))}...");

                    var allAssignments = JsonSerializer.Deserialize<List<MaterialManagement>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (allAssignments != null && allAssignments.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"Total assignments received: {allAssignments.Count}");

                        var userAssignments = allAssignments.Where(a => a.UserId == currentUserId).ToList();

                        System.Diagnostics.Debug.WriteLine($"After filtering: {userAssignments.Count} assignments for user '{currentUserId}'");

                        foreach (var assignment in userAssignments)
                        {
                            System.Diagnostics.Debug.WriteLine($"   Assignment ID: {assignment.Id}, Product: {assignment.Product?.Name ?? "NULL"}, UserId: {assignment.UserId}, Signature: {(string.IsNullOrEmpty(assignment.Signature) ? "Not signed" : "Signed")}");
                        }

                        return userAssignments;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No assignment in response");
                    }

                    return new List<MaterialManagement>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"API error: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"Error detail: {errorContent}");
                    return new List<MaterialManagement>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetMyAssignmentsAsync exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                return new List<MaterialManagement>();
            }
        }

        public async Task<List<MaterialManagement>> GetAllAsync()
        {
            try
            {
                await ConfigureAuthAsync();
                var result = await _httpClient.GetFromJsonAsync<List<MaterialManagement>>(MaterialManagementUrl);
                return result ?? new List<MaterialManagement>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllAsync error: {ex.Message}");
                return new List<MaterialManagement>();
            }
        }

        public async Task<MaterialManagement> GetByIdAsync(int id)
        {
            try
            {
                await ConfigureAuthAsync();
                return await _httpClient.GetFromJsonAsync<MaterialManagement>($"{MaterialManagementUrl}/{id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByIdAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> SignProductAsync(int assignmentId, string signature)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"SignProductAsync - START (ID: {assignmentId})");

                await ConfigureAuthAsync();

                var signatureDto = new SignatureDto
                {
                    Id = assignmentId,
                    Signature = signature
                };

                var url = $"{MaterialManagementUrl}/sign";
                System.Diagnostics.Debug.WriteLine($"Signature URL: {url}");
                System.Diagnostics.Debug.WriteLine($"Signature data: ID={assignmentId}, Signature={signature}");

                var response = await _httpClient.PostAsJsonAsync(url, signatureDto);

                System.Diagnostics.Debug.WriteLine($"Signature status code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Signature sent successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Signature error: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"Error detail: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SignProductAsync exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> AssignProductAsync(int productId, string userId, int stateId)
        {
            try
            {
                await ConfigureAuthAsync();

                var assignmentDto = new AssignmentDto
                {
                    ProductId = productId,
                    UserId = userId,
                    StateId = stateId
                };

                System.Diagnostics.Debug.WriteLine($"Assignment API: ProductId={productId}, UserId={userId}, StateId={stateId}");
                var response = await _httpClient.PostAsJsonAsync($"{MaterialManagementUrl}/assign", assignmentDto);

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Assignment API successful");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Assignment API failed: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"Error detail: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AssignProductAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAssignmentAsync(int id)
        {
            try
            {
                await ConfigureAuthAsync();
                var response = await _httpClient.DeleteAsync($"{MaterialManagementUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteAssignmentAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                await ConfigureAuthAsync();
                var result = await _httpClient.GetFromJsonAsync<List<User>>(UsersUrl);
                return result ?? new List<User>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUsersAsync error: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<List<States>> GetStatesAsync()
        {
            try
            {
                await ConfigureAuthAsync();
                var result = await _httpClient.GetFromJsonAsync<List<States>>(StatesUrl);
                return result ?? new List<States>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetStatesAsync error: {ex.Message}");
                return new List<States>();
            }
        }

        public async Task<string> TestAssignmentsEndpoint()
        {
            try
            {
                await ConfigureAuthAsync();
                var url = $"{MaterialManagementUrl}/my-assignments";
                System.Diagnostics.Debug.WriteLine($"Test endpoint: {url}");

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Content: {content}");

                return $"Status: {response.StatusCode}, Content: {content}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Test error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
    }

    public class SignatureDto
    {
        public int Id { get; set; }
        public string Signature { get; set; } = string.Empty;
    }

    public class AssignmentDto
    {
        public int ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int StateId { get; set; }
    }
}