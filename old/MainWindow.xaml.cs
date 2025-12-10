using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System.Windows;
using System.Windows.Controls;

namespace FitControlAdmin
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;

        public MainWindow(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            LoadUsers();
        }

        private async void LoadUsers()
        {
            StatusText.Text = "A carregar utilizadores...";
            try
            {
                var users = await _apiService.GetUsersAsync();
                if (users != null)
                {
                    MembersDataGrid.ItemsSource = users;
                    StatusText.Text = $"Total: {users.Count} utilizadores";
                }
                else
                {
                    StatusText.Text = "Erro ao carregar utilizadores";
                    MessageBox.Show("Erro ao carregar utilizadores. Verifique a conexão com o servidor.", 
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Erro: " + ex.Message;
                MessageBox.Show($"Erro: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new CreateEditUserWindow(_apiService);
            if (createWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                try
                {
                    var user = await _apiService.GetUserAsync(userId);
                    if (user != null)
                    {
                        var editWindow = new CreateEditUserWindow(_apiService, user);
                        if (editWindow.ShowDialog() == true)
                        {
                            LoadUsers();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar utilizador: {ex.Message}", 
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                var result = MessageBox.Show(
                    "Tem certeza que deseja desativar este utilizador?",
                    "Confirmar", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var success = await _apiService.DeleteUserAsync(userId);
                        if (success)
                        {
                            MessageBox.Show("Utilizador desativado com sucesso!", 
                                "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadUsers();
                        }
                        else
                        {
                            MessageBox.Show("Erro ao desativar utilizador.", 
                                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro: {ex.Message}", 
                            "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
