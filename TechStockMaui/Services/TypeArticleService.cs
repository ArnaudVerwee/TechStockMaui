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

        public async Task<List<TechStockMaui.Models.TypeArticle.TypeArticle>> GetAllAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GET Request vers: {BaseUrl}");

                var result = await _httpClient.GetFromJsonAsync<List<TechStockMaui.Models.TypeArticle.TypeArticle>>(BaseUrl);

                System.Diagnostics.Debug.WriteLine($"Nombre d'éléments récupérés: {result?.Count ?? 0}");

                return result ?? new List<TechStockMaui.Models.TypeArticle.TypeArticle>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur HTTP GetAll: {ex.Message}");
                return new List<TechStockMaui.Models.TypeArticle.TypeArticle>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur GetAll: {ex.Message}");
                return new List<TechStockMaui.Models.TypeArticle.TypeArticle>();
            }
        }

        public async Task<TechStockMaui.Models.TypeArticle.TypeArticle> GetByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<TechStockMaui.Models.TypeArticle.TypeArticle>($"{BaseUrl}/{id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur GetById: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateAsync(TechStockMaui.Models.TypeArticle.TypeArticle item)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"POST Request vers: {BaseUrl}");
                System.Diagnostics.Debug.WriteLine($"Nom à créer: {item.Name}");

                var response = await _httpClient.PostAsJsonAsync(BaseUrl, item);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Création réussie");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Création échouée: {response.StatusCode}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur HTTP Create: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur Create: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(TechStockMaui.Models.TypeArticle.TypeArticle item)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{item.Id}", item);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur Update: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur Delete: {ex.Message}");
                return false;
            }
        }
    }
}