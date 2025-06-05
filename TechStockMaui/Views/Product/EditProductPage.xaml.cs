using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using TechStockMaui.Models;
using TechStockMaui.Services;

namespace TechStockMaui.Views
{
    public partial class EditProductPage : ContentPage
    {
        private Product _product;
        private ProductService _productService;
        private List<TechStockMaui.Models.TypeArticle.TypeArticle> _types;
        private List<TechStockMaui.Models.Supplier.Supplier> _suppliers;

        public EditProductPage(Product product)
        {
            InitializeComponent();
            _product = product;
            _productService = new ProductService();

            // Bind le produit pour les champs Name et SerialNumber
            BindingContext = _product;

            // ✅ AJOUT: S'abonner aux changements de langue
            TranslationService.Instance.CultureChanged += OnCultureChanged;

            // ✅ CONSERVÉ: Votre logique existante
            LoadData();
        }

        // ✅ AJOUT: Charger les traductions au démarrage
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTranslationsAsync();
        }

        // ✅ AJOUT: Charger les traductions
        private async Task LoadTranslationsAsync()
        {
            try
            {
                var currentCulture = TranslationService.Instance.GetCurrentCulture();
                await TranslationService.Instance.LoadTranslationsAsync(currentCulture);
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur chargement traductions: {ex.Message}");
            }
        }

        // ✅ AJOUT: Helper pour récupérer une traduction
        private async Task<string> GetTextAsync(string key, string fallback = null)
        {
            try
            {
                var text = await TranslationService.Instance.GetTranslationAsync(key);
                return !string.IsNullOrEmpty(text) && text != key ? text : (fallback ?? key);
            }
            catch
            {
                return fallback ?? key;
            }
        }

        // ✅ AJOUT: Mettre à jour tous les textes de l'interface
        private async Task UpdateTextsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🌍 Mise à jour des textes EditProduct");

                // ✅ Titre de la page
                Title = await GetTextAsync("Edit", "Edit");

                // ✅ Titre principal
                if (TitleLabel != null)
                    TitleLabel.Text = await GetTextAsync("Edit", "Edit Product");

                // ✅ Labels des champs
                if (NameLabel != null)
                    NameLabel.Text = await GetTextAsync("Name", "Name") + ":";

                if (SerialNumberLabel != null)
                    SerialNumberLabel.Text = await GetTextAsync("Serial Number", "Serial Number") + ":";

                if (TypeLabel != null)
                    TypeLabel.Text = await GetTextAsync("Type", "Type") + ":";

                if (SupplierLabel != null)
                    SupplierLabel.Text = await GetTextAsync("Supplier", "Supplier") + ":";

                // ✅ Placeholders
                if (NameEntry != null)
                    NameEntry.Placeholder = await GetTextAsync("ProductNamePlaceholder", "Product name");

                if (SerialNumberEntry != null)
                    SerialNumberEntry.Placeholder = await GetTextAsync("SerialNumberPlaceholder", "Serial number");

                // ✅ Titles des Pickers
                if (TypePicker != null)
                    TypePicker.Title = await GetTextAsync("Select a type", "Select a type");

                if (SupplierPicker != null)
                    SupplierPicker.Title = await GetTextAsync("Select a supplier", "Select a supplier");

                // ✅ Boutons
                if (SaveButton != null)
                    SaveButton.Text = await GetTextAsync("Save", "Save");

                if (BackButton != null)
                    BackButton.Text = await GetTextAsync("Back to List", "Back to List");

