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

        // ✅ Configuration adaptative pour Android/Windows
        private static string BaseUrl
        {
            get
            {
#if ANDROID
                return "http://10.0.2.2:7236/api";  // Base pour Android émulateur
#else
                return "https://localhost:7237/api"; // Base pour Windows
#endif
            }
        }

        // ✅ URLs spécifiques
        private static string MaterialManagementUrl => $"{BaseUrl}/MaterialManagement";
        private static string UsersUrl => $"{BaseUrl}/User"; // ✅ Correction: /User au lieu de /Users
        private static string StatesUrl => $"{BaseUrl}/States";

        public MaterialManagementService()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 MaterialManagementService constructeur - DÉBUT");

                var handler = new HttpClientHandler();

#if ANDROID
                // Ignorer les erreurs SSL pour Android en développement
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

                _httpClient = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };

                System.Diagnostics.Debug.WriteLine($"🌐 MaterialManagementService utilise: {BaseUrl}");
                System.Diagnostics.Debug.WriteLine("✅ HttpClient créé");
                System.Diagnostics.Debug.WriteLine("✅ MaterialManagementService constructeur - FIN");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERREUR MaterialManagementService constructeur: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");
                throw;
            }
        }

        // ✅ Méthode helper pour configurer l'auth de manière async
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Erreur configuration auth: {ex.Message}");
            }
        }

        // ✅ MÉTHODE HELPER pour extraire l'ID utilisateur du token JWT
        private async Task<string> GetUserIdFromToken()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("❌ Token vide");
                    return "";
                }

                var parts = token.Split('.');
                if (parts.Length != 3)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Token JWT invalide");
                    return "";
                }

                var payload = parts[1];
                while (payload.Length % 4 != 0)
                    payload += "=";

                var bytes = Convert.FromBase64String(payload);
                var json = System.Text.Encoding.UTF8.GetString(bytes);

                System.Diagnostics.Debug.WriteLine($"🔍 Token payload: {json}");

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out var element))
                {
                    var userId = element.GetString();
                    System.Diagnostics.Debug.WriteLine($"✅ ID utilisateur extrait: '{userId}'");
                    return userId ?? "";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Propriété nameidentifier non trouvée dans le token");
                    return "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetUserIdFromToken: {ex.Message}");
                return "";
            }
        }

        // ✅ CORRECTION PRINCIPALE: Récupérer les assignments de l'utilisateur connecté
        public async Task<List<MaterialManagement>> GetMyAssignmentsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 GetMyAssignmentsAsync - DÉBUT - VERSION FILTRÉE");

                await ConfigureAuthAsync();

                // ✅ DEBUG: Vérifier le token
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("❌ Pas de token d'authentification");
                    return new List<MaterialManagement>();
                }
                System.Diagnostics.Debug.WriteLine($"✅ Token présent: {token.Substring(0, Math.Min(20, token.Length))}...");

                // ✅ Récupérer l'ID utilisateur depuis le token
                var currentUserId = await GetUserIdFromToken();
                System.Diagnostics.Debug.WriteLine($"🔍 ID utilisateur connecté: '{currentUserId}'");

                var url = $"{MaterialManagementUrl}";
                System.Diagnostics.Debug.WriteLine($"🌐 URL appelée: {url}");

                var response = await _httpClient.GetAsync(url);
                System.Diagnostics.Debug.WriteLine($"📊 Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"📥 Contenu brut reçu: {content.Substring(0, Math.Min(200, content.Length))}...");

                    var allAssignments = JsonSerializer.Deserialize<List<MaterialManagement>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (allAssignments != null && allAssignments.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"🔍 Total assignments reçus: {allAssignments.Count}");

                        // ✅ FILTRAGE: garder seulement les assignments de l'utilisateur connecté
                        var userAssignments = allAssignments.Where(a => a.UserId == currentUserId).ToList();

                        System.Diagnostics.Debug.WriteLine($"✅ Après filtrage: {userAssignments.Count} assignments pour l'utilisateur '{currentUserId}'");

                        foreach (var assignment in userAssignments)
                        {
                            System.Diagnostics.Debug.WriteLine($"   ✅ Assignment ID: {assignment.Id}, Product: {assignment.Product?.Name ?? "NULL"}, UserId: {assignment.UserId}, Signature: {(string.IsNullOrEmpty(assignment.Signature) ? "Non signé" : "Signé")}");
                        }

                        return userAssignments;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ Aucun assignment dans la réponse");
                    }

                    return new List<MaterialManagement>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Erreur API: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"📄 Erreur détail: {errorContent}");
                    return new List<MaterialManagement>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception GetMyAssignmentsAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");
                return new List<MaterialManagement>();
            }
        }

        // Récupérer tous les assignments (pour admin)
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
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetAllAsync: {ex.Message}");
                return new List<MaterialManagement>();
            }
        }

        // Récupérer un assignment par ID
        public async Task<MaterialManagement> GetByIdAsync(int id)
        {
            try
            {
                await ConfigureAuthAsync();
                return await _httpClient.GetFromJsonAsync<MaterialManagement>($"{MaterialManagementUrl}/{id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetByIdAsync: {ex.Message}");
                return null;
            }
        }

        // ✅ CORRECTION: Signer un produit avec plus de logs
        public async Task<bool> SignProductAsync(int assignmentId, string signature)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 SignProductAsync - DÉBUT (ID: {assignmentId})");

                await ConfigureAuthAsync();

                var signatureDto = new SignatureDto
                {
                    Id = assignmentId,
                    Signature = signature
                };

                var url = $"{MaterialManagementUrl}/sign";
                System.Diagnostics.Debug.WriteLine($"🌐 URL signature: {url}");
                System.Diagnostics.Debug.WriteLine($"📤 Données signature: ID={assignmentId}, Signature={signature}");

                var response = await _httpClient.PostAsJsonAsync(url, signatureDto);

                System.Diagnostics.Debug.WriteLine($"📊 Status Code signature: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Signature envoyée avec succès");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Erreur signature: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"📄 Détail erreur: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception SignProductAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");
                return false;
            }
        }

        // Assigner un produit à un utilisateur
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

                System.Diagnostics.Debug.WriteLine($"🔄 Assignation API: ProductId={productId}, UserId={userId}, StateId={stateId}");
                var response = await _httpClient.PostAsJsonAsync($"{MaterialManagementUrl}/assign", assignmentDto);

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Assignation API réussie");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Assignation API échouée: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"📄 Détail erreur: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur AssignProductAsync: {ex.Message}");
                return false;
            }
        }

        // Supprimer un assignment
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
                System.Diagnostics.Debug.WriteLine($"❌ Erreur DeleteAssignmentAsync: {ex.Message}");
                return false;
            }
        }

        // ✅ CORRECTION: Utiliser UsersUrl au lieu de l'URL hardcodée
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
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetUsersAsync: {ex.Message}");
                return new List<User>();
            }
        }

        // ✅ CORRECTION: Utiliser StatesUrl au lieu de l'URL hardcodée
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
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetStatesAsync: {ex.Message}");
                return new List<States>();
            }
        }

        // ✅ MÉTHODE DE TEST pour vérifier les endpoints
        public async Task<string> TestAssignmentsEndpoint()
        {
            try
            {
                await ConfigureAuthAsync();
                var url = $"{MaterialManagementUrl}/my-assignments";
                System.Diagnostics.Debug.WriteLine($"🧪 Test endpoint: {url}");

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"🧪 Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"🧪 Content: {content}");

                return $"Status: {response.StatusCode}, Content: {content}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🧪 Erreur test: {ex.Message}");
                return $"Erreur: {ex.Message}";
            }
        }
    }

    // DTOs pour les requêtes API
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