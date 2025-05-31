using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using TechStockMaui.Models.Auth;

namespace TechStockMaui.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7237/api/Auth"; // Vérifiez cette URL !

        public AuthService()
        {
            _httpClient = new HttpClient();
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(GetStoredToken());

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔐 Tentative de connexion: {email}");
                System.Diagnostics.Debug.WriteLine($"🌐 URL utilisée: {BaseUrl}/Login");

                var loginRequest = new LoginRequest
                {
                    Email = email,
                    Password = password
                };

                // Debug: Voir les données envoyées
                var jsonData = JsonSerializer.Serialize(loginRequest);
                System.Diagnostics.Debug.WriteLine($"📤 Données envoyées: {jsonData}");

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Login", loginRequest);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"📊 Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"📥 Response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Token reçu, sauvegarde...");

                        await SecureStorage.SetAsync("auth_token", result.Token);
                        await SecureStorage.SetAsync("user_email", email);
                        await SecureStorage.SetAsync("user_id", result.UserId ?? "");

                        SetAuthorizationHeader(result.Token);

                        return new AuthResult { Success = true, Message = "Connexion réussie" };
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ Token manquant dans la réponse");
                        return new AuthResult { Success = false, Message = "Réponse invalide du serveur" };
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Erreur HTTP: {response.StatusCode}");

                    // Essayer de parser l'erreur
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<JsonElement>(content);
                        var errorMessage = errorResponse.TryGetProperty("message", out var msg) ?
                            msg.GetString() : "Erreur de connexion";
                        return new AuthResult { Success = false, Message = errorMessage };
                    }
                    catch
                    {
                        return new AuthResult { Success = false, Message = $"Erreur {response.StatusCode}: {content}" };
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"🌐 Erreur de connexion: {ex.Message}");
                return new AuthResult { Success = false, Message = $"Impossible de contacter le serveur: {ex.Message}" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Erreur inattendue: {ex.Message}");
                return new AuthResult { Success = false, Message = $"Erreur inattendue: {ex.Message}" };
            }
        }

        // Test direct de l'API
        public async Task<string> TestApiConnectionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🧪 Test de connexion vers: {BaseUrl}");

                var response = await _httpClient.GetAsync(BaseUrl);
                var content = await response.Content.ReadAsStringAsync();

                return $"Status: {response.StatusCode}\nContent: {content.Substring(0, Math.Min(200, content.Length))}";
            }
            catch (Exception ex)
            {
                return $"Erreur: {ex.Message}";
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔓 Début déconnexion...");

                // Supprimer l'en-tête d'autorisation d'abord
                _httpClient.DefaultRequestHeaders.Authorization = null;
                System.Diagnostics.Debug.WriteLine("✅ Headers nettoyés");

                // Vider le SecureStorage (version simple)
                await Task.Run(() =>
                {
                    try
                    {
                        SecureStorage.RemoveAll();
                        System.Diagnostics.Debug.WriteLine("✅ SecureStorage vidé");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Erreur SecureStorage: {ex.Message}");
                    }
                });

                System.Diagnostics.Debug.WriteLine("✅ Déconnexion terminée");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur logout: {ex.Message}");
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

                // Vérifier si le token est encore valide en appelant l'API
                try
                {
                    var response = await _httpClient.GetAsync($"{BaseUrl}/ValidateToken");
                    if (response.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Token valide - connexion automatique");
                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ Token expiré - suppression");
                        // Token expiré, on le supprime
                        await LogoutAsync();
                        return false;
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("❌ Erreur validation token - suppression");
                    // Erreur de réseau ou token invalide
                    await LogoutAsync();
                    return false;
                }
            }
            return false;
        }
    }
}