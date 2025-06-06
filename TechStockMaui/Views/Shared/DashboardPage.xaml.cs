using TechStockMaui.Views.Supplier;
using TechStockMaui.Services;
using TechStockMaui.Views;

namespace TechStockMaui.Views.Shared
{
    public partial class DashboardPage : ContentPage
    {
        private ProductService _productService;

        public DashboardPage()
        {
            InitializeComponent();
            _productService = new ProductService();

            // ✅ S'abonner aux changements de langue
            TranslationService.Instance.CultureChanged += OnCultureChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadStatisticsAsync();
            await LoadTranslationsAsync();
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
                System.Diagnostics.Debug.WriteLine($"❌ Erreur chargement traductions: {ex.Message}");
            }
        }

        private async Task<string> GetTextAsync(string key, string fallback = null)
        {
            try
            {
                var text = await TranslationService.Instance.GetTranslationAsync(key);
                System.Diagnostics.Debug.WriteLine($"🔑 GetTextAsync('{key}') -> '{text}' (fallback: '{fallback}')");
                return !string.IsNullOrEmpty(text) && text != key ? text : (fallback ?? key);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetTextAsync pour '{key}': {ex.Message}");
                return fallback ?? key;
            }
        }

        private async Task UpdateTextsAsync()
        {
            try
            {
                var currentCulture = TranslationService.Instance.GetCurrentCulture();
                System.Diagnostics.Debug.WriteLine($"🌍 Mise à jour des textes Dashboard - Culture: {currentCulture}");

                // ✅ Titre de la page
                Title = await GetTextAsync("Dashboard", "Dashboard");

                // ✅ Boutons de navigation
                if (HomeButtonLabel != null)
                    HomeButtonLabel.Text = await GetTextAsync("Home", "Home");

                if (ProductsButtonLabel != null)
                    ProductsButtonLabel.Text = await GetTextAsync("Products", "Products");

                if (SuppliersButtonLabel != null)
                    SuppliersButtonLabel.Text = await GetTextAsync("Suppliers", "Suppliers");

                if (TypesButtonLabel != null)
                    TypesButtonLabel.Text = await GetTextAsync("Types", "Types");

                if (UsersButtonLabel != null)
                    UsersButtonLabel.Text = await GetTextAsync("Users", "Users");

                if (LogoutButtonLabel != null)
                    LogoutButtonLabel.Text = await GetTextAsync("Exit", "Exit");

                // ✅ Messages de bienvenue  
                if (WelcomeLabel != null)
                    WelcomeLabel.Text = await GetTextAsync("Welcome", "Welcome to TechStock");

                if (WelcomeSubLabel != null)
                    WelcomeSubLabel.Text = await GetTextAsync("WelcomeMessage", "Manage your technology inventory easily");

                // ✅ Label "Accès rapide"
                if (QuickAccessLabel != null)
                    QuickAccessLabel.Text = await GetTextAsync("QuickAccess", "Quick Access");

                // ✅ Card Produits
                if (ProductsCardLabel != null)
                    ProductsCardLabel.Text = await GetTextAsync("Products", "Products");

                if (ProductsCardSubLabel != null)
                    ProductsCardSubLabel.Text = await GetTextAsync("ManageInventoryShort", "Manage inventory");

                // ✅ Card Fournisseurs
                if (SuppliersCardLabel != null)
                    SuppliersCardLabel.Text = await GetTextAsync("Suppliers", "Suppliers");

                if (SuppliersCardSubLabel != null)
                    SuppliersCardSubLabel.Text = await GetTextAsync("ManageContacts", "Manage contacts");

                // ✅ Card Utilisateurs
                if (UsersCardLabel != null)
                    UsersCardLabel.Text = await GetTextAsync("Users", "Users");

                if (UsersCardSubLabel != null)
                    UsersCardSubLabel.Text = await GetTextAsync("ManageAccess", "Manage access");

                // ✅ Card Mes Produits
                if (MyProductsCardLabel != null)
                    MyProductsCardLabel.Text = await GetTextAsync("MyProducts", "My Products");

                if (MyProductsCardSubLabel != null)
                    MyProductsCardSubLabel.Text = await GetTextAsync("AssignedProducts", "Assigned products");

                // ✅ Label "Aperçu"
                if (OverviewLabel != null)
                    OverviewLabel.Text = await GetTextAsync("Overview", "Overview");

                // ✅ Statistiques - Labels
                if (TotalLabel != null)
                    TotalLabel.Text = await GetTextAsync("Total", "Total");

                if (AssignedLabel != null)
                    AssignedLabel.Text = await GetTextAsync("Assigned", "Assigned");

                if (AvailableLabel != null)
                    AvailableLabel.Text = await GetTextAsync("Available", "Available");

                // ✅ Mettre à jour l'indicateur de langue
                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("✅ Textes Dashboard mis à jour");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur UpdateTextsAsync: {ex.Message}");
            }
        }

