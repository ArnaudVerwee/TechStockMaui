using TechStockMaui.Services;
using TypeArticleModel = TechStockMaui.Models.TypeArticle.TypeArticle;

namespace TechStockMaui.Views.TypeArticle;

public partial class DeleteTypeArticlePage : ContentPage
{
    private TypeArticleModel _typeArticle;

    // ✅ CONSERVÉ: Votre constructeur original
    public DeleteTypeArticlePage(TypeArticleModel typeArticle)
    {
        InitializeComponent();

        // ✅ AJOUT: S'abonner aux changements de langue
        TranslationService.Instance.CultureChanged += OnCultureChanged;

        _typeArticle = typeArticle;
    }

    // ✅ AJOUT: Charger les traductions au démarrage
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTranslationsAsync();
        LoadTypeArticleData();
    }

    // ✅ AJOUT: Charger les données du type d'article
    private void LoadTypeArticleData()
    {
        if (_typeArticle != null && NameLabel != null)
        {
            NameLabel.Text = _typeArticle.Name ?? "N/A";
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
            System.Diagnostics.Debug.WriteLine("🌍 Mise à jour des textes DeleteTypeArticlePage");

            // ✅ Titre de la page (utilise vos fichiers .resx existants)
            var deleteText = await GetTextAsync("Delete", "Delete");
            var typeArticleText = await GetTextAsync("TypeArticle", "Type Article");
            Title = $"{deleteText} {typeArticleText}";

            // ✅ Titre principal
            if (TitleLabel != null)
                TitleLabel.Text = $"{deleteText} " + await GetTextAsync("Type Product", "un type de produit");

            // ✅ Question de confirmation (utilise vos fichiers .resx existants)
            if (ConfirmationLabel != null)
                ConfirmationLabel.Text = await GetTextAsync("Are you sure you want to delete this?", "Are you sure you want to delete this?");

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
            if (DeleteButton != null)
                DeleteButton.Text = await GetTextAsync("Delete", "Delete");

            if (BackButton != null)
                BackButton.Text = await GetTextAsync("Back to List", "Back to List");

            // ✅ Sélecteur de langue
            if (LanguageLabel != null)
                LanguageLabel.Text = await GetTextAsync("Language", "Language");

            // ✅ Mettre à jour l'indicateur de langue
            await UpdateLanguageFlag();

            System.Diagnostics.Debug.WriteLine("✅ Textes DeleteTypeArticlePage mis à jour");
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
            System.Diagnostics.Debug.WriteLine($"🌍 DeleteTypeArticlePage - Langue changée vers: {newCulture}");
            await UpdateTextsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
        }
    }

    // ✅ MODIFIÉ: Votre méthode existante avec messages traduits
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        try
        {
            // ✅ CONSERVÉ: Votre logique existante avec messages traduits
            var confirmationTitle = await GetTextAsync("Confirmation", "Confirmation");
            var confirmDeleteMessage = await GetTextAsync("Do you really want to delete this type", "Voulez-vous vraiment supprimer ce type ?");
            var yes = await GetTextAsync("Yes", "Yes");
            var no = await GetTextAsync("No", "No");

            bool confirmed = await DisplayAlert(confirmationTitle, confirmDeleteMessage, yes, no);

            if (!confirmed)
                return;

            // TODO: Suppression via API
            var successTitle = await GetTextAsync("Success", "Success");
            var deleteSuccessMessage = await GetTextAsync("Type deleted successfully", "Type supprimé avec succès.");
            await DisplayAlert(successTitle, deleteSuccessMessage, "OK");

            // Retour à la liste
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur OnDeleteClicked: {ex.Message}");
            var errorTitle = await GetTextAsync("Error", "Error");
            var deleteErrorMessage = await GetTextAsync("Delete error", "Erreur lors de la suppression");
            await DisplayAlert(errorTitle, $"{deleteErrorMessage}: {ex.Message}", "OK");
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
    ~DeleteTypeArticlePage()
    {
        try
        {
            TranslationService.Instance.CultureChanged -= OnCultureChanged;
        }
        catch { }
    }
}