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
            System.Diagnostics.Debug.WriteLine("AssignProductPage constructor - START");

            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("InitializeComponent OK");

            TranslationService.Instance.CultureChanged += OnCultureChanged;

            Product = product;
            System.Diagnostics.Debug.WriteLine($"Product assigned: {product.Name}");

            System.Diagnostics.Debug.WriteLine("Creating MaterialManagementService...");
            _service = new MaterialManagementService();
            System.Diagnostics.Debug.WriteLine("MaterialManagementService created");

            System.Diagnostics.Debug.WriteLine("Creating ProductService...");
            _productService = new ProductService();
            System.Diagnostics.Debug.WriteLine("ProductService created");

            System.Diagnostics.Debug.WriteLine("Creating Command...");
            AssignCommand = new Command(async () => await AssignProductToUser());
            System.Diagnostics.Debug.WriteLine("Command created");

            System.Diagnostics.Debug.WriteLine("Setting BindingContext...");
            BindingContext = this;
            System.Diagnostics.Debug.WriteLine("BindingContext assigned");

            System.Diagnostics.Debug.WriteLine("AssignProductPage constructor - END");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AssignProductPage constructor error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Constructor stack: {ex.StackTrace}");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadTranslationsAsync();

        System.Diagnostics.Debug.WriteLine("AssignProductPage appeared - automatic loading");
        await LoadDataAsync();
    }

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
            System.Diagnostics.Debug.WriteLine($"Translations loading error: {ex.Message}");
        }
    }

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

    private async Task UpdateTextsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Updating AssignProductPage texts");

            Title = await GetTextAsync("Assign Product to User", "Assign Product");

            if (TitleLabel != null)
                TitleLabel.Text = await GetTextAsync("Assign product to user", "Assign Product");

            if (ProductInfoHeaderLabel != null)
                ProductInfoHeaderLabel.Text = await GetTextAsync("Product Information", "Product Information");

            if (ProductSpan != null)
                ProductSpan.Text = await GetTextAsync("Product", "Product") + " : ";

            if (SerialNumberSpan != null)
                SerialNumberSpan.Text = await GetTextAsync("Serial Number", "Serial Number") + " : ";

            if (SelectUserLabel != null)
                SelectUserLabel.Text = await GetTextAsync("Select a user", "Select a user") + " :";

            if (SelectStateLabel != null)
                SelectStateLabel.Text = await GetTextAsync("Select product state", "Select the product state") + " :";

            if (UserPicker != null)
                UserPicker.Title = await GetTextAsync("Select a user", "Select a user");

            if (StatePicker != null)
                StatePicker.Title = await GetTextAsync("Select a state", "Select a state");

            if (AssignButton != null)
                AssignButton.Text = await GetTextAsync("Assign", "Assign");

            if (BackButton != null)
                BackButton.Text = await GetTextAsync("Back to list", "Back to list");

            if (LanguageLabel != null)
                LanguageLabel.Text = await GetTextAsync("Language", "Language");

            await UpdateLanguageFlag();

            await UpdateStatesWithTranslations();

            System.Diagnostics.Debug.WriteLine("AssignProductPage texts updated");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateTextsAsync error: {ex.Message}");
        }
    }

    private async Task UpdateStatesWithTranslations()
    {
        try
        {
            if (States.Any())
            {
                var translatedStates = new List<States>();

                foreach (var state in States.ToList())
                {
                    var translatedStatus = state.Status switch
                    {
                        "New Product" => await GetTextAsync("New Product", "New Product"),
                        "Old Product" => await GetTextAsync("Old Product", "Old Product"),
                        "Product to repair" => await GetTextAsync("Product to repair", "Product to repair"),
                        "Broken Product" => await GetTextAsync("Broken Product", "Broken Product"),
                        _ => state.Status
                    };

                    translatedStates.Add(new States { Id = state.Id, Status = translatedStatus });
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var selectedStateId = SelectedState?.Id;
                    States.Clear();
                    foreach (var state in translatedStates)
                    {
                        States.Add(state);
                    }

                    if (selectedStateId.HasValue)
                    {
                        SelectedState = States.FirstOrDefault(s => s.Id == selectedStateId.Value);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateStatesWithTranslations error: {ex.Message}");
        }
    }

    private async void OnCultureChanged(object sender, string newCulture)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"AssignProductPage - Language changed to: {newCulture}");
            await UpdateTextsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("LoadDataAsync - START");

            System.Diagnostics.Debug.WriteLine("Loading users...");
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
                            System.Diagnostics.Debug.WriteLine($"User added: {user}");
                        }
                        System.Diagnostics.Debug.WriteLine($"{Users.Count} users added");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No users received");
                    }
                });
            }
            catch (Exception userEx)
            {
                System.Diagnostics.Debug.WriteLine($"Users loading error: {userEx.Message}");
            }

            System.Diagnostics.Debug.WriteLine("Loading states...");
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
                    System.Diagnostics.Debug.WriteLine($"State added: {state.Status}");
                }
                System.Diagnostics.Debug.WriteLine($"{States.Count} states added");
            });

            await UpdateStatesWithTranslations();

            System.Diagnostics.Debug.WriteLine("LoadDataAsync - END");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadDataAsync error: {ex.Message}");
            var errorTitle = await GetTextAsync("Error", "Error");
            var loadErrorMessage = await GetTextAsync("Data loading error", "Data loading error");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert(errorTitle, $"{loadErrorMessage}: {ex.Message}", "OK");
            });
        }
    }

    private async Task AssignProductToUser()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("AssignProductToUser - START");

            if (SelectedUser == null || SelectedState == null)
            {
                System.Diagnostics.Debug.WriteLine("User or state not selected");
                var errorTitle = await GetTextAsync("Error", "Error");
                var selectionErrorMessage = await GetTextAsync("Please select user and state", "Please select a user and a state.");
                await DisplayAlert(errorTitle, selectionErrorMessage, "OK");
                return;
            }

            string userIdentifier = "";
            var userType = SelectedUser.GetType();

            var emailProp = userType.GetProperty("Email");
            if (emailProp != null)
            {
                userIdentifier = emailProp.GetValue(SelectedUser)?.ToString();
            }
            else if (userType.GetProperty("UserName") != null)
            {
                userIdentifier = userType.GetProperty("UserName").GetValue(SelectedUser)?.ToString();
            }
            else if (userType.GetProperty("Username") != null)
            {
                userIdentifier = userType.GetProperty("Username").GetValue(SelectedUser)?.ToString();
            }
            else if (userType.GetProperty("Name") != null)
            {
                userIdentifier = userType.GetProperty("Name").GetValue(SelectedUser)?.ToString();
            }
            else if (userType.GetProperty("Id") != null)
            {
                userIdentifier = userType.GetProperty("Id").GetValue(SelectedUser)?.ToString();
            }

            if (string.IsNullOrEmpty(userIdentifier))
            {
                var errorTitle = await GetTextAsync("Error", "Error");
                var identifierErrorMessage = await GetTextAsync("Unable to retrieve user identifier", "Unable to retrieve user identifier");
                await DisplayAlert(errorTitle, identifierErrorMessage, "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Assignment: Product={Product.Id}, User={userIdentifier}, State={SelectedState.Id}");

            bool success = await _service.AssignProductAsync(Product.Id, userIdentifier, SelectedState.Id);

            if (success)
            {
                System.Diagnostics.Debug.WriteLine("Assignment successful");
                var successTitle = await GetTextAsync("Success", "Success");
                var successMessage = await GetTextAsync("Product assigned successfully", "Product assigned successfully.");
                await DisplayAlert(successTitle, successMessage, "OK");
                await Navigation.PopAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Assignment failed");
                var errorTitle = await GetTextAsync("Error", "Error");
                var assignErrorMessage = await GetTextAsync("Assignment error", "Error during assignment.");
                await DisplayAlert(errorTitle, assignErrorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AssignProductToUser error: {ex.Message}");
            var errorTitle = await GetTextAsync("Error", "Error");
            var assignErrorMessage = await GetTextAsync("Assignment error", "Error during assignment");
            await DisplayAlert(errorTitle, $"{assignErrorMessage}: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

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
                    System.Diagnostics.Debug.WriteLine($"Changing to: {newCulture}");
                    await translationService.SetCurrentCultureAsync(newCulture);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
        }
    }

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
            System.Diagnostics.Debug.WriteLine($"Flag update error: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    ~AssignProductPage()
    {
        try
        {
            TranslationService.Instance.CultureChanged -= OnCultureChanged;
        }
        catch { }
    }
}