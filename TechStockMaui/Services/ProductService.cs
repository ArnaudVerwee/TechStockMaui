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
    }
}
