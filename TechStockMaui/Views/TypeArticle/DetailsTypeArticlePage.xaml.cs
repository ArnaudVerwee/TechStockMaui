using TechStockMaui.Services;
using TypeArticleModel = TechStockMaui.Models.TypeArticle.TypeArticle;

namespace TechStockMaui.Views.TypeArticle
{
    public partial class DetailsTypeArticlePage : ContentPage
    {
        private TypeArticleModel _typeArticle;
        private TypeArticleService _typeArticleService;

        public DetailsTypeArticlePage(TypeArticleModel typeArticle)
        {
            InitializeComponent();
            _typeArticle = typeArticle;
            _typeArticleService = new TypeArticleService();

            // Charger les données du type d'article
            LoadTypeArticleDetails();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Recharger les données depuis l'API au cas où elles auraient été modifiées
            await RefreshTypeArticleDetails();
        }

        private void LoadTypeArticleDetails()
        {
            if (_typeArticle != null)
            {
                NameLabel.Text = _typeArticle.Name ?? "Non défini";
            }
        }

        private async Task RefreshTypeArticleDetails()
        {
            try
            {
                var updatedTypeArticle = await _typeArticleService.GetByIdAsync(_typeArticle.Id);
                if (updatedTypeArticle != null)
                {
                    _typeArticle = updatedTypeArticle;
                    LoadTypeArticleDetails();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible de recharger les détails: {ex.Message}", "OK");
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            try
            {
                // Naviguer vers la page de modification en passant le type d'article
                await Navigation.PushAsync(new EditTypeArticlePage(_typeArticle));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible d'ouvrir la page de modification: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}