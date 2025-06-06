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

        // ✅ Configuration adaptative pour Android/Windows
        private static string BaseUrl
        {
            get
            {
#if ANDROID
                return "http://10.0.2.2:7236/api/User";  // Pour Android émulateur
#else
                return "https://localhost:7237/api/User"; // Pour Windows
#endif
            }
        }

        public UserService()
        {
            var handler = new HttpClientHandler();

#if ANDROID
            // Ignorer les erreurs SSL pour Android en développement
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            System.Diagnostics.Debug.WriteLine($"🌐 UserService utilise: {BaseUrl}");
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
                    System.Diagnostics.Debug.WriteLine("🔐 Token ajouté aux headers HTTP");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Aucun token trouvé dans SecureStorage");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Erreur config auth UserService: {ex.Message}");
            }
        }

        public async Task<List<UserRolesViewModel>> GetAllUsersAsync()
        {
            try
            {
                await ConfigureAuthAsync();

                System.Diagnostics.Debug.WriteLine($"👥 GetAllUsers URL: {BaseUrl}");

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
                await ConfigureAuthAsync();

                var fullUrl = $"{BaseUrl}/{userName}";
                System.Diagnostics.Debug.WriteLine($"👥 GetRoles URL: {fullUrl}");

                // Créer un HttpClient avec timeout court pour tester
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

                    // Copier les headers d'autorisation
                    var token = await SecureStorage.GetAsync("auth_token");
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
                await ConfigureAuthAsync();
                System.Diagnostics.Debug.WriteLine($"👥 UpdateUserRoles URL: {BaseUrl}/ManageRoles");

                var body = new { UserName = userName, Roles = roles };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/ManageRoles", body);

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Rôles utilisateur mis à jour avec succès");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Erreur mise à jour rôles: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur UpdateUserRolesAsync: {ex.Message}");
                return false;
            }
        }

        // Méthode pour obtenir tous les rôles disponibles
        public async Task<List<string>> GetAvailableRolesAsync()
        {
            try
            {
                await ConfigureAuthAsync();
                // Vous pouvez ajouter un endpoint dans votre API pour ça
                // Pour l'instant, retourner les rôles par défaut
                System.Diagnostics.Debug.WriteLine("👥 GetAvailableRoles - Retour des rôles par défaut");
                return new List<string> { "Admin", "Support", "User" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetAvailableRolesAsync: {ex.Message}");
                return new List<string> { "Admin", "Support", "User" };
            }
        }
    }
}