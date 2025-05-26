namespace TechStockMaui.Views.Shared;

public partial class DashboardPage : ContentPage
{
	public DashboardPage()
	{
		InitializeComponent();
		Title = string.Empty;
	}
    private async void OnWarehouseClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProductPage");
    }
    private async void OnLaptopClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//TypeArticlePage");
    }
    private async void OnAssignedProductsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//AssignedProductsPage");
    }
    private async void OnUsersClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ManagementUserPage");
    }
}