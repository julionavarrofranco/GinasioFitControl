using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
                ShowEmployeeManagement();
            };

            BtnMembros.Click += (s, e) =>
            {
                HighlightActiveButton(BtnMembros);
                UserManagementTitle.Text = "Gestão de Membros";
                ShowUserManagement();
            };

            BtnSubscricoes.Click += async (s, e) =>
            {
                HighlightActiveButton(BtnSubscricoes);
                await ShowSubscriptionManagement();
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

        private void ShowEmployeeManagement()
        {
            HideAllPanels();
            EmployeeManagementPanel.Visibility = Visibility.Visible;
            LoadEmployees();
        }

        private async void LoadEmployees()
        {
            EmployeeStatusText.Text = "A carregar funcionários...";
            try
            {
                var employees = await _apiService.GetAllEmployeesAsync();
                if (employees != null)
                {
                    EmployeesDataGrid.ItemsSource = employees;
                    EmployeeStatusText.Text = $"Total: {employees.Count} funcionários";
                }
                else
                {
                    EmployeeStatusText.Text = "Erro ao carregar funcionários";
                    MessageBox.Show("Erro ao carregar funcionários. Verifique a conexão com o servidor.",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                EmployeeStatusText.Text = "Erro: " + ex.Message;
                MessageBox.Show($"Erro: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshEmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }

        private async void EditEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                try
                {
                    // Get EmployeeDto from DataContext
                    EmployeeDto? employee = null;

                    // Try to get from DataGridRow
                    var parent = button.Parent;
                    while (parent != null && !(parent is System.Windows.Controls.DataGridRow))
                    {
                        parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
                    }

                    if (parent is System.Windows.Controls.DataGridRow row && row.Item is EmployeeDto rowEmployee)
                    {
                        employee = rowEmployee;
                    }
                    else if (button.DataContext is EmployeeDto contextEmployee)
                    {
                        employee = contextEmployee;
                    }
                    else if (EmployeesDataGrid.SelectedItem is EmployeeDto selectedEmployee)
                    {
                        employee = selectedEmployee;
                    }

                    if (employee == null)
                    {
                        MessageBox.Show("Não foi possível identificar o funcionário a editar.",
                            "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Convert EmployeeDto to UserDto
                    var user = new UserDto
                    {
                        IdUser = employee.IdUser,
                        Email = employee.Email,
                        Nome = employee.Nome,
                        Telemovel = employee.Telemovel,
                        Tipo = "Funcionario",
                        Ativo = true, // EmployeeDto doesn't have Ativo property, assume active
                        Funcao = employee.Funcao
                    };

                    var editWindow = new CreateEditUserWindow(_apiService, user);
                    editWindow.Owner = this;
                    if (editWindow.ShowDialog() == true)
                    {
                        LoadEmployees();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar funcionário: {ex.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowExerciseManagement()
        {
            HideAllPanels();
            ExerciseManagementPanel.Visibility = Visibility.Visible;
            LoadExercises();
        }

        private async void ShowPaymentManagement()
        {
            HideAllPanels();
            PaymentManagementPanel.Visibility = Visibility.Visible;
            await LoadPayments();
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
            EmployeeManagementPanel.Visibility = Visibility.Collapsed;
            SettingsPanel.Visibility = Visibility.Collapsed;
            ExerciseManagementPanel.Visibility = Visibility.Collapsed;
            PaymentManagementPanel.Visibility = Visibility.Collapsed;
            PhysicalEvaluationManagementPanel.Visibility = Visibility.Collapsed;
            SubscriptionManagementPanel.Visibility = Visibility.Collapsed;
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
            var buttons = new[] { BtnDashboard, BtnFuncionarios, BtnMembros, BtnSubscricoes, BtnPagamentos, BtnExercicios, BtnAvaliacoesFisicas, BtnConfig };

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
                // Load users and subscriptions in parallel
                var usersTask = _apiService.GetUsersAsync();
                var subscriptionsTask = _apiService.GetSubscriptionsByStateAsync(true);
                
                await Task.WhenAll(usersTask, subscriptionsTask);
                
                var users = await usersTask;
                _subscriptionsForMembers = await subscriptionsTask;
                
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
            if (sender is Button button)
            {
                try
                {
                    // Get MemberDto from DataContext
                    MemberDto? member = null;

                    // Try to get from DataGridRow
                    var parent = button.Parent;
                    while (parent != null && !(parent is System.Windows.Controls.DataGridRow))
                    {
                        parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
                    }

                    if (parent is System.Windows.Controls.DataGridRow row && row.Item is MemberDto rowMember)
                    {
                        member = rowMember;
                    }
                    else if (button.DataContext is MemberDto contextMember)
                    {
                        member = contextMember;
                    }
                    else if (MembersDataGrid.SelectedItem is MemberDto selectedMember)
                    {
                        member = selectedMember;
                    }

                    if (member == null)
                    {
                        MessageBox.Show("Não foi possível identificar o membro a editar.",
                            "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Convert MemberDto to UserDto
                    var user = new UserDto
                    {
                        IdUser = member.IdUser,
                        Email = member.Email,
                        Nome = member.Nome,
                        Telemovel = member.Telemovel,
                        Tipo = "Membro",
                        Ativo = member.Ativo,
                        DataNascimento = member.DataNascimento,
                        IdSubscricao = null // Will be loaded from subscription name if needed
                    };

                    // Try to get subscription ID from subscription name
                    if (!string.IsNullOrEmpty(member.Subscricao))
                    {
                        // Load subscriptions if not already loaded
                        if (_subscriptionsForMembers == null)
                        {
                            _subscriptionsForMembers = await _apiService.GetSubscriptionsByStateAsync(true);
                        }
                        
                        if (_subscriptionsForMembers != null)
                        {
                            var subscription = _subscriptionsForMembers.FirstOrDefault(s => s.Nome == member.Subscricao);
                            if (subscription != null)
                            {
                                user.IdSubscricao = subscription.IdSubscricao;
                            }
                        }
                    }

                    var editWindow = new CreateEditUserWindow(_apiService, user);
                    editWindow.Owner = this;
                    if (editWindow.ShowDialog() == true)
                    {
                        LoadUsers();
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
            if (sender is Button button)
            {
                try
                {
                    // Get MemberDto from DataContext
                    MemberDto? member = null;

                    // Try to get from DataGridRow
                    var parent = button.Parent;
                    while (parent != null && !(parent is System.Windows.Controls.DataGridRow))
                    {
                        parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
                    }

                    if (parent is System.Windows.Controls.DataGridRow row && row.Item is MemberDto rowMember)
                    {
                        member = rowMember;
                    }
                    else if (button.DataContext is MemberDto contextMember)
                    {
                        member = contextMember;
                    }
                    else if (MembersDataGrid.SelectedItem is MemberDto selectedMember)
                    {
                        member = selectedMember;
                    }

                    if (member == null)
                    {
                        MessageBox.Show("Não foi possível identificar o membro a remover.",
                            "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var result = MessageBox.Show(
                        "Tem certeza que deseja desativar este utilizador?",
                        "Confirmar",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Use the change-active-status endpoint instead
                        var success = await _apiService.ChangeUserActiveStatusAsync(member.IdUser, false);
                        if (success)
                        {
                            MessageBox.Show("Utilizador desativado com sucesso!",
                                "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadUsers();
                        }
                        else
                        {
                            MessageBox.Show($"Erro ao desativar utilizador (ID: {member.IdUser}). Verifique se tem permissões.",
                                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro: {ex.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (string.Equals(_userFuncao, "PT", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_userFuncao, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                BtnAvaliacoesFisicas.Visibility = Visibility.Visible;
            }
            else
            {
                BtnAvaliacoesFisicas.Visibility = Visibility.Collapsed;
            }
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
                BtnConfig.Visibility = Visibility.Collapsed;
            }
            else if (string.Equals(_userTipo, "Funcionario", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(_userFuncao, "Rececao", StringComparison.OrdinalIgnoreCase))
                {
                    // Receção tem acesso limitado
                    // (botões já removidos do menu)
                }
                else if (string.Equals(_userFuncao, "PT", StringComparison.OrdinalIgnoreCase))
                {
                    // PT focado em membros, aulas e exercícios
                    BtnFuncionarios.Visibility = Visibility.Collapsed;
                    BtnSubscricoes.Visibility = Visibility.Collapsed;
                    BtnPagamentos.Visibility = Visibility.Collapsed;
                    BtnAvaliacoesFisicas.Visibility = Visibility.Visible; // PTs podem ver avaliações físicas
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

        private void LoadExercises()
        {
            LoadExercisesAsync();
        }

        private async Task LoadExercisesAsync()
        {
            ExerciseStatusText.Text = "A carregar exercícios...";
            try
            {
                // Carregar todos os exercícios (ativos e inativos)
                var activeExercises = await _apiService.GetExercisesByStateAsync(true);
                var inactiveExercises = await _apiService.GetExercisesByStateAsync(false);
                
                _allExercises = new List<ExerciseResponseDto>();
                if (activeExercises != null) _allExercises.AddRange(activeExercises);
                if (inactiveExercises != null) _allExercises.AddRange(inactiveExercises);

                // Popular ComboBox de grupos musculares
                if (ExerciseGrupoMuscularFilterComboBox.Items.Count == 1) // Só tem "Todos"
                {
                    foreach (GrupoMuscular grupo in Enum.GetValues(typeof(GrupoMuscular)))
                    {
                        var displayName = FormatEnumName(grupo.ToString());
                        var item = new ComboBoxItem { Content = displayName, Tag = grupo };
                        ExerciseGrupoMuscularFilterComboBox.Items.Add(item);
                    }
                }

                // Aplicar filtros após carregar
                ApplyExerciseFilters();
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

        private void ExerciseFilters_Changed(object sender, RoutedEventArgs e)
        {
            ApplyExerciseFilters();
        }

        private void ApplyExerciseFilters()
        {
            if (_allExercises == null) return;

            var filtered = _allExercises.AsEnumerable();

            // Filtro por nome (LIKE)
            var nomeFilter = ExerciseNameFilterTextBox?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(nomeFilter))
            {
                filtered = filtered.Where(ex => 
                    ex.Nome.ToLower().Contains(nomeFilter.ToLower()));
            }

            // Filtro por estado
            var estadoSelected = (ExerciseEstadoFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (estadoSelected == "Ativo")
            {
                filtered = filtered.Where(ex => ex.Ativo);
            }
            else if (estadoSelected == "Inativo")
            {
                filtered = filtered.Where(ex => !ex.Ativo);
            }
            // "Todos" não filtra

            // Filtro por grupo muscular
            var grupoSelectedItem = ExerciseGrupoMuscularFilterComboBox?.SelectedItem as ComboBoxItem;
            if (grupoSelectedItem != null && grupoSelectedItem.Tag is GrupoMuscular grupo)
            {
                filtered = filtered.Where(ex => ex.GrupoMuscular == grupo);
            }

            // Ordenação
            var sortBy = (ExerciseSortByComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Nome";
            var sortDirection = (ExerciseSortDirectionComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Ascendente";
            var ascending = sortDirection == "Ascendente";

            filtered = sortBy switch
            {
                "Nome" => ascending 
                    ? filtered.OrderBy(ex => ex.Nome) 
                    : filtered.OrderByDescending(ex => ex.Nome),
                "Grupo Muscular" => ascending 
                    ? filtered.OrderBy(ex => ex.GrupoMuscular.ToString()) 
                    : filtered.OrderByDescending(ex => ex.GrupoMuscular.ToString()),
                "Estado" => ascending 
                    ? filtered.OrderBy(ex => ex.Ativo) 
                    : filtered.OrderByDescending(ex => ex.Ativo),
                _ => filtered.OrderBy(ex => ex.Nome)
            };

            var result = filtered.ToList();
            
            // Atualizar ItemsSource explicitamente
            ExercisesItemsControl.ItemsSource = null;
            ExercisesItemsControl.ItemsSource = result;
            
            ExerciseStatusText.Text = $"Mostrando {result.Count} de {_allExercises.Count} exercícios";
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
                    "Tem certeza que deseja eliminar este exercício?",
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
                            // Recarregar a lista completa da API
                            await LoadExercisesAsync();
                            
                            MessageBox.Show("Exercício eliminado com sucesso!",
                                "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(errorMessage ?? "Erro ao eliminar exercício.",
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

        private List<MemberDto>? _allMembers = null;
        private List<SubscriptionResponseDto>? _allSubscriptions = null;
        private List<SubscriptionResponseDto>? _subscriptionsForMembers = null;

        private async Task LoadPayments()
        {
            System.Diagnostics.Debug.WriteLine("LoadPayments: Starting to load payments");

            // Atualizar na thread da UI
            Dispatcher.Invoke(() =>
            {
                PaymentStatusText.Text = "A carregar pagamentos...";
                PaymentsDataGrid.ItemsSource = null; // Limpar primeiro
            });

            try
            {
                // Carregar pagamentos, membros e subscrições em paralelo
                var paymentsTask = _apiService.GetPaymentsByActiveStateAsync(true);
                var inactivePaymentsTask = _apiService.GetPaymentsByActiveStateAsync(false);
                var membersTask = _apiService.GetAllMembersAsync();
                var subscriptionsTask = _apiService.GetSubscriptionsByStateAsync(true);
                var inactiveSubscriptionsTask = _apiService.GetSubscriptionsByStateAsync(false);

                await Task.WhenAll(paymentsTask, inactivePaymentsTask, membersTask, subscriptionsTask, inactiveSubscriptionsTask);

                var activePayments = await paymentsTask;
                var inactivePayments = await inactivePaymentsTask;
                var members = await membersTask;
                var activeSubscriptions = await subscriptionsTask;
                var inactiveSubscriptions = await inactiveSubscriptionsTask;

                // Debug: Verificar se os dados foram carregados
                System.Diagnostics.Debug.WriteLine($"Active Payments: {activePayments?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"Inactive Payments: {inactivePayments?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"Members: {members?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"Active Subscriptions: {activeSubscriptions?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"Inactive Subscriptions: {inactiveSubscriptions?.Count ?? 0}");

                // Combinar todos os pagamentos
                var allPayments = new List<PaymentResponseDto>();
                if (activePayments != null) allPayments.AddRange(activePayments);
                if (inactivePayments != null) allPayments.AddRange(inactivePayments);

                // Combinar todas as subscrições
                var allSubscriptions = new List<SubscriptionResponseDto>();
                if (activeSubscriptions != null) allSubscriptions.AddRange(activeSubscriptions);
                if (inactiveSubscriptions != null) allSubscriptions.AddRange(inactiveSubscriptions);

                // Debug: Log payment details
                foreach (var payment in allPayments)
                {
                    System.Diagnostics.Debug.WriteLine($"LoadPayments: Payment {payment.IdPagamento} - Subscricao: '{payment.Subscricao}'");
                }

                // Criar lista de display com todos os dados dos pagamentos
                var displayList = allPayments.Select(payment =>
                {
                    var member = members?.FirstOrDefault(m => m.IdMembro == payment.IdMembro);

                    var subscription = allSubscriptions.FirstOrDefault(s => s.Tipo.ToString() == payment.Subscricao);

                    var displayModel = new PaymentDisplayModel
                    {
                        IdPagamento = payment.IdPagamento,
                        IdMembro = payment.IdMembro,
                        NomeMembro = member?.Nome ?? "Membro não encontrado",
                        NomeSubscricao = payment.Subscricao ?? "Subscrição não encontrada",
                        DataPagamento = payment.DataPagamento,
                        ValorPago = payment.ValorPago,
                        MetodoPagamento = FormatEnumName(payment.MetodoPagamento.ToString()),
                        EstadoPagamento = FormatEnumName(payment.EstadoPagamento.ToString()),
                        MesReferente = payment.MesReferente,
                        DataRegisto = payment.DataRegisto,
                        Ativo = payment.DataDesativacao == null,
                        StatusAtivo = payment.DataDesativacao == null ? "Ativo" : payment.DataDesativacao.Value.ToString("dd/MM/yyyy")
                    };

                    return displayModel;
                }).OrderByDescending(p => p.DataRegisto).ToList();

                System.Diagnostics.Debug.WriteLine($"LoadPayments: Created {displayList.Count} display items");

                // Atualizar na thread da UI
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // Sempre definir ItemsSource, mesmo que seja uma lista vazia
                        PaymentsDataGrid.ItemsSource = null; // Limpar primeiro
                        PaymentsDataGrid.ItemsSource = displayList;
                        
                        // Forçar atualização do DataGrid
                        PaymentsDataGrid.UpdateLayout();
                        
                        PaymentStatusText.Text = displayList.Count > 0 
                            ? $"Total: {displayList.Count} pagamentos" 
                            : "Nenhum pagamento encontrado";
                    }
                    catch (Exception uiEx)
                    {
                        PaymentStatusText.Text = $"Erro ao atualizar interface: {uiEx.Message}";
                        MessageBox.Show($"Erro ao atualizar interface: {uiEx.Message}", "Erro",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
            catch (Exception ex)
            {
                // Atualizar na thread da UI
                Dispatcher.Invoke(() =>
                {
                    PaymentsDataGrid.ItemsSource = null;
                    PaymentStatusText.Text = "Erro: " + ex.Message;
                });
                
                MessageBox.Show($"Erro ao carregar pagamentos: {ex.Message}\n\nDetalhes: {ex}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string FormatEnumName(string enumName)
        {
            // Casos especiais
            if (enumName == "MBWay") return "MBWay";
            if (enumName == "Cartao") return "Cartão";
            if (enumName == "Bracos") return "Braços";
            if (enumName == "Mensal") return "mensal";
            if (enumName == "Trimestral") return "trimestral";
            if (enumName == "Anual") return "anual";

            // Adiciona espaço antes de letras maiúsculas (exceto a primeira)
            return System.Text.RegularExpressions.Regex.Replace(enumName, "(?<!^)([A-Z])", " $1");
        }

        private async void RefreshPaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadPayments();
        }

        private async void EditPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idPagamento)
            {
                try
                {
                    // Debug: Log the payment ID being edited
                    System.Diagnostics.Debug.WriteLine($"EditPaymentButton_Click: Editing payment {idPagamento}");

                    // Get payment details from display model
                    PaymentDisplayModel? displayPayment = null;
                    if (PaymentsDataGrid.SelectedItem is PaymentDisplayModel selectedPayment)
                    {
                        displayPayment = selectedPayment;
                    }
                    else
                    {
                        // Try to find in ItemsSource
                        if (PaymentsDataGrid.ItemsSource is System.Collections.IEnumerable items)
                        {
                            foreach (PaymentDisplayModel item in items)
                            {
                                if (item.IdPagamento == idPagamento)
                                {
                                    displayPayment = item;
                                    break;
                                }
                            }
                        }
                    }

                    if (displayPayment == null)
                    {
                        MessageBox.Show("Pagamento não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Get the payment details from API
                    var allPayments = await _apiService.GetPaymentsByActiveStateAsync(true);
                    var inactivePayments = await _apiService.GetPaymentsByActiveStateAsync(false);
                    var allPaymentsList = new List<PaymentResponseDto>();
                    if (allPayments != null) allPaymentsList.AddRange(allPayments);
                    if (inactivePayments != null) allPaymentsList.AddRange(inactivePayments);

                    var payment = allPaymentsList.FirstOrDefault(p => p.IdPagamento == idPagamento);

                    if (payment == null)
                    {
                        MessageBox.Show("Pagamento não encontrado na API.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Debug: Log payment details before editing
                    System.Diagnostics.Debug.WriteLine($"EditPaymentButton_Click: Payment before edit - Id: {payment.IdPagamento}, Subscricao: {payment.Subscricao}");

                    // Get member details
                    var members = await _apiService.GetAllMembersAsync();
                    var member = members?.FirstOrDefault(m => m.IdMembro == payment.IdMembro);

                    if (member == null)
                    {
                        MessageBox.Show("Membro não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Open CreatePaymentWindow in edit mode
                    var createPaymentWindow = new CreatePaymentWindow(_apiService, payment.IdMembro, payment, member);
                    createPaymentWindow.Owner = this;
                    var result = createPaymentWindow.ShowDialog();
                    System.Diagnostics.Debug.WriteLine($"EditPaymentButton_Click: Dialog result: {result}");
                    if (result == true)
                    {
                        System.Diagnostics.Debug.WriteLine("EditPaymentButton_Click: Reloading payments after edit");
                        await LoadPayments();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao abrir pagamento: {ex.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void MarkPaymentAsPaidButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int paymentId)
            {
                var result = MessageBox.Show(
                    "Tem certeza que deseja marcar este pagamento como pago?",
                    "Confirmar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Atualizar pagamento para Pago
                        var updateDto = new UpdatePaymentDto
                        {
                            EstadoPagamento = EstadoPagamento.Pago
                        };

                        var (success, errorMessage) = await _apiService.UpdatePaymentAsync(paymentId, updateDto);
                        if (success)
                        {
                            MessageBox.Show("Pagamento marcado como pago com sucesso!",
                                "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadPayments();
                        }
                        else
                        {
                            MessageBox.Show(errorMessage ?? "Erro ao marcar pagamento como pago.",
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
                // Carregar as 3 tabelas em paralelo
                var activeReservationsTask = _apiService.GetActiveReservationsAsync();
                var completedReservationsTask = _apiService.GetCompletedReservationsAsync();
                var membersTask = _apiService.GetAllMembersAsync();

                await Task.WhenAll(activeReservationsTask, completedReservationsTask, membersTask);

                var activeReservations = await activeReservationsTask;
                var completedReservations = await completedReservationsTask;
                var members = await membersTask;

                // Preencher tabela de reservas ativas
                if (activeReservations != null)
                {
                    // Mapear o estado para português
                    foreach (var reservation in activeReservations)
                    {
                        reservation.Estado = MapEstadoToPortuguese(reservation.Estado);
                    }
                    ReservationsDataGrid.ItemsSource = activeReservations;
                }
                else
                {
                    ReservationsDataGrid.ItemsSource = new List<PhysicalEvaluationReservationResponseDto>();
                }

                // Preencher tabela de reservas completas
                if (completedReservations != null)
                {
                    CompletedReservationsDataGrid.ItemsSource = completedReservations;
                }
                else
                {
                    CompletedReservationsDataGrid.ItemsSource = new List<PhysicalEvaluationReservationResponseDto>();
                }

                // Preencher tabela de membros (histórico)
                if (members != null)
                {
                    MembersHistoryDataGrid.ItemsSource = members;
                }
                else
                {
                    MembersHistoryDataGrid.ItemsSource = new List<MemberDto>();
                }

                PhysicalEvaluationStatusText.Text = $"Reservas: {activeReservations?.Count ?? 0} | Completas: {completedReservations?.Count ?? 0} | Membros: {members?.Count ?? 0}";
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

        private async void ReserveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idAvaliacao)
            {
                try
                {
                    // Buscar a reserva para obter informações do membro
                    var activeReservations = await _apiService.GetActiveReservationsAsync();
                    var reservation = activeReservations?.FirstOrDefault(r => r.IdAvaliacao == idAvaliacao);
                    
                    if (reservation == null)
                    {
                        MessageBox.Show("Reserva não encontrada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Obter IdFuncionario do utilizador atual
                    var currentUser = await _apiService.GetCurrentUserAsync();
                    if (currentUser == null)
                    {
                        MessageBox.Show("Não foi possível obter informações do utilizador atual.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Buscar funcionário pelo IdUser
                    var employees = await _apiService.GetAllEmployeesAsync();
                    var employee = employees?.FirstOrDefault(e => e.IdUser == currentUser.IdUser);
                    
                    if (employee == null)
                    {
                        MessageBox.Show("Utilizador atual não é um funcionário válido.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Abrir janela para inserir dados da avaliação física
                    var createWindow = new CreatePhysicalEvaluationWindow(_apiService, reservation.IdMembro, employee.IdFuncionario, idAvaliacao);
                    createWindow.Owner = this;
                    if (createWindow.ShowDialog() == true)
                    {
                        LoadPhysicalEvaluations();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao abrir janela de reserva: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ViewMemberHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idMembro)
            {
                try
                {
                    // Buscar informações do membro
                    var members = await _apiService.GetAllMembersAsync();
                    var member = members?.FirstOrDefault(m => m.IdMembro == idMembro);

                    if (member == null)
                    {
                        MessageBox.Show("Membro não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Abrir janela de histórico
                    var historyWindow = new MemberEvaluationHistoryWindow(_apiService, member);
                    historyWindow.Owner = this;
                    historyWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao abrir histórico: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string MapEstadoToPortuguese(string estado)
        {
            return estado switch
            {
                "Reservado" => "Pendente",
                "Presente" => "Em Andamento",
                "Cancelado" => "Cancelado",
                "Faltou" => "Faltou",
                _ => estado
            };
        }

        #endregion

        #region Subscription Management

        private async Task ShowSubscriptionManagement()
        {
            HideAllPanels();
            SubscriptionManagementPanel.Visibility = Visibility.Visible;
            await LoadSubscriptions();
        }

        private async Task LoadSubscriptions()
        {
            SubscriptionStatusText.Text = "A carregar subscrições...";
            try
            {
                // Carregar subscrições ativas e inativas
                var activeSubscriptionsTask = _apiService.GetSubscriptionsByStateAsync(true);
                var inactiveSubscriptionsTask = _apiService.GetSubscriptionsByStateAsync(false);

                await Task.WhenAll(activeSubscriptionsTask, inactiveSubscriptionsTask);

                var activeSubscriptions = await activeSubscriptionsTask;
                var inactiveSubscriptions = await inactiveSubscriptionsTask;

                var allSubscriptions = new List<SubscriptionResponseDto>();
                if (activeSubscriptions != null) allSubscriptions.AddRange(activeSubscriptions);
                if (inactiveSubscriptions != null) allSubscriptions.AddRange(inactiveSubscriptions);

                // Ordenar por nome e depois por tipo
                allSubscriptions = allSubscriptions
                    .OrderBy(s => s.Nome)
                    .ThenBy(s => s.Tipo)
                    .ToList();

                SubscriptionsDataGrid.ItemsSource = allSubscriptions;
                SubscriptionStatusText.Text = $"Total: {allSubscriptions.Count} subscrições";
            }
            catch (Exception ex)
            {
                SubscriptionStatusText.Text = "Erro: " + ex.Message;
                MessageBox.Show($"Erro ao carregar subscrições: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshSubscriptionsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadSubscriptions();
        }

        private async void CreateSubscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            // Para criar uma nova subscrição, passamos null como subscription
            // O EditSubscriptionWindow deve detectar isso e mostrar em modo criação
            var createWindow = new EditSubscriptionWindow(_apiService, null);
            createWindow.Owner = this;
            if (createWindow.ShowDialog() == true)
            {
                await LoadSubscriptions();
            }
        }

        // Search handlers
        private void UserSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyUserSearchFilter();
        }

        private void UserSearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UserSearchTextBox.Text == "Pesquisar por nome...")
            {
                UserSearchTextBox.Text = "";
            }
        }

        private void UserSearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserSearchTextBox.Text))
            {
                UserSearchTextBox.Text = "Pesquisar por nome...";
            }
        }

        private void EmployeeSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyEmployeeSearchFilter();
        }

        private void EmployeeSearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EmployeeSearchTextBox.Text == "Pesquisar por nome...")
            {
                EmployeeSearchTextBox.Text = "";
            }
        }

        private void EmployeeSearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmployeeSearchTextBox.Text))
            {
                EmployeeSearchTextBox.Text = "Pesquisar por nome...";
            }
        }

        private void PaymentSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyPaymentSearchFilter();
        }

        private void PaymentSearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PaymentSearchTextBox.Text == "Pesquisar por nome de membro...")
            {
                PaymentSearchTextBox.Text = "";
            }
        }

        private void PaymentSearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PaymentSearchTextBox.Text))
            {
                PaymentSearchTextBox.Text = "Pesquisar por nome de membro...";
            }
        }

        // Search filter methods
        private void ApplyUserSearchFilter()
        {
            // Don't apply filter if controls are not initialized or data is not loaded
            if (MembersDataGrid == null || UserSearchTextBox == null || MembersDataGrid.ItemsSource == null)
            {
                return;
            }

            if (MembersDataGrid.ItemsSource is IEnumerable<MemberDto> allMembers && allMembers != null)
            {
                var searchText = UserSearchTextBox.Text?.Trim().ToLower() ?? string.Empty;

                // Treat placeholder text as empty search
                if (string.IsNullOrWhiteSpace(searchText) || searchText == "pesquisar por nome...")
                {
                    MembersDataGrid.ItemsSource = allMembers;
                    StatusText.Text = $"Total: {allMembers.Count()} utilizadores";
                }
                else
                {
                    var filtered = allMembers.Where(m =>
                        m != null && (
                        (m.Nome?.ToLower().Contains(searchText) ?? false) ||
                        (m.Email?.ToLower().Contains(searchText) ?? false) ||
                        (m.Telemovel?.Contains(searchText) ?? false))
                    ).ToList();

                    MembersDataGrid.ItemsSource = filtered;
                    StatusText.Text = $"Mostrando {filtered.Count} de {allMembers.Count()} utilizadores (filtro: '{searchText}')";
                }
            }
            else
            {
                // If no data is loaded, show empty state
                StatusText.Text = "Nenhum dado carregado";
            }
        }

        private void ApplyEmployeeSearchFilter()
        {
            // Don't apply filter if controls are not initialized or data is not loaded
            if (EmployeesDataGrid == null || EmployeeSearchTextBox == null || EmployeesDataGrid.ItemsSource == null)
            {
                return;
            }

            if (EmployeesDataGrid.ItemsSource is IEnumerable<EmployeeDto> allEmployees && allEmployees != null)
            {
                var searchText = EmployeeSearchTextBox.Text?.Trim().ToLower() ?? string.Empty;

                // Treat placeholder text as empty search
                if (string.IsNullOrWhiteSpace(searchText) || searchText == "pesquisar por nome...")
                {
                    EmployeesDataGrid.ItemsSource = allEmployees;
                    EmployeeStatusText.Text = $"Total: {allEmployees.Count()} funcionários";
                }
                else
                {
                    var filtered = allEmployees.Where(e =>
                        e != null && (
                        (e.Nome?.ToLower().Contains(searchText) ?? false) ||
                        (e.Email?.ToLower().Contains(searchText) ?? false) ||
                        (e.Telemovel?.Contains(searchText) ?? false) ||
                        (e.Funcao?.ToLower().Contains(searchText) ?? false))
                    ).ToList();

                    EmployeesDataGrid.ItemsSource = filtered;
                    EmployeeStatusText.Text = $"Mostrando {filtered.Count} de {allEmployees.Count()} funcionários (filtro: '{searchText}')";
                }
            }
            else
            {
                // If no data is loaded, show empty state
                EmployeeStatusText.Text = "Nenhum dado carregado";
            }
        }

        private void ApplyPaymentSearchFilter()
        {
            // Don't apply filter if controls are not initialized or data is not loaded
            if (PaymentsDataGrid == null || PaymentSearchTextBox == null || PaymentsDataGrid.ItemsSource == null)
            {
                return;
            }

            if (PaymentsDataGrid.ItemsSource is IEnumerable<PaymentDisplayModel> allPayments && allPayments != null)
            {
                var searchText = PaymentSearchTextBox.Text?.Trim().ToLower() ?? string.Empty;

                // Treat placeholder text as empty search
                if (string.IsNullOrWhiteSpace(searchText) || searchText == "pesquisar por nome de membro...")
                {
                    PaymentsDataGrid.ItemsSource = allPayments;
                    PaymentStatusText.Text = $"Total: {allPayments.Count()} pagamentos";
                }
                else
                {
                    var filtered = allPayments.Where(p =>
                        p != null && (p.NomeMembro?.ToLower().Contains(searchText) ?? false)
                    ).ToList();

                    PaymentsDataGrid.ItemsSource = filtered;
                    PaymentStatusText.Text = $"Mostrando {filtered.Count} de {allPayments.Count()} pagamentos (filtro: '{searchText}')";
                }
            }
            else
            {
                // If no data is loaded, show empty state
                PaymentStatusText.Text = "Nenhum dado carregado";
            }
        }

        private async void EditSubscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idSubscricao)
            {
                try
                {
                    // Find the subscription in the current data
                    var subscription = SubscriptionsDataGrid.ItemsSource
                        .Cast<SubscriptionResponseDto>()
                        .FirstOrDefault(s => s.IdSubscricao == idSubscricao);

                    if (subscription == null)
                    {
                        MessageBox.Show("Subscrição não encontrada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var editWindow = new EditSubscriptionWindow(_apiService, subscription);
                    editWindow.Owner = this;
                    if (editWindow.ShowDialog() == true)
                    {
                        await LoadSubscriptions();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao abrir edição de subscrição: {ex.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion
    }

    public class Activity
    {
        public string Date { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
