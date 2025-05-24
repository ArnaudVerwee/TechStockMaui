using TypeArticleModel = TechStockMaui.Models.TypeArticle.TypeArticle;
namespace TechStockMaui.Views.TypeArticle;

public partial class DeleteTypeArticlePage : ContentPage
{
    private TypeArticleModel _typeArticle;
    public DeleteTypeArticlePage(TypeArticleModel typeArticle)
    {
        InitializeComponent();
        _typeArticle = typeArticle;
        
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        // Logique de suppression ici, par exemple appeler un service API

        bool confirmed = await DisplayAlert("Confirmation", "Voulez-vous vraiment supprimer ce type ?", "Oui", "Non");
        if (!confirmed)
            return;

        // TODO: Suppression via API

        await DisplayAlert("Succès", "Type supprimé avec succès.", "OK");

        // Retour à la liste
        await Navigation.PopAsync();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
