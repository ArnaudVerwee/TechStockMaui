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
        // ❌ ON SUPPRIME UserService - il est cassé

        public ObservableCollection<Product> Products { get; set; }

        public ProductPage()
        {
            InitializeComponent();
            _productService = new ProductService();
            _supplierService = new SupplierService();
            _typeArticleService = new TypeArticleService();
            // ❌ PAS de UserService

            Products = new ObservableCollection<Product>();
            ProductList.ItemsSource = Products;
        }

        // AUTO-CHARGEMENT comme avant
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("🔄 Page apparue - chargement automatique");
            await LoadProductsAndFiltersAsync();
        }

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
                await DisplayAlert("Erreur", "Erreur de chargement", "OK");
            }
        }

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

                // UTILISATEURS - VERSION AVEC VRAIE API
                System.Diagnostics.Debug.WriteLine("🔄 Chargement utilisateurs via ProductService...");
                try
                {
                    // ✅ UTILISER ProductService.GetUsersAsync() qui retourne UserRolesViewModel
                    var users = await _productService.GetUsersAsync();

                    var userList = new List<object>();
                    userList.Add(new { Id = "NotAssigned", Name = "Non assigné" });
                    userList.Add(new { Id = "All", Name = "Tous" });

                    if (users != null && users.Any())
                    {
                        foreach (var user in users)
                        {
                            // ✅ CORRECTION: Utiliser UserName (pas Username ni Name)
                            var userId = user.UserName ?? "unknown";
                            var userName = user.UserName ?? "Utilisateur sans nom";

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
                    // Fallback
                    var userList = new List<object>();
                    userList.Add(new { Id = "NotAssigned", Name = "Non assigné" });
                    userList.Add(new { Id = "All", Name = "Tous" });
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

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateProductPage());
        }

        private async void OnBackToDashboardClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // RECHERCHE avec filtres - COMPLÈTE
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
                await DisplayAlert("Erreur", "Erreur lors de la recherche", "OK");
            }
        }

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
                await DisplayAlert("Erreur", "Erreur lors du reset", "OK");
            }
        }

        // Actions
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
                    await DisplayAlert("Erreur", "Impossible d'ouvrir la page d'édition", "OK");
                }
            }
        }
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

                    // Fallback - afficher les détails dans une alerte si la page n'existe pas encore
                    var details = $"Nom: {product.Name}\n" +
                                 $"Série: {product.SerialNumber}\n" +
                                 $"Type: {product.TypeName ?? "N/A"}\n" +
                                 $"Fournisseur: {product.SupplierName ?? "N/A"}\n" +
                                 $"Utilisateur: {product.AssignedUserName ?? "Non assigné"}";
                    await DisplayAlert("Détails", details, "OK");
                }
            }
        }

        // ✅ VERSION AVEC DEBUG RENFORCÉ - Remplace OnAssignClicked dans ProductPage.xaml.cs

        // ✅ VERSION COMPLÈTE CORRIGÉE - Remplace OnAssignClicked dans ProductPage.xaml.cs

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

                        // ✅ DÉSASSIGNER - Demander confirmation
                        var result = await DisplayAlert(
                            "Confirmer",
                            $"Désassigner {product.Name} de {product.AssignedUserName} ?",
                            "Oui",
                            "Non"
                        );

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
                            await DisplayAlert("Erreur Navigation", $"Type: {navEx.GetType().Name}\nMessage: {navEx.Message}", "OK");
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
                await DisplayAlert("Erreur Debug", $"Exception: {ex.Message}", "OK");
            }

            System.Diagnostics.Debug.WriteLine("🔄 OnAssignClicked - FIN");
        }

        // ✅ Méthode pour désassigner un produit
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

                    await DisplayAlert("Succès", $"{product.Name} a été désassigné", "OK");
                }
                else
                {
                    await DisplayAlert("Erreur", "Échec de la désassignation", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur UnassignProduct: {ex.Message}");
                await DisplayAlert("Erreur", "Erreur lors de la désassignation", "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Product product)
            {
                var result = await DisplayAlert("Confirmer", $"Supprimer {product.Name}?", "Oui", "Non");
                if (result)
                {
                    try
                    {
                        var success = await _productService.DeleteProductAsync(product.Id);
                        if (success)
                        {
                            Products.Remove(product);
                            await DisplayAlert("OK", "Produit supprimé", "OK");
                        }
                        else
                        {
                            await DisplayAlert("Erreur", "Échec suppression", "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Erreur suppression: {ex.Message}");
                        await DisplayAlert("Erreur", "Erreur suppression", "OK");
                    }
                }
            }
        }
    }
}