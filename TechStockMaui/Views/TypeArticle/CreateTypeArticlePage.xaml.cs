namespace TechStockMaui.Views.TypeArticle;

public partial class CreateTypeArticlePage : ContentPage
{
    public CreateTypeArticlePage()
    {
        InitializeComponent();
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        string typeName = NameEntry.Text;

        if (string.IsNullOrWhiteSpace(typeName))
        {
            await DisplayAlert("Erreur", "Le nom du type ne peut pas être vide.", "OK");
            return;
        }

        // TODO: Ajouter la logique pour créer le type, par exemple via un service API.

        await DisplayAlert("Succès", $"Type '{typeName}' créé avec succès.", "OK");

        // Revenir à la liste
        await Navigation.PopAsync();
    }

    private async void OnBackToListClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
