using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TechStockMaui.Models;

namespace TechStockMaui.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7237/api/Users";

        public UserService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<UserRolesViewModel>> GetAllUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<UserRolesViewModel>>(BaseUrl);
        }

        public async Task<List<RoleItem>> GetRolesAsync(string userName)
        {
            return await _httpClient.GetFromJsonAsync<List<RoleItem>>($"{BaseUrl}/GetRoles/{userName}");
        }

        public async Task<bool> UpdateUserRolesAsync(string userName, List<string> roles)
        {
            var body = new { UserName = userName, Roles = roles };
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/ManageRoles", body);
            return response.IsSuccessStatusCode;
        }
    }
}