                // ✅ Sélecteur de langue
                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                // ✅ Mettre à jour l'indicateur de langue
                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("✅ Textes EditProduct mis à jour");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur UpdateTextsAsync: {ex.Message}");
            }
        }

        // ✅ AJOUT: Callback quand la langue change
        private async void OnCultureChanged(object sender, string newCulture)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🌍 EditProduct - Langue changée vers: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private async void LoadData()
        {
            try
            {
                // Charger les types et fournisseurs
                _types = await _productService.GetTypesAsync();
                _suppliers = await _productService.GetSuppliersAsync();

                // Assigner aux pickers
                TypePicker.ItemsSource = _types;
                SupplierPicker.ItemsSource = _suppliers;

                // Sélectionner les items actuels si ils existent
                if (_product.TypeId > 0)
                {
                    var selectedType = _types.Find(t => t.Id == _product.TypeId);
                    TypePicker.SelectedItem = selectedType;
                }

                if (_product.SupplierId > 0)
                {
                    var selectedSupplier = _suppliers.Find(s => s.Id == _product.SupplierId);
                    SupplierPicker.SelectedItem = selectedSupplier;
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var dataLoadError = await GetTextAsync("DataLoadError", "Impossible de charger les données");
                await DisplayAlert(errorTitle, $"{dataLoadError}: {ex.Message}", "OK");
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // ✅ CONSERVÉ: Votre logique existante
                _product.Name = NameEntry.Text;
                _product.SerialNumber = SerialNumberEntry.Text;

                // Récupérer les sélections des pickers
                if (TypePicker.SelectedItem is TechStockMaui.Models.TypeArticle.TypeArticle selectedType)
                {
                    _product.TypeId = selectedType.Id;
                    // Ne pas assigner TypeName car c'est readonly
                }

                if (SupplierPicker.SelectedItem is TechStockMaui.Models.Supplier.Supplier selectedSupplier)
                {
                    _product.SupplierId = selectedSupplier.Id;
                    // Ne pas assigner SupplierName car c'est readonly
                }

                // Sauvegarder via l'API
                bool success = await _productService.UpdateProductAsync(_product);

                if (success)
                {
                    var successTitle = await GetTextAsync("Success", "Succès");
                    var productModifiedMsg = await GetTextAsync("ProductModifiedSuccessfully", "Produit modifié avec succès!");
                    await DisplayAlert(successTitle, productModifiedMsg, "OK");

                    // ✅ CONSERVÉ: Votre logique de rechargement
                    var updatedProduct = await _productService.GetProductByIdAsync(_product.Id);
                    if (updatedProduct != null)
                    {
                        // Remplacer complètement l'objet _product
                        _product = updatedProduct;
                    }

                    await Navigation.PopAsync();
                }
                else
                {
                    var errorTitle = await GetTextAsync("Error", "Erreur");
                    var modificationFailedMsg = await GetTextAsync("ProductModificationFailed", "Échec de la modification du produit");
                    await DisplayAlert(errorTitle, modificationFailedMsg, "OK");
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var errorOccurredMsg = await GetTextAsync("ErrorOccurred", "Une erreur s'est produite");
                await DisplayAlert(errorTitle, $"{errorOccurredMsg}: {ex.Message}", "OK");
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // ✅ AJOUT: Gestion du changement de langue
        private async void OnLanguageClicked(object sender, EventArgs e)
        {
            try
            {
                var translationService = TranslationService.Instance;
                var currentCulture = translationService.GetCurrentCulture();

                var options = new List<string>();
                foreach (var culture in translationService.GetSupportedCultures())
                {
                    var flag = translationService.GetLanguageFlag(culture);
                    var name = translationService.GetLanguageDisplayName(culture);
                    var current = culture == currentCulture ? " ✓" : "";
                    options.Add($"{flag} {name}{current}");
                }

                var cancelText = await GetTextAsync("Cancel", "Cancel");
                var titleText = "🌍 " + await GetTextAsync("ChooseLanguage", "Choose language");

                var selectedOption = await DisplayActionSheet(titleText, cancelText, null, options.ToArray());

                if (!string.IsNullOrEmpty(selectedOption) && selectedOption != cancelText)
                {
                    string newCulture = null;
                    if (selectedOption.Contains("EN")) newCulture = "en";
                    else if (selectedOption.Contains("FR")) newCulture = "fr";
                    else if (selectedOption.Contains("NL")) newCulture = "nl";

                    if (newCulture != null && newCulture != currentCulture)
                    {
                        System.Diagnostics.Debug.WriteLine($"🌍 Changement vers: {newCulture}");
                        await translationService.SetCurrentCultureAsync(newCulture);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        // ✅ AJOUT: Mettre à jour le drapeau de langue
        private async Task UpdateLanguageFlag()
        {
            try
            {
                var translationService = TranslationService.Instance;
                var currentCulture = translationService.GetCurrentCulture();
                var flag = translationService.GetLanguageFlag(currentCulture);

                if (LanguageFlag != null)
                    LanguageFlag.Text = flag;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur mise à jour drapeau: {ex.Message}");
            }
        }

        // ✅ AJOUT: Nettoyage
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        // ✅ AJOUT: Destructeur
        ~EditProductPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}