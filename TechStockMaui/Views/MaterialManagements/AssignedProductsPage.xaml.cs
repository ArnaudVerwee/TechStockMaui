using System.Collections.ObjectModel;
using TechStockMaui.Models;
using TechStockMaui.Services;

namespace TechStockMaui.Views.MaterialManagements
{
    public partial class AssignedProductsPage : ContentPage
    {
        private MaterialManagementService _materialManagementService;
        private ObservableCollection<MaterialManagement> _assignments;

        // ✅ CONSERVÉ: Votre constructeur original
        public AssignedProductsPage()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 AssignedProductsPage constructeur - DÉBUT");

                InitializeComponent();

                // ✅ AJOUT: S'abonner aux changements de langue
                TranslationService.Instance.CultureChanged += OnCultureChanged;

                _materialManagementService = new MaterialManagementService();
                _assignments = new ObservableCollection<MaterialManagement>();
                ProductsCollectionView.ItemsSource = _assignments;

                System.Diagnostics.Debug.WriteLine("✅ AssignedProductsPage constructeur - FIN");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur constructeur AssignedProductsPage: {ex.Message}");
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec ajout des traductions
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // ✅ AJOUT: Charger les traductions
            await LoadTranslationsAsync();

            // ✅ CONSERVÉ: Votre logique existante
            System.Diagnostics.Debug.WriteLine("🔄 AssignedProductsPage apparue - chargement automatique");
            await LoadAssignedProductsAsync();
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
                System.Diagnostics.Debug.WriteLine("🌍 Mise à jour des textes AssignedProductsPage");

                // ✅ Titre de la page
                Title = await GetTextAsync("My Assigned Products", "My Assigned Products");

                // ✅ En-tête
                if (HeaderTitleLabel != null)
                    HeaderTitleLabel.Text = await GetTextAsync("Products assigned to me", "Products assigned to me");

                if (HeaderDescriptionLabel != null)
                    HeaderDescriptionLabel.Text = await GetTextAsync("Click Sign to confirm product reception", "Click 'Sign' to confirm product reception");

                // ✅ Labels des détails de produit (Note: Ces labels sont dans le DataTemplate, donc difficiles à traduire directement)
                // Vous pourriez créer des propriétés calculées dans votre modèle MaterialManagement pour les textes traduits

                // ✅ Boutons d'action
                if (RefreshButton != null)
                    RefreshButton.Text = "🔄 " + await GetTextAsync("Refresh", "Refresh");

                if (BackButton != null)
                    BackButton.Text = "🏠 " + await GetTextAsync("Back", "Back");

                // ✅ Sélecteur de langue
                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                // ✅ Mettre à jour l'indicateur de langue
                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("✅ Textes AssignedProductsPage mis à jour");
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
                System.Diagnostics.Debug.WriteLine($"🌍 AssignedProductsPage - Langue changée vers: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private async Task LoadAssignedProductsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Chargement des produits assignés...");

                // ✅ Utiliser votre service existant
                var assignments = await _materialManagementService.GetMyAssignmentsAsync();

                _assignments.Clear();
                if (assignments != null && assignments.Any())
                {
                    foreach (var assignment in assignments)
                    {
                        _assignments.Add(assignment);
                        System.Diagnostics.Debug.WriteLine($"🔍 Assignment ajouté: Produit={assignment.Product?.Name}, Signé={assignment.IsSignatureValid}");
                    }
                    System.Diagnostics.Debug.WriteLine($"✅ {_assignments.Count} assignments chargés");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Aucun produit assigné trouvé");
                }

