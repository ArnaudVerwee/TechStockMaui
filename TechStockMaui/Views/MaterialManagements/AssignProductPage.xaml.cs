using TechStockMaui.Models;
using TechStockMaui.Services;
using System.Collections.ObjectModel;

namespace TechStockMaui.Views.MaterialManagements;

public partial class AssignProductPage : ContentPage
{
    public Product Product { get; }
    public ObservableCollection<User> Users { get; } = new();
    public ObservableCollection<States> States { get; } = new();
    public User SelectedUser { get; set; }
    public States SelectedState { get; set; }
    public Command AssignCommand { get; }

    private readonly MaterialManagementService _service;
    private readonly ProductService _productService;

    public AssignProductPage(Product product)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 AssignProductPage constructeur - DÉBUT");

            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("✅ InitializeComponent OK");

            Product = product;
            System.Diagnostics.Debug.WriteLine($"✅ Produit assigné: {product.Name}");

            System.Diagnostics.Debug.WriteLine("🔄 Création MaterialManagementService...");
            _service = new MaterialManagementService();
            System.Diagnostics.Debug.WriteLine("✅ MaterialManagementService créé");

            System.Diagnostics.Debug.WriteLine("🔄 Création ProductService...");
            _productService = new ProductService();
            System.Diagnostics.Debug.WriteLine("✅ ProductService créé");

            System.Diagnostics.Debug.WriteLine("🔄 Création Command...");
            AssignCommand = new Command(async () => await AssignProductToUser());
            System.Diagnostics.Debug.WriteLine("✅ Command créé");

            System.Diagnostics.Debug.WriteLine("🔄 BindingContext...");
            BindingContext = this;
            System.Diagnostics.Debug.WriteLine("✅ BindingContext assigné");

            System.Diagnostics.Debug.WriteLine("✅ AssignProductPage constructeur - FIN");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ERREUR constructeur AssignProductPage: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack constructeur: {ex.StackTrace}");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine("🔄 AssignProductPage apparue - chargement automatique");
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 LoadDataAsync - DÉBUT");

            // CHARGER UTILISATEURS
            System.Diagnostics.Debug.WriteLine("🔄 Chargement utilisateurs...");
            try
            {
                var users = await _productService.GetUsersAsync();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Users.Clear();
                    if (users != null && users.Any())
                    {
                        foreach (var user in users)
                        {
                            Users.Add(user);
                            System.Diagnostics.Debug.WriteLine($"🔍 Utilisateur ajouté: {user}");
                        }
                        System.Diagnostics.Debug.WriteLine($"✅ {Users.Count} utilisateurs ajoutés");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ Aucun utilisateur reçu");
                    }
                });
            }
            catch (Exception userEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur chargement utilisateurs: {userEx.Message}");
            }

            // CHARGER ÉTATS
            System.Diagnostics.Debug.WriteLine("🔄 Chargement états...");
            var defaultStates = new List<States>
            {
                new States { Id = 1, Status = "New Product" },
                new States { Id = 2, Status = "Old Product" },
                new States { Id = 3, Status = "Product to repair" },
                new States { Id = 4, Status = "Broken Product" }
            };

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                States.Clear();
                foreach (var state in defaultStates)
                {
                    States.Add(state);
                    System.Diagnostics.Debug.WriteLine($"🔍 État ajouté: {state.Status}");
                }
                System.Diagnostics.Debug.WriteLine($"✅ {States.Count} états ajoutés");
            });

            System.Diagnostics.Debug.WriteLine("✅ LoadDataAsync - FIN");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur LoadDataAsync: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert("Erreur", $"Erreur de chargement des données: {ex.Message}", "OK");
            });
        }
    }

    private async Task AssignProductToUser()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 AssignProductToUser - DÉBUT");

            if (SelectedUser == null || SelectedState == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ Utilisateur ou état non sélectionné");
                await DisplayAlert("Erreur", "Veuillez sélectionner un utilisateur et un état.", "OK");
                return;
            }

            // TROUVER LA BONNE PROPRIÉTÉ DE L'UTILISATEUR
            string userIdentifier = "";
            var userType = SelectedUser.GetType();

            // Essayer Email en premier
            var emailProp = userType.GetProperty("Email");
            if (emailProp != null)
            {
                userIdentifier = emailProp.GetValue(SelectedUser)?.ToString();
            }
            // Sinon essayer UserName
            else if (userType.GetProperty("UserName") != null)
            {
                userIdentifier = userType.GetProperty("UserName").GetValue(SelectedUser)?.ToString();
            }
            // Sinon essayer Username
            else if (userType.GetProperty("Username") != null)
            {
                userIdentifier = userType.GetProperty("Username").GetValue(SelectedUser)?.ToString();
            }
            // Sinon essayer Name
            else if (userType.GetProperty("Name") != null)
            {
                userIdentifier = userType.GetProperty("Name").GetValue(SelectedUser)?.ToString();
            }
            // Sinon essayer Id
            else if (userType.GetProperty("Id") != null)
            {
                userIdentifier = userType.GetProperty("Id").GetValue(SelectedUser)?.ToString();
            }

            if (string.IsNullOrEmpty(userIdentifier))
            {
                await DisplayAlert("Erreur", "Impossible de récupérer l'identifiant utilisateur", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"🔄 Assignation: Produit={Product.Id}, Utilisateur={userIdentifier}, État={SelectedState.Id}");

            bool success = await _service.AssignProductAsync(Product.Id, userIdentifier, SelectedState.Id);

            if (success)
            {
                System.Diagnostics.Debug.WriteLine("✅ Assignation réussie");
                await DisplayAlert("Succès", "Produit assigné avec succès.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ Assignation échouée");
                await DisplayAlert("Erreur", "Erreur lors de l'assignation.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur AssignProductToUser: {ex.Message}");
            await DisplayAlert("Erreur", $"Erreur lors de l'assignation: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}