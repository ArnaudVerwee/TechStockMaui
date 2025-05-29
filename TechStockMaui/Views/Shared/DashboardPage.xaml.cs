using TechStockMaui.Views.Supplier;
using TechStockMaui.Services;

namespace TechStockMaui.Views.Shared
{
    public partial class DashboardPage : ContentPage
    {
        private ProductService _productService;

        public DashboardPage()
        {
            InitializeComponent();
            _productService = new ProductService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadStatisticsAsync();
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                var products = await _productService.GetProductsAsync();
                var totalProducts = products?.Count ?? 0;

                // Un produit est assign� s'il a un AssignedUserName non vide
                var assignedProducts = products?.Count(p =>
                    !string.IsNullOrEmpty(p.AssignedUserName)) ?? 0;

                var freeProducts = totalProducts - assignedProducts;

                TotalProductsLabel.Text = totalProducts.ToString();
                AssignedProductsLabel.Text = assignedProducts.ToString();
                FreeProductsLabel.Text = freeProducts.ToString();
            }
            catch (Exception ex)
            {
                // En cas d'erreur, afficher des valeurs par d�faut
                TotalProductsLabel.Text = "N/A";
                AssignedProductsLabel.Text = "N/A";
                FreeProductsLabel.Text = "N/A";
            }
        }

        private async void OnWarehouseClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new ProductPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible d'ouvrir la page des produits: {ex.Message}", "OK");
            }
        }

        private async void OnTruckClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new SupplierPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible d'ouvrir la page des fournisseurs: {ex.Message}", "OK");
            }
        }

        private async void OnLaptopClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new TypeArticlePage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible d'ouvrir la page des types d'articles: {ex.Message}", "OK");
            }
        }

        private async void OnAssignedProductsClicked(object sender, EventArgs e)
        {
            try
            {
                // TODO: Cr�er la page des produits assign�s
                await DisplayAlert("Info", "Page des produits assign�s � impl�menter", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Erreur: {ex.Message}", "OK");
            }
        }

        private async void OnUsersClicked(object sender, EventArgs e)
        {
            try
            {
                // TODO: Cr�er la page des utilisateurs
                await DisplayAlert("Info", "Page des utilisateurs � impl�menter", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Erreur: {ex.Message}", "OK");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            try
            {
                bool confirm = await DisplayAlert("D�connexion", "�tes-vous s�r de vouloir vous d�connecter ?", "Oui", "Non");
                if (confirm)
                {
                    // TODO: Impl�menter la logique de d�connexion
                    await DisplayAlert("Info", "D�connexion � impl�menter", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Erreur: {ex.Message}", "OK");
            }
        }
    }
}