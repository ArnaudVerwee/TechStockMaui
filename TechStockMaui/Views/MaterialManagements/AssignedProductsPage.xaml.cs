using System.Collections.ObjectModel;
using TechStockMaui.Models; // Chang� le namespace
using TechStockMaui.Services;

namespace TechStockMaui.Views.MaterialManagements
{
    public partial class AssignedProductsPage : ContentPage
    {
        private MaterialManagementService _materialManagementService;
        private ObservableCollection<MaterialManagement> _assignments;

        public AssignedProductsPage()
        {
            InitializeComponent();
            _materialManagementService = new MaterialManagementService();
            _assignments = new ObservableCollection<MaterialManagement>();
            ProductsCollectionView.ItemsSource = _assignments;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAssignedProductsAsync();
        }

        private async Task LoadAssignedProductsAsync()
        {
            try
            {
                // Charger les produits assign�s � l'utilisateur connect�
                var assignments = await _materialManagementService.GetMyAssignmentsAsync();

                _assignments.Clear();
                foreach (var assignment in assignments)
                {
                    _assignments.Add(assignment);
                }

                // Commenter cette ligne si NoProductsLabel n'existe pas dans votre XAML
                // NoProductsLabel.IsVisible = !_assignments.Any();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible de charger les produits assign�s: {ex.Message}", "OK");
            }
        }

        private async void OnSignClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is int assignmentId)
                {
                    // R�cup�rer l'assignment correspondant
                    var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId);
                    if (assignment == null)
                    {
                        await DisplayAlert("Erreur", "Assignment introuvable", "OK");
                        return;
                    }

                    // Demander confirmation
                    bool confirm = await DisplayAlert(
                        "Signature",
                        $"Confirmez-vous avoir re�u le produit '{assignment.Product?.Name}' ?",
                        "Oui",
                        "Non");

                    if (confirm)
                    {
                        // G�n�rer une signature simple (ou ouvrir une page de signature)
                        string signature = await GetUserSignature(assignment);

                        if (!string.IsNullOrEmpty(signature))
                        {
                            // Envoyer la signature � l'API
                            bool success = await _materialManagementService.SignProductAsync(assignmentId, signature);

                            if (success)
                            {
                                await DisplayAlert("Succ�s", "Produit sign� avec succ�s!", "OK");

                                // Rafra�chir la liste
                                await LoadAssignedProductsAsync();
                            }
                            else
                            {
                                await DisplayAlert("Erreur", "�chec de la signature", "OK");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Erreur lors de la signature: {ex.Message}", "OK");
            }
        }

        private async Task<string> GetUserSignature(MaterialManagement assignment)
        {
            // Version simple : demander le nom de l'utilisateur comme signature
            string signature = await DisplayPromptAsync(
                "Signature",
                $"Entrez votre nom pour confirmer la r�ception de '{assignment.Product?.Name}':",
                "OK",
                "Annuler",
                "Votre nom");

            // TODO: Impl�menter une vraie signature graphique avec une page d�di�e
            // await Navigation.PushAsync(new SignaturePage(assignment));

            return signature;
        }
    }
}