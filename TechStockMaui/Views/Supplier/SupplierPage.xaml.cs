using System.Collections.ObjectModel;
using TechStockMaui.Models.Supplier;
using TechStockMaui.Services;

namespace TechStockMaui.Views.Supplier
{
    public partial class SupplierPage : ContentPage
    {
        private SupplierService _supplierService; // Changé de ProductService
        private ObservableCollection<TechStockMaui.Models.Supplier.Supplier> _suppliers;

        public SupplierPage()
        {
            InitializeComponent();
            _supplierService = new SupplierService(); // Nouveau service dédié
            _suppliers = new ObservableCollection<TechStockMaui.Models.Supplier.Supplier>();
            SupplierList.ItemsSource = _suppliers;
        }

       

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadSuppliersAsync();
        }

        private async Task LoadSuppliersAsync()
        {
            try
            {
                var suppliers = await _supplierService.GetSuppliersAsync(); // Utilise SupplierService

                _suppliers.Clear();
                foreach (var supplier in suppliers)
                {
                    _suppliers.Add(supplier);
                }

                // Afficher/masquer le message "aucun fournisseur"
                NoSuppliersLabel.IsVisible = !_suppliers.Any();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible de charger les fournisseurs: {ex.Message}", "OK");
            }
        }

        private async void OnBackToDashboardClicked(object sender, EventArgs e)
        {
            try
            {
                // Utiliser PopAsync() pour retourner à la page précédente
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible de retourner en arrière: {ex.Message}", "OK");
            }
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new CreateSupplierPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible d'ouvrir la page de création: {ex.Message}", "OK");
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is TechStockMaui.Models.Supplier.Supplier supplier)
                {
                    // Navigation vers EditSupplierPage en passant le fournisseur
                    await Navigation.PushAsync(new EditSupplierPage(supplier));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible d'ouvrir la page de modification: {ex.Message}", "OK");
            }
        }

        private async void OnDetailsClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is TechStockMaui.Models.Supplier.Supplier supplier)
                {
                    // Navigation vers DetailsSupplierPage en passant le fournisseur
                    await Navigation.PushAsync(new DetailsSupplierPage(supplier));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible d'ouvrir les détails: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is TechStockMaui.Models.Supplier.Supplier supplier)
                {
                    bool confirm = await DisplayAlert(
                        "Confirmation",
                        $"Êtes-vous sûr de vouloir supprimer le fournisseur '{supplier.Name}' ?",
                        "Oui",
                        "Non");

                    if (confirm)
                    {
                        // Appel de l'API pour supprimer le fournisseur
                        bool success = await _supplierService.DeleteSupplierAsync(supplier.Id);

                        if (success)
                        {
                            // Retirer de la liste locale
                            _suppliers.Remove(supplier);
                            await DisplayAlert("Succès", "Fournisseur supprimé avec succès", "OK");
                        }
                        else
                        {
                            await DisplayAlert("Erreur", "Échec de la suppression du fournisseur", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible de supprimer le fournisseur: {ex.Message}", "OK");
            }
        }
    }
}