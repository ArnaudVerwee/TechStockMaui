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
        private const string BaseUrl = "https://localhost:7237/api/Suppliers";

        public SupplierService()
        {
            _httpClient = new HttpClient();
        }

        // Récupérer tous les fournisseurs
        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Supplier>>(BaseUrl) ?? new List<Supplier>();
            }
            catch
            {
                return new List<Supplier>();
            }
        }

        // Récupérer un fournisseur par ID
        public async Task<Supplier> GetSupplierByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Supplier>($"{BaseUrl}/{id}");
            }
            catch
            {
                return null;
            }
        }

        // Créer un nouveau fournisseur
        public async Task<bool> CreateSupplierAsync(Supplier supplier)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(BaseUrl, supplier);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Mettre à jour un fournisseur
        public async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{supplier.Id}", supplier);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Supprimer un fournisseur
        public async Task<bool> DeleteSupplierAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Filtrer les fournisseurs par nom
        public async Task<List<Supplier>> GetSuppliersFilterAsync(string name = null)
        {
            try
            {
                var allSuppliers = await GetSuppliersAsync();

                if (string.IsNullOrWhiteSpace(name))
                    return allSuppliers;

                return allSuppliers
                    .Where(s => s.Name != null && s.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            catch
            {
                return new List<Supplier>();
            }
        }
    }
}