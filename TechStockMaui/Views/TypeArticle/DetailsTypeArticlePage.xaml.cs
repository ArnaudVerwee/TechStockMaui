using TechStockMaui.Services;
using TypeArticleModel = TechStockMaui.Models.TypeArticle.TypeArticle;

namespace TechStockMaui.Views.TypeArticle
{
    public partial class DetailsTypeArticlePage : ContentPage
    {
        private TypeArticleModel _typeArticle;
        private TypeArticleService _typeArticleService;

        // ✅ CONSERVÉ: Votre constructeur original
        public DetailsTypeArticlePage(TypeArticleModel typeArticle)
        {
            InitializeComponent();

            // ✅ AJOUT: S'abonner aux changements de langue
            TranslationService.Instance.CultureChanged += OnCultureChanged;

            _typeArticle = typeArticle;
            _typeArticleService = new TypeArticleService();

            // Charger les données du type d'article
            LoadTypeArticleDetails();
        }

        // ✅ MODIFIÉ: Votre méthode existante avec ajout des traductions
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // ✅ AJOUT: Charger les traductions
            await LoadTranslationsAsync();

            // ✅ CONSERVÉ: Votre logique existante
            await RefreshTypeArticleDetails();
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
                System.Diagnostics.Debug.WriteLine("🌍 Mise à jour des textes DetailsTypeArticlePage");

                // ✅ Titre de la page (utilise vos fichiers .resx existants)
                var detailsText = await GetTextAsync("Details", "Details");
                var typeArticleText = await GetTextAsync("TypeArticle", "Type Article");
                Title = $"{detailsText} {typeArticleText}";

                // ✅ Titre principal
                if (TitleLabel != null)
                    TitleLabel.Text = $"{detailsText} " + await GetTextAsync("Type Product", "du Type de Produit");

                // ✅ En-tête des informations
                if (TypeInfoHeaderLabel != null)
                    TypeInfoHeaderLabel.Text = await GetTextAsync("Type Article Information", "Informations du type d'article");

                // ✅ Label nom (utilise vos fichiers .resx existants)
                if (NameHeaderLabel != null)
                {
                    var nameText = await GetTextAsync("Name", "Name");
                    NameHeaderLabel.Text = nameText + " :";
                }

                // ✅ Boutons (utilise vos fichiers .resx existants)
                if (EditButton != null)
                    EditButton.Text = await GetTextAsync("Edit", "Edit");

                if (BackButton != null)
                    BackButton.Text = await GetTextAsync("Back to List", "Back to List");

                // ✅ Sélecteur de langue
                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                // ✅ Mettre à jour l'indicateur de langue
                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("✅ Textes DetailsTypeArticlePage mis à jour");
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
                System.Diagnostics.Debug.WriteLine($"🌍 DetailsTypeArticlePage - Langue changée vers: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private void LoadTypeArticleDetails()
        {
            if (_typeArticle != null)
            {
                NameLabel.Text = _typeArticle.Name ?? "Non défini";
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec messages traduits
        private async Task RefreshTypeArticleDetails()
        {
            try
            {
                var updatedTypeArticle = await _typeArticleService.GetByIdAsync(_typeArticle.Id);
                if (updatedTypeArticle != null)
                {
                    _typeArticle = updatedTypeArticle;
                    LoadTypeArticleDetails();
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Error");
                var reloadErrorMessage = await GetTextAsync("Unable to reload details", "Impossible de recharger les détails");
                await DisplayAlert(errorTitle, $"{reloadErrorMessage}: {ex.Message}", "OK");
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec messages traduits
        private async void OnEditClicked(object sender, EventArgs e)
        {
            try
            {
                // Naviguer vers la page de modification en passant le type d'article
                await Navigation.PushAsync(new EditTypeArticlePage(_typeArticle));
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Error");
                var editErrorMessage = await GetTextAsync("Unable to open edit page", "Impossible d'ouvrir la page de modification");
                await DisplayAlert(errorTitle, $"{editErrorMessage}: {ex.Message}", "OK");
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
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
        ~DetailsTypeArticlePage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}