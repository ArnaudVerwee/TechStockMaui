using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TechStockMaui.Models;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace TechStockMaui.Services
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;

        // ✅ Configuration adaptative pour Android/Windows
        private static string BaseUrl
        {
            get
            {
#if ANDROID
                return "http://10.0.2.2:7236/api";  // Pour Android émulateur
#else
                return "https://localhost:7237/api"; // Pour Windows
#endif
            }
        }

        // ✅ URLs spécifiques pour chaque endpoint
        private static string ProductUrl => $"{BaseUrl}/Product";
        private static string UserUrl => $"{BaseUrl}/User";
        private static string TypeArticleUrl => $"{BaseUrl}/TypeArticles";
        private static string SupplierUrl => $"{BaseUrl}/Suppliers";
        private static string MaterialManagementUrl => $"{BaseUrl}/MaterialManagement";

        public ProductService()
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

            System.Diagnostics.Debug.WriteLine($"🌐 ProductService utilise: {BaseUrl}");
        }

        private async Task<HttpClient> GetAuthenticatedHttpClientAsync()
        {
            try
            {
                // Récupérer le token stocké
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

                return _httpClient;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur récupération token: {ex.Message}");
                return _httpClient;
            }
        }

        // ✅ CORRECTION: Utiliser ProductUrl au lieu de BaseUrl
        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                System.Diagnostics.Debug.WriteLine($"🔍 Récupération produits depuis: {ProductUrl}");
                return await client.GetFromJsonAsync<List<Product>>(ProductUrl) ?? new List<Product>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetProductsAsync: {ex.Message}");
                return new List<Product>();
            }
        }

        // ✅ CORRECTION: Utiliser ProductUrl au lieu de BaseUrl
        public async Task<bool> CreateProductAsync(Product product)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 Début CreateProductAsync");
                System.Diagnostics.Debug.WriteLine($"📦 Produit à créer: {product.Name}");
                System.Diagnostics.Debug.WriteLine($"🔢 SerialNumber: {product.SerialNumber}");
                System.Diagnostics.Debug.WriteLine($"🏷️ TypeId: {product.TypeId}");
                System.Diagnostics.Debug.WriteLine($"📦 SupplierId: {product.SupplierId}");

                var client = await GetAuthenticatedHttpClientAsync();

                // Créer un objet simplifié pour l'API
                var createRequest = new CreateProductRequest
                {
                    Name = product.Name,
                    SerialNumber = product.SerialNumber,
                    TypeId = product.TypeId,
                    SupplierId = product.SupplierId,
                    AssignedUserName = product.AssignedUserName
                };

                System.Diagnostics.Debug.WriteLine($"🌐 URL: {ProductUrl}");
                System.Diagnostics.Debug.WriteLine($"📤 Données envoyées: Name={createRequest.Name}, TypeId={createRequest.TypeId}, SupplierId={createRequest.SupplierId}");

                var response = await client.PostAsJsonAsync(ProductUrl, createRequest);

                System.Diagnostics.Debug.WriteLine($"📊 Status Code: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Erreur API: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"📄 Contenu erreur: {errorContent}");
                }
                else
                {
                    var successContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"✅ Succès API");
                    System.Diagnostics.Debug.WriteLine($"📄 Contenu réponse: {successContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception CreateProductAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"📍 StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsFilterAsync(
            string? name = null,
            string? serialNumber = null,
            int? typeId = null,
            int? supplierId = null,
            string? userId = null)
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();

                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(name))
                    queryParams.Add($"name={Uri.EscapeDataString(name)}");

                if (!string.IsNullOrWhiteSpace(serialNumber))
                    queryParams.Add($"serialNumber={Uri.EscapeDataString(serialNumber)}");

                if (typeId.HasValue)
                    queryParams.Add($"typeId={typeId.Value}");

                if (supplierId.HasValue)
                    queryParams.Add($"supplierId={supplierId.Value}");

                if (!string.IsNullOrWhiteSpace(userId))
                    queryParams.Add($"userId={Uri.EscapeDataString(userId)}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                // ✅ CORRECTION: Utiliser ProductUrl au lieu de BaseUrl
                var url = $"{ProductUrl}/filter{queryString}";

                System.Diagnostics.Debug.WriteLine($"🔍 URL filtrage: {url}");

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<IEnumerable<Product>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return products ?? new List<Product>();
                }

                return new List<Product>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetProductsFilterAsync: {ex.Message}");
                return new List<Product>();
            }
        }

        // ✅ CORRECTION: Utiliser ProductUrl au lieu de BaseUrl
        public async Task<Product> GetProductByIdAsync(int id)
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                return await client.GetFromJsonAsync<Product>($"{ProductUrl}/{id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetProductByIdAsync: {ex.Message}");
                return null;
            }
        }

        // ✅ CORRECTION: Utiliser ProductUrl au lieu de BaseUrl
        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                var response = await client.PutAsJsonAsync($"{ProductUrl}/{product.Id}", product);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur UpdateProductAsync: {ex.Message}");
                return false;
            }
        }

        // ✅ CORRECTION: Utiliser ProductUrl au lieu de BaseUrl
        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                var response = await client.DeleteAsync($"{ProductUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur DeleteProductAsync: {ex.Message}");
                return false;
            }
        }

        // ✅ CORRECTION: Utiliser MaterialManagementUrl au lieu de l'URL hardcodée
        public async Task<bool> UnassignProductAsync(int productId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 Désassignation du produit {productId}...");

                var client = await GetAuthenticatedHttpClientAsync();
                var response = await client.DeleteAsync($"{MaterialManagementUrl}/product/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Produit désassigné avec succès");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Erreur API désassignation: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur UnassignProductAsync: {ex.Message}");
                return false;
            }
        }

        // ✅ CORRECTION: Utiliser UserUrl au lieu de l'URL hardcodée
        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                System.Diagnostics.Debug.WriteLine($"🔍 Récupération utilisateurs depuis: {UserUrl}");
                return await client.GetFromJsonAsync<List<User>>(UserUrl) ?? new List<User>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetUsersAsync: {ex.Message}");
                return new List<User>();
            }
        }

        // ✅ CORRECTION: Utiliser TypeArticleUrl au lieu de l'URL hardcodée
        public async Task<List<TechStockMaui.Models.TypeArticle.TypeArticle>> GetTypesAsync()
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                System.Diagnostics.Debug.WriteLine($"🔍 Récupération types depuis: {TypeArticleUrl}");
                return await client.GetFromJsonAsync<List<TechStockMaui.Models.TypeArticle.TypeArticle>>(TypeArticleUrl) ?? new List<TechStockMaui.Models.TypeArticle.TypeArticle>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetTypesAsync: {ex.Message}");
                return new List<TechStockMaui.Models.TypeArticle.TypeArticle>();
            }
        }

        // ✅ CORRECTION: Utiliser SupplierUrl au lieu de l'URL hardcodée
        public async Task<List<TechStockMaui.Models.Supplier.Supplier>> GetSuppliersAsync()
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                System.Diagnostics.Debug.WriteLine($"🔍 Récupération fournisseurs depuis: {SupplierUrl}");
                return await client.GetFromJsonAsync<List<TechStockMaui.Models.Supplier.Supplier>>(SupplierUrl) ?? new List<TechStockMaui.Models.Supplier.Supplier>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetSuppliersAsync: {ex.Message}");
                return new List<TechStockMaui.Models.Supplier.Supplier>();
            }
        }

        // ✅ Méthode de débogage pour tester la connectivité
        public async Task<string> TestProductEndpoint()
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                System.Diagnostics.Debug.WriteLine($"🧪 Test de l'endpoint: {ProductUrl}");

                var response = await client.GetAsync(ProductUrl);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"🧪 Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"🧪 Content: {content}");
                System.Diagnostics.Debug.WriteLine($"🧪 Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");

                return $"Status: {response.StatusCode}, Content: {content}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🧪 Erreur test: {ex.Message}");
                return $"Erreur: {ex.Message}";
            }
        }
    }
}