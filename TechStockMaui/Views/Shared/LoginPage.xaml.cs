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

            TranslationService.Instance.CultureChanged += OnCultureChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadTranslationsAsync();

            await LoadSavedCredentialsAsync();

        }

        private async Task LoadSavedCredentialsAsync()
        {
            try
            {
                var rememberMeEnabled = await _authService.IsRememberMeEnabledAsync();

                if (rememberMeEnabled)
                {
                    var savedEmail = await SecureStorage.GetAsync("saved_email");
                    var savedPassword = await SecureStorage.GetAsync("saved_password");

                    if (!string.IsNullOrEmpty(savedEmail) && !string.IsNullOrEmpty(savedPassword))
                    {
                        System.Diagnostics.Debug.WriteLine($"Loading saved credentials for: {savedEmail}");

                        
                        if (EmailEntry != null)
                            EmailEntry.Text = savedEmail;

                        if (PasswordEntry != null)
                            PasswordEntry.Text = savedPassword;

                        if (RememberMeSwitch != null)
                            RememberMeSwitch.IsToggled = true;

                        System.Diagnostics.Debug.WriteLine("Credentials loaded and fields pre-filled");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Remember me enabled but no credentials found");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Remember me not enabled");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading saved credentials: {ex.Message}");
            }
        }
        private async Task TryAutoLoginAsync()
        {
            try
            {
                if (await _authService.TryAutoLoginAsync())
                {
                    System.Diagnostics.Debug.WriteLine("Auto-login successful, navigating to dashboard");
                    await NavigateToDashboard();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Auto-login failed or disabled");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-login error: {ex.Message}");
            }
        }

        private async Task LoadTranslationsAsync()
        {
            try
            {
                var currentCulture = TranslationService.Instance.GetCurrentCulture();
                await TranslationService.Instance.LoadTranslationsAsync(currentCulture);
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Translations loading error: {ex.Message}");
            }
        }

        private async Task<string> GetTextAsync(string key, string fallback = null)
        {
            try
            {
                var text = await TranslationService.Instance.GetTranslationAsync(key);
                return !string.IsNullOrEmpty(text) && text != key ? text : (fallback ?? key);
            }
            catch
            {
                return fallback ?? key;
            }
        }

        private async Task UpdateTextsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Updating Login texts");

                Title = await GetTextAsync("Login", "Login");

                if (SubtitleLabel != null)
                    SubtitleLabel.Text = await GetTextAsync("ConnectToAccount", "Connect to your account");

                if (EmailLabel != null)
                    EmailLabel.Text = await GetTextAsync("Email", "Email");

                if (PasswordLabel != null)
                    PasswordLabel.Text = await GetTextAsync("Password", "Password");

                if (EmailEntry != null)
                    EmailEntry.Placeholder = await GetTextAsync("EmailPlaceholder", "your.email@example.com");

                if (PasswordEntry != null)
                    PasswordEntry.Placeholder = await GetTextAsync("PasswordPlaceholder", "Your password");

                if (LoginButton != null)
                    LoginButton.Text = await GetTextAsync("Login", "Login");

                if (RememberMeLabel != null)
                    RememberMeLabel.Text = await GetTextAsync("RememberMe", "Remember me");

                if (ForgotPasswordLabel != null)
                    ForgotPasswordLabel.Text = await GetTextAsync("ForgotPassword", "Forgot password?");

                if (NoAccountLabel != null)
                    NoAccountLabel.Text = await GetTextAsync("NoAccount", "No account?");

                if (RegisterLabel != null)
                    RegisterLabel.Text = await GetTextAsync("SignUp", "Sign up");

                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("Login texts updated");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTextsAsync error: {ex.Message}");
            }
        }

        private async void OnCultureChanged(object sender, string newCulture)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Login - Language changed to: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var emailRequiredMsg = await GetTextAsync("EmailRequired", "Veuillez saisir votre email");
                await DisplayAlert(errorTitle, emailRequiredMsg, "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var passwordRequiredMsg = await GetTextAsync("PasswordRequired", "Veuillez saisir votre mot de passe");
                await DisplayAlert(errorTitle, passwordRequiredMsg, "OK");
                return;
            }

            SetLoadingState(true);

            try
            {
                
                var rememberMe = RememberMeSwitch?.IsToggled ?? false;

                var result = await _authService.LoginAsync(EmailEntry.Text.Trim(), PasswordEntry.Text, rememberMe);
                if (result.Success)
                {
                    await NavigateToDashboard();
                }
                else
                {
                    var connectionErrorTitle = await GetTextAsync("ConnectionError", "Erreur de connexion");
                    var invalidCredentialsMsg = await GetTextAsync("InvalidCredentials", "Email ou mot de passe incorrect");
                    await DisplayAlert(connectionErrorTitle,
                        result.Message ?? invalidCredentialsMsg,
                        "OK");
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var serverConnectionMsg = await GetTextAsync("ServerConnectionError", "Impossible de se connecter au serveur");
                await DisplayAlert(errorTitle, serverConnectionMsg, "OK");
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
            var forgotPasswordTitle = await GetTextAsync("ForgotPassword", "Mot de passe oublié");
            var featureToImplementMsg = await GetTextAsync("FeatureToImplement", "Fonctionnalité à implémenter");
            await DisplayAlert(forgotPasswordTitle, featureToImplementMsg, "OK");
        }

        private async void OnRegisterTapped(object sender, TappedEventArgs e)
        {
            var registerTitle = await GetTextAsync("Register", "Inscription");
            var featureToImplementMsg = await GetTextAsync("FeatureToImplement", "Fonctionnalité à implémenter");
            await DisplayAlert(registerTitle, featureToImplementMsg, "OK");
        }

        private async void OnLanguageClicked(object sender, EventArgs e)
        {
            try
            {
                var translationService = TranslationService.Instance;
                var currentCulture = translationService.GetCurrentCulture();

                var options = new List<string>();
                foreach (var culture in translationService.GetSupportedCultures())
                {
                    var flag = translationService.GetLanguageFlag(culture);
                    var name = translationService.GetLanguageDisplayName(culture);
                    var current = culture == currentCulture ? " ✓" : "";
                    options.Add($"{flag} {name}{current}");
                }

                var cancelText = await GetTextAsync("Cancel", "Cancel");
                var titleText = "🌍 " + await GetTextAsync("ChooseLanguage", "Choose language");

                var selectedOption = await DisplayActionSheet(titleText, cancelText, null, options.ToArray());

                if (!string.IsNullOrEmpty(selectedOption) && selectedOption != cancelText)
                {
                    string newCulture = null;
                    if (selectedOption.Contains("EN")) newCulture = "en";
                    else if (selectedOption.Contains("FR")) newCulture = "fr";
                    else if (selectedOption.Contains("NL")) newCulture = "nl";

                    if (newCulture != null && newCulture != currentCulture)
                    {
                        System.Diagnostics.Debug.WriteLine($"Changing to: {newCulture}");
                        await translationService.SetCurrentCultureAsync(newCulture);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

        private async Task UpdateLanguageFlag()
        {
            try
            {
                var translationService = TranslationService.Instance;
                var currentCulture = translationService.GetCurrentCulture();
                var flag = translationService.GetLanguageFlag(currentCulture);

                if (LanguageFlag != null)
                    LanguageFlag.Text = flag;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Flag update error: {ex.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        ~LoginPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}