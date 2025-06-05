using TechStockMaui.Services;
using TechStockMaui.Models;
using System.Collections.ObjectModel;

namespace TechStockMaui.Views
{
    public partial class ProductPage : ContentPage
    {
        private readonly ProductService _productService;
        private readonly SupplierService _supplierService;
        private readonly TypeArticleService _typeArticleService;

        public ObservableCollection<Product> Products { get; set; }

        public ProductPage()
        {
            InitializeComponent();
            _productService = new ProductService();
            _supplierService = new SupplierService();
            _typeArticleService = new TypeArticleService();

            Products = new ObservableCollection<Product>();
            ProductList.ItemsSource = Products;

            // ✅ AJOUT: S'abonner aux changements de langue
            TranslationService.Instance.CultureChanged += OnCultureChanged;
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("🔄 Page apparue - chargement automatique");

            // ✅ AJOUT: Charger les traductions en premier
            await LoadTranslationsAsync();

            // ✅ CONSERVÉ: Votre logique existante
            await LoadProductsAndFiltersAsync();
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
                System.Diagnostics.Debug.WriteLine("🌍 Mise à jour des textes Products");

                // ✅ Titre de la page
                Title = await GetTextAsync("Products", "Products");

                // ✅ Titre de la page dans l'header
                try
                {
                    if (PageTitleLabel != null)
                        PageTitleLabel.Text = await GetTextAsync("ProductManagement", "Product Management");
                }
                catch { /* PageTitleLabel n'existe pas */ }

                // ✅ Boutons
                try
                {
                    if (SearchButton != null)
                        SearchButton.Text = "🔍 " + await GetTextAsync("Search", "Search");
                }
                catch { /* SearchButton n'existe pas */ }

                try
                {
                    if (ResetButton != null)
                        ResetButton.Text = "🔄 " + await GetTextAsync("Reset", "Reset");
                }
                catch { /* ResetButton n'existe pas */ }

                // ✅ Placeholders des Entry de recherche
                if (NameEntry != null)
                    NameEntry.Placeholder = await GetTextAsync("NamePlaceholder", "Name...");

                if (SerialEntry != null)
                    SerialEntry.Placeholder = await GetTextAsync("SerialPlaceholder", "Serial number...");

                // ✅ Titles des Pickers
                if (TypePicker != null)
                    TypePicker.Title = await GetTextAsync("Type", "Type");

                if (SupplierPicker != null)
                    SupplierPicker.Title = await GetTextAsync("Supplier", "Supplier");

                if (UserPicker != null)
                    UserPicker.Title = await GetTextAsync("User", "User");

                // ✅ Headers de tableau
                try
                {
                    if (NameHeader != null)
                        NameHeader.Text = await GetTextAsync("Name", "Name");
                }
                catch { /* NameHeader n'existe pas */ }

                try
                {
                    if (SerialHeader != null)
                        SerialHeader.Text = await GetTextAsync("SerialNumber", "Serial Number");
                }
                catch { /* SerialHeader n'existe pas */ }

                try
                {
                    if (TypeHeader != null)
                        TypeHeader.Text = await GetTextAsync("Type", "Type");
                }
                catch { /* TypeHeader n'existe pas */ }

                try
                {
                    if (SupplierHeader != null)
                        SupplierHeader.Text = await GetTextAsync("Supplier", "Supplier");
                }
                catch { /* SupplierHeader n'existe pas */ }

                try
                {
                    if (UserHeader != null)
                        UserHeader.Text = await GetTextAsync("User", "User");
                }
                catch { /* UserHeader n'existe pas */ }

                try
                {
                    if (ActionsHeader != null)
                        ActionsHeader.Text = await GetTextAsync("Actions", "Actions");
                }
                catch { /* ActionsHeader n'existe pas */ }

                // ✅ Mettre à jour l'indicateur de langue
                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("✅ Textes Products mis à jour");
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
                System.Diagnostics.Debug.WriteLine($"🌍 Products - Langue changée vers: {newCulture}");
                await UpdateTextsAsync();

                // ✅ Recharger les filtres avec les nouvelles traductions
                await ReloadFiltersWithTranslations();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        // ✅ AJOUT: Recharger les filtres avec traductions
        private async Task ReloadFiltersWithTranslations()
        {
            try
            {
                // Recharger seulement la partie utilisateur avec traductions
                var users = await _productService.GetUsersAsync();
                var userList = new List<object>();

                var notAssignedText = await GetTextAsync("NotAssigned", "Not assigned");
                var allText = await GetTextAsync("All", "All");

                userList.Add(new { Id = "NotAssigned", Name = notAssignedText });
                userList.Add(new { Id = "All", Name = allText });

                if (users != null && users.Any())
                {
                    foreach (var user in users)
                    {
                        var userId = user.UserName ?? "unknown";
                        var userName = user.UserName ?? await GetTextAsync("UnnamedUser", "Unnamed user");
                        userList.Add(new { Id = userId, Name = userName });
                    }
                }

                UserPicker.ItemsSource = userList;
                UserPicker.ItemDisplayBinding = new Binding("Name");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur ReloadFiltersWithTranslations: {ex.Message}");
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private async Task LoadProductsAndFiltersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📞 Chargement produits ET filtres...");

                // Charger les produits
                await LoadProductsOnlyAsync();

                // Charger les filtres APRES les produits
                await LoadFiltersAsync();

                System.Diagnostics.Debug.WriteLine("✅ Tout chargé avec succès");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur LoadProductsAndFiltersAsync: {ex.Message}");
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var loadingErrorMsg = await GetTextAsync("LoadingError", "Erreur de chargement");
                await DisplayAlert(errorTitle, loadingErrorMsg, "OK");
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private async Task LoadProductsOnlyAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📞 Chargement des produits...");

                // ✅ UTILISER LA MÉTHODE FILTER pour avoir TypeName et SupplierName
                var products = await _productService.GetProductsFilterAsync();
                Products.Clear();

                if (products != null)
                {
                    foreach (var product in products.OrderByDescending(p => p.Id))
                    {
                        // DEBUG temporaire pour voir le contenu des produits
                        System.Diagnostics.Debug.WriteLine($"🔍 Produit: Name={product.Name}, TypeName={product.TypeName}, SupplierName={product.SupplierName}, AssignedUserName={product.AssignedUserName}");
                        Products.Add(product);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ {Products.Count} produits chargés");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur LoadProductsOnlyAsync: {ex.Message}");
                throw;
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async Task LoadFiltersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📞 Chargement des filtres...");

                // Charger types
                System.Diagnostics.Debug.WriteLine("🔄 Chargement types...");
                var types = await _typeArticleService.GetAllAsync();
                if (types != null && types.Any())
                {
                    TypePicker.ItemsSource = types.ToList();
                    TypePicker.ItemDisplayBinding = new Binding("Name");
                    System.Diagnostics.Debug.WriteLine($"✅ {types.Count()} types chargés");
                }

                // Charger fournisseurs
                System.Diagnostics.Debug.WriteLine("🔄 Chargement fournisseurs...");
                var suppliers = await _supplierService.GetSuppliersAsync();
                if (suppliers != null && suppliers.Any())
                {
                    SupplierPicker.ItemsSource = suppliers.ToList();
                    SupplierPicker.ItemDisplayBinding = new Binding("Name");
                    System.Diagnostics.Debug.WriteLine($"✅ {suppliers.Count()} fournisseurs chargés");
                }

                // UTILISATEURS - VERSION AVEC VRAIE API ET TRADUCTIONS
                System.Diagnostics.Debug.WriteLine("🔄 Chargement utilisateurs via ProductService...");
                try
                {
                    // ✅ UTILISER ProductService.GetUsersAsync() qui retourne UserRolesViewModel
                    var users = await _productService.GetUsersAsync();

                    var userList = new List<object>();

                    // ✅ Options traduites
                    var notAssignedText = await GetTextAsync("NotAssigned", "Not assigned");
                    var allText = await GetTextAsync("All", "All");

                    userList.Add(new { Id = "NotAssigned", Name = notAssignedText });
                    userList.Add(new { Id = "All", Name = allText });

                    if (users != null && users.Any())
                    {
                        foreach (var user in users)
                        {
                            // ✅ CORRECTION: Utiliser UserName (pas Username ni Name)
                            var userId = user.UserName ?? "unknown";
                            var userName = user.UserName ?? await GetTextAsync("UnnamedUser", "Unnamed user");

                            userList.Add(new { Id = userId, Name = userName });
                        }
                        System.Diagnostics.Debug.WriteLine($"✅ {users.Count} utilisateurs ajoutés");
                    }

                    UserPicker.ItemsSource = userList;
                    UserPicker.ItemDisplayBinding = new Binding("Name");
                    System.Diagnostics.Debug.WriteLine($"✅ {userList.Count} options utilisateur au total");
                }
                catch (Exception userEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Erreur utilisateurs: {userEx.Message}");
                    // Fallback avec traductions
                    var userList = new List<object>();
                    var notAssignedText = await GetTextAsync("NotAssigned", "Not assigned");
                    var allText = await GetTextAsync("All", "All");

                    userList.Add(new { Id = "NotAssigned", Name = notAssignedText });
                    userList.Add(new { Id = "All", Name = allText });
                    UserPicker.ItemsSource = userList;
                    UserPicker.ItemDisplayBinding = new Binding("Name");
                }

                System.Diagnostics.Debug.WriteLine("✅ Filtres chargés avec succès");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur LoadFiltersAsync: {ex.Message}");
                // Ne pas planter si les filtres échouent
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private async void OnCreateClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateProductPage());
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private async void OnBackToDashboardClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async void OnSearchClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 Recherche lancée");

                var name = string.IsNullOrWhiteSpace(NameEntry.Text) ? null : NameEntry.Text;
                var serialNumber = string.IsNullOrWhiteSpace(SerialEntry.Text) ? null : SerialEntry.Text;
                var typeId = TypePicker.SelectedItem is TechStockMaui.Models.TypeArticle.TypeArticle selectedType ? selectedType.Id : (int?)null;
                var supplierId = SupplierPicker.SelectedItem is TechStockMaui.Models.Supplier.Supplier selectedSupplier ? selectedSupplier.Id : (int?)null;

                // ✅ GESTION CORRECTE DU FILTRE UTILISATEUR
                string userId = null;
                if (UserPicker.SelectedItem != null)
                {
                    var selectedUser = UserPicker.SelectedItem as dynamic;
                    userId = selectedUser?.Id?.ToString();
                    System.Diagnostics.Debug.WriteLine($"🔍 Utilisateur sélectionné: {userId}");
                }

                System.Diagnostics.Debug.WriteLine($"🔍 Filtres: Name={name}, Serial={serialNumber}, TypeId={typeId}, SupplierId={supplierId}, UserId={userId}");

                var filteredProducts = await _productService.GetProductsFilterAsync(name, serialNumber, typeId, supplierId, userId);

                Products.Clear();
                if (filteredProducts != null)
                {
                    foreach (var product in filteredProducts.OrderByDescending(p => p.Id))
                    {
                        Products.Add(product);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ {Products.Count} produits trouvés");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur recherche: {ex.Message}");
                var errorTitle = await GetTextAsync("Error", "Error");
                var searchErrorMsg = await GetTextAsync("SearchError", "Search error");
                await DisplayAlert(errorTitle, searchErrorMsg, "OK");
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async void OnResetClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Reset filtres");

                NameEntry.Text = string.Empty;
                SerialEntry.Text = string.Empty;
                TypePicker.SelectedItem = null;
                SupplierPicker.SelectedItem = null;
                UserPicker.SelectedItem = null;

                await LoadProductsOnlyAsync();
                System.Diagnostics.Debug.WriteLine("✅ Reset terminé");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur reset: {ex.Message}");
                var errorTitle = await GetTextAsync("Error", "Error");
                var resetErrorMsg = await GetTextAsync("ResetError", "Reset error");
                await DisplayAlert(errorTitle, resetErrorMsg, "OK");
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Product product)
            {
                try
                {
                    // Navigation vers EditProductPage en passant le produit
                    await Navigation.PushAsync(new Views.EditProductPage(product));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Erreur navigation Edit: {ex.Message}");
                    var errorTitle = await GetTextAsync("Error", "Error");
                    var editErrorMsg = await GetTextAsync("CannotOpenEditPage", "Cannot open edit page");
                    await DisplayAlert(errorTitle, editErrorMsg, "OK");
                }
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async void OnDetailsClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Product product)
            {
                try
                {
                    // Navigation vers DetailsProductPage en passant le produit
                    await Navigation.PushAsync(new Views.ProductDetailsPage(product));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Erreur navigation Details: {ex.Message}");

                    // ✅ Fallback avec traductions
                    var nameLabel = await GetTextAsync("Name", "Name");
                    var serialLabel = await GetTextAsync("SerialNumber", "Serial");
                    var typeLabel = await GetTextAsync("Type", "Type");
                    var supplierLabel = await GetTextAsync("Supplier", "Supplier");
                    var userLabel = await GetTextAsync("User", "User");
                    var notAssignedText = await GetTextAsync("NotAssigned", "Not assigned");
                    var detailsTitle = await GetTextAsync("Details", "Details");

                    var details = $"{nameLabel}: {product.Name}\n" +
                                 $"{serialLabel}: {product.SerialNumber}\n" +
                                 $"{typeLabel}: {product.TypeName ?? "N/A"}\n" +
                                 $"{supplierLabel}: {product.SupplierName ?? "N/A"}\n" +
                                 $"{userLabel}: {product.AssignedUserName ?? notAssignedText}";

                    await DisplayAlert(detailsTitle, details, "OK");
                }
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async void OnAssignClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("🔄 OnAssignClicked - DÉBUT");

            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 Vérification sender et CommandParameter...");

                if (sender is Button button && button.CommandParameter is Product product)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Bouton et produit OK - Produit: {product.Name}");

                    // ✅ Vérifier si le produit est déjà assigné
                    bool isAssigned = !string.IsNullOrEmpty(product.AssignedUserName);

                    System.Diagnostics.Debug.WriteLine($"🔍 Produit assigné: {isAssigned}");
                    System.Diagnostics.Debug.WriteLine($"🔍 AssignedUserName: '{product.AssignedUserName}'");

                    if (isAssigned)
                    {
                        System.Diagnostics.Debug.WriteLine("🔄 Tentative de désassignation...");

                        // ✅ DÉSASSIGNER avec traductions
                        var confirmTitle = await GetTextAsync("Confirm", "Confirm");
                        var unassignQuestion = await GetTextAsync("UnassignQuestion", "Unassign {0} from {1}?");
                        var unassignMsg = string.Format(unassignQuestion, product.Name, product.AssignedUserName);
                        var yesText = await GetTextAsync("Yes", "Yes");
                        var noText = await GetTextAsync("No", "No");

                        var result = await DisplayAlert(confirmTitle, unassignMsg, yesText, noText);
                        System.Diagnostics.Debug.WriteLine($"🔍 Résultat confirmation: {result}");

                        if (result)
                        {
                            System.Diagnostics.Debug.WriteLine("🔄 Début désassignation...");
                            await UnassignProduct(product);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("🔄 Tentative d'assignation - Navigation vers AssignProductPage...");

                        try
                        {
                            System.Diagnostics.Debug.WriteLine("🔄 Étape 1 - Début navigation réelle...");

                            // ✅ ÉTAPE 1 : Créer la page étape par étape
                            System.Diagnostics.Debug.WriteLine("🔄 Étape 2 - Création AssignProductPage...");
                            var assignPage = new Views.MaterialManagements.AssignProductPage(product);
                            System.Diagnostics.Debug.WriteLine("✅ Étape 2 réussie - Page créée !");

                            // ✅ ÉTAPE 2 : Navigation
                            System.Diagnostics.Debug.WriteLine("🔄 Étape 3 - Navigation.PushAsync...");
                            await Navigation.PushAsync(assignPage);
                            System.Diagnostics.Debug.WriteLine("✅ Étape 3 réussie - Navigation terminée !");

                        }
                        catch (Exception navEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ ERREUR NAVIGATION: {navEx.Message}");
                            System.Diagnostics.Debug.WriteLine($"❌ Type d'erreur: {navEx.GetType().Name}");
                            System.Diagnostics.Debug.WriteLine($"❌ Stack navigation: {navEx.StackTrace}");

                            var errorTitle = await GetTextAsync("NavigationError", "Navigation Error");
                            var errorMsg = $"Type: {navEx.GetType().Name}\nMessage: {navEx.Message}";
                            await DisplayAlert(errorTitle, errorMsg, "OK");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Sender ou CommandParameter invalide");
                    System.Diagnostics.Debug.WriteLine($"   Sender type: {sender?.GetType()}");
                    System.Diagnostics.Debug.WriteLine($"   CommandParameter type: {(sender as Button)?.CommandParameter?.GetType()}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ EXCEPTION OnAssignClicked: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");

                var errorTitle = await GetTextAsync("Error", "Error");
                await DisplayAlert(errorTitle, $"Exception: {ex.Message}", "OK");
            }

            System.Diagnostics.Debug.WriteLine("🔄 OnAssignClicked - FIN");
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async Task UnassignProduct(Product product)
        {
            try
            {
                // ✅ Utiliser la méthode publique du ProductService
                bool success = await _productService.UnassignProductAsync(product.Id);

                if (success)
                {
                    // ✅ Mettre à jour l'affichage
                    await LoadProductsOnlyAsync(); // Recharger la liste

                    var successTitle = await GetTextAsync("Success", "Success");
                    var unassignedMsg = await GetTextAsync("ProductUnassigned", "{0} has been unassigned");
                    var message = string.Format(unassignedMsg, product.Name);
                    await DisplayAlert(successTitle, message, "OK");
                }
                else
                {
                    var errorTitle = await GetTextAsync("Error", "Error");
                    var unassignFailedMsg = await GetTextAsync("UnassignFailed", "Unassign failed");
                    await DisplayAlert(errorTitle, unassignFailedMsg, "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur UnassignProduct: {ex.Message}");
                var errorTitle = await GetTextAsync("Error", "Error");
                var unassignErrorMsg = await GetTextAsync("UnassignError", "Error during unassign");
                await DisplayAlert(errorTitle, unassignErrorMsg, "OK");
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Product product)
            {
                // ✅ Confirmation avec traductions
                var confirmTitle = await GetTextAsync("Confirm", "Confirm");
                var deleteQuestion = await GetTextAsync("DeleteQuestion", "Delete {0}?");
                var deleteMsg = string.Format(deleteQuestion, product.Name);
                var yesText = await GetTextAsync("Yes", "Yes");
                var noText = await GetTextAsync("No", "No");

                var result = await DisplayAlert(confirmTitle, deleteMsg, yesText, noText);

                if (result)
                {
                    try
                    {
                        var success = await _productService.DeleteProductAsync(product.Id);
                        if (success)
                        {
                            Products.Remove(product);
                            var successTitle = await GetTextAsync("Success", "Success");
                            var deletedMsg = await GetTextAsync("ProductDeleted", "Product deleted");
                            await DisplayAlert(successTitle, deletedMsg, "OK");
                        }
                        else
                        {
                            var errorTitle = await GetTextAsync("Error", "Error");
                            var deleteFailedMsg = await GetTextAsync("DeleteFailed", "Delete failed");
                            await DisplayAlert(errorTitle, deleteFailedMsg, "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Erreur suppression: {ex.Message}");
                        var errorTitle = await GetTextAsync("Error", "Error");
                        var deleteErrorMsg = await GetTextAsync("DeleteError", "Delete error");
                        await DisplayAlert(errorTitle, deleteErrorMsg, "OK");
                    }
                }
            }
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

                // ✅ Uniquement si l'élément LanguageFlag existe
                try
                {
                    if (LanguageFlag != null)
                        LanguageFlag.Text = flag;
                }
                catch
                {
                    // LanguageFlag n'existe pas dans le XAML - pas grave
                    System.Diagnostics.Debug.WriteLine("ℹ️ LanguageFlag non trouvé dans le XAML");
                }
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

        // ✅ AJOUT: Destructeur pour nettoyer l'événement
        ~ProductPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}