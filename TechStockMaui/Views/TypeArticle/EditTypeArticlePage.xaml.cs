using TechStockMaui.Services;
using TypeArticleModel = TechStockMaui.Models.TypeArticle.TypeArticle;

namespace TechStockMaui.Views.TypeArticle
{
    public partial class EditTypeArticlePage : ContentPage
    {
        private TypeArticleModel _typeArticle;
        private TypeArticleService _typeArticleService;

        public EditTypeArticlePage(TypeArticleModel typeArticle)
        {
            InitializeComponent();
            _typeArticle = typeArticle;
            _typeArticleService = new TypeArticleService();

            // Pr�-remplir les champs avec les donn�es existantes
            LoadTypeArticleData();
        }

        private void LoadTypeArticleData()
        {
            if (_typeArticle != null)
            {
                NameEntry.Text = _typeArticle.Name;
            }
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

                // Mettre � jour l'objet
                _typeArticle.Name = name;

                // Appeler l'API pour sauvegarder les modifications
                bool success = await _typeArticleService.UpdateAsync(_typeArticle);

                if (success)
                {
                    await DisplayAlert("Succ�s", $"Type d'article '{name}' modifi� avec succ�s!", "OK");

                    // Retourner � la page pr�c�dente
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Erreur", "�chec de la modification du type d'article", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Une erreur s'est produite: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            // Demander confirmation si des modifications ont �t� faites
            if (HasChanges())
            {
                bool confirmExit = await DisplayAlert(
                    "Modifications non sauvegard�es",
                    "Vous avez des modifications non sauvegard�es. Voulez-vous vraiment quitter ?",
                    "Oui",
                    "Non");

                if (!confirmExit)
                    return;
            }

            await Navigation.PopAsync();
        }

        private bool HasChanges()
        {
            return _typeArticle.Name != NameEntry.Text?.Trim();
        }
    }
}