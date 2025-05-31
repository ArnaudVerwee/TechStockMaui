using TechStockMaui.Services;
using TechStockMaui.Models;
using System.Collections.ObjectModel;

namespace TechStockMaui.Views
{
    public partial class CreateProductPage : ContentPage
    {
        private readonly ProductService _productService;
        private readonly SupplierService _supplierService;
        private readonly TypeArticleService _typeArticleService;

        public CreateProductPage()
        {
            InitializeComponent();
            _productService = new ProductService();
            _supplierService = new SupplierService();
            _typeArticleService = new TypeArticleService();
        }

        // Charger les données quand la page apparaît
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPickerDataAsync();
        }

        private async Task LoadPickerDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Chargement des données pour les Pickers...");

                // Afficher un indicateur de chargement si vous en avez un
                // LoadingIndicator.IsVisible = true;

                // Charger les fournisseurs
                System.Diagnostics.Debug.WriteLine("📞 Appel GetSuppliersAsync...");
                var suppliers = await _supplierService.GetSuppliersAsync();
                if (suppliers != null && suppliers.Any())
                {
                    SupplierPicker.ItemsSource = suppliers.ToList();
                    SupplierPicker.ItemDisplayBinding = new Binding("Name");
                    System.Diagnostics.Debug.WriteLine($"✅ {suppliers.Count()} fournisseurs chargés");

                    // Debug: afficher les noms des fournisseurs
                    foreach (var supplier in suppliers.Take(3))
                    {
                        System.Diagnostics.Debug.WriteLine($"   📦 Fournisseur: {supplier.Name} (ID: {supplier.Id})");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Aucun fournisseur trouvé");
                    await DisplayAlert("Attention", "Aucun fournisseur disponible", "OK");
                }

                // Charger les types
                System.Diagnostics.Debug.WriteLine("📞 Appel GetAllAsync...");
                var types = await _typeArticleService.GetAllAsync();
                if (types != null && types.Any())
                {
                    TypePicker.ItemsSource = types.ToList();
                    TypePicker.ItemDisplayBinding = new Binding("Name");
                    System.Diagnostics.Debug.WriteLine($"✅ {types.Count()} types chargés");

                    // Debug: afficher les noms des types
                    foreach (var type in types.Take(3))
                    {
                        System.Diagnostics.Debug.WriteLine($"   🏷️ Type: {type.Name} (ID: {type.Id})");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Aucun type trouvé");
                    await DisplayAlert("Attention", "Aucun type disponible", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur lors du chargement des données: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"📍 StackTrace: {ex.StackTrace}");
                await DisplayAlert("Erreur", $"Impossible de charger les données: {ex.Message}", "OK");
            }
            finally
            {
                // Masquer l'indicateur de chargement
                // LoadingIndicator.IsVisible = false;
            }
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 Début de création du produit");

                // Validation des champs
                if (string.IsNullOrWhiteSpace(NameEntry.Text))
                {
                    await DisplayAlert("Erreur", "Le nom du produit est requis", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(SerialNumberEntry.Text))
                {
                    await DisplayAlert("Erreur", "Le numéro de série est requis", "OK");
                    return;
                }

                if (TypePicker.SelectedItem == null)
                {
                    await DisplayAlert("Erreur", "Veuillez sélectionner un type", "OK");
                    return;
                }

                if (SupplierPicker.SelectedItem == null)
                {
                    await DisplayAlert("Erreur", "Veuillez sélectionner un fournisseur", "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("✅ Validation réussie");

                var selectedType = TypePicker.SelectedItem as TechStockMaui.Models.TypeArticle.TypeArticle;
                var selectedSupplier = SupplierPicker.SelectedItem as TechStockMaui.Models.Supplier.Supplier;

                System.Diagnostics.Debug.WriteLine($"🔍 Type sélectionné: {selectedType?.Name} (ID: {selectedType?.Id})");
                System.Diagnostics.Debug.WriteLine($"🔍 Fournisseur sélectionné: {selectedSupplier?.Name} (ID: {selectedSupplier?.Id})");

                var newProduct = new Product
                {
                    Name = NameEntry.Text,
                    SerialNumber = SerialNumberEntry.Text,
                    TypeId = selectedType?.Id ?? 0,
                    SupplierId = selectedSupplier?.Id ?? 0
                    // Pas d'objets complets - juste les IDs
                };

                System.Diagnostics.Debug.WriteLine($"🆕 Création du produit: {newProduct.Name}");
                System.Diagnostics.Debug.WriteLine($"📋 TypeId: {newProduct.TypeId}, SupplierId: {newProduct.SupplierId}");
                System.Diagnostics.Debug.WriteLine($"🔢 SerialNumber: {newProduct.SerialNumber}");

                System.Diagnostics.Debug.WriteLine("📞 Appel CreateProductAsync...");
                bool result = await _productService.CreateProductAsync(newProduct);
                System.Diagnostics.Debug.WriteLine($"📊 Résultat CreateProductAsync: {result}");

                if (result)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Produit créé avec succès");
                    await DisplayAlert("Succès", "Produit créé avec succès", "OK");

                    // SOLUTION: Envoyer un message pour rafraîchir la liste
                    MessagingCenter.Send(this, "ProductCreated");

                    await Navigation.PopAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Échec de la création");
                    await DisplayAlert("Erreur", "La création du produit a échoué", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception création produit: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"📍 StackTrace: {ex.StackTrace}");
                await DisplayAlert("Erreur", $"Erreur lors de la création: {ex.Message}", "OK");
            }
        }

        private async void OnBackToListClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}