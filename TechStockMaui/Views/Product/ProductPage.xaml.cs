using TechStockMaui.Models; // Assure-toi que ce namespace est correct
using TechStockMaui.Views.MaterialManagements;

namespace TechStockMaui.Views;

public partial class ProductPage : ContentPage
{
    public ProductPage()
    {
        InitializeComponent();
    }

    private void OnSearchClicked(object sender, EventArgs e)
    {
        // Impl�mente la logique de recherche ici
        DisplayAlert("Recherche", "Fonction de recherche appel�e", "OK");
    }

    private void OnResetClicked(object sender, EventArgs e)
    {
        // R�initialiser les filtres
        NameEntry.Text = string.Empty;
        SerialEntry.Text = string.Empty;
        TypePicker.SelectedItem = null;
        SupplierPicker.SelectedItem = null;
        UserPicker.SelectedItem = null;
    }

    private void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Product product)
        {
            DisplayAlert("Modifier", $"Modifier le produit : {product.Name}", "OK");
            // Navigation vers une page de modification possible ici
        }
    }

    private void OnDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Product product)
        {
            DisplayAlert("D�tails", $"Nom : {product.Name}\nS�rie : {product.SerialNumber}", "OK");
        }
    }

    private async void OnAssignClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Product product)
        {
            await Navigation.PushAsync(new AssignProductPage(product));
        }
    }

    private async void OnUnassignClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Product product)
        {
            bool confirm = await DisplayAlert("Confirmation", $"D�sassigner le produit {product.Name} ?", "Oui", "Non");
            if (confirm)
            {
                // Appel API ou logique pour d�sassigner
                await DisplayAlert("D�sassign�", $"Produit {product.Name} d�sassign�", "OK");
            }
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Product product)
        {
            bool confirm = await DisplayAlert("Confirmation", $"Supprimer le produit {product.Name} ?", "Oui", "Non");
            if (confirm)
            {
                // Appel API pour suppression
                await DisplayAlert("Supprim�", $"Produit {product.Name} supprim�", "OK");
            }
        }
    }
}
