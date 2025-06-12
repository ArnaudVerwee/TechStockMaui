using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TechStockMaui.Models;
using TechStockMaui.Services;

namespace TechStockMaui.ViewModels
{
    public class ManageRolesViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private string _userName;
        private bool _isLoading;

        public ObservableCollection<RoleItem> AvailableRoles { get; set; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ManageRolesViewModel(string userName)
        {
            _userService = new UserService();
            UserName = userName;
            AvailableRoles = new ObservableCollection<RoleItem>();

            SaveCommand = new Command(async () => await SaveRolesAsync());
            CancelCommand = new Command(async () => await CancelAsync());

            System.Diagnostics.Debug.WriteLine($"ManageRolesViewModel created for: {userName}");
        }

        public async Task LoadRolesAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine($"Loading roles for: {UserName}");

                var roleItems = await _userService.GetRolesAsync(UserName);

                System.Diagnostics.Debug.WriteLine($"Processing {roleItems.Count} roles from API");

                await Application.Current.Dispatcher.DispatchAsync(() =>
                {
                    AvailableRoles.Clear();
                    foreach (var role in roleItems)
                    {
                        AvailableRoles.Add(role);
                        System.Diagnostics.Debug.WriteLine($"Role added: {role.RoleName} - Selected: {role.IsSelected}");
                    }
                });

                System.Diagnostics.Debug.WriteLine($"{roleItems.Count} roles loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error LoadRolesAsync: {ex.Message}");

                await Application.Current.Dispatcher.DispatchAsync(() =>
                {
                    AvailableRoles.Clear();
                    AvailableRoles.Add(new RoleItem { RoleName = "Admin", IsSelected = false });
                    AvailableRoles.Add(new RoleItem { RoleName = "Support", IsSelected = false });
                    AvailableRoles.Add(new RoleItem { RoleName = "User", IsSelected = true });
                });
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("End of LoadRolesAsync");
            }
        }

        private List<string> GetUserCurrentRoles(string userName)
        {
            if (userName.Contains("admin"))
                return new List<string> { "Admin" };
            else if (userName.Contains("support"))
                return new List<string> { "Support" };
            else
                return new List<string> { "User" };
        }

        private async Task SaveRolesAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine($"Saving roles for: {UserName}");

                var selectedRoles = AvailableRoles
                    .Where(r => r.IsSelected)
                    .Select(r => r.RoleName)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Selected roles: {string.Join(", ", selectedRoles)}");

                var success = await _userService.UpdateUserRolesAsync(UserName, selectedRoles);

                if (success)
                {
                    System.Diagnostics.Debug.WriteLine("Roles saved successfully");
                    await Application.Current.MainPage.DisplayAlert("Success", "Roles have been updated successfully", "OK");
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Unable to save roles", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error SaveRolesAsync: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Error during save: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CancelAsync()
        {
            System.Diagnostics.Debug.WriteLine("Cancelling modifications");
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}