using TechStockMaui.Services;
using TypeArticleModel = TechStockMaui.Models.TypeArticle.TypeArticle;

namespace TechStockMaui.Views.TypeArticle
{
    public partial class EditTypeArticlePage : ContentPage
    {
        private TypeArticleModel _typeArticle;
        private TypeArticleService _typeArticleService;

        // ✅ CONSERVÉ: Votre constructeur original
        public EditTypeArticlePage(TypeArticleModel typeArticle)
        {
            InitializeComponent();

            // ✅ AJOUT: S'abonner aux changements de langue
            TranslationService.Instance.CultureChanged += OnCultureChanged;

            _typeArticle = typeArticle;
            _typeArticleService = new TypeArticleService();

            // Pré-remplir les champs avec les données existantes
            LoadTypeArticleData();
        }

        // ✅ AJOUT: Charger les traductions au démarrage
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTranslationsAsync();
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
                System.Diagnostics.Debug.WriteLine("🌍 Mise à jour des textes EditTypeArticlePage");

                // ✅ Titre de la page (utilise vos fichiers .resx existants)
                var editText = await GetTextAsync("Edit", "Edit");
                var typeArticleText = await GetTextAsync("TypeArticle", "Type Article");
                Title = $"{editText} {typeArticleText}";

                // ✅ Titre principal
                if (TitleLabel != null)
                    TitleLabel.Text = $"{editText} " + await GetTextAsync("TypeArticle", "TypeArticle");

                // ✅ En-tête du formulaire
                if (FormHeaderLabel != null)
                    FormHeaderLabel.Text = await GetTextAsync("Type Article Information", "Informations du type d'article");

                // ✅ Label nom (utilise vos fichiers .resx existants)
                if (NameLabel != null)
                    NameLabel.Text = await GetTextAsync("Name", "Name");

                // ✅ Placeholder de l'Entry
                if (NameEntry != null)
                    NameEntry.Placeholder = await GetTextAsync("Enter name", "Entrez le nom");

                // ✅ Boutons (utilise vos fichiers .resx existants)
                if (SaveButton != null)
                    SaveButton.Text = await GetTextAsync("Save", "Save");

                if (BackButton != null)
                    BackButton.Text = await GetTextAsync("Back to List", "Back to List");

                // ✅ Sélecteur de langue
                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                // ✅ Mettre à jour l'indicateur de langue
                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("✅ Textes EditTypeArticlePage mis à jour");
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
                System.Diagnostics.Debug.WriteLine($"🌍 EditTypeArticlePage - Langue changée vers: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private void LoadTypeArticleData()
        {
            if (_typeArticle != null)
            {
                NameEntry.Text = _typeArticle.Name;
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec messages traduits
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // ✅ CONSERVÉ: Votre validation existante avec messages traduits
                string name = NameEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    var errorTitle = await GetTextAsync("Error", "Error");
                    var nameRequiredMessage = await GetTextAsync("Type name required", "Le nom du type d'article est obligatoire.");
                    await DisplayAlert(errorTitle, nameRequiredMessage, "OK");
                    return;
                }

                // ✅ CONSERVÉ: Votre logique de mise à jour existante
                _typeArticle.Name = name;

                // Appeler l'API pour sauvegarder les modifications
                bool success = await _typeArticleService.UpdateAsync(_typeArticle);

                if (success)
                {
                    var successTitle = await GetTextAsync("Success", "Success");
                    var updateSuccessMessage = await GetTextAsync("Type updated successfully", "Type d'article '{0}' modifié avec succès!");
                    await DisplayAlert(successTitle, string.Format(updateSuccessMessage, name), "OK");

                    // Retourner à la page précédente
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

        // ✅ MODIFIÉ: Votre méthode existante avec messages traduits
        private async void OnBackClicked(object sender, EventArgs e)
        {
            // ✅ CONSERVÉ: Votre logique de vérification des modifications avec messages traduits
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

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private bool HasChanges()
        {
            return _typeArticle.Name != NameEntry.Text?.Trim();
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