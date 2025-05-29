using TechStockMaui.Services;
using TechStockMaui.Models.Supplier;

namespace TechStockMaui.Views.Supplier
{
    public partial class DetailsSupplierPage : ContentPage
    {
        private SupplierService _supplierService;
        private TechStockMaui.Models.Supplier.Supplier _supplier;

        public DetailsSupplierPage(TechStockMaui.Models.Supplier.Supplier supplier)
        {
            InitializeComponent();
            _supplierService = new SupplierService();
            _supplier = supplier;

            // Charger les données du fournisseur
            LoadSupplierDetails();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Recharger les données du fournisseur depuis l'API au cas où elles auraient été modifiées
            await RefreshSupplierDetails();
        }

        private void LoadSupplierDetails()
        {
            if (_supplier != null)
            {
                NameLabel.Text = _supplier.Name ?? "Non défini";
            }
        }

        private async Task RefreshSupplierDetails()
        {
            try
            {
                var updatedSupplier = await _supplierService.GetSupplierByIdAsync(_supplier.Id);
                if (updatedSupplier != null)
                {
                    _supplier = updatedSupplier;
                    LoadSupplierDetails();
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
                // Naviguer vers la page de modification en passant le fournisseur
                await Navigation.PushAsync(new EditSupplierPage(_supplier));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible d'ouvrir la page de modification: {ex.Message}", "OK");
            }
        }

        private async void OnBackToListClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}