using TechStockMaui.Services;
using TypeArticleModel = TechStockMaui.Models.TypeArticle.TypeArticle;

namespace TechStockMaui.Views.TypeArticle
{
    public partial class EditTypeArticlePage : ContentPage
    {
        private TypeArticleModel _typeArticle;
        private TypeArticleService _typeArticleService;

        public EditTypeArticlePage(TypeArticleModel typeArticle)
        {
            InitializeComponent();

            TranslationService.Instance.CultureChanged += OnCultureChanged;

            _typeArticle = typeArticle;
            _typeArticleService = new TypeArticleService();

            LoadTypeArticleData();
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
                System.Diagnostics.Debug.WriteLine("Updating EditTypeArticlePage texts");

                var editText = await GetTextAsync("Edit", "Edit");
                var typeArticleText = await GetTextAsync("TypeArticle", "Type Article");
                Title = $"{editText} {typeArticleText}";

                if (TitleLabel != null)
                    TitleLabel.Text = $"{editText} " + await GetTextAsync("TypeArticle", "TypeArticle");

                if (FormHeaderLabel != null)
                    FormHeaderLabel.Text = await GetTextAsync("Type Article Information", "Informations du type d'article");

                if (NameLabel != null)
                    NameLabel.Text = await GetTextAsync("Name", "Name");

                if (NameEntry != null)
                    NameEntry.Placeholder = await GetTextAsync("Enter name", "Entrez le nom");

                if (SaveButton != null)
                    SaveButton.Text = await GetTextAsync("Save", "Save");

                if (BackButton != null)
                    BackButton.Text = await GetTextAsync("Back to List", "Back to List");

                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("EditTypeArticlePage texts updated");
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
                System.Diagnostics.Debug.WriteLine($"EditTypeArticlePage - Language changed to: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

        private void LoadTypeArticleData()
        {
            if (_typeArticle != null)
            {
                NameEntry.Text = _typeArticle.Name;
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

                _typeArticle.Name = name;

                bool success = await _typeArticleService.UpdateAsync(_typeArticle);

                if (success)
                {
                    var successTitle = await GetTextAsync("Success", "Success");
                    var updateSuccessMessage = await GetTextAsync("Type updated successfully", "Type d'article '{0}' modifié avec succès!");
                    await DisplayAlert(successTitle, string.Format(updateSuccessMessage, name), "OK");

                    await Navigation.PopAsync();
                }
                else
                {
                    var errorTitle = await GetTextAsync("Error", "Error");
                    var updateFailMessage = await GetTextAsync("Update failed", "Échec de la modification du type d'article");
                    await DisplayAlert(errorTitle, updateFailMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Error");
                var errorOccurredMessage = await GetTextAsync("An error occurred", "Une erreur s'est produite");
                await DisplayAlert(errorTitle, $"{errorOccurredMessage}: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            if (HasChanges())
            {
                var unsavedTitle = await GetTextAsync("Unsaved changes", "Modifications non sauvegardées");
                var unsavedMessage = await GetTextAsync("Unsaved changes message", "Vous avez des modifications non sauvegardées. Voulez-vous vraiment quitter ?");
                var yes = await GetTextAsync("Yes", "Yes");
                var no = await GetTextAsync("No", "No");

                bool confirmExit = await DisplayAlert(unsavedTitle, unsavedMessage, yes, no);

                if (!confirmExit)
                    return;
            }

            await Navigation.PopAsync();
        }

        private bool HasChanges()
        {
            return _typeArticle.Name != NameEntry.Text?.Trim();
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

        ~EditTypeArticlePage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}