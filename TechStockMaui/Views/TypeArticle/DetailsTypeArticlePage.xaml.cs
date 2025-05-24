using Microsoft.Maui.Controls;
using TypeArticleModel = TechStockMaui.Models.TypeArticle.TypeArticle;

namespace TechStockMaui.Views.TypeArticle;

public partial class DetailsTypeArticlePage : ContentPage
{
    private TypeArticleModel _typeArticle;
    public DetailsTypeArticlePage(TypeArticleModel typeArticle)
    {
        InitializeComponent();
        _typeArticle = typeArticle;
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        // Logique lors du clic sur Modifier
        // Par exemple, naviguer vers la page d'�dition en passant l'objet actuel

        // Supposons que tu as un TypeArticle courant, tu peux passer ses donn�es � la page Edit
        // await Navigation.PushAsync(new EditTypeArticlePage(currentTypeArticle));

        await DisplayAlert("Modifier", "Bouton Modifier cliqu�.", "OK");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