                // ✅ MODIFIÉ: Message traduit
                if (!_assignments.Any())
                {
                    var infoTitle = await GetTextAsync("Information", "Information");
                    var noProductsMessage = await GetTextAsync("No products currently assigned", "No products are currently assigned to you");
                    await DisplayAlert(infoTitle, noProductsMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur LoadAssignedProductsAsync: {ex.Message}");
                var errorTitle = await GetTextAsync("Error", "Error");
                var errorMessage = await GetTextAsync("Unable to load assigned products", "Unable to load assigned products");
                await DisplayAlert(errorTitle, $"{errorMessage}: {ex.Message}", "OK");
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec messages traduits
        private async void OnSignClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 OnSignClicked - DÉBUT");

                if (sender is Button button && button.CommandParameter is int assignmentId)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Assignment ID: {assignmentId}");

                    // Récupérer l'assignment correspondant
                    var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId);
                    if (assignment == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Assignment {assignmentId} introuvable");
                        var errorTitle = await GetTextAsync("Error", "Error");
                        var notFoundMessage = await GetTextAsync("Assignment not found", "Assignment not found");
                        await DisplayAlert(errorTitle, notFoundMessage, "OK");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ Assignment trouvé: {assignment.Product?.Name}");

                    // ✅ Utiliser vos propriétés calculées existantes
                    if (assignment.IsSignatureValid)
                    {
                        var infoTitle = await GetTextAsync("Information", "Information");
                        var alreadySignedMessage = await GetTextAsync("Product already signed", "This product has already been signed");
                        await DisplayAlert(infoTitle, alreadySignedMessage, "OK");
                        return;
                    }

                    // ✅ Messages traduits pour la confirmation
                    var confirmTitle = await GetTextAsync("Reception Confirmation", "Reception Confirmation");
                    var confirmMessage = await GetTextAsync("Confirm product reception", "Do you confirm that you have received the product") + $" '{assignment.Product?.Name}' ?\n\n" +
                                       await GetTextAsync("By signing you attest possession", "By signing, you attest to having taken possession of this equipment.");
                    var yesConfirm = await GetTextAsync("Yes I confirm", "Yes, I confirm");
                    var cancel = await GetTextAsync("Cancel", "Cancel");

                    bool confirm = await DisplayAlert(confirmTitle, confirmMessage, yesConfirm, cancel);

                    if (confirm)
                    {
                        System.Diagnostics.Debug.WriteLine("🔄 Utilisateur a confirmé, demande de signature...");

                        // Demander la signature
                        string signature = await GetUserSignature(assignment);

                        if (!string.IsNullOrEmpty(signature))
                        {
                            System.Diagnostics.Debug.WriteLine($"🔄 Signature reçue: {signature.Substring(0, Math.Min(20, signature.Length))}...");

                            // ✅ Utiliser votre service existant
                            bool success = await _materialManagementService.SignProductAsync(assignmentId, signature);

                            if (success)
                            {
                                System.Diagnostics.Debug.WriteLine("✅ Signature envoyée avec succès");
                                var successTitle = await GetTextAsync("Success", "Success");
                                var successMessage = await GetTextAsync("Product signed successfully", "Product signed successfully! Thank you for confirming reception.");
                                await DisplayAlert(successTitle, successMessage, "OK");

                                // Rafraîchir la liste
                                await LoadAssignedProductsAsync();
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("❌ Échec envoi signature");
                                var errorTitle = await GetTextAsync("Error", "Error");
                                var saveErrorMessage = await GetTextAsync("Signature save failed", "Failed to save signature. Please try again.");
                                await DisplayAlert(errorTitle, saveErrorMessage, "OK");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("⚠️ Signature annulée par l'utilisateur");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ Confirmation annulée par l'utilisateur");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Sender ou CommandParameter invalide");
                    var errorTitle = await GetTextAsync("Error", "Error");
                    var technicalErrorMessage = await GetTextAsync("Technical error during signing", "Technical error during signing");
                    await DisplayAlert(errorTitle, technicalErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur OnSignClicked: {ex.Message}");
                var errorTitle = await GetTextAsync("Error", "Error");
                var signingErrorMessage = await GetTextAsync("Error during signing", "Error during signing");
                await DisplayAlert(errorTitle, $"{signingErrorMessage}: {ex.Message}", "OK");
            }

            System.Diagnostics.Debug.WriteLine("🔄 OnSignClicked - FIN");
        }

        // ✅ MODIFIÉ: Votre méthode existante avec messages traduits
        private async Task<string> GetUserSignature(MaterialManagement assignment)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Demande de signature utilisateur...");

                // ✅ Messages traduits pour la signature
                var signatureTitle = await GetTextAsync("Electronic Signature", "Electronic Signature");
                var signaturePrompt = await GetTextAsync("Enter full name to confirm", "To confirm reception of") + $" '{assignment.Product?.Name}', " +
                                     await GetTextAsync("please enter your full name", "please enter your full name") + " :\n\n" +
                                     await GetTextAsync("Signature will be timestamped", "(This signature will be timestamped and recorded)");
                var confirm = await GetTextAsync("Confirm", "Confirm");
                var cancel = await GetTextAsync("Cancel", "Cancel");
                var placeholder = await GetTextAsync("Your full name", "Your full name");

                string signature = await DisplayPromptAsync(signatureTitle, signaturePrompt, confirm, cancel, placeholder, maxLength: 100);

                if (!string.IsNullOrWhiteSpace(signature))
                {
                    // ✅ Enrichir la signature avec des infos supplémentaires
                    var enrichedSignature = $"{signature.Trim()} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    System.Diagnostics.Debug.WriteLine($"✅ Signature créée: {enrichedSignature}");
                    return enrichedSignature;
                }

                System.Diagnostics.Debug.WriteLine("⚠️ Signature vide ou annulée");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetUserSignature: {ex.Message}");
                var errorTitle = await GetTextAsync("Error", "Error");
                var captureErrorMessage = await GetTextAsync("Error during signature capture", "Error during signature capture");
                await DisplayAlert(errorTitle, captureErrorMessage, "OK");
                return null;
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadAssignedProductsAsync();
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
        ~AssignedProductsPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}