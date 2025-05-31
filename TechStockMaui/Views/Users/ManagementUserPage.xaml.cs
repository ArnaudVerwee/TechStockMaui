using TechStockMaui.ViewModels;

namespace TechStockMaui.Views.Users
{
    public partial class ManagementUserPage : ContentPage
    {
        public ManagementUserPage()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("📄 ManagementUserPage créée");

            BindingContext = new UserManagementViewModel();

            System.Diagnostics.Debug.WriteLine("📄 BindingContext défini");
        }
    }
}