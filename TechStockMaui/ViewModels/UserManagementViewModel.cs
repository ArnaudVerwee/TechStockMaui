using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TechStockMaui.Models;
using TechStockMaui.Services;
using TechStockMaui.Views.Users;

namespace TechStockMaui.ViewModels
{
    public class UserManagementViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private bool _isLoading;

        public ObservableCollection<UserRolesViewModel> Users { get; set; }
        public ICommand RefreshCommand { get; }
        public ICommand ManageRolesCommand { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public UserManagementViewModel()
        {
            _userService = new UserService();
            Users = new ObservableCollection<UserRolesViewModel>();

            RefreshCommand = new Command(async () => await LoadUsersAsync());
            ManageRolesCommand = new Command<string>(async (userName) => await ManageUserRoles(userName));

            _ = Task.Run(LoadUsersAsync);
        }

        public async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("Starting to load users");

                var users = await _userService.GetAllUsersAsync();

                await Application.Current.Dispatcher.DispatchAsync(() =>
                {
                    Users.Clear();
                    foreach (var user in users)
                    {
                        Users.Add(user);
                        System.Diagnostics.Debug.WriteLine($"User added: {user.UserName} with roles: {string.Join(", ", user.Roles)}");
                    }
                });

                System.Diagnostics.Debug.WriteLine($"{users.Count} users loaded");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error LoadUsersAsync: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Unable to load users",
                    "OK");
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("LoadUsersAsync completed");
            }
        }

        private async Task ManageUserRoles(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                {
                    System.Diagnostics.Debug.WriteLine("ManageUserRoles called with empty userName");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Managing roles for: {userName}");

                var user = Users.FirstOrDefault(u => u.UserName == userName);
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine($"User not found: {userName}");
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "User not found",
                        "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Navigating to ManageRolesPage for user: {userName}");
                await Application.Current.MainPage.Navigation.PushAsync(
                    new ManageRolesPage(userName));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ManageUserRoles: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Unable to open role management",
                    "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}