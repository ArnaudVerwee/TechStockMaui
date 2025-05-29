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

            // Charger les donn�es des pickers
            LoadData();
        }

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

                // S�lectionner les items actuels si ils existent
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
                await DisplayAlert("Erreur", $"Impossible de charger les donn�es: {ex.Message}", "OK");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // R�cup�rer les valeurs des contr�les
                _product.Name = NameEntry.Text;
                _product.SerialNumber = SerialNumberEntry.Text;

                // R�cup�rer les s�lections des pickers
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
                    await DisplayAlert("Succ�s", "Produit modifi� avec succ�s!", "OK");

                    // Recharger le produit depuis l'API pour avoir les donn�es fra�ches
                    var updatedProduct = await _productService.GetProductByIdAsync(_product.Id);
                    if (updatedProduct != null)
                    {
                        // Remplacer compl�tement l'objet _product
                        _product = updatedProduct;
                    }

                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Erreur", "�chec de la modification du produit", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Une erreur s'est produite: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}