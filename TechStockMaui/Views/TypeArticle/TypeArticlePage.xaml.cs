using System;
using Microsoft.Maui.Controls;
using TechStockMaui.Views.TypeArticle;
using TypeArticleModel = TechStockMaui.Models.TypeArticle.TypeArticle;

namespace TechStockMaui.Views
{
    public partial class TypeArticlePage : ContentPage
    {
        public TypeArticlePage()
        {
            InitializeComponent();
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            // Exemple : naviguer vers la page de création d’un nouveau type article
            await Navigation.PushAsync(new CreateTypeArticlePage());
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var typeArticle = button?.CommandParameter as TechStockMaui.Models.TypeArticle.TypeArticle;

            if (typeArticle != null)
            {
                await Navigation.PushAsync(new TechStockMaui.Views.TypeArticle.EditTypeArticlePage(typeArticle));
            }
        }

        private async void OnDetailsClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var typeArticle = button?.CommandParameter as TechStockMaui.Models.TypeArticle.TypeArticle;


            if (typeArticle != null)
            {
                await Navigation.PushAsync(new DetailsTypeArticlePage(typeArticle));
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var typeArticle = button?.CommandParameter as TechStockMaui.Models.TypeArticle.TypeArticle;


            if (typeArticle != null)
            {
                await Navigation.PushAsync(new DeleteTypeArticlePage(typeArticle));
            }
        }
    }
}
