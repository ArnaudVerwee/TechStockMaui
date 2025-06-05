using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TechStockMaui.Models;

namespace TechStockMaui.Services
{
    public class MaterialManagementService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;
        private const string BaseUrl = "https://localhost:7237/api/MaterialManagement";

        public MaterialManagementService()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 MaterialManagementService constructeur - DÉBUT");

                _httpClient = new HttpClient();
                System.Diagnostics.Debug.WriteLine("✅ HttpClient créé");

                _authService = new AuthService();
                System.Diagnostics.Debug.WriteLine("✅ AuthService créé");

                // ✅ NE PAS configurer l'auth dans le constructeur
                // On le fera de manière async dans chaque méthode
                System.Diagnostics.Debug.WriteLine("⚠️ Configuration auth reportée (async)");

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
                // ✅ Utiliser SecureStorage directement de manière async
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
                // Continue sans auth si erreur
            }
        }

        // Récupérer tous les assignments
        public async Task<List<MaterialManagement>> GetAllAsync()
        {
            try
            {
                await ConfigureAuthAsync(); // ✅ Config auth avant chaque appel
                var result = await _httpClient.GetFromJsonAsync<List<MaterialManagement>>(BaseUrl);
                return result ?? new List<MaterialManagement>();
            }
            catch
            {
                return new List<MaterialManagement>();
            }
        }

        // Récupérer les assignments de l'utilisateur connecté
        public async Task<List<MaterialManagement>> GetMyAssignmentsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 GetMyAssignmentsAsync - DÉBUT");

                await ConfigureAuthAsync();

                // ✅ DEBUG: Vérifier le token
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("❌ Pas de token d'authentification");
                    return new List<MaterialManagement>();
                }
                System.Diagnostics.Debug.WriteLine($"✅ Token présent: {token.Substring(0, Math.Min(20, token.Length))}...");

                // ✅ DEBUG: URL appelée
                var url = $"{BaseUrl}/User";
                System.Diagnostics.Debug.WriteLine($"🌐 URL appelée: {url}");

                var response = await _httpClient.GetAsync(url);
                System.Diagnostics.Debug.WriteLine($"📊 Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"📥 Contenu brut reçu: {content}");

                    var result = await response.Content.ReadFromJsonAsync<List<MaterialManagement>>();

                    if (result != null && result.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ {result.Count} assignments trouvés");
                        foreach (var assignment in result)
                        {
                            System.Diagnostics.Debug.WriteLine($"   - Assignment ID: {assignment.Id}, Product: {assignment.Product?.Name ?? "NULL"}, User: {assignment.UserId}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ Aucun assignment dans la réponse");
                    }

                    return result ?? new List<MaterialManagement>();
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

        // Récupérer un assignment par ID
        public async Task<MaterialManagement> GetByIdAsync(int id)
        {
            try
            {
                await ConfigureAuthAsync();
                return await _httpClient.GetFromJsonAsync<MaterialManagement>($"{BaseUrl}/{id}");
            }
            catch
            {
                return null;
            }
        }

        // Signer un produit
        public async Task<bool> SignProductAsync(int assignmentId, string signature)
        {
            try
            {
                await ConfigureAuthAsync();
                var signatureDto = new SignatureDto
                {
                    Id = assignmentId,
                    Signature = signature
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Sign", signatureDto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
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
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Assign", assignmentDto);

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Assignation API réussie");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Assignation API échouée: {response.StatusCode}");
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
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Récupérer tous les utilisateurs (si nécessaire pour l'assignation)
        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                await ConfigureAuthAsync();
                var result = await _httpClient.GetFromJsonAsync<List<User>>("https://localhost:7237/api/Users");
                return result ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        // Récupérer tous les états possibles
        public async Task<List<States>> GetStatesAsync()
        {
            try
            {
                await ConfigureAuthAsync();
                var result = await _httpClient.GetFromJsonAsync<List<States>>("https://localhost:7237/api/States");
                return result ?? new List<States>();
            }
            catch
            {
                return new List<States>();
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