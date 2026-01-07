using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FitControlAdmin
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        private Brush _activeBtnBg;

        private string? _userTipo;
        private string? _userFuncao;

        public MainWindow(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;

            _activeBtnBg = (Brush?)new BrushConverter().ConvertFrom("#02acc3") ?? Brushes.Cyan;

            ExtractUserInfoFromToken();
            ConfigureMenuVisibilityByRole();

            InitializeDashboard();
            SetupSidebarHandlers();
            ShowDashboard();
            HighlightActiveButton(BtnDashboard);
        }

        #region Dashboard / Sidebar

        private void InitializeDashboard()
        {
            // Valores de exemplo; podes ligar isto a endpoints reais mais tarde
            TotalMembers.Text = "0";
            ActiveMembers.Text = "0";
            MonthlyRevenue.Text = "€0,00";

            var recentActivities = new List<Activity>
            {
                new Activity { Date = DateTime.Now.ToString("dd/MM"), Description = "Sistema iniciado" }
            };

            RecentActivityList.ItemsSource = recentActivities;
        }

        private void SetupSidebarHandlers()
        {
            BtnDashboard.Click += (s, e) =>
            {
                HighlightActiveButton(BtnDashboard);
                ShowDashboard();
            };

            BtnFuncionarios.Click += (s, e) =>
            {
                HighlightActiveButton(BtnFuncionarios);
                UserManagementTitle.Text = "Gestão de Funcionários";
                ShowUserManagement();
            };

            BtnMembros.Click += (s, e) =>
            {
                HighlightActiveButton(BtnMembros);
                UserManagementTitle.Text = "Gestão de Membros";
                ShowUserManagement();
            };

            BtnSubscricoes.Click += (s, e) =>
            {
                HighlightActiveButton(BtnSubscricoes);
                ShowPlaceholder("Gestão de Subscrições");
            };

            BtnPagamentos.Click += (s, e) =>
            {
                HighlightActiveButton(BtnPagamentos);
                ShowPaymentManagement();
            };

            BtnAulas.Click += (s, e) =>
            {
                HighlightActiveButton(BtnAulas);
                ShowPlaceholder("Gestão de Aulas");
            };

            BtnExercicios.Click += (s, e) =>
            {
                HighlightActiveButton(BtnExercicios);
                ShowExerciseManagement();
            };

            BtnAvaliacoesFisicas.Click += (s, e) =>
            {
                HighlightActiveButton(BtnAvaliacoesFisicas);
                ShowPhysicalEvaluationManagement();
            };

            BtnAtribuirPTs.Click += (s, e) =>
            {
                HighlightActiveButton(BtnAtribuirPTs);
                ShowPlaceholder("Atribuir PTs");
            };

            BtnRelatorios.Click += (s, e) =>
            {
                HighlightActiveButton(BtnRelatorios);
                ShowPlaceholder("Relatórios");
            };

            BtnConfig.Click += (s, e) =>
            {
                HighlightActiveButton(BtnConfig);
                ShowSettings();
            };

            BtnLogout.Click += async (s, e) =>
            {
                await LogoutAndReturnToLogin();
            };
        }

        private void ShowDashboard()
        {
            HideAllPanels();
            DashboardPanel.Visibility = Visibility.Visible;
        }

        private void ShowUserManagement()
        {
            HideAllPanels();
            UserManagementPanel.Visibility = Visibility.Visible;
            LoadUsers();
        }

        private void ShowExerciseManagement()
        {
            HideAllPanels();
            ExerciseManagementPanel.Visibility = Visibility.Visible;
            LoadExercises();
        }

        private void ShowPaymentManagement()
        {
            HideAllPanels();
            PaymentManagementPanel.Visibility = Visibility.Visible;
            LoadPayments();
        }

        private void ShowPhysicalEvaluationManagement()
        {
            HideAllPanels();
            PhysicalEvaluationManagementPanel.Visibility = Visibility.Visible;
            LoadPhysicalEvaluations();
        }

        private void ShowPlaceholder(string text)
        {
            HideAllPanels();
            PlaceholderText.Text = text;
            PlaceholderText.Visibility = Visibility.Visible;
        }

        private void HideAllPanels()
        {
            DashboardPanel.Visibility = Visibility.Collapsed;
            UserManagementPanel.Visibility = Visibility.Collapsed;
            SettingsPanel.Visibility = Visibility.Collapsed;
            ExerciseManagementPanel.Visibility = Visibility.Collapsed;
            PaymentManagementPanel.Visibility = Visibility.Collapsed;
            PhysicalEvaluationManagementPanel.Visibility = Visibility.Collapsed;
            PlaceholderText.Visibility = Visibility.Collapsed;
        }

        private async void ShowSettings()
        {
            HideAllPanels();
            SettingsPanel.Visibility = Visibility.Visible;
            await LoadCurrentUserData();
        }

        private async Task LoadCurrentUserData()
        {
            try
            {
                // Show loading state
                AccountTypeText.Text = "A carregar...";
                AccountNameText.Text = "A carregar...";
                AccountContactText.Text = "A carregar...";
                AccountFunctionText.Text = "A carregar...";
                SettingsEmailTextBox.Text = "A carregar...";

                var user = await _apiService.GetCurrentUserAsync();
                if (user != null)
                {
                    AccountTypeText.Text = user.Tipo ?? "-";
                    AccountNameText.Text = user.Nome ?? "-";
                    AccountContactText.Text = user.Telemovel ?? "-";
                    AccountFunctionText.Text = user.Funcao ?? "-";
                    SettingsEmailTextBox.Text = user.Email ?? "-";
                }
                else
                {
                    AccountTypeText.Text = "Erro ao carregar";
                    AccountNameText.Text = "Erro ao carregar";
                    AccountContactText.Text = "Erro ao carregar";
                    AccountFunctionText.Text = "-";
                    SettingsEmailTextBox.Text = "Erro ao carregar";
                    
                    MessageBox.Show("Não foi possível carregar os dados do utilizador. Verifique a conexão com o servidor.",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                AccountTypeText.Text = "Erro";
                AccountNameText.Text = "Erro";
                AccountContactText.Text = "Erro";
                AccountFunctionText.Text = "-";
                SettingsEmailTextBox.Text = "Erro";
                
                MessageBox.Show($"Erro ao carregar dados do utilizador: {ex.Message}",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var changePasswordDialog = new ChangePasswordDialog(_apiService);
            changePasswordDialog.Owner = this;
            changePasswordDialog.ShowDialog();
        }

        private void HighlightActiveButton(Button active)
        {
            var buttons = new[]
            {
                BtnDashboard, BtnFuncionarios, BtnMembros, BtnSubscricoes,
                BtnPagamentos, BtnAulas, BtnExercicios, BtnAvaliacoesFisicas, BtnAtribuirPTs, BtnRelatorios, BtnConfig
            };

            foreach (var btn in buttons)
            {
                if (btn == null) continue;

                var stackPanel = btn.Content as StackPanel;
                if (stackPanel == null || stackPanel.Children.Count < 2) continue;

                var emojiBlock = stackPanel.Children[0] as TextBlock;
                var textBlock = stackPanel.Children[1] as TextBlock;

                if (btn == active)
                {
                    btn.Background = (Brush?)new BrushConverter().ConvertFrom("#1a4d5c") ?? Brushes.DarkCyan;
                    if (emojiBlock != null) emojiBlock.Foreground = Brushes.White;
                    if (textBlock != null) textBlock.Foreground = Brushes.White;
                    btn.FontWeight = FontWeights.Normal;
                }
                else
                {
                    btn.Background = Brushes.Transparent;
                    if (emojiBlock != null) emojiBlock.Foreground = _activeBtnBg;
                    if (textBlock != null) textBlock.Foreground = Brushes.White;
                    btn.FontWeight = FontWeights.Normal;
                }
            }
        }

        #endregion

        #region User Management (existing logic)

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

        private void CreateMembroButton_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new CreateEditUserWindow(_apiService, null, "Membro");
            if (createWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void CreateFuncionarioButton_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new CreateEditUserWindow(_apiService, null, "Funcionario");
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

        #endregion

        #region Role-based menu visibility (using JWT)

        private void ExtractUserInfoFromToken()
        {
            var token = _apiService.CurrentAccessToken;
            if (string.IsNullOrEmpty(token))
                return;

            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2)
                    return;

                string payload = parts[1];
                payload = payload.Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var bytes = Convert.FromBase64String(payload);
                using var doc = JsonDocument.Parse(bytes);
                var root = doc.RootElement;

                if (root.TryGetProperty("Tipo", out var tipoProp))
                {
                    _userTipo = tipoProp.GetString();
                }

                if (root.TryGetProperty("Funcao", out var funcaoProp))
                {
                    _userFuncao = funcaoProp.GetString();
                }
            }
            catch
            {
                // Se falhar o parse, mantemos os valores nulos
            }
        }

        private void ConfigureMenuVisibilityByRole()
        {
            // Defaults: tudo visível
            BtnFuncionarios.Visibility = Visibility.Visible;
            BtnMembros.Visibility = Visibility.Visible;
            BtnSubscricoes.Visibility = Visibility.Visible;
            BtnPagamentos.Visibility = Visibility.Visible;
            BtnAulas.Visibility = Visibility.Visible;
            BtnExercicios.Visibility = Visibility.Visible;
            BtnAvaliacoesFisicas.Visibility = Visibility.Visible;
            BtnAtribuirPTs.Visibility = Visibility.Visible;
            BtnRelatorios.Visibility = Visibility.Visible;
            BtnConfig.Visibility = Visibility.Visible;

            if (string.Equals(_userTipo, "Membro", StringComparison.OrdinalIgnoreCase))
            {
                // Membros veem apenas dashboard e talvez aulas
                BtnFuncionarios.Visibility = Visibility.Collapsed;
                BtnMembros.Visibility = Visibility.Collapsed;
                BtnSubscricoes.Visibility = Visibility.Collapsed;
                BtnPagamentos.Visibility = Visibility.Collapsed;
                BtnExercicios.Visibility = Visibility.Collapsed;
                BtnAvaliacoesFisicas.Visibility = Visibility.Collapsed;
                BtnAtribuirPTs.Visibility = Visibility.Collapsed;
                BtnRelatorios.Visibility = Visibility.Collapsed;
                BtnConfig.Visibility = Visibility.Collapsed;
            }
            else if (string.Equals(_userTipo, "Funcionario", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(_userFuncao, "Rececao", StringComparison.OrdinalIgnoreCase))
                {
                    // Receção não gere PTs nem relatórios avançados
                    BtnAtribuirPTs.Visibility = Visibility.Collapsed;
                    BtnRelatorios.Visibility = Visibility.Collapsed;
                }
                else if (string.Equals(_userFuncao, "PT", StringComparison.OrdinalIgnoreCase))
                {
                    // PT focado em membros, aulas e exercícios
                    BtnFuncionarios.Visibility = Visibility.Collapsed;
                    BtnSubscricoes.Visibility = Visibility.Collapsed;
                    BtnPagamentos.Visibility = Visibility.Collapsed;
                    BtnAvaliacoesFisicas.Visibility = Visibility.Visible; // PTs podem ver avaliações físicas
                    BtnAtribuirPTs.Visibility = Visibility.Collapsed;
                    BtnRelatorios.Visibility = Visibility.Collapsed;
                    BtnConfig.Visibility = Visibility.Collapsed;
                }
                // Admin vê tudo
            }
        }

        #endregion

        #region Logout

        private async Task LogoutAndReturnToLogin()
        {
            // Show confirmation dialog
            var confirmDialog = new ConfirmDialog(
                "Tem certeza que deseja fazer logout?",
                "Confirmar Logout");
            
            confirmDialog.Owner = this;
            
            if (confirmDialog.ShowDialog() != true || !confirmDialog.Result)
            {
                return; // User cancelled
            }

            try
            {
                // Call logout API
                await _apiService.LogoutAsync();

                // Close MainWindow and open LoginWindow
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                // Even if logout fails, still return to login
                MessageBox.Show($"Erro ao fazer logout: {ex.Message}", 
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        #endregion

        #region Exercise Management

        private List<ExerciseResponseDto>? _allExercises = null;

        private async void LoadExercises()
        {
            ExerciseStatusText.Text = "A carregar exercícios...";
            try
            {
                var exercises = await _apiService.GetExercisesByStateAsync(true);
                if (exercises != null)
                {
                    _allExercises = exercises;
                    ExercisesItemsControl.ItemsSource = exercises;
                    ExerciseStatusText.Text = $"Total: {exercises.Count} exercícios ativos";
                }
                else
                {
                    ExerciseStatusText.Text = "Erro ao carregar exercícios";
                    MessageBox.Show("Erro ao carregar exercícios. Verifique a conexão com o servidor.",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ExerciseStatusText.Text = "Erro: " + ex.Message;
                MessageBox.Show($"Erro: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshExercisesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadExercises();
        }

        private void ExerciseSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allExercises == null) return;

            var searchText = ExerciseSearchTextBox.Text?.ToLower() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                ExercisesItemsControl.ItemsSource = _allExercises;
            }
            else
            {
                var filtered = _allExercises.Where(ex =>
                    ex.Nome.ToLower().Contains(searchText) ||
                    ex.Descricao.ToLower().Contains(searchText) ||
                    ex.GrupoMuscular.ToString().ToLower().Contains(searchText)
                ).ToList();
                
                ExercisesItemsControl.ItemsSource = filtered;
                ExerciseStatusText.Text = $"Mostrando {filtered.Count} de {_allExercises.Count} exercícios";
            }
        }

        private void CreateExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new CreateEditExerciseWindow(_apiService, null);
            createWindow.Owner = this;
            if (createWindow.ShowDialog() == true)
            {
                LoadExercises();
            }
        }

        private async void EditExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int exerciseId)
            {
                try
                {
                    // Buscar o exercício completo da lista
                    var exercise = _allExercises?.FirstOrDefault(ex => ex.IdExercicio == exerciseId);
                    if (exercise == null)
                    {
                        MessageBox.Show("Exercício não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var editWindow = new CreateEditExerciseWindow(_apiService, exercise);
                    editWindow.Owner = this;
                    if (editWindow.ShowDialog() == true)
                    {
                        LoadExercises();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar exercício: {ex.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int exerciseId)
            {
                var result = MessageBox.Show(
                    "Tem certeza que deseja desativar este exercício?",
                    "Confirmar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var (success, errorMessage) = await _apiService.ChangeExerciseStatusAsync(exerciseId, false);
                        if (success)
                        {
                            MessageBox.Show("Exercício desativado com sucesso!",
                                "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadExercises();
                        }
                        else
                        {
                            MessageBox.Show(errorMessage ?? "Erro ao desativar exercício.",
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

        #endregion

        #region Payment Management

        private async void LoadPayments()
        {
            PaymentStatusText.Text = "A carregar pagamentos...";
            try
            {
                var payments = await _apiService.GetPaymentsByActiveStateAsync(true);
                if (payments != null)
                {
                    PaymentsDataGrid.ItemsSource = payments;
                    PaymentStatusText.Text = $"Total: {payments.Count} pagamentos ativos";
                }
                else
                {
                    PaymentStatusText.Text = "Erro ao carregar pagamentos";
                    MessageBox.Show("Erro ao carregar pagamentos. Verifique a conexão com o servidor.",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                PaymentStatusText.Text = "Erro: " + ex.Message;
                MessageBox.Show($"Erro: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshPaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPayments();
        }

        private void CreatePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Criar janela de criar pagamento
            MessageBox.Show("Funcionalidade de criar pagamento será implementada.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void EditPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int paymentId)
            {
                // TODO: Criar janela de editar pagamento
                MessageBox.Show($"Editar pagamento {paymentId} - Funcionalidade será implementada.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeletePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int paymentId)
            {
                var result = MessageBox.Show(
                    "Tem certeza que deseja desativar este pagamento?",
                    "Confirmar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var (success, errorMessage) = await _apiService.ChangePaymentStatusAsync(paymentId, false);
                        if (success)
                        {
                            MessageBox.Show("Pagamento desativado com sucesso!",
                                "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadPayments();
                        }
                        else
                        {
                            MessageBox.Show(errorMessage ?? "Erro ao desativar pagamento.",
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

        #endregion

        #region Physical Evaluation Management

        private async void LoadPhysicalEvaluations()
        {
            PhysicalEvaluationStatusText.Text = "A carregar avaliações físicas...";
            try
            {
                // Por enquanto, vamos precisar de um membro específico ou listar todas
                // Vou criar um método genérico que pode ser expandido depois
                PhysicalEvaluationStatusText.Text = "Selecione um membro para ver avaliações";
                // TODO: Implementar listagem completa quando a API suportar
            }
            catch (Exception ex)
            {
                PhysicalEvaluationStatusText.Text = "Erro: " + ex.Message;
                MessageBox.Show($"Erro: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshPhysicalEvaluationsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPhysicalEvaluations();
        }

        private void CreatePhysicalEvaluationButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Criar janela de criar avaliação física
            MessageBox.Show("Funcionalidade de criar avaliação física será implementada.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }

    public class Activity
    {
        public string Date { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
