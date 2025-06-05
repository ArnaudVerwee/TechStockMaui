using TechStockMaui.Models;
using TechStockMaui.Services;
using System.Collections.ObjectModel;

namespace TechStockMaui.Views.MaterialManagements;

public partial class AssignProductPage : ContentPage
{
    public Product Product { get; }
    public ObservableCollection<User> Users { get; } = new();
    public ObservableCollection<States> States { get; } = new();
    public User SelectedUser { get; set; }
    public States SelectedState { get; set; }
    public Command AssignCommand { get; }

    private readonly MaterialManagementService _service;
    private readonly ProductService _productService;

    // ✅ CONSERVÉ: Votre constructeur original
    public AssignProductPage(Product product)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 AssignProductPage constructeur - DÉBUT");

            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("✅ InitializeComponent OK");

            // ✅ AJOUT: S'abonner aux changements de langue
            TranslationService.Instance.CultureChanged += OnCultureChanged;

            Product = product;
            System.Diagnostics.Debug.WriteLine($"✅ Produit assigné: {product.Name}");

            System.Diagnostics.Debug.WriteLine("🔄 Création MaterialManagementService...");
            _service = new MaterialManagementService();
            System.Diagnostics.Debug.WriteLine("✅ MaterialManagementService créé");

            System.Diagnostics.Debug.WriteLine("🔄 Création ProductService...");
            _productService = new ProductService();
            System.Diagnostics.Debug.WriteLine("✅ ProductService créé");

            System.Diagnostics.Debug.WriteLine("🔄 Création Command...");
            AssignCommand = new Command(async () => await AssignProductToUser());
            System.Diagnostics.Debug.WriteLine("✅ Command créé");

            System.Diagnostics.Debug.WriteLine("🔄 BindingContext...");
            BindingContext = this;
            System.Diagnostics.Debug.WriteLine("✅ BindingContext assigné");

