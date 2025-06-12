using TechStockMaui.Services;
using TypeArticleModel = TechStockMaui.Models.TypeArticle.TypeArticle;

namespace TechStockMaui.Views.TypeArticle
{
    public partial class DetailsTypeArticlePage : ContentPage
    {
        private TypeArticleModel _typeArticle;
        private TypeArticleService _typeArticleService;

        public DetailsTypeArticlePage(TypeArticleModel typeArticle)
        {
            InitializeComponent();

            TranslationService.Instance.CultureChanged += OnCultureChanged;

            _typeArticle = typeArticle;
            _typeArticleService = new TypeArticleService();

            LoadTypeArticleDetails();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadTranslationsAsync();

            await RefreshTypeArticleDetails();
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
                System.Diagnostics.Debug.WriteLine("Updating DetailsTypeArticlePage texts");

                var detailsText = await GetTextAsync("Details", "Details");
                var typeArticleText = await GetTextAsync("TypeArticle", "Type Article");
                Title = $"{detailsText} {typeArticleText}";

                if (TitleLabel != null)
                    TitleLabel.Text = $"{detailsText} " + await GetTextAsync("Type Product", "du Type de Produit");

                if (TypeInfoHeaderLabel != null)
                    TypeInfoHeaderLabel.Text = await GetTextAsync("Type Article Information", "Informations du type d'article");

                if (NameHeaderLabel != null)
                {
                    var nameText = await GetTextAsync("Name", "Name");
                    NameHeaderLabel.Text = nameText + " :";
                }

                if (EditButton != null)
                    EditButton.Text = await GetTextAsync("Edit", "Edit");

                if (BackButton != null)
                    BackButton.Text = await GetTextAsync("Back to List", "Back to List");

                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("DetailsTypeArticlePage texts updated");
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
                System.Diagnostics.Debug.WriteLine($"DetailsTypeArticlePage - Language changed to: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

        private void LoadTypeArticleDetails()
        {
            if (_typeArticle != null)
            {
                NameLabel.Text = _typeArticle.Name ?? "Non défini";
            }
        }

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

        private async void OnEditClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new EditTypeArticlePage(_typeArticle));
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Error");
                var editErrorMessage = await GetTextAsync("Unable to open edit page", "Impossible d'ouvrir la page de modification");
                await DisplayAlert(errorTitle, $"{editErrorMessage}: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
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