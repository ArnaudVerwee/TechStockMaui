using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TechStockMaui.Models;

namespace TechStockMaui.Services
{
    public class MaterialManagementService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7237/api/MaterialManagements";

        public MaterialManagementService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<MaterialManagement>> GetAllAssignedAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<MaterialManagement>>($"{BaseUrl}/AssignedToMe");
        }

        public async Task<MaterialManagement> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<MaterialManagement>($"{BaseUrl}/{id}");
        }

        public async Task<bool> SaveSignatureAsync(int id, string signature)
        {
            var body = new { Id = id, Signature = signature };
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/SaveSignature", body);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AssignProductAsync(int productId, string userId, int stateId)
        {
            var body = new { Id = productId, UserId = userId, StateId = stateId };
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/AssignToUser", body);
            return response.IsSuccessStatusCode;
        }

 
        public async Task<List<User>> GetUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<User>>("https://localhost:7237/api/User");
        }

        public async Task<List<States>> GetStatesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<States>>("https://localhost:7237/api/States");
        }
    }
}
