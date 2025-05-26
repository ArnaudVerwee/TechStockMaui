using TechStockMaui.Models;

namespace TechStockMaui.Views
{
    public partial class ProductDetailsPage : ContentPage
    {
        public ProductDetailsPage(Product product)
        {
            InitializeComponent();
            BindingContext = product;
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditProductPage((Product)sender));
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
