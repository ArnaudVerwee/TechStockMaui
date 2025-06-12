using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TechStockMaui.Models.Supplier;

namespace TechStockMaui.Services
{
    public class SupplierService
    {
        private readonly HttpClient _httpClient;

        private static string BaseUrl
        {
            get
            {
#if ANDROID
                return "http://10.0.2.2:7236/api/Suppliers";
#else
                return "https://localhost:7237/api/Suppliers";
#endif
            }
        }

        public SupplierService()
        {
            var handler = new HttpClientHandler();

#if ANDROID
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            System.Diagnostics.Debug.WriteLine($"SupplierService using: {BaseUrl}");
        }

        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GET Request to: {BaseUrl}");

                var result = await _httpClient.GetFromJsonAsync<List<Supplier>>(BaseUrl);

                System.Diagnostics.Debug.WriteLine($"Number of suppliers retrieved: {result?.Count ?? 0}");

                return result ?? new List<Supplier>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error GetSuppliersAsync: {ex.Message}");
                return new List<Supplier>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetSuppliersAsync: {ex.Message}");
                return new List<Supplier>();
            }
        }

        public async Task<Supplier> GetSupplierByIdAsync(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GET Request to: {BaseUrl}/{id}");

                var result = await _httpClient.GetFromJsonAsync<Supplier>($"{BaseUrl}/{id}");

                if (result != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Supplier retrieved: {result.Name}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No supplier found with this ID");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error GetSupplierByIdAsync: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetSupplierByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateSupplierAsync(Supplier supplier)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"POST Request to: {BaseUrl}");
                System.Diagnostics.Debug.WriteLine($"Supplier to create: {supplier.Name}");

                var response = await _httpClient.PostAsJsonAsync(BaseUrl, supplier);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Supplier creation successful");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Supplier creation failed: {response.StatusCode}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error CreateSupplierAsync: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error CreateSupplierAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"PUT Request to: {BaseUrl}/{supplier.Id}");
                System.Diagnostics.Debug.WriteLine($"Supplier to update: {supplier.Name}");

                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{supplier.Id}", supplier);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Supplier update successful");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Supplier update failed: {response.StatusCode}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error UpdateSupplierAsync: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error UpdateSupplierAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteSupplierAsync(int id)
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
                    System.Diagnostics.Debug.WriteLine("Supplier deletion successful");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Supplier deletion failed: {response.StatusCode}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error DeleteSupplierAsync: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error DeleteSupplierAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Supplier>> GetSuppliersFilterAsync(string name = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Filtering suppliers by name: {name ?? "no filter"}");

                var allSuppliers = await GetSuppliersAsync();

                if (string.IsNullOrWhiteSpace(name))
                {
                    System.Diagnostics.Debug.WriteLine($"No filter applied, returning all {allSuppliers.Count} suppliers");
                    return allSuppliers;
                }

                var filteredSuppliers = allSuppliers
                    .Where(s => s.Name != null && s.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Filter applied, returning {filteredSuppliers.Count} suppliers");
                return filteredSuppliers;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetSuppliersFilterAsync: {ex.Message}");
                return new List<Supplier>();
            }
        }
    }
}