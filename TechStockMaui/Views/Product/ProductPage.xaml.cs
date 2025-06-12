using TechStockMaui.Services;
using TechStockMaui.Models;
using System.Collections.ObjectModel;

namespace TechStockMaui.Views
{
    public partial class ProductPage : ContentPage
    {
        private readonly ProductService _productService;
        private readonly SupplierService _supplierService;
        private readonly TypeArticleService _typeArticleService;

        public ObservableCollection<Product> Products { get; set; }

        public ProductPage()
        {
            InitializeComponent();
            _productService = new ProductService();
            _supplierService = new SupplierService();
            _typeArticleService = new TypeArticleService();

            Products = new ObservableCollection<Product>();
            ProductList.ItemsSource = Products;

            TranslationService.Instance.CultureChanged += OnCultureChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("Page appeared - automatic loading");

            await LoadTranslationsAsync();

            await LoadProductsAndFiltersAsync();
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
                System.Diagnostics.Debug.WriteLine("Updating Products texts");

                Title = await GetTextAsync("Products", "Products");

                try
                {
                    if (PageTitleLabel != null)
                        PageTitleLabel.Text = await GetTextAsync("ProductManagement", "Product Management");
                }
                catch { }

                try
                {
                    if (SearchButton != null)
                        SearchButton.Text = await GetTextAsync("Search", "Search");
                }
                catch { }

                try
                {
                    if (ResetButton != null)
                        ResetButton.Text = await GetTextAsync("Reset", "Reset");
                }
                catch { }

                if (NameEntry != null)
                    NameEntry.Placeholder = await GetTextAsync("NamePlaceholder", "Name...");

                if (SerialEntry != null)
                    SerialEntry.Placeholder = await GetTextAsync("SerialPlaceholder", "Serial number...");

                if (TypePicker != null)
                    TypePicker.Title = await GetTextAsync("Type", "Type");

                if (SupplierPicker != null)
                    SupplierPicker.Title = await GetTextAsync("Supplier", "Supplier");

                if (UserPicker != null)
                    UserPicker.Title = await GetTextAsync("User", "User");

                try
                {
                    if (NameHeader != null)
                        NameHeader.Text = await GetTextAsync("Name", "Name");
                }
                catch { }

                try
                {
                    if (SerialHeader != null)
                        SerialHeader.Text = await GetTextAsync("SerialNumber", "Serial Number");
                }
                catch { }

                try
                {
                    if (TypeHeader != null)
                        TypeHeader.Text = await GetTextAsync("Type", "Type");
                }
                catch { }

                try
                {
                    if (SupplierHeader != null)
                        SupplierHeader.Text = await GetTextAsync("Supplier", "Supplier");
                }
                catch { }

                try
                {
                    if (UserHeader != null)
                        UserHeader.Text = await GetTextAsync("User", "User");
                }
                catch { }

                try
                {
                    if (ActionsHeader != null)
                        ActionsHeader.Text = await GetTextAsync("Actions", "Actions");
                }
                catch { }

                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("Products texts updated");
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
                System.Diagnostics.Debug.WriteLine($"Products - Language changed to: {newCulture}");
                await UpdateTextsAsync();

                await ReloadFiltersWithTranslations();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

        private async Task ReloadFiltersWithTranslations()
        {
            try
            {
                var users = await _productService.GetUsersAsync();
                var userList = new List<object>();

                var notAssignedText = await GetTextAsync("NotAssigned", "Not assigned");
                var allText = await GetTextAsync("All", "All");

                userList.Add(new { Id = "NotAssigned", Name = notAssignedText });
                userList.Add(new { Id = "All", Name = allText });

                if (users != null && users.Any())
                {
                    foreach (var user in users)
                    {
                        var userId = user.UserName ?? "unknown";
                        var userName = user.UserName ?? await GetTextAsync("UnnamedUser", "Unnamed user");
                        userList.Add(new { Id = userId, Name = userName });
                    }
                }

                UserPicker.ItemsSource = userList;
                UserPicker.ItemDisplayBinding = new Binding("Name");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReloadFiltersWithTranslations error: {ex.Message}");
            }
        }

        private async Task LoadProductsAndFiltersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading products AND filters...");

                await LoadProductsOnlyAsync();

                await LoadFiltersAsync();

                System.Diagnostics.Debug.WriteLine("Everything loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadProductsAndFiltersAsync error: {ex.Message}");
                var errorTitle = await GetTextAsync("Error", "Error");
                var loadingErrorMsg = await GetTextAsync("LoadingError", "Loading error");
                await DisplayAlert(errorTitle, loadingErrorMsg, "OK");
            }
        }

        private async Task LoadProductsOnlyAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading products...");

