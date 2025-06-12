using System.Net;
using System.Net.Http.Headers;

namespace TechStockMaui.Services
{
    public class TokenRefreshHandler : DelegatingHandler
    {
        private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);
        private bool _isRefreshing = false;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized &&
                !request.RequestUri.ToString().Contains("/Login"))
            {
                System.Diagnostics.Debug.WriteLine("Token expired detected, attempting auto-refresh");

                if (await TryRefreshTokenAsync())
                {
                    var newToken = await SecureStorage.GetAsync("auth_token");
                    if (!string.IsNullOrEmpty(newToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                        System.Diagnostics.Debug.WriteLine("Retrying request with new token");
                        response = await base.SendAsync(request, cancellationToken);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Auto-refresh failed, user needs to login manually");
                }
            }

            return response;
        }

        private async Task<bool> TryRefreshTokenAsync()
        {
            await _refreshSemaphore.WaitAsync();
            try
            {
                if (_isRefreshing)
                {
                    System.Diagnostics.Debug.WriteLine("Already refreshing token, waiting...");
                    return true;
                }

                _isRefreshing = true;

                var savedEmail = await SecureStorage.GetAsync("saved_email");
                var savedPassword = await SecureStorage.GetAsync("saved_password");
                var rememberMe = await SecureStorage.GetAsync("remember_me");

                if (string.IsNullOrEmpty(savedEmail) || string.IsNullOrEmpty(savedPassword) || rememberMe != "true")
                {
                    System.Diagnostics.Debug.WriteLine("No saved credentials or remember me disabled");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Attempting auto-reconnection for: {savedEmail}");

                using (var authService = new AuthService())
                {
                    var loginResult = await authService.LoginAsync(savedEmail, savedPassword, true);

                    if (loginResult.Success)
                    {
                        System.Diagnostics.Debug.WriteLine("Auto-reconnection successful");
                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Auto-reconnection failed, clearing saved credentials");
                        await ClearSavedCredentialsAsync();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during token refresh: {ex.Message}");
                return false;
            }
            finally
            {
                _isRefreshing = false;
                _refreshSemaphore.Release();
            }
        }

        private async Task ClearSavedCredentialsAsync()
        {
            try
            {
                SecureStorage.Remove("saved_email");
                SecureStorage.Remove("saved_password");
                SecureStorage.Remove("remember_me");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing credentials: {ex.Message}");
            }
        }
    }
}