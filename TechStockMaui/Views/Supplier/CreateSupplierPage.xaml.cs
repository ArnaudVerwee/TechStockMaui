using TechStockMaui.Services;
using TechStockMaui.Models.Supplier;

namespace TechStockMaui.Views.Supplier
{
    public partial class CreateSupplierPage : ContentPage
    {
        private SupplierService _supplierService; // Changé de ProductService

        public CreateSupplierPage()
        {
            InitializeComponent();
            _supplierService = new SupplierService(); // Nouveau service dédié
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            try
            {
                // Valider les données
                string name = NameEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    await DisplayAlert("Erreur", "Le nom du fournisseur est obligatoire.", "OK");
                    return;
                }

                // Créer l'objet Supplier
                var newSupplier = new TechStockMaui.Models.Supplier.Supplier
                {
                    Name = name
                    // Ajoutez d'autres propriétés si nécessaire (Email, etc.)
                };

                // Appeler l'API pour créer le fournisseur
                bool success = await _supplierService.CreateSupplierAsync(newSupplier);

                if (success)
                {
                    await DisplayAlert("Succès", $"Fournisseur '{name}' créé avec succès!", "OK");

                    // Retourner à la page précédente (SupplierPage)
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Erreur", "Échec de la création du fournisseur", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Une erreur s'est produite: {ex.Message}", "OK");
            }
        }

        private async void OnBackToListClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}