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
        // Implémente la logique de recherche ici
        DisplayAlert("Recherche", "Fonction de recherche appelée", "OK");
    }

    private void OnResetClicked(object sender, EventArgs e)
    {
        // Réinitialiser les filtres
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
            DisplayAlert("Détails", $"Nom : {product.Name}\nSérie : {product.SerialNumber}", "OK");
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
            bool confirm = await DisplayAlert("Confirmation", $"Désassigner le produit {product.Name} ?", "Oui", "Non");
            if (confirm)
            {
                // Appel API ou logique pour désassigner
                await DisplayAlert("Désassigné", $"Produit {product.Name} désassigné", "OK");
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
                await DisplayAlert("Supprimé", $"Produit {product.Name} supprimé", "OK");
            }
        }
    }
}
