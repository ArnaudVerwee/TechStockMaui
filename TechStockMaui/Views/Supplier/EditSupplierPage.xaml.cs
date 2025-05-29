using TechStockMaui.Services;
using TechStockMaui.Models.Supplier;

namespace TechStockMaui.Views.Supplier
{
    public partial class EditSupplierPage : ContentPage
    {
        private SupplierService _supplierService;
        private TechStockMaui.Models.Supplier.Supplier _supplier;

        public EditSupplierPage(TechStockMaui.Models.Supplier.Supplier supplier)
        {
            InitializeComponent();
            _supplierService = new SupplierService();
            _supplier = supplier;

            // Pr�-remplir les champs avec les donn�es existantes
            LoadSupplierData();
        }

        private void LoadSupplierData()
        {
            if (_supplier != null)
            {
                NameEntry.Text = _supplier.Name;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // Validation
                string supplierName = NameEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(supplierName))
                {
                    await DisplayAlert("Erreur", "Le nom du fournisseur est obligatoire.", "OK");
                    return;
                }

                // Mettre � jour l'objet supplier
                _supplier.Name = supplierName;

                // Appeler l'API pour sauvegarder les modifications
                bool success = await _supplierService.UpdateSupplierAsync(_supplier);

                if (success)
                {
                    await DisplayAlert("Succ�s", $"Le fournisseur '{supplierName}' a �t� modifi� avec succ�s!", "OK");

                    // Retourner � la page pr�c�dente
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Erreur", "�chec de la modification du fournisseur", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Une erreur s'est produite: {ex.Message}", "OK");
            }
        }

        private async void OnBackToListClicked(object sender, EventArgs e)
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
            return _supplier.Name != NameEntry.Text?.Trim();
        }
    }
}