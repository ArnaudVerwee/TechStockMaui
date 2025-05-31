using TechStockMaui.ViewModels;

namespace TechStockMaui.Views.Users
{
    public partial class ManageRolesPage : ContentPage
    {
        public string UserName { get; private set; }

        // Constructeur par défaut (gardez-le pour le XAML)
        public ManageRolesPage()
        {
            InitializeComponent();
        }

        // Constructeur avec paramètre userName
        public ManageRolesPage(string userName) : this()
        {
            UserName = userName;
            Title = $"🔐 Rôles - {userName}";

            System.Diagnostics.Debug.WriteLine($"📄 ManageRolesPage créée pour: {userName}");

            // Initialiser le ViewModel avec le userName
            BindingContext = new ManageRolesViewModel(userName);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Charger les rôles quand la page apparaît
            if (BindingContext is ManageRolesViewModel viewModel)
            {
                await viewModel.LoadRolesAsync();
            }
        }
    }
}