using TechStockMaui.Views.Supplier;
using TechStockMaui.Services;
using TechStockMaui.Views;

namespace TechStockMaui.Views.Shared
{
    public partial class DashboardPage : ContentPage
    {
        private ProductService _productService;

        public DashboardPage()
        {
            InitializeComponent();
            _productService = new ProductService();

            TranslationService.Instance.CultureChanged += OnCultureChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadStatisticsAsync();
            await LoadTranslationsAsync();
            await Task.Delay(100);
            await ConfigureUserInterfaceBasedOnRoleAsync();
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

        private async Task ConfigureUserInterfaceBasedOnRoleAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Configuring interface based on role...");

                var authService = new AuthService();
                var currentUser = await authService.GetCurrentUserAsync();

                if (currentUser != null && currentUser.Roles.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"Connected user: {currentUser.UserName}, Roles: {string.Join(", ", currentUser.Roles)}");

                    var isUser = currentUser.Roles.Contains("User");
                    var isSupport = currentUser.Roles.Contains("Support");
                    var isAdmin = currentUser.Roles.Contains("Admin");

                    System.Diagnostics.Debug.WriteLine($"Is User: {isUser}, Is Support: {isSupport}, Is Admin: {isAdmin}");

                    if (isUser && !isSupport && !isAdmin)
                    {
                        System.Diagnostics.Debug.WriteLine("User role only detected - hiding all admin/support features");

                        await Application.Current.Dispatcher.DispatchAsync(() =>
                        {
                            try
                            {
                                if (ProductsButtonFrame != null)
                                {
                                    ProductsButtonFrame.IsVisible = false;
                                    System.Diagnostics.Debug.WriteLine("ProductsButtonFrame hidden");
                                }
                                if (ProductsCard != null)
                                {
                                    ProductsCard.IsVisible = false;
                                    System.Diagnostics.Debug.WriteLine("ProductsCard hidden");
                                }

                                if (SuppliersButtonFrame != null)
                                {
                                    SuppliersButtonFrame.IsVisible = false;
                                    System.Diagnostics.Debug.WriteLine("SuppliersButtonFrame hidden");
                                }
                                if (SuppliersCard != null)
                                {
                                    SuppliersCard.IsVisible = false;
                                    System.Diagnostics.Debug.WriteLine("SuppliersCard hidden");
                                }

                                if (TypesButtonFrame != null)
                                {
                                    TypesButtonFrame.IsVisible = false;
                                    System.Diagnostics.Debug.WriteLine("TypesButtonFrame hidden");
                                }

                                if (UsersButtonFrame != null)
                                {
                                    UsersButtonFrame.IsVisible = false;
                                    System.Diagnostics.Debug.WriteLine("UsersButtonFrame hidden");
                                }
                                if (UsersCard != null)
                                {
                                    UsersCard.IsVisible = false;
                                    System.Diagnostics.Debug.WriteLine("UsersCard hidden");
                                }

                                if (OverviewLabel != null)
                                {
                                    OverviewLabel.IsVisible = false;
                                    System.Diagnostics.Debug.WriteLine("OverviewLabel hidden");
                                }
                                if (OverviewFrame != null)
                                {
                                    OverviewFrame.IsVisible = false;
                                    System.Diagnostics.Debug.WriteLine("OverviewFrame hidden");
                                }

                                if (MyProductsCard != null)
                                {
                                    MyProductsCard.IsVisible = true;
                                    System.Diagnostics.Debug.WriteLine("MyProductsCard kept visible");
                                }

                                System.Diagnostics.Debug.WriteLine("Only AssignedProducts accessible for User role");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error configuring UI for User: {ex.Message}");
                            }
                        });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Admin or Support role detected - showing appropriate features");

                        await Application.Current.Dispatcher.DispatchAsync(() =>
                        {
                            try
                            {
                                if (ProductsButtonFrame != null)
                                {
                                    ProductsButtonFrame.IsVisible = true;
                                    System.Diagnostics.Debug.WriteLine("ProductsButtonFrame visible for Support/Admin");
                                }
                                if (ProductsCard != null)
                                {
                                    ProductsCard.IsVisible = true;
                                    System.Diagnostics.Debug.WriteLine("ProductsCard visible for Support/Admin");
                                }

                                if (SuppliersButtonFrame != null)
                                {
                                    SuppliersButtonFrame.IsVisible = true;
                                    System.Diagnostics.Debug.WriteLine("SuppliersButtonFrame visible for Support/Admin");
                                }
                                if (SuppliersCard != null)
                                {
                                    SuppliersCard.IsVisible = true;
                                    System.Diagnostics.Debug.WriteLine("SuppliersCard visible for Support/Admin");
                                }

                                if (TypesButtonFrame != null)
                                {
                                    TypesButtonFrame.IsVisible = true;
                                    System.Diagnostics.Debug.WriteLine("TypesButtonFrame visible for Support/Admin");
                                }

                                if (UsersButtonFrame != null)
                                {
                                    UsersButtonFrame.IsVisible = isAdmin;
                                    System.Diagnostics.Debug.WriteLine($"UsersButtonFrame visible: {isAdmin}");
                                }
                                if (UsersCard != null)
                                {
                                    UsersCard.IsVisible = isAdmin;
                                    System.Diagnostics.Debug.WriteLine($"UsersCard visible: {isAdmin}");
                                }

                                if (MyProductsCard != null)
                                {
                                    MyProductsCard.IsVisible = true;
                                    System.Diagnostics.Debug.WriteLine("MyProductsCard visible for Support/Admin");
                                }

                                if (OverviewLabel != null)
                                {
                                    OverviewLabel.IsVisible = true;
                                    System.Diagnostics.Debug.WriteLine("OverviewLabel visible for Support/Admin");
                                }
                                if (OverviewFrame != null)
                                {
                                    OverviewFrame.IsVisible = true;
                                    System.Diagnostics.Debug.WriteLine("OverviewFrame visible for Support/Admin");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error configuring UI for Support/Admin: {ex.Message}");
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConfigureUserInterfaceBasedOnRoleAsync error: {ex.Message}");
            }
        }

        private void HideFramesByGestureHandler(string[] handlerNames)
        {
            try
            {
                var frames = GetAllFramesInPage();
                foreach (var frame in frames)
                {
                    if (frame.GestureRecognizers?.Any() == true)
                    {
                        foreach (var gesture in frame.GestureRecognizers.OfType<TapGestureRecognizer>())
                        {
                            var methodName = GetTapGestureMethodName(gesture);
                            if (handlerNames.Contains(methodName))
                            {
                                frame.IsVisible = false;
                                System.Diagnostics.Debug.WriteLine($"Hidden frame with gesture: {methodName}");
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error hiding frames: {ex.Message}");
            }
        }

        private void ShowFramesByGestureHandler(string[] handlerNames)
        {
            try
            {
                var frames = GetAllFramesInPage();
                foreach (var frame in frames)
                {
                    if (frame.GestureRecognizers?.Any() == true)
                    {
                        foreach (var gesture in frame.GestureRecognizers.OfType<TapGestureRecognizer>())
                        {
                            var methodName = GetTapGestureMethodName(gesture);
                            if (handlerNames.Contains(methodName))
                            {
                                frame.IsVisible = true;
                                System.Diagnostics.Debug.WriteLine($"Shown frame with gesture: {methodName}");
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing frames: {ex.Message}");
            }
        }

        private List<Frame> GetAllFramesInPage()
        {
            var frames = new List<Frame>();
            try
            {
                TraverseVisualTree(this.Content, frames);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting frames: {ex.Message}");
            }
            return frames;
        }

        private void TraverseVisualTree(IView view, List<Frame> frames)
        {
            if (view is Frame frame)
            {
                frames.Add(frame);
            }

            if (view is Layout layout)
            {
                foreach (var child in layout.Children)
                {
                    TraverseVisualTree(child, frames);
                }
            }
            else if (view is ContentView contentView && contentView.Content != null)
            {
                TraverseVisualTree(contentView.Content, frames);
            }
            else if (view is ScrollView scrollView && scrollView.Content != null)
            {
                TraverseVisualTree(scrollView.Content, frames);
            }
        }

        private string GetTapGestureMethodName(TapGestureRecognizer gesture)
        {
            try
            {
                var field = gesture.GetType().GetField("_tapped", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field?.GetValue(gesture) is EventHandler handler)
                {
                    return handler.Method.Name;
                }
            }
            catch
            {
            }
            return "";
        }

        private async Task<string> GetTextAsync(string key, string fallback = null)
        {
            try
            {
                var text = await TranslationService.Instance.GetTranslationAsync(key);
                System.Diagnostics.Debug.WriteLine($"GetTextAsync('{key}') -> '{text}' (fallback: '{fallback}')");
                return !string.IsNullOrEmpty(text) && text != key ? text : (fallback ?? key);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTextAsync error for '{key}': {ex.Message}");
                return fallback ?? key;
            }
        }

        private async Task UpdateTextsAsync()
        {
            try
            {
                var currentCulture = TranslationService.Instance.GetCurrentCulture();
                System.Diagnostics.Debug.WriteLine($"Updating Dashboard texts - Culture: {currentCulture}");

                Title = await GetTextAsync("Dashboard", "Dashboard");

                if (HomeButtonLabel != null)
                    HomeButtonLabel.Text = await GetTextAsync("Home", "Home");

                if (ProductsButtonLabel != null)
                    ProductsButtonLabel.Text = await GetTextAsync("Products", "Products");

                if (SuppliersButtonLabel != null)
                    SuppliersButtonLabel.Text = await GetTextAsync("Suppliers", "Suppliers");

                if (TypesButtonLabel != null)
                    TypesButtonLabel.Text = await GetTextAsync("Types", "Types");

                if (UsersButtonLabel != null)
                    UsersButtonLabel.Text = await GetTextAsync("Users", "Users");

                if (LogoutButtonLabel != null)
                    LogoutButtonLabel.Text = await GetTextAsync("Exit", "Exit");

                if (WelcomeLabel != null)
                    WelcomeLabel.Text = await GetTextAsync("Welcome", "Welcome to TechStock");

                if (WelcomeSubLabel != null)
                    WelcomeSubLabel.Text = await GetTextAsync("WelcomeMessage", "Manage your technology inventory easily");

                if (QuickAccessLabel != null)
                    QuickAccessLabel.Text = await GetTextAsync("QuickAccess", "Quick Access");

                if (ProductsCardLabel != null)
                    ProductsCardLabel.Text = await GetTextAsync("Products", "Products");

                if (ProductsCardSubLabel != null)
                    ProductsCardSubLabel.Text = await GetTextAsync("ManageInventoryShort", "Manage inventory");

                if (SuppliersCardLabel != null)
                    SuppliersCardLabel.Text = await GetTextAsync("Suppliers", "Suppliers");

                if (SuppliersCardSubLabel != null)
                    SuppliersCardSubLabel.Text = await GetTextAsync("ManageContacts", "Manage contacts");

                if (UsersCardLabel != null)
                    UsersCardLabel.Text = await GetTextAsync("Users", "Users");

                if (UsersCardSubLabel != null)
                    UsersCardSubLabel.Text = await GetTextAsync("ManageAccess", "Manage access");

                if (MyProductsCardLabel != null)
                    MyProductsCardLabel.Text = await GetTextAsync("MyProducts", "My Products");

                if (MyProductsCardSubLabel != null)
                    MyProductsCardSubLabel.Text = await GetTextAsync("AssignedProducts", "Assigned products");

                if (OverviewLabel != null)
                    OverviewLabel.Text = await GetTextAsync("Overview", "Overview");

                if (TotalLabel != null)
                    TotalLabel.Text = await GetTextAsync("Total", "Total");

                if (AssignedLabel != null)
                    AssignedLabel.Text = await GetTextAsync("Assigned", "Assigned");

                if (AvailableLabel != null)
                    AvailableLabel.Text = await GetTextAsync("Available", "Available");

                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("Dashboard texts updated");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTextsAsync error: {ex.Message}");
            }
        }

        private async void OnCultureChanged(object sender, string newCulture)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard - Language changed to: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting Dashboard statistics loading...");

                var products = await _productService.GetProductsFilterAsync();

                if (products != null)
                {
                    var productsList = products.ToList();
                    System.Diagnostics.Debug.WriteLine($"{productsList.Count} products retrieved via filter");

                    foreach (var product in productsList)
                    {
                        var assigned = string.IsNullOrEmpty(product.AssignedUserName) ? "NOT ASSIGNED" : product.AssignedUserName;
                        System.Diagnostics.Debug.WriteLine($"Product: {product.Name}, AssignedUserName: '{product.AssignedUserName}' -> {assigned}");
                    }

                    var totalProducts = productsList.Count;
                    var assignedProducts = productsList.Count(p => !string.IsNullOrEmpty(p.AssignedUserName));
                    var freeProducts = totalProducts - assignedProducts;

                    System.Diagnostics.Debug.WriteLine($"DASHBOARD STATS -> Total: {totalProducts}, Assigned: {assignedProducts}, Free: {freeProducts}");

                    TotalProductsLabel.Text = totalProducts.ToString();
                    AssignedProductsLabel.Text = assignedProducts.ToString();
                    FreeProductsLabel.Text = freeProducts.ToString();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("products filter is null");
                    TotalProductsLabel.Text = "N/A";
                    AssignedProductsLabel.Text = "N/A";
                    FreeProductsLabel.Text = "N/A";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadStatisticsAsync error: {ex.Message}");
                TotalProductsLabel.Text = "N/A";
                AssignedProductsLabel.Text = "N/A";
                FreeProductsLabel.Text = "N/A";
            }
        }

        private async void OnWarehouseClicked(object sender, EventArgs e)
        {
            try
            {
                var authService = new AuthService();
                var currentUser = await authService.GetCurrentUserAsync();

                if (currentUser != null)
                {
                    var isUser = currentUser.Roles.Contains("User");
                    var isSupport = currentUser.Roles.Contains("Support");
                    var isAdmin = currentUser.Roles.Contains("Admin");

                    if (isUser && !isSupport && !isAdmin)
                    {
                        await DisplayAlert("Access Denied", "You don't have permission to access this page", "OK");
                        return;
                    }
                }

                await Navigation.PushAsync(new ProductPage());
            }
            catch (Exception ex)
            {
                var errorText = await GetTextAsync("Error", "Error");
                await DisplayAlert(errorText, $"Unable to open products page: {ex.Message}", "OK");
            }
        }

        private async void OnTruckClicked(object sender, EventArgs e)
        {
            try
            {
                var authService = new AuthService();
                var currentUser = await authService.GetCurrentUserAsync();

                if (currentUser != null)
                {
                    var isUser = currentUser.Roles.Contains("User");
                    var isSupport = currentUser.Roles.Contains("Support");
                    var isAdmin = currentUser.Roles.Contains("Admin");

                    if (isUser && !isSupport && !isAdmin)
                    {
                        await DisplayAlert("Access Denied", "You don't have permission to access this page", "OK");
                        return;
                    }
                }

                await Navigation.PushAsync(new SupplierPage());
            }
            catch (Exception ex)
            {
                var errorText = await GetTextAsync("Error", "Error");
                await DisplayAlert(errorText, $"Unable to open suppliers page: {ex.Message}", "OK");
            }
        }

        private async void OnLaptopClicked(object sender, EventArgs e)
        {
            try
            {
                var authService = new AuthService();
                var currentUser = await authService.GetCurrentUserAsync();

                if (currentUser != null)
                {
                    var isUser = currentUser.Roles.Contains("User");
                    var isSupport = currentUser.Roles.Contains("Support");
                    var isAdmin = currentUser.Roles.Contains("Admin");

                    if (isUser && !isSupport && !isAdmin)
                    {
                        await DisplayAlert("Access Denied", "You don't have permission to access this page", "OK");
                        return;
                    }
                }

                await Navigation.PushAsync(new TypeArticlePage());
            }
            catch (Exception ex)
            {
                var errorText = await GetTextAsync("Error", "Error");
                await DisplayAlert(errorText, $"Unable to open article types page: {ex.Message}", "OK");
            }
        }

        private async void OnAssignedProductsClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new MaterialManagements.AssignedProductsPage());
            }
            catch (Exception ex)
            {
                var errorText = await GetTextAsync("Error", "Error");
                await DisplayAlert(errorText, $"Unable to open assigned products page: {ex.Message}", "OK");
            }
        }

        private async void OnUsersClicked(object sender, EventArgs e)
        {
            try
            {
                var authService = new AuthService();
                var currentUser = await authService.GetCurrentUserAsync();

                if (currentUser != null)
                {
                    var isAdmin = currentUser.Roles.Contains("Admin");

                    if (!isAdmin)
                    {
                        await DisplayAlert("Access Denied", "Only administrators can access this page", "OK");
                        return;
                    }
                }

                await Navigation.PushAsync(new Views.Users.ManagementUserPage());
            }
            catch (Exception ex)
            {
                var errorText = await GetTextAsync("Error", "Error");
                await DisplayAlert(errorText, $"Error: {ex.Message}", "OK");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            try
            {
                var confirmTitle = await GetTextAsync("Logout", "Logout");
                var confirmMessage = await GetTextAsync("ConfirmLogout", "Are you sure you want to logout?");
                var yesText = await GetTextAsync("Yes", "Yes");
                var noText = await GetTextAsync("No", "No");

                bool confirm = await DisplayAlert(confirmTitle, confirmMessage, yesText, noText);
                if (confirm)
                {
                    var authService = new AuthService();
                    await authService.LogoutAsync();
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                }
            }
            catch (Exception ex)
            {
                var errorText = await GetTextAsync("Error", "Error");
                await DisplayAlert(errorText, $"Error during logout: {ex.Message}", "OK");
            }
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
                var titleText = "🌍 " + await GetTextAsync("ChooseLanguage", "Choose Language");

                var selectedOption = await DisplayActionSheet(titleText, cancelText, null, options.ToArray());

                if (!string.IsNullOrEmpty(selectedOption) && selectedOption != cancelText)
                {
                    string newCulture = null;
                    if (selectedOption.Contains("FR")) newCulture = "fr";
                    else if (selectedOption.Contains("EN")) newCulture = "en";
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

        ~DashboardPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}