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
        private readonly AuthService _authService;
        private const string BaseUrl = "https://localhost:7237/api/User";

        public UserService()
        {
            _httpClient = new HttpClient();
            _authService = new AuthService();
        }

        private void SetAuthorizationHeader()
        {
            var token = _authService.GetStoredToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<UserRolesViewModel>> GetAllUsersAsync()
        {
            try
            {
                SetAuthorizationHeader();

                System.Diagnostics.Debug.WriteLine($"🔍 Appel API: {BaseUrl}");
                System.Diagnostics.Debug.WriteLine($"🔑 Token: {_authService.GetStoredToken()?.Substring(0, 20)}...");

                var response = await _httpClient.GetAsync(BaseUrl);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"📊 Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"📥 Response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var users = System.Text.Json.JsonSerializer.Deserialize<List<UserRolesViewModel>>(content, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    System.Diagnostics.Debug.WriteLine($"✅ {users?.Count ?? 0} utilisateurs récupérés");
                    return users ?? new List<UserRolesViewModel>();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Erreur API: {response.StatusCode} - {content}");
                    return new List<UserRolesViewModel>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Exception GetAllUsersAsync: {ex.Message}");
                return new List<UserRolesViewModel>();
            }
        }

        public async Task<List<RoleItem>> GetRolesAsync(string userName)
        {
            try
            {
                SetAuthorizationHeader();

                var fullUrl = $"{BaseUrl}/{userName}";
                System.Diagnostics.Debug.WriteLine($"🔍 GetRolesAsync - URL complète: {fullUrl}");
                System.Diagnostics.Debug.WriteLine($"🔑 Token présent: {!string.IsNullOrEmpty(_authService.GetStoredToken())}");

                // Créer un HttpClient avec timeout court pour tester
                using var httpClientWithTimeout = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(10) // Timeout de 10 secondes
                };

                // Copier les headers d'autorisation
                var token = _authService.GetStoredToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClientWithTimeout.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                System.Diagnostics.Debug.WriteLine("🚀 Démarrage de la requête HTTP...");

                var response = await httpClientWithTimeout.GetAsync(fullUrl);

                System.Diagnostics.Debug.WriteLine($"📊 Réponse reçue - Status: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"📥 Response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var userViewModel = System.Text.Json.JsonSerializer.Deserialize<UserRolesViewModel>(content, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (userViewModel != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"📥 Utilisateur reçu: {userViewModel.UserName} - Rôles: {string.Join(", ", userViewModel.Roles)}");

                        var allRoles = new List<string> { "Admin", "Support", "User" };
                        var roleItems = allRoles.Select(role => new RoleItem
                        {
                            RoleName = role,
                            IsSelected = userViewModel.Roles.Contains(role)
                        }).ToList();

                        System.Diagnostics.Debug.WriteLine($"✅ {roleItems.Count} rôles transformés");
                        return roleItems;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Erreur HTTP: {response.StatusCode} - {content}");
                }

                return new List<RoleItem>();
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"⏰ Timeout de la requête: {ex.Message}");
                return new List<RoleItem>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception GetRolesAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Type: {ex.GetType().Name}");
                return new List<RoleItem>();
            }
        }

        public async Task<bool> UpdateUserRolesAsync(string userName, List<string> roles)
        {
            try
            {
                SetAuthorizationHeader();
                var body = new { UserName = userName, Roles = roles };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/ManageRoles", body);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur UpdateUserRolesAsync: {ex.Message}");
                return false;
            }
        }

        // Méthode pour obtenir tous les rôles disponibles
        public async Task<List<string>> GetAvailableRolesAsync()
        {
            try
            {
                SetAuthorizationHeader();
                // Vous pouvez ajouter un endpoint dans votre API pour ça
                // Pour l'instant, retourner les rôles par défaut
                return new List<string> { "Admin", "Support", "User" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur GetAvailableRolesAsync: {ex.Message}");
                return new List<string> { "Admin", "Support", "User" };
            }
        }
    }
}