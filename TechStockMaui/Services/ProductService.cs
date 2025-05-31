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
        private const string BaseUrl = "https://localhost:7237/api/Product";

        public ProductService()
        {
            _httpClient = new HttpClient();
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

        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                return await client.GetFromJsonAsync<List<Product>>(BaseUrl) ?? new List<Product>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetProductsAsync: {ex.Message}");
                return new List<Product>();
            }
        }

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

                System.Diagnostics.Debug.WriteLine($"🌐 URL: {BaseUrl}");
                System.Diagnostics.Debug.WriteLine($"📤 Données envoyées: Name={createRequest.Name}, TypeId={createRequest.TypeId}, SupplierId={createRequest.SupplierId}");

                var response = await client.PostAsJsonAsync(BaseUrl, createRequest);

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
                // ✅ CORRECTION: Utiliser GetAuthenticatedHttpClientAsync au lieu de SetAuthorizationHeader
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
                var url = $"{BaseUrl}/filter{queryString}";

                System.Diagnostics.Debug.WriteLine($"🔍 URL filtrage: {url}");

                // ✅ CORRECTION: Utiliser client au lieu de _httpClient
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

        public async Task<Product> GetProductByIdAsync(int id)
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                return await client.GetFromJsonAsync<Product>($"{BaseUrl}/{id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetProductByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                var response = await client.PutAsJsonAsync($"{BaseUrl}/{product.Id}", product);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur UpdateProductAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                var response = await client.DeleteAsync($"{BaseUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur DeleteProductAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UnassignProductAsync(int productId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 Désassignation du produit {productId}...");

                var client = await GetAuthenticatedHttpClientAsync();
                var response = await client.DeleteAsync($"https://localhost:7237/api/MaterialManagement/product/{productId}");

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

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                // ✅ UTILISER GetAuthenticatedHttpClientAsync comme les autres méthodes
                var client = await GetAuthenticatedHttpClientAsync();
                return await client.GetFromJsonAsync<List<User>>("https://localhost:7237/api/User") ?? new List<User>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetUsersAsync: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<List<TechStockMaui.Models.TypeArticle.TypeArticle>> GetTypesAsync()
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                return await client.GetFromJsonAsync<List<TechStockMaui.Models.TypeArticle.TypeArticle>>("https://localhost:7237/api/TypeArticles") ?? new List<TechStockMaui.Models.TypeArticle.TypeArticle>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetTypesAsync: {ex.Message}");
                return new List<TechStockMaui.Models.TypeArticle.TypeArticle>();
            }
        }

        public async Task<List<TechStockMaui.Models.Supplier.Supplier>> GetSuppliersAsync()
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                return await client.GetFromJsonAsync<List<TechStockMaui.Models.Supplier.Supplier>>("https://localhost:7237/api/Suppliers") ?? new List<TechStockMaui.Models.Supplier.Supplier>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetSuppliersAsync: {ex.Message}");
                return new List<TechStockMaui.Models.Supplier.Supplier>();
            }
        }
    }
}