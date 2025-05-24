using TechStockMaui.Models;
using TechStockMaui.Services;
using System.Collections.ObjectModel;

namespace TechStockMaui.Views;

public partial class AssignProductPage : ContentPage
{
    public Product Product { get; }
    public ObservableCollection<User> Users { get; } = new();
    public ObservableCollection<States> States { get; } = new();

    public User SelectedUser { get; set; }
    public States SelectedState { get; set; }

    public Command AssignCommand { get; }

    private readonly MaterialManagementService _service;

    public AssignProductPage(Product product)
    {
        InitializeComponent();
        Product = product;
        _service = new MaterialManagementService();

        AssignCommand = new Command(async () => await AssignProductToUser());

        BindingContext = this;

        LoadUsersAndStates();
    }

    private void InitializeComponent()
    {
        throw new NotImplementedException();
    }

    private async void LoadUsersAndStates()
    {
        var users = await _service.GetUsersAsync();
        foreach (var u in users)
            Users.Add(u);

        var states = await _service.GetStatesAsync();
        foreach (var s in states)
            States.Add(s);
    }

    private async Task AssignProductToUser()
    {
        if (SelectedUser == null || SelectedState == null)
        {
            await DisplayAlert("Erreur", "Veuillez sélectionner un utilisateur et un état.", "OK");
            return;
        }

        bool success = await _service.AssignProductAsync(Product.Id, SelectedUser.Id, SelectedState.Id);

        if (success)
        {
            await DisplayAlert("Succès", "Produit assigné avec succès.", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Erreur", "Erreur lors de l'assignation.", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
