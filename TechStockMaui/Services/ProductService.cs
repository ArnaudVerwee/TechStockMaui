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

        private static string ProductUrl => $"{BaseUrl}/Product";
        private static string UserUrl => $"{BaseUrl}/User";
        private static string TypeArticleUrl => $"{BaseUrl}/TypeArticles";
        private static string SupplierUrl => $"{BaseUrl}/Suppliers";
        private static string MaterialManagementUrl => $"{BaseUrl}/MaterialManagement";

        public ProductService()
        {
            var handler = new HttpClientHandler();

#if ANDROID
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            System.Diagnostics.Debug.WriteLine($"ProductService uses: {BaseUrl}");
        }

        private async Task<HttpClient> GetAuthenticatedHttpClientAsync()
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

                return _httpClient;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Token retrieval error: {ex.Message}");
                return _httpClient;
            }
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                System.Diagnostics.Debug.WriteLine($"Retrieving products from: {ProductUrl}");
                return await client.GetFromJsonAsync<List<Product>>(ProductUrl) ?? new List<Product>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetProductsAsync error: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<bool> CreateProductAsync(Product product)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Start CreateProductAsync");
                System.Diagnostics.Debug.WriteLine($"Product to create: {product.Name}");
                System.Diagnostics.Debug.WriteLine($"SerialNumber: {product.SerialNumber}");
                System.Diagnostics.Debug.WriteLine($"TypeId: {product.TypeId}");
                System.Diagnostics.Debug.WriteLine($"SupplierId: {product.SupplierId}");

                var client = await GetAuthenticatedHttpClientAsync();

                var createRequest = new CreateProductRequest
                {
                    Name = product.Name,
                    SerialNumber = product.SerialNumber,
                    TypeId = product.TypeId,
                    SupplierId = product.SupplierId,
                    AssignedUserName = product.AssignedUserName
                };

                System.Diagnostics.Debug.WriteLine($"URL: {ProductUrl}");
                System.Diagnostics.Debug.WriteLine($"Data sent: Name={createRequest.Name}, TypeId={createRequest.TypeId}, SupplierId={createRequest.SupplierId}");

                var response = await client.PostAsJsonAsync(ProductUrl, createRequest);

                System.Diagnostics.Debug.WriteLine($"Status Code: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"API error: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"Error content: {errorContent}");
                }
                else
                {
                    var successContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine("API success");
                    System.Diagnostics.Debug.WriteLine($"Response content: {successContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateProductAsync exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
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
                var url = $"{ProductUrl}/filter{queryString}";

                System.Diagnostics.Debug.WriteLine($"Filter URL: {url}");

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
                System.Diagnostics.Debug.WriteLine($"GetProductsFilterAsync error: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                return await client.GetFromJsonAsync<Product>($"{ProductUrl}/{id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetProductByIdAsync error: {ex.Message}");
                return null;
            }
        }

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
                System.Diagnostics.Debug.WriteLine($"UpdateProductAsync error: {ex.Message}");
                return false;
            }
        }

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
                System.Diagnostics.Debug.WriteLine($"DeleteProductAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UnassignProductAsync(int productId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Unassigning product {productId}...");

                var client = await GetAuthenticatedHttpClientAsync();
                var response = await client.DeleteAsync($"{MaterialManagementUrl}/product/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Product unassigned successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Unassignment API error: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UnassignProductAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                System.Diagnostics.Debug.WriteLine($"Retrieving users from: {UserUrl}");
                return await client.GetFromJsonAsync<List<User>>(UserUrl) ?? new List<User>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUsersAsync error: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<List<TechStockMaui.Models.TypeArticle.TypeArticle>> GetTypesAsync()
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                System.Diagnostics.Debug.WriteLine($"Retrieving types from: {TypeArticleUrl}");
                return await client.GetFromJsonAsync<List<TechStockMaui.Models.TypeArticle.TypeArticle>>(TypeArticleUrl) ?? new List<TechStockMaui.Models.TypeArticle.TypeArticle>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTypesAsync error: {ex.Message}");
                return new List<TechStockMaui.Models.TypeArticle.TypeArticle>();
            }
        }

        public async Task<List<TechStockMaui.Models.Supplier.Supplier>> GetSuppliersAsync()
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                System.Diagnostics.Debug.WriteLine($"Retrieving suppliers from: {SupplierUrl}");
                return await client.GetFromJsonAsync<List<TechStockMaui.Models.Supplier.Supplier>>(SupplierUrl) ?? new List<TechStockMaui.Models.Supplier.Supplier>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSuppliersAsync error: {ex.Message}");
                return new List<TechStockMaui.Models.Supplier.Supplier>();
            }
        }

        public async Task<string> TestProductEndpoint()
        {
            try
            {
                var client = await GetAuthenticatedHttpClientAsync();
                System.Diagnostics.Debug.WriteLine($"Testing endpoint: {ProductUrl}");

                var response = await client.GetAsync(ProductUrl);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Content: {content}");
                System.Diagnostics.Debug.WriteLine($"Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");

                return $"Status: {response.StatusCode}, Content: {content}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Test error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
    }
}