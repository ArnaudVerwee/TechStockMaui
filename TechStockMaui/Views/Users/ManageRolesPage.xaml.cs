using TechStockMaui.Services;
using TechStockMaui.ViewModels;

namespace TechStockMaui.Views.Users
{
    public partial class ManageRolesPage : ContentPage
    {
        public string UserName { get; private set; }

        // ✅ CONSERVÉ: Constructeur par défaut (gardez-le pour le XAML)
        public ManageRolesPage()
        {
            InitializeComponent();

            // ✅ AJOUT: S'abonner aux changements de langue
            TranslationService.Instance.CultureChanged += OnCultureChanged;
        }

        // ✅ CONSERVÉ: Constructeur avec paramètre userName
        public ManageRolesPage(string userName) : this()
        {
            UserName = userName;
            Title = $"🔐 Rôles - {userName}";
            System.Diagnostics.Debug.WriteLine($"📄 ManageRolesPage créée pour: {userName}");
            // Initialiser le ViewModel avec le userName
            BindingContext = new ManageRolesViewModel(userName);
        }

        // ✅ MODIFIÉ: Votre méthode existante avec ajout des traductions
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // ✅ AJOUT: Charger les traductions
            await LoadTranslationsAsync();

            // ✅ CONSERVÉ: Charger les rôles quand la page apparaît
            if (BindingContext is ManageRolesViewModel viewModel)
            {
                await viewModel.LoadRolesAsync();
            }
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
                System.Diagnostics.Debug.WriteLine("🌍 Mise à jour des textes ManageRolesPage");

                // ✅ Titre de la page
                var rolesText = await GetTextAsync("Roles", "Roles");
                if (!string.IsNullOrEmpty(UserName))
                    Title = $"🔐 {rolesText} - {UserName}";

                // ✅ En-tête utilisateur
                if (UserHeaderLabel != null)
                {
                    var userText = await GetTextAsync("User", "User");
                    if (!string.IsNullOrEmpty(UserName))
                        UserHeaderLabel.Text = $"{userText}: {UserName}";
                }

                // ✅ Description de la page
                if (PageDescriptionLabel != null)
                    PageDescriptionLabel.Text = await GetTextAsync("Role management page", "Role management page");

                // ✅ Label de chargement
                if (LoadingLabel != null)
                    LoadingLabel.Text = await GetTextAsync("Loading", "Loading") + "...";

                // ✅ Label rôles disponibles
                if (AvailableRolesLabel != null)
                    AvailableRolesLabel.Text = await GetTextAsync("Available roles", "Available roles") + ":";

                // ✅ Boutons
                if (SaveButton != null)
                    SaveButton.Text = await GetTextAsync("Save", "Save");

                if (CancelButton != null)
                    CancelButton.Text = await GetTextAsync("Cancel", "Cancel");

                // ✅ Sélecteur de langue
                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                // ✅ Mettre à jour l'indicateur de langue
                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("✅ Textes ManageRolesPage mis à jour");
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
                System.Diagnostics.Debug.WriteLine($"🌍 ManageRolesPage - Langue changée vers: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
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

        // ✅ AJOUT: Destructeur
        ~ManageRolesPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}