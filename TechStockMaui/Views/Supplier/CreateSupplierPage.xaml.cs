namespace TechStockMaui.Views.Supplier;

public partial class CreateSupplierPage : ContentPage
{
	public CreateSupplierPage()
	{
		InitializeComponent();
	}
    private void OnCreateClicked(object sender, EventArgs e)
    {
        // Exemple : r�cup�re le texte saisi
        string name = NameEntry.Text;

        if (string.IsNullOrWhiteSpace(name))
        {
            DisplayAlert("Erreur", "Le nom du fournisseur est obligatoire.", "OK");
            return;
        }

        // TODO : appeler ton service API pour cr�er le fournisseur

        DisplayAlert("Succ�s", $"Fournisseur '{name}' cr��.", "OK");
    }

    // M�thode appel�e au clic sur "Retour � la liste"
    private async void OnBackToListClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}