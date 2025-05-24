namespace TechStockMaui.Views.Supplier;

public partial class DetailsSupplierPage : ContentPage
{
    public DetailsSupplierPage()
    {
        InitializeComponent();
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        // Code à exécuter au clic sur le bouton Modifier
        // Par exemple, naviguer vers une page de modification
        await Navigation.PushAsync(new EditSupplierPage());
    }

    private async void OnBackToListClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
