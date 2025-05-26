using TechStockMaui.Models;
using TechStockMaui.Services;
using TechStockMaui.Views.MaterialManagements;

namespace TechStockMaui.Views;

public partial class ProductPage : ContentPage
{
    private readonly ProductService _productService;
    private List<Product> _allProducts = new();
    private List<Models.TypeArticle.TypeArticle> _types = new();
    private List<Models.Supplier.Supplier> _suppliers = new();
    private List<User> _users = new();

    public ProductPage()
    {
        InitializeComponent();
        _productService = new ProductService();
        LoadInitialData();
    }

    private async void LoadInitialData()
    {
        try
        {
            // Charger toutes les données nécessaires (comme dans votre contrôleur ASP.NET)
            var tasksToRun = new[]
            {
                LoadProducts(),
                LoadTypes(),
                LoadSuppliers(),
                LoadUsers()
            };

            await Task.WhenAll(tasksToRun);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de charger les données: {ex.Message}", "OK");
        }
    }

    private async Task LoadProducts(
        string searchName = null,
        string searchSerialNumber = null,
        int? searchType = null,
        int? searchSupplier = null,
        string searchUser = null)
    {
        try
        {
            _allProducts = await _productService.GetProductsFilterAsync(
                searchName, searchSerialNumber, searchType, searchSupplier, searchUser);
            ProductList.ItemsSource = _allProducts;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de charger les produits: {ex.Message}", "OK");
        }
    }

    private async Task LoadTypes()
    {
        try
        {
            _types = await _productService.GetTypesAsync();
            TypePicker.ItemsSource = _types;
            TypePicker.ItemDisplayBinding = new Binding("Name");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur chargement types: {ex.Message}");
        }
    }

    private async Task LoadSuppliers()
    {
        try
        {
            _suppliers = await _productService.GetSuppliersAsync();
            SupplierPicker.ItemsSource = _suppliers;
            SupplierPicker.ItemDisplayBinding = new Binding("Name");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur chargement fournisseurs: {ex.Message}");
        }
    }

    private async Task LoadUsers()
    {
        try
        {
            _users = await _productService.GetUsersAsync();
            UserPicker.ItemsSource = _users;
            UserPicker.ItemDisplayBinding = new Binding("UserName");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur chargement utilisateurs: {ex.Message}");
        }
    }


    private async void OnSearchClicked(object sender, EventArgs e)
    {
        try
        {
            var searchName = string.IsNullOrWhiteSpace(NameEntry.Text) ? null : NameEntry.Text;
            var searchSerial = string.IsNullOrWhiteSpace(SerialEntry.Text) ? null : SerialEntry.Text;
            var searchType = TypePicker.SelectedItem is Models.TypeArticle.TypeArticle type ? type.Id : (int?)null;
            var searchSupplier = SupplierPicker.SelectedItem is Models.Supplier.Supplier supplier ? supplier.Id : (int?)null;
            var searchUser = UserPicker.SelectedItem is User user ? user.Id : null;

            await LoadProducts(searchName, searchSerial, searchType, searchSupplier, searchUser);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Erreur lors de la recherche: {ex.Message}", "OK");
        }
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        // Réinitialiser les filtres
        NameEntry.Text = string.Empty;
        SerialEntry.Text = string.Empty;
        TypePicker.SelectedItem = null;
        SupplierPicker.SelectedItem = null;
        UserPicker.SelectedItem = null;

        // Recharger tous les produits
        await LoadProducts();
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Product product)
        {
            
            await Navigation.PushAsync(new EditProductPage(product));
        }
    }

    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Product product)
        {
            await Navigation.PushAsync(new ProductDetailsPage(product));
        }
    }

    private async void OnAssignClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Product product)
        {
            
            await Navigation.PushAsync(new AssignProductPage(product));
        }
    }

    private async void OnUnassignClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Product product)
        {
            bool confirm = await DisplayAlert("Confirmation",
                $"Désassigner le produit {product.Name} de {product.AssignedUserName} ?",
                "Oui", "Non");

            if (confirm)
            {
                var success = await _productService.UnassignProductAsync(product.Id);
                if (success)
                {
                    await DisplayAlert("Succès", $"Produit {product.Name} désassigné", "OK");
                    await LoadProducts(); 
                }
                else
                {
                    await DisplayAlert("Erreur", "Impossible de désassigner le produit", "OK");
                }
            }
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Product product)
        {
            bool confirm = await DisplayAlert("Confirmation",
                $"Supprimer définitivement le produit {product.Name} ?",
                "Oui", "Non");

            if (confirm)
            {
                var success = await _productService.DeleteProductAsync(product.Id);
                if (success)
                {
                    await DisplayAlert("Succès", $"Produit {product.Name} supprimé", "OK");
                    await LoadProducts(); 
                }
                else
                {
                    await DisplayAlert("Erreur", "Impossible de supprimer le produit", "OK");
                }
            }
        }
    }
}