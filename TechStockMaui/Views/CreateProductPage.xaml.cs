using TechStockMaui.Services;
using TechStockWeb.Models;
using Microsoft.Maui.Controls;
using TechStockMaui.Models;

namespace TechStockMaui.Views
{
    public partial class CreateProductPage : ContentPage
    {
        private readonly ProductService _productService;

        public CreateProductPage()
        {
            InitializeComponent();
            _productService = new ProductService();
        }

        // Le gestionnaire d'événement pour le bouton "Créer"
        private async void OnCreateClicked(object sender, EventArgs e)
        {
            var newProduct = new Product
            {
                Name = NameEntry.Text,
                SerialNumber = SerialNumberEntry.Text,
                TypeId = (TypePicker.SelectedItem as TypeArticle)?.Id ?? 0, // Assure-toi que tu as les données nécessaires
                SupplierId = (SupplierPicker.SelectedItem as Supplier)?.Id ?? 0
            };

            bool result = await _productService.CreateProductAsync(newProduct);

            if (result)
            {
                await DisplayAlert("Succès", "Produit créé avec succès", "OK");
                // Naviguer vers la page de liste ou autre logique après création
            }
            else
            {
                await DisplayAlert("Erreur", "La création du produit a échoué", "OK");
            }
        }

        // Le gestionnaire d'événement pour le bouton "Retour à la liste"
        private void OnBackToListClicked(object sender, EventArgs e)
        {
            // Logique pour revenir à la page de la liste (par exemple)
            Navigation.PopAsync();
        }
    }
}
