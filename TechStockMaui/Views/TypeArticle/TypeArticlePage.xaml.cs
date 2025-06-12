using System.Collections.ObjectModel;
using TechStockMaui.Models.TypeArticle;
using TechStockMaui.Services;

namespace TechStockMaui.Views
{
    public partial class TypeArticlePage : ContentPage
    {
        private TypeArticleService _typeArticleService;
        private ObservableCollection<TechStockMaui.Models.TypeArticle.TypeArticle> _typeArticles;

        public TypeArticlePage()
        {
            InitializeComponent();

            TranslationService.Instance.CultureChanged += OnCultureChanged;

            _typeArticleService = new TypeArticleService();
            _typeArticles = new ObservableCollection<TechStockMaui.Models.TypeArticle.TypeArticle>();
            TypeArticleList.ItemsSource = _typeArticles;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadTranslationsAsync();

            await LoadTypeArticlesAsync();
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
                System.Diagnostics.Debug.WriteLine("Updating TypeArticlePage texts");

                var productTypesText = await GetTextAsync("Products Types", "Product Types");
                Title = productTypesText;

                if (TitleLabel != null)
                    TitleLabel.Text = productTypesText;

                if (CreateButton != null)
                {
                    var createNewText = await GetTextAsync("Create New", "Create New");
                    CreateButton.Text = createNewText;
                }

                if (ListHeaderLabel != null)
                {
                    var listText = await GetTextAsync("Type List", "Type List");
                    ListHeaderLabel.Text = listText;
                }

                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("TypeArticlePage texts updated");
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
                System.Diagnostics.Debug.WriteLine($"TypeArticlePage - Language changed to: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

        private async Task LoadTypeArticlesAsync()
        {
            try
            {
                var typeArticles = await _typeArticleService.GetAllAsync();

                _typeArticles.Clear();
                if (typeArticles != null)
                {
                    foreach (var typeArticle in typeArticles)
                    {
                        _typeArticles.Add(typeArticle);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Error");
                var loadErrorMessage = await GetTextAsync("Unable to load types", "Unable to load types");
                await DisplayAlert(errorTitle, $"{loadErrorMessage}: {ex.Message}", "OK");
            }
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new CreateTypeArticlePage());
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Error");
                var createErrorMessage = await GetTextAsync("Unable to open create page", "Unable to open create page");
                await DisplayAlert(errorTitle, $"{createErrorMessage}: {ex.Message}", "OK");
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is TechStockMaui.Models.TypeArticle.TypeArticle typeArticle)
                {
                    await Navigation.PushAsync(new TypeArticle.EditTypeArticlePage(typeArticle));
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Error");
                var editErrorMessage = await GetTextAsync("Unable to open edit page", "Unable to open edit page");
                await DisplayAlert(errorTitle, $"{editErrorMessage}: {ex.Message}", "OK");
            }
        }

        private async void OnDetailsClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is TechStockMaui.Models.TypeArticle.TypeArticle typeArticle)
                {
                    await Navigation.PushAsync(new TypeArticle.DetailsTypeArticlePage(typeArticle));
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Error");
                var detailsErrorMessage = await GetTextAsync("Unable to open details", "Unable to open details");
                await DisplayAlert(errorTitle, $"{detailsErrorMessage}: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is TechStockMaui.Models.TypeArticle.TypeArticle typeArticle)
                {
                    var confirmTitle = await GetTextAsync("Confirmation", "Confirmation");
                    var deleteConfirmMessage = await GetTextAsync("Are you sure delete type", "Are you sure you want to delete the type") + $" '{typeArticle.Name}' ?";
                    var yes = await GetTextAsync("Yes", "Yes");
                    var no = await GetTextAsync("No", "No");

                    bool confirm = await DisplayAlert(confirmTitle, deleteConfirmMessage, yes, no);

                    if (confirm)
                    {
                        bool success = await _typeArticleService.DeleteAsync(typeArticle.Id);

                        if (success)
                        {
                            _typeArticles.Remove(typeArticle);
                            var successTitle = await GetTextAsync("Success", "Success");
                            var deleteSuccessMessage = await GetTextAsync("Type deleted successfully", "Type deleted successfully");
                            await DisplayAlert(successTitle, deleteSuccessMessage, "OK");
                        }
                        else
                        {
                            var errorTitle = await GetTextAsync("Error", "Error");
                            var deleteFailMessage = await GetTextAsync("Delete failed", "Delete failed");
                            await DisplayAlert(errorTitle, deleteFailMessage, "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Error");
                var deleteErrorMessage = await GetTextAsync("Unable to delete", "Unable to delete");
                await DisplayAlert(errorTitle, $"{deleteErrorMessage}: {ex.Message}", "OK");
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

        ~TypeArticlePage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}