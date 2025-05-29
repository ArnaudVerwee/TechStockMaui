using TechStockMaui.Models;
using TechStockMaui.Services;

namespace TechStockMaui.Views
{
    public partial class ProductDetailsPage : ContentPage
    {
        private Product _product; // Stocker le produit comme champ
        private ProductService _productService;

        public ProductDetailsPage(Product product)
        {
            InitializeComponent();
            _product = product; // Sauvegarder le produit
            _productService = new ProductService();
            BindingContext = product;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Recharger le produit depuis l'API chaque fois que la page apparaît
            await RefreshProduct();
        }

        private async Task RefreshProduct()
        {
            try
            {
                var updatedProduct = await _productService.GetProductByIdAsync(_product.Id);
                if (updatedProduct != null)
                {
                    _product = updatedProduct;
                    BindingContext = _product; // Re-assigner le BindingContext avec les nouvelles données
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible de recharger le produit: {ex.Message}", "OK");
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            // Passer _product au lieu de sender
            await Navigation.PushAsync(new EditProductPage(_product));
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}