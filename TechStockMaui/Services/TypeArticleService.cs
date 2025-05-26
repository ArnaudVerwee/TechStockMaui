using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TechStockMaui.Models;
using TechStockMaui.Models.TypeArticle;

namespace TechStockMaui.Services
{
    public class TypeArticleService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7237/api/TypeArticles";

        public TypeArticleService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<TypeArticle>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TypeArticle>>(BaseUrl);
        }

        public async Task<TypeArticle> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<TypeArticle>($"{BaseUrl}/{id}");
        }

        public async Task<bool> CreateAsync(TypeArticle item)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, item);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(TypeArticle item)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{item.Id}", item);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
