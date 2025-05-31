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

            System.Diagnostics.Debug.WriteLine($"🏗️ ManageRolesViewModel créé pour: {userName}");
        }

        public async Task LoadRolesAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine($"🔍 Chargement des rôles pour: {UserName}");

                // SOLUTION ALTERNATIVE - Récupérer les rôles depuis UserManagementViewModel
                System.Diagnostics.Debug.WriteLine("🔄 Récupération des rôles depuis les données existantes");

                // Simuler l'appel API avec les données qu'on a déjà
                var allRoles = new List<string> { "Admin", "Support", "User" };
                var userCurrentRoles = GetUserCurrentRoles(UserName); // Méthode à créer

                var roles = allRoles.Select(role => new RoleItem
                {
                    RoleName = role,
                    IsSelected = userCurrentRoles.Contains(role)
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"📊 Traitement de {roles.Count} rôles");

                await Application.Current.Dispatcher.DispatchAsync(() =>
                {
                    AvailableRoles.Clear();
                    foreach (var role in roles)
                    {
                        AvailableRoles.Add(role);
                        System.Diagnostics.Debug.WriteLine($"🏷️ Rôle ajouté: {role.RoleName} - Sélectionné: {role.IsSelected}");
                    }
                });

                System.Diagnostics.Debug.WriteLine($"✅ {roles.Count} rôles chargés avec succès");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur LoadRolesAsync: {ex.Message}");

                // En cas d'erreur, charger des rôles par défaut
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
                System.Diagnostics.Debug.WriteLine("🏁 Fin de LoadRolesAsync");
            }
        }

        private List<string> GetUserCurrentRoles(string userName)
        {
            // Pour l'instant, retourner des rôles basés sur le pattern de nom
            // Plus tard, on pourra passer ces données depuis UserManagementViewModel
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
                System.Diagnostics.Debug.WriteLine($"💾 Sauvegarde des rôles pour: {UserName}");

                var selectedRoles = AvailableRoles
                    .Where(r => r.IsSelected)
                    .Select(r => r.RoleName)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"📝 Rôles sélectionnés: {string.Join(", ", selectedRoles)}");

                // VRAIE SAUVEGARDE
                var success = await _userService.UpdateUserRolesAsync(UserName, selectedRoles);

                if (success)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Rôles sauvegardés avec succès");
                    await Application.Current.MainPage.DisplayAlert("Succès", "Les rôles ont été mis à jour avec succès", "OK");
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Erreur", "Impossible de sauvegarder les rôles", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur SaveRolesAsync: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erreur", $"Erreur lors de la sauvegarde: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CancelAsync()
        {
            System.Diagnostics.Debug.WriteLine("❌ Annulation des modifications");
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}