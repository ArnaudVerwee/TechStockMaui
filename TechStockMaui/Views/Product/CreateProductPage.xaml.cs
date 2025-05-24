using TechStockMaui.Services;
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

        // Le gestionnaire d'�v�nement pour le bouton "Cr�er"
        private async void OnCreateClicked(object sender, EventArgs e)
        {
            var newProduct = new Product
            {
                Name = NameEntry.Text,
                SerialNumber = SerialNumberEntry.Text,
                TypeId = (TypePicker.SelectedItem as TechStockMaui.Models.TypeArticle.TypeArticle)?.Id ?? 0,
                SupplierId = (SupplierPicker.SelectedItem as TechStockMaui.Models.Supplier.Supplier)?.Id ?? 0
            };

            bool result = await _productService.CreateProductAsync(newProduct);

            if (result)
            {
                await DisplayAlert("Succ�s", "Produit cr�� avec succ�s", "OK");
                // Naviguer vers la page de liste ou autre logique apr�s cr�ation
            }
            else
            {
                await DisplayAlert("Erreur", "La cr�ation du produit a �chou�", "OK");
            }
        }

        // Le gestionnaire d'�v�nement pour le bouton "Retour � la liste"
        private void OnBackToListClicked(object sender, EventArgs e)
        {
            // Logique pour revenir � la page de la liste (par exemple)
            Navigation.PopAsync();
        }
    }
}