            System.Diagnostics.Debug.WriteLine("✅ AssignProductPage constructeur - FIN");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ERREUR constructeur AssignProductPage: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack constructeur: {ex.StackTrace}");
        }
    }

    // ✅ MODIFIÉ: Votre méthode existante avec ajout des traductions
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // ✅ AJOUT: Charger les traductions
        await LoadTranslationsAsync();

        // ✅ CONSERVÉ: Votre logique existante
        System.Diagnostics.Debug.WriteLine("🔄 AssignProductPage apparue - chargement automatique");
        await LoadDataAsync();
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
            System.Diagnostics.Debug.WriteLine("🌍 Mise à jour des textes AssignProductPage");

            // ✅ Titre de la page
            Title = await GetTextAsync("Assign Product", "Assign Product");

            // ✅ Titre principal
            if (TitleLabel != null)
                TitleLabel.Text = await GetTextAsync("Assign product to user", "Assign a product to a user");

            // ✅ En-tête informations produit
            if (ProductInfoHeaderLabel != null)
                ProductInfoHeaderLabel.Text = await GetTextAsync("Product Information", "Product Information");

            // ✅ Labels des informations produit
            if (ProductSpan != null)
                ProductSpan.Text = await GetTextAsync("Product", "Product") + " : ";

            if (SerialNumberSpan != null)
                SerialNumberSpan.Text = await GetTextAsync("Serial Number", "Serial Number") + " : ";

            // ✅ Labels de sélection
            if (SelectUserLabel != null)
                SelectUserLabel.Text = await GetTextAsync("Select a user", "Select a user") + " :";

            if (SelectStateLabel != null)
                SelectStateLabel.Text = await GetTextAsync("Select product state", "Select the product state") + " :";

            // ✅ Placeholders des Pickers
            if (UserPicker != null)
                UserPicker.Title = await GetTextAsync("Select a user", "Select a user");

            if (StatePicker != null)
                StatePicker.Title = await GetTextAsync("Select a state", "Select a state");

            // ✅ Boutons
            if (AssignButton != null)
                AssignButton.Text = await GetTextAsync("Assign", "Assign");

            if (BackButton != null)
                BackButton.Text = await GetTextAsync("Back to list", "Back to list");

            // ✅ Sélecteur de langue
            if (LanguageLabel != null)
                LanguageLabel.Text = await GetTextAsync("Language", "Language");

            // ✅ Mettre à jour l'indicateur de langue
            await UpdateLanguageFlag();

            // ✅ Mettre à jour les états traduits
            await UpdateStatesWithTranslations();

            System.Diagnostics.Debug.WriteLine("✅ Textes AssignProductPage mis à jour");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur UpdateTextsAsync: {ex.Message}");
        }
    }

    // ✅ AJOUT: Mettre à jour les états avec traductions
    private async Task UpdateStatesWithTranslations()
    {
        try
        {
            if (States.Any())
            {
                // Créer une nouvelle liste avec les traductions
                var translatedStates = new List<States>();

                foreach (var state in States.ToList())
                {
                    var translatedStatus = state.Status switch
                    {
                        "New Product" => await GetTextAsync("New Product", "New Product"),
                        "Old Product" => await GetTextAsync("Old Product", "Old Product"),
                        "Product to repair" => await GetTextAsync("Product to repair", "Product to repair"),
                        "Broken Product" => await GetTextAsync("Broken Product", "Broken Product"),
                        _ => state.Status
                    };

                    translatedStates.Add(new States { Id = state.Id, Status = translatedStatus });
                }

                // Mettre à jour la collection sur le thread principal
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var selectedStateId = SelectedState?.Id;
                    States.Clear();
                    foreach (var state in translatedStates)
                    {
                        States.Add(state);
                    }

                    // Restaurer la sélection si elle existait
                    if (selectedStateId.HasValue)
                    {
                        SelectedState = States.FirstOrDefault(s => s.Id == selectedStateId.Value);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur UpdateStatesWithTranslations: {ex.Message}");
        }
    }

    // ✅ AJOUT: Callback quand la langue change
    private async void OnCultureChanged(object sender, string newCulture)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🌍 AssignProductPage - Langue changée vers: {newCulture}");
            await UpdateTextsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
        }
    }

    // ✅ CONSERVÉ: Votre méthode existante inchangée
    private async Task LoadDataAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 LoadDataAsync - DÉBUT");

            // CHARGER UTILISATEURS
            System.Diagnostics.Debug.WriteLine("🔄 Chargement utilisateurs...");
            try
            {
                var users = await _productService.GetUsersAsync();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Users.Clear();
                    if (users != null && users.Any())
                    {
                        foreach (var user in users)
                        {
                            Users.Add(user);
                            System.Diagnostics.Debug.WriteLine($"🔍 Utilisateur ajouté: {user}");
                        }
                        System.Diagnostics.Debug.WriteLine($"✅ {Users.Count} utilisateurs ajoutés");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ Aucun utilisateur reçu");
                    }
                });
            }
            catch (Exception userEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur chargement utilisateurs: {userEx.Message}");
            }

            // CHARGER ÉTATS
            System.Diagnostics.Debug.WriteLine("🔄 Chargement états...");
            var defaultStates = new List<States>
            {
                new States { Id = 1, Status = "New Product" },
                new States { Id = 2, Status = "Old Product" },
                new States { Id = 3, Status = "Product to repair" },
                new States { Id = 4, Status = "Broken Product" }
            };

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                States.Clear();
                foreach (var state in defaultStates)
                {
                    States.Add(state);
                    System.Diagnostics.Debug.WriteLine($"🔍 État ajouté: {state.Status}");
                }
                System.Diagnostics.Debug.WriteLine($"✅ {States.Count} états ajoutés");
            });

            // ✅ AJOUT: Appliquer les traductions après le chargement
            await UpdateStatesWithTranslations();

            System.Diagnostics.Debug.WriteLine("✅ LoadDataAsync - FIN");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur LoadDataAsync: {ex.Message}");
            var errorTitle = await GetTextAsync("Error", "Error");
            var loadErrorMessage = await GetTextAsync("Data loading error", "Data loading error");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert(errorTitle, $"{loadErrorMessage}: {ex.Message}", "OK");
            });
        }
    }

    // ✅ MODIFIÉ: Votre méthode existante avec messages traduits
    private async Task AssignProductToUser()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 AssignProductToUser - DÉBUT");

            if (SelectedUser == null || SelectedState == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ Utilisateur ou état non sélectionné");
                var errorTitle = await GetTextAsync("Error", "Error");
                var selectionErrorMessage = await GetTextAsync("Please select user and state", "Please select a user and a state.");
                await DisplayAlert(errorTitle, selectionErrorMessage, "OK");
                return;
            }

            // TROUVER LA BONNE PROPRIÉTÉ DE L'UTILISATEUR
            string userIdentifier = "";
            var userType = SelectedUser.GetType();

            // Essayer Email en premier
            var emailProp = userType.GetProperty("Email");
            if (emailProp != null)
            {
                userIdentifier = emailProp.GetValue(SelectedUser)?.ToString();
            }
            // Sinon essayer UserName
            else if (userType.GetProperty("UserName") != null)
            {
                userIdentifier = userType.GetProperty("UserName").GetValue(SelectedUser)?.ToString();
            }
            // Sinon essayer Username
            else if (userType.GetProperty("Username") != null)
            {
                userIdentifier = userType.GetProperty("Username").GetValue(SelectedUser)?.ToString();
            }
            // Sinon essayer Name
            else if (userType.GetProperty("Name") != null)
            {
                userIdentifier = userType.GetProperty("Name").GetValue(SelectedUser)?.ToString();
            }
            // Sinon essayer Id
            else if (userType.GetProperty("Id") != null)
            {
                userIdentifier = userType.GetProperty("Id").GetValue(SelectedUser)?.ToString();
            }

            if (string.IsNullOrEmpty(userIdentifier))
            {
                var errorTitle = await GetTextAsync("Error", "Error");
                var identifierErrorMessage = await GetTextAsync("Unable to retrieve user identifier", "Unable to retrieve user identifier");
                await DisplayAlert(errorTitle, identifierErrorMessage, "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"🔄 Assignation: Produit={Product.Id}, Utilisateur={userIdentifier}, État={SelectedState.Id}");

            bool success = await _service.AssignProductAsync(Product.Id, userIdentifier, SelectedState.Id);

            if (success)
            {
                System.Diagnostics.Debug.WriteLine("✅ Assignation réussie");
                var successTitle = await GetTextAsync("Success", "Success");
                var successMessage = await GetTextAsync("Product assigned successfully", "Product assigned successfully.");
                await DisplayAlert(successTitle, successMessage, "OK");
                await Navigation.PopAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ Assignation échouée");
                var errorTitle = await GetTextAsync("Error", "Error");
                var assignErrorMessage = await GetTextAsync("Assignment error", "Error during assignment.");
                await DisplayAlert(errorTitle, assignErrorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur AssignProductToUser: {ex.Message}");
            var errorTitle = await GetTextAsync("Error", "Error");
            var assignErrorMessage = await GetTextAsync("Assignment error", "Error during assignment");
            await DisplayAlert(errorTitle, $"{assignErrorMessage}: {ex.Message}", "OK");
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
    ~AssignProductPage()
    {
        try
        {
            TranslationService.Instance.CultureChanged -= OnCultureChanged;
        }
        catch { }
    }
}