                var products = await _productService.GetProductsFilterAsync();
                Products.Clear();

                if (products != null)
                {
                    foreach (var product in products.OrderByDescending(p => p.Id))
                    {
                        System.Diagnostics.Debug.WriteLine($"Product: Name={product.Name}, TypeName={product.TypeName}, SupplierName={product.SupplierName}, AssignedUserName='{product.AssignedUserName}' (Length: {product.AssignedUserName?.Length ?? -1})");
                        Products.Add(product);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"{Products.Count} products loaded");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadProductsOnlyAsync error: {ex.Message}");
                throw;
            }
        }

        private async Task LoadFiltersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading filters...");

                System.Diagnostics.Debug.WriteLine("Loading types...");
                var types = await _typeArticleService.GetAllAsync();
                if (types != null && types.Any())
                {
                    TypePicker.ItemsSource = types.ToList();
                    TypePicker.ItemDisplayBinding = new Binding("Name");
                    System.Diagnostics.Debug.WriteLine($"{types.Count()} types loaded");
                }

                System.Diagnostics.Debug.WriteLine("Loading suppliers...");
                var suppliers = await _supplierService.GetSuppliersAsync();
                if (suppliers != null && suppliers.Any())
                {
                    SupplierPicker.ItemsSource = suppliers.ToList();
                    SupplierPicker.ItemDisplayBinding = new Binding("Name");
                    System.Diagnostics.Debug.WriteLine($"{suppliers.Count()} suppliers loaded");
                }

                System.Diagnostics.Debug.WriteLine("Loading users via ProductService...");
                try
                {
                    var users = await _productService.GetUsersAsync();

                    var userList = new List<object>();

                    var notAssignedText = await GetTextAsync("NotAssigned", "Not assigned");
                    var allText = await GetTextAsync("All", "All");

                    userList.Add(new { Id = "NotAssigned", Name = notAssignedText });
                    userList.Add(new { Id = "All", Name = allText });

                    if (users != null && users.Any())
                    {
                        foreach (var user in users)
                        {
                            var userId = user.UserName ?? "unknown";
                            var userName = user.UserName ?? await GetTextAsync("UnnamedUser", "Unnamed user");

                            userList.Add(new { Id = userId, Name = userName });
                        }
                        System.Diagnostics.Debug.WriteLine($"{users.Count} users added");
                    }

                    UserPicker.ItemsSource = userList;
                    UserPicker.ItemDisplayBinding = new Binding("Name");
                    System.Diagnostics.Debug.WriteLine($"{userList.Count} user options total");
                }
                catch (Exception userEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Users error: {userEx.Message}");
                    var userList = new List<object>();
                    var notAssignedText = await GetTextAsync("NotAssigned", "Not assigned");
                    var allText = await GetTextAsync("All", "All");

                    userList.Add(new { Id = "NotAssigned", Name = notAssignedText });
                    userList.Add(new { Id = "All", Name = allText });
                    UserPicker.ItemsSource = userList;
                    UserPicker.ItemDisplayBinding = new Binding("Name");
                }

                System.Diagnostics.Debug.WriteLine("Filters loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadFiltersAsync error: {ex.Message}");
            }
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateProductPage());
        }

        private async void OnBackToDashboardClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Search launched");

                var name = string.IsNullOrWhiteSpace(NameEntry.Text) ? null : NameEntry.Text;
                var serialNumber = string.IsNullOrWhiteSpace(SerialEntry.Text) ? null : SerialEntry.Text;
                var typeId = TypePicker.SelectedItem is TechStockMaui.Models.TypeArticle.TypeArticle selectedType ? selectedType.Id : (int?)null;
                var supplierId = SupplierPicker.SelectedItem is TechStockMaui.Models.Supplier.Supplier selectedSupplier ? selectedSupplier.Id : (int?)null;

                string userId = null;
                if (UserPicker.SelectedItem != null)
                {
                    var selectedUser = UserPicker.SelectedItem as dynamic;
                    string selectedUserId = selectedUser?.Id?.ToString();

                    System.Diagnostics.Debug.WriteLine($"Raw selected user ID: '{selectedUserId}'");

                    System.Diagnostics.Debug.WriteLine($"Selected user object type: {UserPicker.SelectedItem.GetType()}");
                    System.Diagnostics.Debug.WriteLine($"Selected user object: {UserPicker.SelectedItem}");

                    if (selectedUserId != "NotAssigned" && selectedUserId != "All")
                    {
                        userId = selectedUserId;
                        System.Diagnostics.Debug.WriteLine($"Final userId for API: '{userId}'");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Special value detected: '{selectedUserId}' - not sending to API");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Filters: Name={name}, Serial={serialNumber}, TypeId={typeId}, SupplierId={supplierId}, UserId={userId}");

                System.Diagnostics.Debug.WriteLine("=== DEBUG: Current products and their assigned users ===");
                foreach (var prod in Products)
                {
                    System.Diagnostics.Debug.WriteLine($"Product: '{prod.Name}' -> AssignedUserName: '{prod.AssignedUserName}'");
                }
                System.Diagnostics.Debug.WriteLine("=== END DEBUG ===");

                var filteredProducts = await _productService.GetProductsFilterAsync(name, serialNumber, typeId, supplierId, userId);

                Products.Clear();
                if (filteredProducts != null)
                {
                    System.Diagnostics.Debug.WriteLine($"API returned {filteredProducts.Count()} products");
                    foreach (var product in filteredProducts.OrderByDescending(p => p.Id))
                    {
                        System.Diagnostics.Debug.WriteLine($"Filtered product: '{product.Name}' -> AssignedUserName: '{product.AssignedUserName}'");
                        Products.Add(product);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("API returned null");
                }

                System.Diagnostics.Debug.WriteLine($"{Products.Count} products found after filtering");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
                var errorTitle = await GetTextAsync("Error", "Error");
                var searchErrorMsg = await GetTextAsync("SearchError", "Search error");
                await DisplayAlert(errorTitle, searchErrorMsg, "OK");
            }
        }

        private async void OnResetClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Reset filters");

                NameEntry.Text = string.Empty;
                SerialEntry.Text = string.Empty;
                TypePicker.SelectedItem = null;
                SupplierPicker.SelectedItem = null;
                UserPicker.SelectedItem = null;

                await LoadProductsOnlyAsync();
                System.Diagnostics.Debug.WriteLine("Reset completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reset error: {ex.Message}");
                var errorTitle = await GetTextAsync("Error", "Error");
                var resetErrorMsg = await GetTextAsync("ResetError", "Reset error");
                await DisplayAlert(errorTitle, resetErrorMsg, "OK");
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Product product)
            {
                try
                {
                    await Navigation.PushAsync(new Views.EditProductPage(product));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Edit navigation error: {ex.Message}");
                    var errorTitle = await GetTextAsync("Error", "Error");
                    var editErrorMsg = await GetTextAsync("CannotOpenEditPage", "Cannot open edit page");
                    await DisplayAlert(errorTitle, editErrorMsg, "OK");
                }
            }
        }

        private async void OnDetailsClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Product product)
            {
                try
                {
                    await Navigation.PushAsync(new Views.ProductDetailsPage(product));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Details navigation error: {ex.Message}");

                    var nameLabel = await GetTextAsync("Name", "Name");
                    var serialLabel = await GetTextAsync("SerialNumber", "Serial");
                    var typeLabel = await GetTextAsync("Type", "Type");
                    var supplierLabel = await GetTextAsync("Supplier", "Supplier");
                    var userLabel = await GetTextAsync("User", "User");
                    var notAssignedText = await GetTextAsync("NotAssigned", "Not assigned");
                    var detailsTitle = await GetTextAsync("Details", "Details");

                    var details = $"{nameLabel}: {product.Name}\n" +
                                 $"{serialLabel}: {product.SerialNumber}\n" +
                                 $"{typeLabel}: {product.TypeName ?? "N/A"}\n" +
                                 $"{supplierLabel}: {product.SupplierName ?? "N/A"}\n" +
                                 $"{userLabel}: {product.AssignedUserName ?? notAssignedText}";

                    await DisplayAlert(detailsTitle, details, "OK");
                }
            }
        }

        private async void OnAssignClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnAssignClicked - START");

            try
            {
                System.Diagnostics.Debug.WriteLine("Checking sender and CommandParameter...");

                if (sender is Button button && button.CommandParameter is Product product)
                {
                    System.Diagnostics.Debug.WriteLine($"Button and product OK - Product: {product.Name}");

                    bool isAssigned = !string.IsNullOrEmpty(product.AssignedUserName);

                    System.Diagnostics.Debug.WriteLine($"Product assigned: {isAssigned}");
                    System.Diagnostics.Debug.WriteLine($"AssignedUserName: '{product.AssignedUserName}'");

                    if (isAssigned)
                    {
                        System.Diagnostics.Debug.WriteLine("Attempting unassignment...");

                        var confirmTitle = await GetTextAsync("Confirm", "Confirm");
                        var unassignQuestion = await GetTextAsync("UnassignQuestion", "Unassign {0} from {1}?");
                        var unassignMsg = string.Format(unassignQuestion, product.Name, product.AssignedUserName);
                        var yesText = await GetTextAsync("Yes", "Yes");
                        var noText = await GetTextAsync("No", "No");

                        var result = await DisplayAlert(confirmTitle, unassignMsg, yesText, noText);
                        System.Diagnostics.Debug.WriteLine($"Confirmation result: {result}");

                        if (result)
                        {
                            System.Diagnostics.Debug.WriteLine("Starting unassignment...");
                            await UnassignProduct(product);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Attempting assignment - Navigation to AssignProductPage...");

                        try
                        {
                            System.Diagnostics.Debug.WriteLine("Step 1 - Starting real navigation...");

                            System.Diagnostics.Debug.WriteLine("Step 2 - Creating AssignProductPage...");
                            var assignPage = new Views.MaterialManagements.AssignProductPage(product);
                            System.Diagnostics.Debug.WriteLine("Step 2 successful - Page created!");

                            System.Diagnostics.Debug.WriteLine("Step 3 - Navigation.PushAsync...");
                            await Navigation.PushAsync(assignPage);
                            System.Diagnostics.Debug.WriteLine("Step 3 successful - Navigation completed!");

                        }
                        catch (Exception navEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"NAVIGATION ERROR: {navEx.Message}");
                            System.Diagnostics.Debug.WriteLine($"Error type: {navEx.GetType().Name}");
                            System.Diagnostics.Debug.WriteLine($"Navigation stack: {navEx.StackTrace}");

                            var errorTitle = await GetTextAsync("NavigationError", "Navigation Error");
                            var errorMsg = $"Type: {navEx.GetType().Name}\nMessage: {navEx.Message}";
                            await DisplayAlert(errorTitle, errorMsg, "OK");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Invalid sender or CommandParameter");
                    System.Diagnostics.Debug.WriteLine($"   Sender type: {sender?.GetType()}");
                    System.Diagnostics.Debug.WriteLine($"   CommandParameter type: {(sender as Button)?.CommandParameter?.GetType()}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPTION OnAssignClicked: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                var errorTitle = await GetTextAsync("Error", "Error");
                await DisplayAlert(errorTitle, $"Exception: {ex.Message}", "OK");
            }

            System.Diagnostics.Debug.WriteLine("OnAssignClicked - END");
        }

        private async Task UnassignProduct(Product product)
        {
            try
            {
                bool success = await _productService.UnassignProductAsync(product.Id);

                if (success)
                {
                    await LoadProductsOnlyAsync();

                    var successTitle = await GetTextAsync("Success", "Success");
                    var unassignedMsg = await GetTextAsync("ProductUnassigned", "{0} has been unassigned");
                    var message = string.Format(unassignedMsg, product.Name);
                    await DisplayAlert(successTitle, message, "OK");
                }
                else
                {
                    var errorTitle = await GetTextAsync("Error", "Error");
                    var unassignFailedMsg = await GetTextAsync("UnassignFailed", "Unassign failed");
                    await DisplayAlert(errorTitle, unassignFailedMsg, "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UnassignProduct error: {ex.Message}");
                var errorTitle = await GetTextAsync("Error", "Error");
                var unassignErrorMsg = await GetTextAsync("UnassignError", "Error during unassign");
                await DisplayAlert(errorTitle, unassignErrorMsg, "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Product product)
            {
                var confirmTitle = await GetTextAsync("Confirm", "Confirm");
                var deleteQuestion = await GetTextAsync("DeleteQuestion", "Delete {0}?");
                var deleteMsg = string.Format(deleteQuestion, product.Name);
                var yesText = await GetTextAsync("Yes", "Yes");
                var noText = await GetTextAsync("No", "No");

                var result = await DisplayAlert(confirmTitle, deleteMsg, yesText, noText);

                if (result)
                {
                    try
                    {
                        var success = await _productService.DeleteProductAsync(product.Id);
                        if (success)
                        {
                            Products.Remove(product);
                            var successTitle = await GetTextAsync("Success", "Success");
                            var deletedMsg = await GetTextAsync("ProductDeleted", "Product deleted");
                            await DisplayAlert(successTitle, deletedMsg, "OK");
                        }
                        else
                        {
                            var errorTitle = await GetTextAsync("Error", "Error");
                            var deleteFailedMsg = await GetTextAsync("DeleteFailed", "Delete failed");
                            await DisplayAlert(errorTitle, deleteFailedMsg, "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Deletion error: {ex.Message}");
                        var errorTitle = await GetTextAsync("Error", "Error");
                        var deleteErrorMsg = await GetTextAsync("DeleteError", "Delete error");
                        await DisplayAlert(errorTitle, deleteErrorMsg, "OK");
                    }
                }
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

                try
                {
                    if (LanguageFlag != null)
                        LanguageFlag.Text = flag;
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("LanguageFlag not found in XAML");
                }
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

        ~ProductPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}