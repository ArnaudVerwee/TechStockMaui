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

        private static string BaseUrl
        {
            get
            {
#if ANDROID
                return "http://10.0.2.2:7236/api/TypeArticles";
#else
                return "https://localhost:7237/api/TypeArticles";
#endif
            }
        }

        public TypeArticleService()
        {
            var handler = new HttpClientHandler();

#if ANDROID
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            System.Diagnostics.Debug.WriteLine($"TypeArticleService using: {BaseUrl}");
        }

        public async Task<List<TechStockMaui.Models.TypeArticle.TypeArticle>> GetAllAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GET Request to: {BaseUrl}");

                var result = await _httpClient.GetFromJsonAsync<List<TechStockMaui.Models.TypeArticle.TypeArticle>>(BaseUrl);

                System.Diagnostics.Debug.WriteLine($"Number of items retrieved: {result?.Count ?? 0}");

                return result ?? new List<TechStockMaui.Models.TypeArticle.TypeArticle>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error GetAll: {ex.Message}");
                return new List<TechStockMaui.Models.TypeArticle.TypeArticle>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetAll: {ex.Message}");
                return new List<TechStockMaui.Models.TypeArticle.TypeArticle>();
            }
        }

        public async Task<TechStockMaui.Models.TypeArticle.TypeArticle> GetByIdAsync(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GET Request to: {BaseUrl}/{id}");

                var result = await _httpClient.GetFromJsonAsync<TechStockMaui.Models.TypeArticle.TypeArticle>($"{BaseUrl}/{id}");

                if (result != null)
                {
                    System.Diagnostics.Debug.WriteLine($"TypeArticle retrieved: {result.Name}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No TypeArticle found with this ID");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error GetById: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetById: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateAsync(TechStockMaui.Models.TypeArticle.TypeArticle item)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"POST Request to: {BaseUrl}");
                System.Diagnostics.Debug.WriteLine($"Name to create: {item.Name}");

                var response = await _httpClient.PostAsJsonAsync(BaseUrl, item);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Creation successful");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Creation failed: {response.StatusCode}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error Create: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Create: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(TechStockMaui.Models.TypeArticle.TypeArticle item)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"PUT Request to: {BaseUrl}/{item.Id}");
                System.Diagnostics.Debug.WriteLine($"Name to update: {item.Name}");

                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{item.Id}", item);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Update successful");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Update failed: {response.StatusCode}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error Update: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Update: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"DELETE Request to: {BaseUrl}/{id}");

                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Deletion successful");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Deletion failed: {response.StatusCode}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error Delete: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Delete: {ex.Message}");
                return false;
            }
        }
    }
}