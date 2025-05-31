using TechStockMaui.Models;
using TechStockMaui.Services;

namespace TechStockMaui.Views
{
    public partial class ProductDetailsPage : ContentPage
    {
        private Product _product;

        public ProductDetailsPage(Product product)
        {
            InitializeComponent();
            _product = product;
            BindingContext = product; // Le produit du tableau a déjà TypeName et SupplierName !

            // ✅ Debug pour voir ce qu'on reçoit
            System.Diagnostics.Debug.WriteLine($"🔍 Product reçu: Name={product.Name}, TypeName={product.TypeName}, SupplierName={product.SupplierName}");
        }

        // ✅ PAS de OnAppearing ni RefreshProduct - le produit vient du tableau avec toutes les infos !

        private async void OnEditClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new EditProductPage(_product));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur navigation Edit: {ex.Message}");
                await DisplayAlert("Erreur", "Impossible d'ouvrir la page d'édition", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur retour: {ex.Message}");
            }
        }
    }
}