        private async void OnCultureChanged(object sender, string newCulture)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🌍 Dashboard - Langue changée vers: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                var products = await _productService.GetProductsAsync();
                var totalProducts = products?.Count ?? 0;
                var assignedProducts = products?.Count(p =>
                    !string.IsNullOrEmpty(p.AssignedUserName)) ?? 0;
                var freeProducts = totalProducts - assignedProducts;

                TotalProductsLabel.Text = totalProducts.ToString();
                AssignedProductsLabel.Text = assignedProducts.ToString();
                FreeProductsLabel.Text = freeProducts.ToString();
            }
            catch (Exception ex)
            {
                TotalProductsLabel.Text = "N/A";
                AssignedProductsLabel.Text = "N/A";
                FreeProductsLabel.Text = "N/A";
            }
        }

        private async void OnWarehouseClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new ProductPage());
            }
            catch (Exception ex)
            {
                var errorText = await GetTextAsync("Error", "Erreur");
                await DisplayAlert(errorText, $"Impossible d'ouvrir la page des produits: {ex.Message}", "OK");
            }
        }

        private async void OnTruckClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new SupplierPage());
            }
            catch (Exception ex)
            {
                var errorText = await GetTextAsync("Error", "Erreur");
                await DisplayAlert(errorText, $"Impossible d'ouvrir la page des fournisseurs: {ex.Message}", "OK");
            }
        }

        private async void OnLaptopClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new TypeArticlePage());
            }
            catch (Exception ex)
            {
                var errorText = await GetTextAsync("Error", "Erreur");
                await DisplayAlert(errorText, $"Impossible d'ouvrir la page des types d'articles: {ex.Message}", "OK");
            }
        }

        private async void OnAssignedProductsClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new MaterialManagements.AssignedProductsPage());
            }
            catch (Exception ex)
            {
                var errorText = await GetTextAsync("Error", "Erreur");
                await DisplayAlert(errorText, $"Impossible d'ouvrir la page des produits assignés: {ex.Message}", "OK");
            }
        }

        private async void OnUsersClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new Views.Users.ManagementUserPage());
            }
            catch (Exception ex)
            {
                var errorText = await GetTextAsync("Error", "Erreur");
                await DisplayAlert(errorText, $"Erreur: {ex.Message}", "OK");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            try
            {
                var confirmTitle = await GetTextAsync("Logout", "Déconnexion");
                var confirmMessage = await GetTextAsync("ConfirmLogout", "Êtes-vous sûr de vouloir vous déconnecter ?");
                var yesText = await GetTextAsync("Yes", "Oui");
                var noText = await GetTextAsync("No", "Non");

                bool confirm = await DisplayAlert(confirmTitle, confirmMessage, yesText, noText);
                if (confirm)
                {
                    var authService = new AuthService();
                    await authService.LogoutAsync();
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                }
            }
            catch (Exception ex)
            {
                var errorText = await GetTextAsync("Error", "Erreur");
                await DisplayAlert(errorText, $"Erreur lors de la déconnexion: {ex.Message}", "OK");
            }
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

                var cancelText = await GetTextAsync("Cancel", "Annuler");
                var titleText = "🌍 " + await GetTextAsync("ChooseLanguage", "Choisir la langue");

                var selectedOption = await DisplayActionSheet(titleText, cancelText, null, options.ToArray());

                if (!string.IsNullOrEmpty(selectedOption) && selectedOption != cancelText)
                {
                    string newCulture = null;
                    if (selectedOption.Contains("FR")) newCulture = "fr";
                    else if (selectedOption.Contains("EN")) newCulture = "en";
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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        ~DashboardPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}