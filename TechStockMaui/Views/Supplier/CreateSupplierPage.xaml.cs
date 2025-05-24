namespace TechStockMaui.Views.Supplier;

public partial class CreateSupplierPage : ContentPage
{
	public CreateSupplierPage()
	{
		InitializeComponent();
	}
    private void OnCreateClicked(object sender, EventArgs e)
    {
        // Exemple : récupère le texte saisi
        string name = NameEntry.Text;

        if (string.IsNullOrWhiteSpace(name))
        {
            DisplayAlert("Erreur", "Le nom du fournisseur est obligatoire.", "OK");
            return;
        }

        // TODO : appeler ton service API pour créer le fournisseur

        DisplayAlert("Succès", $"Fournisseur '{name}' créé.", "OK");
    }

    // Méthode appelée au clic sur "Retour à la liste"
    private async void OnBackToListClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}