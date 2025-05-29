using System.Collections.ObjectModel;
using TechStockMaui.Models.TypeArticle;
using TechStockMaui.Services;

namespace TechStockMaui.Views
{
    public partial class TypeArticlePage : ContentPage
    {
        private TypeArticleService _typeArticleService;
        private ObservableCollection<TechStockMaui.Models.TypeArticle.TypeArticle> _typeArticles;

        public TypeArticlePage()
        {
            InitializeComponent();
            _typeArticleService = new TypeArticleService();
            _typeArticles = new ObservableCollection<TechStockMaui.Models.TypeArticle.TypeArticle>();
            TypeArticleList.ItemsSource = _typeArticles;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTypeArticlesAsync();
        }

        private async Task LoadTypeArticlesAsync()
        {
            try
            {
                var typeArticles = await _typeArticleService.GetAllAsync();

                _typeArticles.Clear();
                if (typeArticles != null)
                {
                    foreach (var typeArticle in typeArticles)
                    {
                        _typeArticles.Add(typeArticle);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible de charger les types: {ex.Message}", "OK");
            }
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new CreateTypeArticlePage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible d'ouvrir la page de création: {ex.Message}", "OK");
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is TechStockMaui.Models.TypeArticle.TypeArticle typeArticle)
                {
                    await Navigation.PushAsync(new TypeArticle.EditTypeArticlePage(typeArticle));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible d'ouvrir la page de modification: {ex.Message}", "OK");
            }
        }

        private async void OnDetailsClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is TechStockMaui.Models.TypeArticle.TypeArticle typeArticle)
                {
                    await Navigation.PushAsync(new TypeArticle.DetailsTypeArticlePage(typeArticle));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible d'ouvrir les détails: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is TechStockMaui.Models.TypeArticle.TypeArticle typeArticle)
                {
                    bool confirm = await DisplayAlert(
                        "Confirmation",
                        $"Êtes-vous sûr de vouloir supprimer le type '{typeArticle.Name}' ?",
                        "Oui",
                        "Non");

                    if (confirm)
                    {
                        bool success = await _typeArticleService.DeleteAsync(typeArticle.Id);

                        if (success)
                        {
                            _typeArticles.Remove(typeArticle);
                            await DisplayAlert("Succès", "Type supprimé avec succès", "OK");
                        }
                        else
                        {
                            await DisplayAlert("Erreur", "Échec de la suppression", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible de supprimer: {ex.Message}", "OK");
            }
        }
    }
}