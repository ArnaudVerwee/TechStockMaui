using TechStockMaui.Services;
using TypeArticleModel = TechStockMaui.Models.TypeArticle.TypeArticle;

namespace TechStockMaui.Views.TypeArticle;

public partial class DeleteTypeArticlePage : ContentPage
{
    private TypeArticleModel _typeArticle;

    public DeleteTypeArticlePage(TypeArticleModel typeArticle)
    {
        InitializeComponent();

        TranslationService.Instance.CultureChanged += OnCultureChanged;

        _typeArticle = typeArticle;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTranslationsAsync();
        LoadTypeArticleData();
    }

    private void LoadTypeArticleData()
    {
        if (_typeArticle != null && NameLabel != null)
        {
            NameLabel.Text = _typeArticle.Name ?? "N/A";
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
            System.Diagnostics.Debug.WriteLine("Updating DeleteTypeArticlePage texts");

            var deleteText = await GetTextAsync("Delete", "Delete");
            var typeArticleText = await GetTextAsync("TypeArticle", "Type Article");
            Title = $"{deleteText} {typeArticleText}";

            if (TitleLabel != null)
                TitleLabel.Text = $"{deleteText} " + await GetTextAsync("Type Product", "un type de produit");

            if (ConfirmationLabel != null)
                ConfirmationLabel.Text = await GetTextAsync("Are you sure you want to delete this?", "Are you sure you want to delete this?");

            if (TypeInfoHeaderLabel != null)
                TypeInfoHeaderLabel.Text = await GetTextAsync("Type Article Information", "Informations du type d'article");

            if (NameHeaderLabel != null)
            {
                var nameText = await GetTextAsync("Name", "Name");
                NameHeaderLabel.Text = nameText + " :";
            }

            if (DeleteButton != null)
                DeleteButton.Text = await GetTextAsync("Delete", "Delete");

            if (BackButton != null)
                BackButton.Text = await GetTextAsync("Back to List", "Back to List");

            if (LanguageLabel != null)
                LanguageLabel.Text = await GetTextAsync("Language", "Language");

            await UpdateLanguageFlag();

            System.Diagnostics.Debug.WriteLine("DeleteTypeArticlePage texts updated");
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
            System.Diagnostics.Debug.WriteLine($"DeleteTypeArticlePage - Language changed to: {newCulture}");
            await UpdateTextsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        try
        {
            var confirmationTitle = await GetTextAsync("Confirmation", "Confirmation");
            var confirmDeleteMessage = await GetTextAsync("Do you really want to delete this type", "Voulez-vous vraiment supprimer ce type ?");
            var yes = await GetTextAsync("Yes", "Yes");
            var no = await GetTextAsync("No", "No");

            bool confirmed = await DisplayAlert(confirmationTitle, confirmDeleteMessage, yes, no);

            if (!confirmed)
                return;

            var successTitle = await GetTextAsync("Success", "Success");
            var deleteSuccessMessage = await GetTextAsync("Type deleted successfully", "Type supprimé avec succès.");
            await DisplayAlert(successTitle, deleteSuccessMessage, "OK");

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnDeleteClicked error: {ex.Message}");
            var errorTitle = await GetTextAsync("Error", "Error");
            var deleteErrorMessage = await GetTextAsync("Delete error", "Erreur lors de la suppression");
            await DisplayAlert(errorTitle, $"{deleteErrorMessage}: {ex.Message}", "OK");
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

    ~DeleteTypeArticlePage()
    {
        try
        {
            TranslationService.Instance.CultureChanged -= OnCultureChanged;
        }
        catch { }
    }
}