using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TechStockMaui.Models; 

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

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Product>>(BaseUrl);
        }

        public async Task<List<Product>> GetProductsFilterAsync(string name = null, string serialNumber = null, int? typeId = null, int? supplierId = null, string userName = null)
        {
            var allProducts = await GetProductsAsync(); 

            var filtered = allProducts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                filtered = filtered.Where(p => p.Name != null && p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(serialNumber))
                filtered = filtered.Where(p => p.SerialNumber != null && p.SerialNumber.Contains(serialNumber, StringComparison.OrdinalIgnoreCase));

            if (typeId.HasValue)
                filtered = filtered.Where(p => p.TypeId == typeId.Value);

            if (supplierId.HasValue)
                filtered = filtered.Where(p => p.SupplierId == supplierId.Value);

            if (!string.IsNullOrWhiteSpace(userName))
                filtered = filtered.Where(p => p.AssignedUserName == userName);

            return filtered.ToList();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Product>($"{BaseUrl}/{id}");
        }

        public async Task<bool> CreateProductAsync(Product product)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, product);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{product.Id}", product);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> UnassignProductAsync(int productId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{BaseUrl}/Products/UnassignFromUser/{productId}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public async Task<List<User>> GetUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<User>>("https://localhost:7237/api/User");
        }

        public async Task<List<TechStockMaui.Models.TypeArticle.TypeArticle>> GetTypesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TechStockMaui.Models.TypeArticle.TypeArticle>>("https://localhost:7237/api/TypeArticles");
        }

        public async Task<List<TechStockMaui.Models.Supplier.Supplier>> GetSuppliersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TechStockMaui.Models.Supplier.Supplier>>("https://localhost:7237/api/Suppliers");
        }

    }
}
