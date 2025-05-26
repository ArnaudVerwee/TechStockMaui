namespace TechStockMaui.Views.MaterialManagements
{
    public partial class AssignedProductsPage : ContentPage
    {
        public AssignedProductsPage()
        {
            InitializeComponent();
        }

        private void OnSignClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int productId)
            {
                // Naviguer vers la page de signature
                Navigation.PushAsync(new SignProductPage(productId));
            }
        }
    }
}
