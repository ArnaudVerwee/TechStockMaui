using TechStockMaui.Services;

namespace TechStockMaui.Views
{
    public partial class CreateTypeArticlePage : ContentPage
    {
        private TypeArticleService _typeArticleService;

        public CreateTypeArticlePage()
        {
            InitializeComponent();
            _typeArticleService = new TypeArticleService();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // Validation
                string name = NameEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    await DisplayAlert("Erreur", "Le nom du type d'article est obligatoire.", "OK");
                    return;
                }

                // Créer l'objet TypeArticle
                var newTypeArticle = new TechStockMaui.Models.TypeArticle.TypeArticle
                {
                    Name = name
                };

                // Appeler l'API pour créer le type d'article
                bool success = await _typeArticleService.CreateAsync(newTypeArticle);

                if (success)
                {
                    await DisplayAlert("Succès", $"Type d'article '{name}' créé avec succès!", "OK");

                    // Retourner à la page précédente (TypeArticlePage)
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Erreur", "Échec de la création du type d'article", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Une erreur s'est produite: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            // Demander confirmation si du texte a été saisi
            if (!string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                bool confirmExit = await DisplayAlert(
                    "Confirmation",
                    "Voulez-vous vraiment annuler ? Les données saisies seront perdues.",
                    "Oui",
                    "Non");

                if (!confirmExit)
                    return;
            }

            await Navigation.PopAsync();
        }
    }
}