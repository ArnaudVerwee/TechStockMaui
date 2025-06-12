using TechStockMaui.Services;
using TechStockMaui.ViewModels;

namespace TechStockMaui.Views.Users
{
    public partial class ManageRolesPage : ContentPage
    {
        public string UserName { get; private set; }

        public ManageRolesPage()
        {
            InitializeComponent();

            TranslationService.Instance.CultureChanged += OnCultureChanged;
        }

        public ManageRolesPage(string userName) : this()
        {
            UserName = userName;
            Title = $"🔐 Rôles - {userName}";
            System.Diagnostics.Debug.WriteLine($"ManageRolesPage created for: {userName}");
            BindingContext = new ManageRolesViewModel(userName);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadTranslationsAsync();

            if (BindingContext is ManageRolesViewModel viewModel)
            {
                await viewModel.LoadRolesAsync();
            }
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
                System.Diagnostics.Debug.WriteLine("Updating ManageRolesPage texts");

                var rolesText = await GetTextAsync("Roles", "Roles");
                if (!string.IsNullOrEmpty(UserName))
                    Title = $"🔐 {rolesText} - {UserName}";

                if (UserHeaderLabel != null)
                {
                    var userText = await GetTextAsync("User", "User");
                    if (!string.IsNullOrEmpty(UserName))
                        UserHeaderLabel.Text = $"{userText}: {UserName}";
                }

                if (PageDescriptionLabel != null)
                    PageDescriptionLabel.Text = await GetTextAsync("Role management page", "Role management page");

                if (LoadingLabel != null)
                    LoadingLabel.Text = await GetTextAsync("Loading", "Loading") + "...";

                if (AvailableRolesLabel != null)
                    AvailableRolesLabel.Text = await GetTextAsync("Available roles", "Available roles") + ":";

                if (SaveButton != null)
                    SaveButton.Text = await GetTextAsync("Save", "Save");

                if (CancelButton != null)
                    CancelButton.Text = await GetTextAsync("Cancel", "Cancel");

                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("ManageRolesPage texts updated");
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
                System.Diagnostics.Debug.WriteLine($"ManageRolesPage - Language changed to: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
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

        ~ManageRolesPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}