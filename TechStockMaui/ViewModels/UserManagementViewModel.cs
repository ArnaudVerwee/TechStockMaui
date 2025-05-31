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

            // Charger les utilisateurs au démarrage
            _ = Task.Run(LoadUsersAsync);
        }

        public async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;

                var users = await _userService.GetAllUsersAsync();

                // Mettre à jour sur le thread UI
                await Application.Current.Dispatcher.DispatchAsync(() =>
                {
                    Users.Clear();
                    foreach (var user in users)
                    {
                        Users.Add(user);
                    }
                });

                System.Diagnostics.Debug.WriteLine($"✅ {users.Count} utilisateurs chargés");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur LoadUsersAsync: {ex.Message}");

                await Application.Current.MainPage.DisplayAlert(
                    "Erreur",
                    "Impossible de charger les utilisateurs",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ManageUserRoles(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    return;

                System.Diagnostics.Debug.WriteLine($"🔧 Gestion des rôles pour: {userName}");

                // Trouver l'utilisateur
                var user = Users.FirstOrDefault(u => u.UserName == userName);
                if (user == null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Erreur",
                        "Utilisateur non trouvé",
                        "OK");
                    return;
                }

                // Naviguer vers la page de gestion des rôles
                await Application.Current.MainPage.Navigation.PushAsync(
                    new ManageRolesPage(userName));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur ManageUserRoles: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Erreur",
                    "Impossible d'ouvrir la gestion des rôles",
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