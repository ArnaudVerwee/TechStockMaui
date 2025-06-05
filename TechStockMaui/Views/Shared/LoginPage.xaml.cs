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

            // ✅ AJOUT: S'abonner aux changements de langue
            TranslationService.Instance.CultureChanged += OnCultureChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // ✅ AJOUT: Charger les traductions
            await LoadTranslationsAsync();

            /*
            // Vérifier si l'utilisateur est déjà connecté
            if (await _authService.TryRestoreAuthenticationAsync())
            {
                await NavigateToDashboard();
            }
            */
        }

        // ✅ AJOUT: Charger les traductions
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
                System.Diagnostics.Debug.WriteLine($"❌ Erreur chargement traductions: {ex.Message}");
            }
        }

        // ✅ AJOUT: Helper pour récupérer une traduction
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

        // ✅ AJOUT: Mettre à jour tous les textes de l'interface
        private async Task UpdateTextsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🌍 Mise à jour des textes Login");

                // ✅ Titre de la page
                Title = await GetTextAsync("Login", "Login");

                // ✅ Sous-titre
                if (SubtitleLabel != null)
                    SubtitleLabel.Text = await GetTextAsync("ConnectToAccount", "Connect to your account");

                // ✅ Labels des champs
                if (EmailLabel != null)
                    EmailLabel.Text = await GetTextAsync("Email", "Email");

                if (PasswordLabel != null)
                    PasswordLabel.Text = await GetTextAsync("Password", "Password");

                // ✅ Placeholders des Entry
                if (EmailEntry != null)
                    EmailEntry.Placeholder = await GetTextAsync("EmailPlaceholder", "your.email@example.com");

                if (PasswordEntry != null)
                    PasswordEntry.Placeholder = await GetTextAsync("PasswordPlaceholder", "Your password");

                // ✅ Bouton de connexion
                if (LoginButton != null)
                    LoginButton.Text = await GetTextAsync("Login", "Login");

                // ✅ Checkbox "Se souvenir de moi"
                if (RememberMeLabel != null)
                    RememberMeLabel.Text = await GetTextAsync("RememberMe", "Remember me");

                // ✅ Liens
                if (ForgotPasswordLabel != null)
                    ForgotPasswordLabel.Text = await GetTextAsync("ForgotPassword", "Forgot password?");

                if (NoAccountLabel != null)
                    NoAccountLabel.Text = await GetTextAsync("NoAccount", "No account?");

                if (RegisterLabel != null)
                    RegisterLabel.Text = await GetTextAsync("SignUp", "Sign up");

                // ✅ Sélecteur de langue
                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                // ✅ Mettre à jour l'indicateur de langue
                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("✅ Textes Login mis à jour");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur UpdateTextsAsync: {ex.Message}");
            }
        }

        // ✅ AJOUT: Callback quand la langue change
        private async void OnCultureChanged(object sender, string newCulture)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🌍 Login - Langue changée vers: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
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
                var result = await _authService.LoginAsync(EmailEntry.Text.Trim(), PasswordEntry.Text);
                if (result.Success)
                {
                    await NavigateToDashboard();
                }
                else
                {
                    // Afficher seulement les erreurs réelles de connexion
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
                // Log l'erreur pour debug sans afficher à l'utilisateur
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private async Task NavigateToDashboard()
        {
            Application.Current.MainPage = new AppShell();
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private void SetLoadingState(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
            LoginButton.IsEnabled = !isLoading;
            EmailEntry.IsEnabled = !isLoading;
            PasswordEntry.IsEnabled = !isLoading;
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
        {
            var forgotPasswordTitle = await GetTextAsync("ForgotPassword", "Mot de passe oublié");
            var featureToImplementMsg = await GetTextAsync("FeatureToImplement", "Fonctionnalité à implémenter");
            await DisplayAlert(forgotPasswordTitle, featureToImplementMsg, "OK");
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async void OnRegisterTapped(object sender, TappedEventArgs e)
        {
            var registerTitle = await GetTextAsync("Register", "Inscription");
            var featureToImplementMsg = await GetTextAsync("FeatureToImplement", "Fonctionnalité à implémenter");
            await DisplayAlert(registerTitle, featureToImplementMsg, "OK");
        }

        // ✅ AJOUT: Gestion du changement de langue
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
                        System.Diagnostics.Debug.WriteLine($"🌍 Changement vers: {newCulture}");
                        await translationService.SetCurrentCultureAsync(newCulture);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        // ✅ AJOUT: Mettre à jour le drapeau de langue
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
                System.Diagnostics.Debug.WriteLine($"❌ Erreur mise à jour drapeau: {ex.Message}");
            }
        }

        // ✅ AJOUT: Nettoyage
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        // ✅ AJOUT: Destructeur pour nettoyer l'événement
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