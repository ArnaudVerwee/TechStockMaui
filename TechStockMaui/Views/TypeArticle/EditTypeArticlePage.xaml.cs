using Microsoft.Maui.Controls;
using TypeArticleModel = TechStockMaui.Models.TypeArticle.TypeArticle;
namespace TechStockMaui.Views.TypeArticle
{
    public partial class EditTypeArticlePage : ContentPage
    {
        private TypeArticleModel _typeArticle;

        public EditTypeArticlePage(TypeArticleModel typeArticle)
        {
            InitializeComponent();
            _typeArticle = typeArticle;
        }

        public EditTypeArticlePage()
        {
            InitializeComponent();
        }
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            string name = NameEntry.Text;
            

            await DisplayAlert("Sauvegardé", $"TypeArticle modifié : {name}", "OK");
            await Navigation.PopAsync();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
