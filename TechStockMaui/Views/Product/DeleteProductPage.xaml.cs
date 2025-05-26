using Microsoft.Maui.Controls;

namespace TechStockMaui.Views
{
    public partial class DeleteProductPage : ContentPage
    {
        public DeleteProductPage()
        {
            InitializeComponent();
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            // Ici, tu peux ajouter la logique pour supprimer le produit
            DisplayAlert("Supprimé", "Le produit a été supprimé.", "OK");
        }

        private void OnBackToListClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}
