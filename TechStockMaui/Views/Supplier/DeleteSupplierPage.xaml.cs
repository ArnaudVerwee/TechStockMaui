namespace TechStockMaui.Views.Supplier;

public partial class DeleteSupplierPage : ContentPage
{
    public DeleteSupplierPage()
    {
        InitializeComponent();
    }

    // Méthode appelée au clic sur le bouton Supprimer
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        // TODO: appeler ton API pour supprimer le fournisseur ici

        bool confirmed = await DisplayAlert("Confirmation", "Le fournisseur a été supprimé.", "OK", null);
        if (confirmed)
        {
            await Navigation.PopAsync();
        }
    }

    // Méthode appelée au clic sur le bouton Annuler
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
