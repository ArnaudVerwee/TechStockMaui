using TechStockMaui.Services;
using TechStockMaui.Models.Supplier;

namespace TechStockMaui.Views.Supplier
{
    public partial class CreateSupplierPage : ContentPage
    {
        private SupplierService _supplierService; // Chang� de ProductService

        public CreateSupplierPage()
        {
            InitializeComponent();
            _supplierService = new SupplierService(); // Nouveau service d�di�
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            try
            {
                // Valider les donn�es
                string name = NameEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    await DisplayAlert("Erreur", "Le nom du fournisseur est obligatoire.", "OK");
                    return;
                }

                // Cr�er l'objet Supplier
                var newSupplier = new TechStockMaui.Models.Supplier.Supplier
                {
                    Name = name
                    // Ajoutez d'autres propri�t�s si n�cessaire (Email, etc.)
                };

                // Appeler l'API pour cr�er le fournisseur
                bool success = await _supplierService.CreateSupplierAsync(newSupplier);

                if (success)
                {
                    await DisplayAlert("Succ�s", $"Fournisseur '{name}' cr�� avec succ�s!", "OK");

                    // Retourner � la page pr�c�dente (SupplierPage)
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Erreur", "�chec de la cr�ation du fournisseur", "OK");
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