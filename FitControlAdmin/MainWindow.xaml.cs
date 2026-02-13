using FitControlAdmin.Helper;
using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System;
using System.Collections.Generic;
using System.IO;
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
        private int? _userId;

        // Data storage for filtering
        private List<PhysicalEvaluationHistoryDto>? _allHistory;
        private List<AulaResponseDto>? _allClasses;

        public MainWindow(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;

            _activeBtnBg = (Brush?)new BrushConverter().ConvertFrom("#02acc3") ?? Brushes.Cyan;

            ExtractUserInfoFromToken();
            ConfigureMenuVisibilityByRole();

            InitializeDashboard();
            SetupSidebarHandlers();

            var isPt = string.Equals(_userFuncao, "PT", StringComparison.OrdinalIgnoreCase);
            if (isPt)
            {
                ShowClassManagement();
                HighlightActiveButton(BtnAulas);
            }
            else
            {
                ShowDashboard();
                HighlightActiveButton(BtnDashboard);
            }
        }

        #region Dashboard / Sidebar

        private void InitializeDashboard()
        {
            TotalMembers.Text = "—";
            ActiveMembers.Text = "—";
            MonthlyRevenue.Text = "—";
            RecentActivityList.ItemsSource = new List<Activity> { new Activity { Date = DateTime.Now.ToString("dd/MM"), Description = "A carregar..." } };
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
                ShowClassManagement();
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

            BtnPlanosTreino.Click += (s, e) =>
            {
                HighlightActiveButton(BtnPlanosTreino);
                ShowTrainingPlans();
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

        private async void ShowDashboard()
        {
            HideAllPanels();
            DashboardPanel.Visibility = Visibility.Visible;
            await LoadDashboardDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDashboardDataAsync()
        {
            try
            {
                var members = await _apiService.GetAllMembersAsync();
                var dash = await _apiService.GetDashboardSummaryAsync();

                int total = members?.Count ?? 0;
                int ativos = dash?.MembrosAtivos ?? 0;
                decimal receita = dash?.ReceitaMensal ?? 0;

                TotalMembers.Text = total.ToString();
                ActiveMembers.Text = ativos.ToString();
                MonthlyRevenue.Text = receita.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("pt-PT"));

                var activities = new List<Activity>
                {
                    new Activity { Date = DateTime.Now.ToString("dd/MM"), Description = "Sistema iniciado" },
                    new Activity { Date = DateTime.Now.ToString("dd/MM HH:mm"), Description = "Dashboard atualizado" }
                };
                RecentActivityList.ItemsSource = activities;
            }
            catch
            {
                TotalMembers.Text = "—";
                ActiveMembers.Text = "—";
                MonthlyRevenue.Text = "—";
                RecentActivityList.ItemsSource = new List<Activity> { new Activity { Date = DateTime.Now.ToString("dd/MM"), Description = "Erro ao carregar" } };
            }
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
            LoadEmployeeAtivoCacheFromFile();
            EmployeeStatusText.Text = "A carregar funcionários...";
            try
            {
                var employees = await _apiService.GetAllEmployeesAsync();
                if (employees != null)
                {
                    var display = new List<EmployeeDisplayDto>();
                    foreach (var e in employees)
                    {
                        var user = await _apiService.GetUserByIdAsync(e.IdUser);
                        bool ativo = _employeeAtivoCache.TryGetValue(e.IdUser, out var cached)
                            ? cached
                            : (user?.Ativo ?? true);
                        display.Add(new EmployeeDisplayDto
                        {
                            IdUser = e.IdUser,
                            IdFuncionario = e.IdFuncionario,
                            Nome = e.Nome,
                            Email = e.Email,
                            Telemovel = e.Telemovel,
                            Funcao = e.Funcao,
                            Ativo = ativo,
                            DataRegisto = null
                        });
                    }
                    _allEmployees = display;
                    EmployeesDataGrid.ItemsSource = display;
                    EmployeeStatusText.Text = $"Total: {display.Count} funcionários";
                }
                else
                {
                    _allEmployees = null;
                    EmployeesDataGrid.ItemsSource = null;
                    EmployeeStatusText.Text = "Erro ao carregar funcionários";
                    MessageBox.Show("Erro ao carregar funcionários. Verifique a conexão com o servidor.",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _allEmployees = null;
                EmployeesDataGrid.ItemsSource = null;
                EmployeeStatusText.Text = "Erro: " + ex.Message;
                MessageBox.Show($"Erro: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshEmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }

        private void EditEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                try
                {
                    EmployeeDisplayDto? employee = null;

                    var parent = button.Parent;
                    while (parent != null && !(parent is System.Windows.Controls.DataGridRow))
                    {
                        parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
                    }

                    if (parent is System.Windows.Controls.DataGridRow row && row.Item is EmployeeDisplayDto rowEmployee)
                    {
                        employee = rowEmployee;
                    }
                    else if (button.DataContext is EmployeeDisplayDto contextEmployee)
                    {
                        employee = contextEmployee;
                    }
                    else if (EmployeesDataGrid.SelectedItem is EmployeeDisplayDto selectedEmployee)
                    {
                        employee = selectedEmployee;
                    }

                    if (employee == null)
                    {
                        MessageBox.Show("Não foi possível identificar o funcionário a editar.",
                            "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var editWindow = new CreateFuncionarioWindow(_apiService, employee);
                    editWindow.Owner = this;
                    if (editWindow.ShowDialog() == true)
                    {
                        if (editWindow.SavedIdUser is int idUser && editWindow.SavedAtivo is bool ativo && _allEmployees != null)
                        {
                            _employeeAtivoCache[idUser] = ativo;
                            SaveEmployeeAtivoCacheToFile();
                            var idx = _allEmployees.FindIndex(e => e.IdUser == idUser);
                            if (idx >= 0)
                            {
                                _allEmployees[idx].Ativo = ativo;
                                EmployeesDataGrid.ItemsSource = null;
                                EmployeesDataGrid.ItemsSource = _allEmployees;
                            }
                        }
                        else
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

        /// <summary>
        /// True quando o utilizador é PT - esconde Editar/Eliminar, mostra apenas Agendar (templates).
        /// </summary>
        public bool IsPtClassView { get; set; }

        private void ShowClassManagement()
        {
            HideAllPanels();
            IsPtClassView = string.Equals(_userFuncao, "PT", StringComparison.OrdinalIgnoreCase);
            CreateClassButton.Visibility = IsPtClassView ? Visibility.Collapsed : Visibility.Visible;
            ClassManagementTitle.Text = IsPtClassView ? "Aulas - Agendar" : "Gestão de Aulas";
            // Admin: esconder coluna Ações em Aulas Agendadas e Aulas Terminadas
            ClassReservationsActionsColumn.Visibility = IsPtClassView ? Visibility.Visible : Visibility.Collapsed;
            FinishedClassesActionsColumn.Visibility = IsPtClassView ? Visibility.Visible : Visibility.Collapsed;
            ClassManagementPanel.Visibility = Visibility.Visible;
            LoadClasses();
        }

        private void ShowTrainingPlans()
        {
            HideAllPanels();
            TrainingPlansPanel.Visibility = Visibility.Visible;
            LoadTrainingPlans();
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
            ClassManagementPanel.Visibility = Visibility.Collapsed;
            TrainingPlansPanel.Visibility = Visibility.Collapsed;
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
            var buttons = new[] { BtnDashboard, BtnFuncionarios, BtnMembros, BtnSubscricoes, BtnPagamentos, BtnAulas, BtnExercicios, BtnAvaliacoesFisicas, BtnPlanosTreino, BtnConfig };

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
                    _allMembers = users;
                    MembersDataGrid.ItemsSource = users;
                    StatusText.Text = $"Total: {users.Count} utilizadores";
                }
                else
                {
                    _allMembers = null;
                    MembersDataGrid.ItemsSource = null;
                    StatusText.Text = "Erro ao carregar utilizadores";
                    MessageBox.Show("Erro ao carregar utilizadores. Verifique a conexão com o servidor.",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _allMembers = null;
                MembersDataGrid.ItemsSource = null;
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
            var createWindow = new CreateFuncionarioWindow(_apiService);
            createWindow.Owner = this;
            if (createWindow.ShowDialog() == true)
            {
                LoadEmployees();
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
                        var (success, errorMessage) = await _apiService.ChangeUserActiveStatusAsync(member.IdUser, false);
                        if (success)
                        {
                            MessageBox.Show("Utilizador desativado com sucesso!",
                                "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadUsers();
                        }
                        else
                        {
                            MessageBox.Show(errorMessage ?? $"Erro ao desativar utilizador (ID: {member.IdUser}). Verifique se tem permissões.",
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

                // Try different possible field names for user ID
                System.Diagnostics.Debug.WriteLine("Checking for user ID fields...");
                if (root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out var nameIdentifierProp))
                {
                    System.Diagnostics.Debug.WriteLine($"Found nameidentifier: {nameIdentifierProp}, ValueKind: {nameIdentifierProp.ValueKind}");

                    // Try to parse as string first, then convert to int
                    var stringValue = nameIdentifierProp.ToString().Trim('"'); // Remove quotes if present
                    System.Diagnostics.Debug.WriteLine($"Raw value: '{nameIdentifierProp}', String value: '{stringValue}'");

                    if (int.TryParse(stringValue, out var nameIdentifierId))
                    {
                        _userId = nameIdentifierId;
                        System.Diagnostics.Debug.WriteLine($"Successfully parsed user ID: {_userId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to parse '{stringValue}' as int");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("nameidentifier field not found");
                }

                if (root.TryGetProperty("IdUser", out var idUserProp) && idUserProp.TryGetInt32(out var idUser))
                {
                    _userId = idUser;
                }
                else if (root.TryGetProperty("sub", out var subProp) && int.TryParse(subProp.GetString(), out var subId))
                {
                    _userId = subId;
                }
                else if (root.TryGetProperty("nameid", out var nameIdProp) && int.TryParse(nameIdProp.GetString(), out var nameId))
                {
                    _userId = nameId;
                }
                else if (root.TryGetProperty("userId", out var userIdProp) && userIdProp.TryGetInt32(out var userId))
                {
                    _userId = userId;
                }
                else if (root.TryGetProperty("UserId", out var UserIdProp) && UserIdProp.TryGetInt32(out var UserId))
                {
                    _userId = UserId;
                }

                // Debug: Print all properties in the JWT
                System.Diagnostics.Debug.WriteLine("JWT Properties:");
                foreach (var property in root.EnumerateObject())
                {
                    System.Diagnostics.Debug.WriteLine($"  {property.Name}: {property.Value}");
                }

                if (root.TryGetProperty("Tipo", out var tipoProp))
                {
                    _userTipo = tipoProp.GetString();
                }

                if (root.TryGetProperty("Funcao", out var funcaoProp))
                {
                    _userFuncao = funcaoProp.GetString();
                }

                // Debug: Log extracted values
                System.Diagnostics.Debug.WriteLine($"ExtractUserInfoFromToken: _userId={_userId}, _userTipo={_userTipo}, _userFuncao={_userFuncao}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExtractUserInfoFromToken error: {ex.Message}");
                // Se falhar o parse, mantemos os valores nulos
            }
        }

        private void ConfigureMenuVisibilityByRole()
        {
            BtnFuncionarios.Visibility = Visibility.Visible;
            BtnMembros.Visibility = Visibility.Visible;
            BtnSubscricoes.Visibility = Visibility.Visible;
            BtnPagamentos.Visibility = Visibility.Visible;
            BtnAulas.Visibility = Visibility.Visible;
            BtnExercicios.Visibility = Visibility.Visible;
            BtnDashboard.Visibility = Visibility.Visible;
            if (string.Equals(_userFuncao, "PT", StringComparison.OrdinalIgnoreCase))
            {
                BtnAvaliacoesFisicas.Visibility = Visibility.Visible;
                BtnPlanosTreino.Visibility = Visibility.Visible;
            }
            else
            {
                BtnAvaliacoesFisicas.Visibility = Visibility.Collapsed;
                BtnPlanosTreino.Visibility = Visibility.Collapsed;
            }
            BtnConfig.Visibility = Visibility.Visible;

            if (string.Equals(_userTipo, "Membro", StringComparison.OrdinalIgnoreCase))
            {
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
                    // Receção: apenas Membros, Subscrições, Pagamentos, Configurações e Logout
                    BtnDashboard.Visibility = Visibility.Collapsed;
                    BtnFuncionarios.Visibility = Visibility.Collapsed;
                    BtnExercicios.Visibility = Visibility.Collapsed;
                    BtnAvaliacoesFisicas.Visibility = Visibility.Collapsed;
                    BtnPlanosTreino.Visibility = Visibility.Collapsed;
                    BtnAulas.Visibility = Visibility.Collapsed;
                    BtnMembros.Visibility = Visibility.Visible;
                    BtnSubscricoes.Visibility = Visibility.Visible;
                    BtnPagamentos.Visibility = Visibility.Visible;
                    BtnConfig.Visibility = Visibility.Visible;
                }
                else if (string.Equals(_userFuncao, "PT", StringComparison.OrdinalIgnoreCase))
                {
                    // PT: retirar Dashboard, Membros e Exercícios; mostrar Config (dados como Admin)
                    BtnDashboard.Visibility = Visibility.Collapsed;
                    BtnMembros.Visibility = Visibility.Collapsed;
                    BtnExercicios.Visibility = Visibility.Collapsed;
                    BtnFuncionarios.Visibility = Visibility.Collapsed;
                    BtnSubscricoes.Visibility = Visibility.Collapsed;
                    BtnPagamentos.Visibility = Visibility.Collapsed;
                    BtnAvaliacoesFisicas.Visibility = Visibility.Visible;
                    BtnPlanosTreino.Visibility = Visibility.Visible;
                    BtnConfig.Visibility = Visibility.Visible;
                }
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

        #region Employee Management

        private List<EmployeeDisplayDto>? _allEmployees = null;
        /// <summary>Cache do estado Ativo por IdUser (a API não expõe GET User/{id}, por isso usamos o que guardámos ao editar).</summary>
        private readonly Dictionary<int, bool> _employeeAtivoCache = new Dictionary<int, bool>();

        private static string EmployeeAtivoCacheFilePath()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FitControlAdmin");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return Path.Combine(dir, "employee_ativo_cache.json");
        }

        private void LoadEmployeeAtivoCacheFromFile()
        {
            _employeeAtivoCache.Clear();
            try
            {
                var path = EmployeeAtivoCacheFilePath();
                if (!File.Exists(path)) return;
                var json = File.ReadAllText(path);
                var dict = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
                if (dict == null) return;
                foreach (var kv in dict)
                    if (int.TryParse(kv.Key, out var id))
                        _employeeAtivoCache[id] = kv.Value;
            }
            catch { /* ignorar erros de leitura */ }
        }

        private void SaveEmployeeAtivoCacheToFile()
        {
            try
            {
                var dict = new Dictionary<string, bool>();
                foreach (var kv in _employeeAtivoCache)
                    dict[kv.Key.ToString()] = kv.Value;
                var json = JsonSerializer.Serialize(dict);
                File.WriteAllText(EmployeeAtivoCacheFilePath(), json);
            }
            catch { /* ignorar erros de escrita */ }
        }

        #endregion

        #region Payment Management

        private List<MemberDto>? _allMembers = null;
        private List<SubscriptionResponseDto>? _allSubscriptions = null;
        private List<SubscriptionResponseDto>? _subscriptionsForMembers = null;
        /// <summary>Lista completa de pagamentos para exibição (usada pelo filtro de pesquisa).</summary>
        private List<PaymentDisplayModel>? _allPaymentDisplay = null;

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
                        _allPaymentDisplay = displayList;
                        PaymentsDataGrid.ItemsSource = null;
                        PaymentsDataGrid.ItemsSource = displayList;
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
                    _allPaymentDisplay = null;
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
                var activeReservationsTask = _apiService.GetActiveReservationsAsync();
                var completedReservationsTask = _apiService.GetCompletedReservationsAsync();
                var historyTask = _apiService.GetAllEvaluationsAsync();
                await Task.WhenAll(activeReservationsTask, completedReservationsTask, historyTask);

                var activeReservations = await activeReservationsTask;
                var completedReservations = await completedReservationsTask;
                var history = await historyTask;

                // Preencher tabela de reservas ativas
                if (activeReservations != null)
                {
                    ReservationsDataGrid.ItemsSource = activeReservations;
                }
                else
                {
                    ReservationsDataGrid.ItemsSource = new List<MemberEvaluationReservationSummaryDto>();
                }

                // Preencher tabela "Reservas completas": unir reservas do endpoint "completed" (canceladas, faltas, etc.) com as do histórico (feitas com sucesso)
                var reservasCompletas = new List<MemberEvaluationReservationSummaryDto>();
                if (completedReservations != null)
                    reservasCompletas.AddRange(completedReservations);
                if (history != null)
                {
                    foreach (var h in history)
                    {
                        var jaIncluida = reservasCompletas.Any(r => r.IdMembro == h.IdMembro && r.DataReserva.Date == h.DataAvaliacao.Date);
                        if (!jaIncluida)
                        {
                            reservasCompletas.Add(new MemberEvaluationReservationSummaryDto
                            {
                                IdMembro = h.IdMembro,
                                NomeMembro = h.NomeMembro,
                                DataReserva = h.DataAvaliacao,
                                EstadoString = "Presente",
                                NomeFuncionario = h.NomeFuncionario,
                                IdAvaliacaoFisica = h.IdAvaliacao
                            });
                        }
                    }
                }
                foreach (var r in reservasCompletas)
                {
                    var estado = r.EstadoString?.Trim();
                    if ((string.Equals(estado, "Cancelado", StringComparison.OrdinalIgnoreCase) || string.Equals(estado, "Faltou", StringComparison.OrdinalIgnoreCase)) && string.IsNullOrWhiteSpace(r.NomeFuncionario))
                        r.NomeFuncionario = "Avaliação não concluída";
                }
                reservasCompletas = reservasCompletas.OrderByDescending(r => r.DataReserva).ToList();
                CompletedEvaluationsDataGrid.ItemsSource = reservasCompletas;

                // Preencher tabela de histórico (apenas avaliações realizadas com sucesso)
                if (history != null)
                {
                    _allHistory = history.ToList();
                    MembersHistoryDataGrid.ItemsSource = _allHistory;
                }
                else
                {
                    _allHistory = new List<PhysicalEvaluationHistoryDto>();
                    MembersHistoryDataGrid.ItemsSource = _allHistory;
                }

                PhysicalEvaluationStatusText.Text = $"Reservas: {activeReservations?.Count ?? 0} | Reservas completas: {reservasCompletas.Count} | Histórico: {history?.Count ?? 0}";
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

        private async void CompleteEvaluationFromReservationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not MemberEvaluationReservationSummaryDto reservation)
                return;
            try
            {
                if (!string.Equals(_userFuncao, "PT", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Apenas o PT pode completar avaliações físicas.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var currentUser = await _apiService.GetCurrentUserAsync();
                if (currentUser?.IdFuncionario == null || currentUser.IdFuncionario.Value <= 0)
                {
                    MessageBox.Show("Não foi possível identificar o PT. Faça logout e login novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var employee = new EmployeeDto
                {
                    IdUser = currentUser.IdUser,
                    IdFuncionario = currentUser.IdFuncionario.Value,
                    Nome = currentUser.Nome ?? "PT",
                    Email = currentUser.Email ?? "",
                    Telemovel = currentUser.Telemovel ?? "",
                    Funcao = currentUser.FuncaoFuncionario ?? Enum.Parse<Funcao>(_userFuncao ?? "PT")
                };
                var createWindow = new CreatePhysicalEvaluationWindow(_apiService, reservation.IdMembro, employee.IdFuncionario, reservation.IdMembroAvaliacao);
                createWindow.Owner = this;
                if (createWindow.ShowDialog() == true)
                    LoadPhysicalEvaluations();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MarkNoShowReservationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not MemberEvaluationReservationSummaryDto reservation)
                return;
            var result = MessageBox.Show(
                "Tem certeza que deseja marcar esta reserva como falta de comparência?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
            try
            {
                if (!string.Equals(_userFuncao, "PT", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Apenas o PT pode marcar falta de comparência.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var currentUser = await _apiService.GetCurrentUserAsync();
                if (currentUser?.IdFuncionario == null || currentUser.IdFuncionario.Value <= 0)
                {
                    MessageBox.Show("Não foi possível identificar o PT.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var confirmResult = await _apiService.ConfirmReservationAsync(reservation.IdMembro, reservation.IdMembroAvaliacao, currentUser.IdFuncionario.Value);
                if (!confirmResult.Success)
                {
                    MessageBox.Show(confirmResult.ErrorMessage ?? "Erro ao confirmar reserva.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var noShowDto = new MarkAttendanceDto
                {
                    Presente = false,
                    IdFuncionario = currentUser.IdFuncionario.Value,
                    Peso = 0,
                    Altura = 0,
                    Imc = 0,
                    MassaMuscular = 0,
                    MassaGorda = 0,
                    Observacoes = null
                };
                var (success, errorMessage) = await _apiService.MarkAttendanceAsync(reservation.IdMembro, reservation.IdMembroAvaliacao, noShowDto);
                if (success)
                {
                    MessageBox.Show("Falta de comparência registada.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadPhysicalEvaluations();
                }
                else
                    MessageBox.Show(errorMessage ?? "Erro ao marcar falta.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ViewEvaluationDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idAvaliacao)
            {
                try
                {
                    // Buscar a avaliação do histórico
                    var history = await _apiService.GetAllEvaluationsAsync();
                    var evaluation = history?.FirstOrDefault(h => h.IdAvaliacao == idAvaliacao);

                    if (evaluation == null)
                    {
                        MessageBox.Show("Avaliação não encontrada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Mostrar detalhes
                    var details = $"Membro: {evaluation.NomeMembro}\n" +
                                  $"PT: {evaluation.NomeFuncionario}\n" +
                                  $"Data: {evaluation.DataAvaliacao:dd/MM/yyyy HH:mm}\n\n" +
                                  $"Peso: {evaluation.Peso:F1} kg\n" +
                                  $"Altura: {evaluation.Altura:F2} m\n" +
                                  $"IMC: {evaluation.Imc:F1}\n" +
                                  $"Massa Muscular: {evaluation.MassaMuscular:F1} kg\n" +
                                  $"Massa Gorda: {evaluation.MassaGorda:F1} kg\n\n" +
                                  $"Observações:\n{evaluation.Observacoes ?? "Sem observações"}";

                    MessageBox.Show(details, "Detalhes da Avaliação Física", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao ver detalhes: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void MakeEvaluationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idMembroAvaliacao)
            {
                try
                {
                    // Apenas PT pode criar avaliações físicas (API: OnlyPT)
                    if (!string.Equals(_userFuncao, "PT", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Apenas o PT pode fazer avaliações físicas.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Buscar a reserva completa
                    var completedReservations = await _apiService.GetCompletedReservationsAsync();
                    var reservation = completedReservations?.FirstOrDefault(r => r.IdMembroAvaliacao == idMembroAvaliacao);

                    if (reservation == null)
                    {
                        MessageBox.Show("Reserva não encontrada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var currentUser = await _apiService.GetCurrentUserAsync();
                    if (currentUser?.IdFuncionario == null || currentUser.IdFuncionario.Value <= 0)
                    {
                        MessageBox.Show("Não foi possível identificar o PT. Faça logout e login novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var employee = new EmployeeDto
                    {
                        IdUser = currentUser.IdUser,
                        IdFuncionario = currentUser.IdFuncionario.Value,
                        Nome = currentUser.Nome ?? "PT",
                        Email = currentUser.Email ?? "",
                        Telemovel = currentUser.Telemovel ?? "",
                        Funcao = currentUser.FuncaoFuncionario ?? Enum.Parse<Funcao>(_userFuncao ?? "PT")
                    };

                    var createWindow = new CreatePhysicalEvaluationWindow(_apiService, reservation.IdMembro, employee.IdFuncionario, idMembroAvaliacao);
                    createWindow.Owner = this;
                    if (createWindow.ShowDialog() == true)
                    {
                        LoadPhysicalEvaluations();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao abrir avaliação: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (_allMembers == null) return;

            var searchText = UserSearchTextBox.Text?.Trim().ToLower() ?? string.Empty;

            // Treat placeholder text as empty search
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "pesquisar por nome...")
            {
                MembersDataGrid.ItemsSource = _allMembers;
                StatusText.Text = $"Total: {_allMembers.Count} utilizadores";
            }
            else
            {
                var filtered = _allMembers.Where(m =>
                    m != null && (
                    (m.Nome?.ToLower().Contains(searchText) ?? false) ||
                    (m.Email?.ToLower().Contains(searchText) ?? false) ||
                    (m.Telemovel?.Contains(searchText) ?? false))
                ).ToList();

                MembersDataGrid.ItemsSource = filtered;
                StatusText.Text = $"Mostrando {filtered.Count} de {_allMembers.Count} utilizadores (filtro: '{searchText}')";
            }
        }

        private void ApplyEmployeeSearchFilter()
        {
            if (_allEmployees == null) return;

            var searchText = EmployeeSearchTextBox.Text?.Trim().ToLower() ?? string.Empty;

            // Treat placeholder text as empty search
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "pesquisar por nome...")
            {
                EmployeesDataGrid.ItemsSource = _allEmployees;
                EmployeeStatusText.Text = $"Total: {_allEmployees.Count} funcionários";
            }
            else
            {
                var filtered = _allEmployees.Where(e =>
                    e != null && (
                    (e.Nome?.ToLower().Contains(searchText) ?? false) ||
                    (e.Email?.ToLower().Contains(searchText) ?? false) ||
                    (e.Telemovel?.Contains(searchText) ?? false) ||
                    FormatEnumName(e.Funcao.ToString()).ToLower().Contains(searchText))
                ).ToList();

                EmployeesDataGrid.ItemsSource = filtered;
                EmployeeStatusText.Text = $"Mostrando {filtered.Count} de {_allEmployees.Count} funcionários (filtro: '{searchText}')";
            }
        }

        private void ApplyPaymentSearchFilter()
        {
            if (PaymentsDataGrid == null || PaymentSearchTextBox == null)
                return;
            if (_allPaymentDisplay == null)
            {
                PaymentStatusText.Text = "Nenhum dado carregado";
                return;
            }

            var searchText = PaymentSearchTextBox.Text?.Trim().ToLower() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "pesquisar por nome de membro...")
            {
                PaymentsDataGrid.ItemsSource = _allPaymentDisplay;
                PaymentStatusText.Text = $"Total: {_allPaymentDisplay.Count} pagamentos";
            }
            else
            {
                var filtered = _allPaymentDisplay.Where(p =>
                    p != null && (p.NomeMembro?.ToLower().Contains(searchText) ?? false)
                ).ToList();
                PaymentsDataGrid.ItemsSource = filtered;
                PaymentStatusText.Text = $"Mostrando {filtered.Count} de {_allPaymentDisplay.Count} pagamentos (filtro: '{searchText}')";
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

        #region Class Management

        private async void LoadClassReservations()
        {
            try
            {
                var reservations = await _apiService.GetClassReservationsAsync();
                var today = DateTime.Today;
                var upcoming = reservations?
                    .Where(r => r.DataAula.Date >= today)
                    .OrderBy(r => r.DataAula)
                    .ToList() ?? new List<ClassReservationSummaryDto>();
                var finished = reservations?
                    .Where(r => r.DataAula.Date < today)
                    .OrderByDescending(r => r.DataAula)
                    .ToList() ?? new List<ClassReservationSummaryDto>();

                ClassReservationsDataGrid.ItemsSource = upcoming;
                FinishedClassesDataGrid.ItemsSource = finished;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar reservas: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadClasses()
        {
            System.Diagnostics.Debug.WriteLine("LoadClasses: Starting to load classes");
            ClassStatusText.Text = "A carregar aulas...";
            try
            {
                List<AulaResponseDto>? classes = null;
                List<ClassReservationSummaryDto>? reservations = null;

                if (IsPtClassView)
                {
                    // PT: carregar apenas as suas aulas (templates) e as suas aulas agendadas
                    var currentUser = await _apiService.GetCurrentUserAsync();
                    var idFuncionario = currentUser?.IdFuncionario ?? 0;
                    if (idFuncionario <= 0)
                    {
                        ClassStatusText.Text = "Erro: Não foi possível identificar o PT.";
                        ClassesDataGrid.ItemsSource = new List<AulaResponseDto>();
                        ClassReservationsDataGrid.ItemsSource = new List<ClassReservationSummaryDto>();
                        FinishedClassesDataGrid.ItemsSource = new List<ClassReservationSummaryDto>();
                        return;
                    }
                    classes = await _apiService.GetClassesByPtAsync(idFuncionario);
                    var ptScheduled = await _apiService.GetScheduledClassesByPTAsync(idFuncionario);
                    // Converter AulaMarcadaResponseDto para ClassReservationSummaryDto
                    reservations = ptScheduled?.Select(s => new ClassReservationSummaryDto
                    {
                        IdAulaMarcada = s.IdAulaMarcada,
                        DataAula = s.DataAula,
                        NomeAula = s.NomeAula,
                        Sala = s.Sala,
                        HoraInicio = s.HoraInicio,
                        HoraFim = s.HoraFim,
                        Capacidade = s.Capacidade,
                        TotalReservas = s.TotalReservas
                    }).ToList() ?? new List<ClassReservationSummaryDto>();
                }
                else
                {
                    // Admin: carregar todas as aulas ativas e reservas
                    var classesTask = _apiService.GetClassesByStateAsync(ativo: true);
                    var reservationsTask = _apiService.GetClassReservationsAsync();
                    await Task.WhenAll(classesTask, reservationsTask);
                    classes = await classesTask;
                    reservations = await reservationsTask;
                }

                System.Diagnostics.Debug.WriteLine($"LoadClasses: Retrieved {classes?.Count ?? 0} classes and {reservations?.Count ?? 0} reservations");

                // Preencher tabela de aulas
                if (classes != null)
                {
                    _allClasses = classes.ToList();
                    ClassesDataGrid.ItemsSource = _allClasses;
                }
                else
                {
                    _allClasses = new List<AulaResponseDto>();
                    ClassesDataGrid.ItemsSource = _allClasses;
                }

                // Preencher tabela de reservas/aulas agendadas (futuras e em curso)
                var today = DateTime.Today;
                var upcomingReservations = reservations?
                    .Where(r => r.DataAula.Date >= today)
                    .OrderBy(r => r.DataAula)
                    .ToList() ?? new List<ClassReservationSummaryDto>();
                var finishedReservations = reservations?
                    .Where(r => r.DataAula.Date < today)
                    .OrderByDescending(r => r.DataAula)
                    .ToList() ?? new List<ClassReservationSummaryDto>();

                ClassReservationsDataGrid.ItemsSource = upcomingReservations;
                FinishedClassesDataGrid.ItemsSource = finishedReservations;

                ClassStatusText.Text = IsPtClassView
                    ? $"As suas aulas (templates): {classes?.Count ?? 0} | Agendadas: {upcomingReservations.Count} | Terminadas: {finishedReservations.Count}"
                    : $"Aulas: {classes?.Count ?? 0} | Agendadas: {upcomingReservations.Count} | Terminadas: {finishedReservations.Count}";

                if ((classes?.Count ?? 0) == 0 && !IsPtClassView)
                {
                    ClassStatusText.Text += " ⚠️ API: Endpoint GET '/api/Class' não implementado";
                }

                System.Diagnostics.Debug.WriteLine($"LoadClasses: Completed - Set status to: {ClassStatusText.Text}");
            }
            catch (Exception ex)
            {
                ClassStatusText.Text = "Erro: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"LoadClasses: Exception - {ex.Message}");
                MessageBox.Show($"Erro ao carregar aulas: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshClassesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadClasses();
        }

        private void CreateClassButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("CreateClassButton_Click: Opening create class window");
            var createWindow = new CreateEditClassWindow(_apiService);
            createWindow.Owner = this;
            var result = createWindow.ShowDialog();
            System.Diagnostics.Debug.WriteLine($"CreateClassButton_Click: Dialog result: {result}");
            if (result == true)
            {
                System.Diagnostics.Debug.WriteLine("CreateClassButton_Click: Reloading classes after creation");
                LoadClasses();
            }
        }

        private async void EditClassButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idAula)
            {
                try
                {
                    // Buscar a aula completa da lista
                    var allClasses = ClassesDataGrid.ItemsSource as IEnumerable<AulaResponseDto>;
                    var aula = allClasses?.FirstOrDefault(c => c.IdAula == idAula);

                    if (aula == null)
                    {
                        MessageBox.Show("Aula não encontrada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var editWindow = new CreateEditClassWindow(_apiService, aula);
                    editWindow.Owner = this;
                    if (editWindow.ShowDialog() == true)
                    {
                        LoadClasses();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar aula: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ScheduleClassButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idAula)
            {
                try
                {
                    // Obter detalhes da aula
                    var allClasses = await _apiService.GetAllClassesAsync();
                    var selectedClass = allClasses?.FirstOrDefault(c => c.IdAula == idAula);
                    
                    if (selectedClass == null)
                    {
                        MessageBox.Show("Aula não encontrada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Criar lista com apenas esta aula para o diálogo
                    var classList = new List<AulaResponseDto> { selectedClass };
                    
                    // Abrir diálogo de criar aula agendada
                    var dialog = new CreateScheduledClassDialog(classList);
                    dialog.Owner = this;
                    
                    if (dialog.ShowDialog() == true)
                    {
                        // Usar DateTimeKind.Unspecified para evitar conversão de timezone no JSON
                        var dataSelecionada = dialog.SelectedDate;
                        var scheduleDto = new ScheduleClassDto
                        {
                            IdAula = dialog.SelectedClassId,
                            Sala = dialog.SelectedSala,
                            DataAula = new DateTime(dataSelecionada.Year, dataSelecionada.Month, dataSelecionada.Day, 0, 0, 0, DateTimeKind.Unspecified)
                        };

                        // Validar: o dia da data deve corresponder ao dia da semana da aula
                        var diaDaData = DiaSemanaHelper.FromDayOfWeek(scheduleDto.DataAula.DayOfWeek);
                        if (diaDaData != selectedClass.DiaSemana)
                        {
                            var nomeDiaAula = selectedClass.DiaSemana.ToString();
                            var nomeDiaData = diaDaData.ToString();
                            MessageBox.Show($"A data selecionada ({scheduleDto.DataAula:dd/MM/yyyy}) é {nomeDiaData}, mas a aula '{selectedClass.Nome}' está agendada para {nomeDiaAula}. Por favor, selecione uma data que corresponda ao dia da aula.", "Dia incorreto", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Validar: mínimo 1 dia de antecedência
                        if (scheduleDto.DataAula.Date <= DateTime.Today)
                        {
                            MessageBox.Show("A aula deve ser agendada com pelo menos 1 dia de antecedência.", "Aviso", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Validar: máximo 2 semanas
                        if (scheduleDto.DataAula.Date > DateTime.Today.AddDays(14))
                        {
                            MessageBox.Show("Só é possível agendar aulas até 2 semanas à frente.", "Aviso", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var (success, errorMessage, data) = await _apiService.CreateScheduledClassAsync(scheduleDto);
                        
                        if (success)
                        {
                            MessageBox.Show($"Aula '{selectedClass.Nome}' agendada com sucesso para {scheduleDto.DataAula:dd/MM/yyyy}!", 
                                "Sucesso", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            // Se foi erro 500/Internal, verificar se a aula foi criada (workaround para API que cria mas falha na resposta)
                            var idFuncionario = selectedClass.IdFuncionario ?? 0;
                            var isServerError = errorMessage != null && (errorMessage.Contains("Internal", StringComparison.OrdinalIgnoreCase) || errorMessage.Contains("500"));
                            if (isServerError && idFuncionario > 0)
                            {
                                var scheduledClasses = await _apiService.GetScheduledClassesByPTAsync(idFuncionario);
                                var foiCriada = scheduledClasses?.Any(sc => sc.IdAula == scheduleDto.IdAula && sc.DataAula.Date == scheduleDto.DataAula.Date) == true;
                                if (foiCriada)
                                {
                                    MessageBox.Show($"Aula '{selectedClass.Nome}' agendada com sucesso para {scheduleDto.DataAula:dd/MM/yyyy}!", 
                                        "Sucesso", 
                                        MessageBoxButton.OK, 
                                        MessageBoxImage.Information);
                                    return;
                                }
                            }

                            // Se "já existe aula" - pode ter sido criada na tentativa anterior
                            if (errorMessage != null && (errorMessage.Contains("já existe", StringComparison.OrdinalIgnoreCase) || errorMessage.Contains("existe aula")))
                            {
                                MessageBox.Show($"{errorMessage}\n\nA aula pode ter sido criada numa tentativa anterior. Verifique a lista de aulas agendadas.", "Aviso", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }

                            MessageBox.Show($"Erro ao agendar aula: {errorMessage}", "Erro", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro: {ex.Message}", "Erro", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteClassButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idAula)
            {
                var result = MessageBox.Show(
                    "Tem certeza que deseja desativar esta aula?\n\nA aula será marcada como inativa e não aparecerá mais na lista de aulas ativas.",
                    "Confirmar Desativação",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var (success, errorMessage) = await _apiService.DeleteClassAsync(idAula);
                        if (success)
                        {
                            MessageBox.Show("Aula desativada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadClasses();
                        }
                        else
                        {
                            MessageBox.Show(errorMessage ?? "Erro ao desativar aula.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void MarkClassAttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idAulaMarcada)
            {
                try
                {
                    var attendanceWindow = new MarkAttendanceWindow(_apiService, idAulaMarcada, somenteConsulta: false);
                    attendanceWindow.Owner = this;
                    attendanceWindow.ShowDialog();
                    LoadClasses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao abrir janela de presenças: {ex.Message}", "Erro", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViewClassAttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idAulaMarcada)
            {
                try
                {
                    var attendanceWindow = new MarkAttendanceWindow(_apiService, idAulaMarcada, somenteConsulta: true);
                    attendanceWindow.Owner = this;
                    attendanceWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao abrir janela de presenças: {ex.Message}", "Erro", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void CancelScheduledClassButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag == null)
                return;
            int idAulaMarcada = button.Tag is int id ? id : Convert.ToInt32(button.Tag);
            var result = MessageBox.Show(
                "Tem certeza que deseja cancelar esta aula agendada?\n\nTodas as reservas associadas serão canceladas.",
                "Confirmar cancelamento",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
            try
            {
                var (success, errorMessage) = await _apiService.CancelScheduledClassAsync(idAulaMarcada);
                if (success)
                {
                    MessageBox.Show("Aula agendada cancelada com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadClasses();
                }
                else
                    MessageBox.Show(errorMessage ?? "Erro ao cancelar aula agendada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Search handlers
        private void ClassSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyClassSearchFilter();
        }

        private void ClassSearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ClassSearchTextBox.Text == "Pesquisar por nome...")
            {
                ClassSearchTextBox.Text = "";
            }
        }

        private void ClassSearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ClassSearchTextBox.Text))
            {
                ClassSearchTextBox.Text = "Pesquisar por nome...";
            }
        }

        // History search handlers
        private void HistorySearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyHistorySearchFilter();
        }

        private void HistorySearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (HistorySearchTextBox.Text == "Pesquisar por nome do membro...")
            {
                HistorySearchTextBox.Text = "";
            }
        }

        private void HistorySearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(HistorySearchTextBox.Text))
            {
                HistorySearchTextBox.Text = "Pesquisar por nome do membro...";
            }
        }

        private void ApplyClassSearchFilter()
        {
            if (ClassesDataGrid == null || _allClasses == null)
                return;

            var searchText = ClassSearchTextBox.Text?.Trim().ToLower() ?? string.Empty;

            // Treat placeholder text as empty search
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "pesquisar por nome...")
            {
                ClassesDataGrid.ItemsSource = _allClasses;
            }
            else
            {
                var filtered = _allClasses.Where(c =>
                    c != null && (
                    (c.Nome?.ToLower().Contains(searchText) ?? false) ||
                    (c.Funcionario?.Nome?.ToLower().Contains(searchText) ?? false))
                ).ToList();

                ClassesDataGrid.ItemsSource = filtered;
            }
        }

        private void ApplyHistorySearchFilter()
        {
            if (MembersHistoryDataGrid == null || _allHistory == null)
                return;

            var searchText = HistorySearchTextBox.Text?.Trim().ToLower() ?? string.Empty;

            // Treat placeholder text as empty search
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "pesquisar por nome do membro...")
            {
                MembersHistoryDataGrid.ItemsSource = _allHistory;
            }
            else
            {
                var filtered = _allHistory.Where(h =>
                    h != null && (h.NomeMembro?.ToLower().Contains(searchText) ?? false)
                ).ToList();

                MembersHistoryDataGrid.ItemsSource = filtered;
            }
        }

        #endregion

        #region Training Plan Management

        private async void LoadTrainingPlans()
        {
            if (TrainingPlanStatusText == null) return;

            TrainingPlanStatusText.Text = "A carregar planos...";
            try
            {
                var active = await _apiService.GetTrainingPlansByStateAsync(ativo: true);
                var inactive = await _apiService.GetTrainingPlansByStateAsync(ativo: false);

                var all = new List<TrainingPlanSummaryDto>();
                if (active != null) all.AddRange(active);
                if (inactive != null) all.AddRange(inactive);

                all = all.OrderByDescending(p => p.DataCriacao).ToList();

                TrainingPlansDataGrid.ItemsSource = all;
                TrainingPlanStatusText.Text = $"Planos: {all.Count} (ativos: {active?.Count ?? 0})";

                var members = await _apiService.GetAllMembersAsync();
                MembersWithPlansDataGrid.ItemsSource = members ?? new List<MemberDto>();
            }
            catch (Exception ex)
            {
                TrainingPlanStatusText.Text = "Erro: " + ex.Message;
                MessageBox.Show($"Erro ao carregar planos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshTrainingPlansButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTrainingPlans();
        }

        private async void CreateTrainingPlanButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentUser = await _apiService.GetCurrentUserAsync();
                var idFunc = currentUser?.IdFuncionario ?? 0;
                if (idFunc <= 0)
                {
                    MessageBox.Show("Não foi possível identificar o PT. Faça logout e login novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var win = new CreateEditTrainingPlanWindow(_apiService, idFuncionario: idFunc, idPlano: null);
                win.Owner = this;
                if (win.ShowDialog() == true)
                    LoadTrainingPlans();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewTrainingPlanButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idPlano)
            {
                try
                {
                    var win = new ViewTrainingPlanWindow(_apiService, idPlano);
                    win.Owner = this;
                    win.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EditTrainingPlanButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TrainingPlanSummaryDto plan)
            {
                try
                {
                    var currentUser = await _apiService.GetCurrentUserAsync();
                    var idFunc = currentUser?.IdFuncionario ?? 0;
                    var win = new CreateEditTrainingPlanWindow(_apiService, idFuncionario: idFunc, idPlano: plan.IdPlano, planNome: plan.Nome);
                    win.Owner = this;
                    if (win.ShowDialog() == true)
                        LoadTrainingPlans();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void AssignTrainingPlanButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idPlano)
            {
                try
                {
                    var members = await _apiService.GetAllMembersAsync();
                    if (members == null || members.Count == 0)
                    {
                        MessageBox.Show("Não há membros disponíveis.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    var dlg = new AssignPlanToMemberDialog(members, idPlano, _apiService);
                    dlg.Owner = this;
                    if (dlg.ShowDialog() == true)
                        LoadTrainingPlans();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ToggleTrainingPlanStateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TrainingPlanSummaryDto plan)
            {
                try
                {
                    var newState = !plan.Ativo;
                    var (success, errorMessage) = await _apiService.ChangeTrainingPlanStateAsync(plan.IdPlano, newState);
                    if (success)
                    {
                        MessageBox.Show(newState ? "Plano ativado." : "Plano desativado.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTrainingPlans();
                    }
                    else
                        MessageBox.Show(errorMessage ?? "Erro ao alterar estado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
