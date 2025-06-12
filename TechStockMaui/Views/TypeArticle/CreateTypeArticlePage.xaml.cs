using TechStockMaui.Services;

namespace TechStockMaui.Views
{
    public partial class CreateTypeArticlePage : ContentPage
    {
        private TypeArticleService _typeArticleService;

        public CreateTypeArticlePage()
        {
            InitializeComponent();

            TranslationService.Instance.CultureChanged += OnCultureChanged;

            _typeArticleService = new TypeArticleService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
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
                System.Diagnostics.Debug.WriteLine("Updating CreateTypeArticlePage texts");

                var createText = await GetTextAsync("Create", "Create");
                var typeArticleText = await GetTextAsync("TypeArticle", "Type Article");
                Title = $"{createText} {typeArticleText}";

                if (TitleLabel != null)
                    TitleLabel.Text = $"{createText} " + await GetTextAsync("New Type Article", "nouveau type d'article");

                if (FormHeaderLabel != null)
                    FormHeaderLabel.Text = await GetTextAsync("Type Article Information", "Informations du type d'article");

                if (NameLabel != null)
                {
                    var nameText = await GetTextAsync("Name", "Name");
                    NameLabel.Text = nameText + " :";
                }

                if (NameEntry != null)
                    NameEntry.Placeholder = await GetTextAsync("Enter type name", "Entrez le nom du type d'article");

                if (SaveButton != null)
                    SaveButton.Text = await GetTextAsync("Save", "Save");

                if (CancelButton != null)
                    CancelButton.Text = await GetTextAsync("Cancel", "Cancel");

                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("CreateTypeArticlePage texts updated");
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
                System.Diagnostics.Debug.WriteLine($"CreateTypeArticlePage - Language changed to: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                string name = NameEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    var errorTitle = await GetTextAsync("Error", "Error");
                    var nameRequiredMessage = await GetTextAsync("Type name required", "Le nom du type d'article est obligatoire.");
                    await DisplayAlert(errorTitle, nameRequiredMessage, "OK");
                    return;
                }

                var newTypeArticle = new TechStockMaui.Models.TypeArticle.TypeArticle
                {
                    Name = name
                };

                bool success = await _typeArticleService.CreateAsync(newTypeArticle);

                if (success)
                {
                    var successTitle = await GetTextAsync("Success", "Success");
                    var createdSuccessMessage = await GetTextAsync("Type created successfully", "Type d'article '{0}' créé avec succès!");
                    await DisplayAlert(successTitle, string.Format(createdSuccessMessage, name), "OK");

                    await Navigation.PopAsync();
                }
                else
                {
                    var errorTitle = await GetTextAsync("Error", "Error");
                    var createFailMessage = await GetTextAsync("Creation failed", "Échec de la création du type d'article");
                    await DisplayAlert(errorTitle, createFailMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Error");
                var errorOccurredMessage = await GetTextAsync("An error occurred", "Une erreur s'est produite");
                await DisplayAlert(errorTitle, $"{errorOccurredMessage}: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                var confirmTitle = await GetTextAsync("Confirmation", "Confirmation");
                var confirmExitMessage = await GetTextAsync("Cancel confirmation message", "Voulez-vous vraiment annuler ? Les données saisies seront perdues.");
                var yes = await GetTextAsync("Yes", "Yes");
                var no = await GetTextAsync("No", "No");

                bool confirmExit = await DisplayAlert(confirmTitle, confirmExitMessage, yes, no);

                if (!confirmExit)
                    return;
            }

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

        ~CreateTypeArticlePage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}