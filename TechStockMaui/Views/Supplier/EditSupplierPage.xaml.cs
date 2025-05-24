namespace TechStockMaui.Views.Supplier;

public partial class EditSupplierPage : ContentPage
{
    public EditSupplierPage()
    {
        InitializeComponent();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Récupère le nom du fournisseur depuis le Entry
        string supplierName = NameEntry.Text;

        // TODO: Ajouter la logique pour sauvegarder les modifications,
        // par exemple appeler un service API.

        // Pour l'instant juste un message de confirmation
        await DisplayAlert("Sauvegardé", $"Le fournisseur '{supplierName}' a été enregistré.", "OK");

        // Revenir à la liste des fournisseurs
        await Navigation.PopAsync();
    }

    private async void OnBackToListClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
