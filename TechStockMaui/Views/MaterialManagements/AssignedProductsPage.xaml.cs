using System.Collections.ObjectModel;
using TechStockMaui.Models;
using TechStockMaui.Services;

namespace TechStockMaui.Views.MaterialManagements
{
    public partial class AssignedProductsPage : ContentPage
    {
        private MaterialManagementService _materialManagementService;
        private ObservableCollection<MaterialManagement> _assignments;

        public AssignedProductsPage()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 AssignedProductsPage constructeur - DÉBUT");

                InitializeComponent();
                _materialManagementService = new MaterialManagementService();
                _assignments = new ObservableCollection<MaterialManagement>();
                ProductsCollectionView.ItemsSource = _assignments;

                System.Diagnostics.Debug.WriteLine("✅ AssignedProductsPage constructeur - FIN");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur constructeur AssignedProductsPage: {ex.Message}");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("🔄 AssignedProductsPage apparue - chargement automatique");
            await LoadAssignedProductsAsync();
        }

        private async Task LoadAssignedProductsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Chargement des produits assignés...");

                // ✅ Utiliser votre service existant
                var assignments = await _materialManagementService.GetMyAssignmentsAsync();

                _assignments.Clear();
                if (assignments != null && assignments.Any())
                {
                    foreach (var assignment in assignments)
                    {
                        _assignments.Add(assignment);
                        System.Diagnostics.Debug.WriteLine($"🔍 Assignment ajouté: Produit={assignment.Product?.Name}, Signé={assignment.IsSignatureValid}");
                    }
                    System.Diagnostics.Debug.WriteLine($"✅ {_assignments.Count} assignments chargés");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Aucun produit assigné trouvé");
                }

                // Optionnel : Afficher un message si aucun produit
                if (!_assignments.Any())
                {
                    await DisplayAlert("Information", "Aucun produit ne vous est actuellement assigné.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur LoadAssignedProductsAsync: {ex.Message}");
                await DisplayAlert("Erreur", $"Impossible de charger les produits assignés: {ex.Message}", "OK");
            }
        }

        private async void OnSignClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 OnSignClicked - DÉBUT");

                if (sender is Button button && button.CommandParameter is int assignmentId)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Assignment ID: {assignmentId}");

                    // Récupérer l'assignment correspondant
                    var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId);
                    if (assignment == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Assignment {assignmentId} introuvable");
                        await DisplayAlert("Erreur", "Assignment introuvable", "OK");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ Assignment trouvé: {assignment.Product?.Name}");

                    // ✅ Utiliser vos propriétés calculées existantes
                    if (assignment.IsSignatureValid)
                    {
                        await DisplayAlert("Information", "Ce produit a déjà été signé.", "OK");
                        return;
                    }

                    // Demander confirmation
                    bool confirm = await DisplayAlert(
                        "Confirmation de réception",
                        $"Confirmez-vous avoir reçu le produit '{assignment.Product?.Name}' ?\n\nEn signant, vous attestez avoir pris possession de ce matériel.",
                        "Oui, je confirme",
                        "Annuler");

                    if (confirm)
                    {
                        System.Diagnostics.Debug.WriteLine("🔄 Utilisateur a confirmé, demande de signature...");

                        // Demander la signature
                        string signature = await GetUserSignature(assignment);

                        if (!string.IsNullOrEmpty(signature))
                        {
                            System.Diagnostics.Debug.WriteLine($"🔄 Signature reçue: {signature.Substring(0, Math.Min(20, signature.Length))}...");

                            // ✅ Utiliser votre service existant
                            bool success = await _materialManagementService.SignProductAsync(assignmentId, signature);

                            if (success)
                            {
                                System.Diagnostics.Debug.WriteLine("✅ Signature envoyée avec succès");
                                await DisplayAlert("Succès", "Produit signé avec succès! Merci de confirmer la réception.", "OK");

                                // Rafraîchir la liste
                                await LoadAssignedProductsAsync();
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("❌ Échec envoi signature");
                                await DisplayAlert("Erreur", "Échec de l'enregistrement de la signature. Veuillez réessayer.", "OK");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("⚠️ Signature annulée par l'utilisateur");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ Confirmation annulée par l'utilisateur");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Sender ou CommandParameter invalide");
                    await DisplayAlert("Erreur", "Erreur technique lors de la signature", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur OnSignClicked: {ex.Message}");
                await DisplayAlert("Erreur", $"Erreur lors de la signature: {ex.Message}", "OK");
            }

            System.Diagnostics.Debug.WriteLine("🔄 OnSignClicked - FIN");
        }

        private async Task<string> GetUserSignature(MaterialManagement assignment)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Demande de signature utilisateur...");

                // ✅ VERSION SIMPLE : Demander le nom complet comme signature
                string signature = await DisplayPromptAsync(
                    "Signature électronique",
                    $"Pour confirmer la réception de '{assignment.Product?.Name}', veuillez saisir votre nom complet :\n\n(Cette signature sera horodatée et enregistrée)",
                    "Confirmer",
                    "Annuler",
                    "Votre nom complet",
                    maxLength: 100);

                if (!string.IsNullOrWhiteSpace(signature))
                {
                    // ✅ Enrichir la signature avec des infos supplémentaires
                    var enrichedSignature = $"{signature.Trim()} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    System.Diagnostics.Debug.WriteLine($"✅ Signature créée: {enrichedSignature}");
                    return enrichedSignature;
                }

                System.Diagnostics.Debug.WriteLine("⚠️ Signature vide ou annulée");
                return null;

                // TODO FUTUR: Implémenter une vraie signature graphique
                // await Navigation.PushAsync(new SignaturePage(assignment));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetUserSignature: {ex.Message}");
                await DisplayAlert("Erreur", "Erreur lors de la capture de signature", "OK");
                return null;
            }
        }

        // ✅ MÉTHODE POUR ACTUALISER MANUELLEMENT
        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadAssignedProductsAsync();
        }

        // ✅ MÉTHODE POUR RETOURNER AU DASHBOARD
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}