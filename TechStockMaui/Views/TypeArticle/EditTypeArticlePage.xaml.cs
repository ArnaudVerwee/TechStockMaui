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

            // Pré-remplir les champs avec les données existantes
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

                // Mettre à jour l'objet
                _typeArticle.Name = name;

                // Appeler l'API pour sauvegarder les modifications
                bool success = await _typeArticleService.UpdateAsync(_typeArticle);

                if (success)
                {
                    await DisplayAlert("Succès", $"Type d'article '{name}' modifié avec succès!", "OK");

                    // Retourner à la page précédente
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Erreur", "Échec de la modification du type d'article", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Une erreur s'est produite: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            // Demander confirmation si des modifications ont été faites
            if (HasChanges())
            {
                bool confirmExit = await DisplayAlert(
                    "Modifications non sauvegardées",
                    "Vous avez des modifications non sauvegardées. Voulez-vous vraiment quitter ?",
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