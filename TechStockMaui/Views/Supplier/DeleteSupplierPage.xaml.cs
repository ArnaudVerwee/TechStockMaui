namespace TechStockMaui.Views.Supplier;

public partial class DeleteSupplierPage : ContentPage
{
    public DeleteSupplierPage()
    {
        InitializeComponent();
    }

    // M�thode appel�e au clic sur le bouton Supprimer
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        // TODO: appeler ton API pour supprimer le fournisseur ici

        bool confirmed = await DisplayAlert("Confirmation", "Le fournisseur a �t� supprim�.", "OK", null);
        if (confirmed)
        {
            await Navigation.PopAsync();
        }
    }

    // M�thode appel�e au clic sur le bouton Annuler
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
