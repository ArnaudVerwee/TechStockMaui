using System.Collections.ObjectModel;
using TechStockMaui.Models; // Changé le namespace
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
                // Charger les produits assignés à l'utilisateur connecté
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
                await DisplayAlert("Erreur", $"Impossible de charger les produits assignés: {ex.Message}", "OK");
            }
        }

        private async void OnSignClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is int assignmentId)
                {
                    // Récupérer l'assignment correspondant
                    var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId);
                    if (assignment == null)
                    {
                        await DisplayAlert("Erreur", "Assignment introuvable", "OK");
                        return;
                    }

                    // Demander confirmation
                    bool confirm = await DisplayAlert(
                        "Signature",
                        $"Confirmez-vous avoir reçu le produit '{assignment.Product?.Name}' ?",
                        "Oui",
                        "Non");

                    if (confirm)
                    {
                        // Générer une signature simple (ou ouvrir une page de signature)
                        string signature = await GetUserSignature(assignment);

                        if (!string.IsNullOrEmpty(signature))
                        {
                            // Envoyer la signature à l'API
                            bool success = await _materialManagementService.SignProductAsync(assignmentId, signature);

                            if (success)
                            {
                                await DisplayAlert("Succès", "Produit signé avec succès!", "OK");

                                // Rafraîchir la liste
                                await LoadAssignedProductsAsync();
                            }
                            else
                            {
                                await DisplayAlert("Erreur", "Échec de la signature", "OK");
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
                $"Entrez votre nom pour confirmer la réception de '{assignment.Product?.Name}':",
                "OK",
                "Annuler",
                "Votre nom");

            // TODO: Implémenter une vraie signature graphique avec une page dédiée
            // await Navigation.PushAsync(new SignaturePage(assignment));

            return signature;
        }
    }
}