using Microsoft.Maui.Controls;
using System;
using TechStockMaui.Models;

namespace TechStockMaui.Views
{
    public partial class EditProductPage : ContentPage
    {
        private Product _product;

        public EditProductPage(Product product)
        {
            InitializeComponent();
            _product = product;
            BindingContext = _product; 
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            // Logique d'enregistrement ici
        }

        private void OnBackClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}
