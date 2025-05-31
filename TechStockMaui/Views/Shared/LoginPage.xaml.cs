using TechStockMaui.Services;

namespace TechStockMaui.Views
{
    public partial class LoginPage : ContentPage
    {
        private AuthService _authService;

        public LoginPage()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            /*

            // Vérifier si l'utilisateur est déjà connecté
            if (await _authService.TryRestoreAuthenticationAsync())
            {
                await NavigateToDashboard();
            }
            */
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                await DisplayAlert("Erreur", "Veuillez saisir votre email", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Erreur", "Veuillez saisir votre mot de passe", "OK");
                return;
            }

            SetLoadingState(true);

            try
            {
                var result = await _authService.LoginAsync(EmailEntry.Text.Trim(), PasswordEntry.Text);

                if (result.Success)
                {
                    await NavigateToDashboard();
                }
                else
                {
                    // Afficher seulement les erreurs réelles de connexion
                    await DisplayAlert("Erreur de connexion",
                        result.Message ?? "Email ou mot de passe incorrect",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", "Impossible de se connecter au serveur", "OK");
                // Log l'erreur pour debug sans afficher à l'utilisateur
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async Task NavigateToDashboard()
        {
            Application.Current.MainPage = new AppShell();
        }

        private void SetLoadingState(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
            LoginButton.IsEnabled = !isLoading;
            EmailEntry.IsEnabled = !isLoading;
            PasswordEntry.IsEnabled = !isLoading;
        }

        private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Mot de passe oublié", "Fonctionnalité à implémenter", "OK");
        }

        private async void OnRegisterTapped(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Inscription", "Fonctionnalité à implémenter", "OK");
        }
    }
}