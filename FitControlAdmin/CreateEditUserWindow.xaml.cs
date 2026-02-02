using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FitControlAdmin
{
    public partial class CreateEditUserWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly UserDto? _existingUser;
        private readonly bool _isEditMode;

        public CreateEditUserWindow(ApiService apiService, UserDto? existingUser = null, string? preSelectedTipo = null)
        {
            InitializeComponent();
            _apiService = apiService;
            _existingUser = existingUser;
            _isEditMode = existingUser != null;

            if (_isEditMode)
            {
                Title = "Editar Membro";
                TitleText.Text = "Editar Membro";
                LoadUserData();
                AtivoCheckBox.IsEnabled = true;
            }
            else
            {
                Title = "Criar Membro";
                TitleText.Text = "Criar Membro";
                AtivoCheckBox.IsChecked = true;
                AtivoCheckBox.IsEnabled = false;
                if (!string.IsNullOrEmpty(preSelectedTipo))
                {
                    if (preSelectedTipo == "Membro") TipoComboBox.SelectedIndex = 0;
                    else if (preSelectedTipo == "Funcionario") TipoComboBox.SelectedIndex = 1;
                }
            }

            TipoComboBox.SelectionChanged += TipoComboBox_SelectionChanged;
            
            // Load subscriptions and payment methods when creating a member
            // Only load after InitializeComponent has completed
            if (!_isEditMode && TipoComboBox.SelectedIndex == 0)
            {
                // Use Dispatcher to ensure controls are initialized
                Dispatcher.BeginInvoke(new System.Action(async () =>
                {
                    await LoadSubscriptionsAndPaymentMethodsAsync();
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private static string FormatEnumName(string enumName)
        {
            // Casos especiais
            if (enumName == "MBWay") return "MBWay";
            if (enumName == "Cartao") return "Cartão";
            if (enumName == "Bracos") return "Braços";
            
            // Adiciona espaço antes de letras maiúsculas (exceto a primeira)
            return System.Text.RegularExpressions.Regex.Replace(enumName, "(?<!^)([A-Z])", " $1");
        }

        private async void LoadUserData()
        {
            if (_existingUser == null) return;

            EmailTextBox.Text = _existingUser.Email;
            EmailTextBox.IsReadOnly = true;
            NomeTextBox.Text = _existingUser.Nome;
            TelemovelTextBox.Text = _existingUser.Telemovel;
            AtivoCheckBox.IsChecked = _existingUser.Ativo;

            if (_existingUser.Tipo == "Membro")
            {
                TipoComboBox.SelectedIndex = 0;
                if (_existingUser.DataNascimento.HasValue)
                    DataNascimentoDatePicker.SelectedDate = _existingUser.DataNascimento.Value;
                
                // Load subscriptions and payment methods
                await LoadSubscriptionsAndPaymentMethodsAsync();
                
                // Select the subscription from user
                if (SubscricaoComboBox != null && _existingUser.IdSubscricao.HasValue)
                {
                    var subscriptions = SubscricaoComboBox.ItemsSource as System.Collections.IEnumerable;
                    if (subscriptions != null)
                    {
                        foreach (SubscriptionResponseDto sub in subscriptions)
                        {
                            if (sub.IdSubscricao == _existingUser.IdSubscricao.Value)
                            {
                                SubscricaoComboBox.SelectedItem = sub;
                                break;
                            }
                        }
                    }
                }
                
                // Load and select the last payment method for this member
                // Wait a bit to ensure ComboBox is fully populated
                await Task.Delay(100);
                
                if (MetodoPagamentoComboBox != null && MetodoPagamentoComboBox.Items.Count > 0)
                {
                    var allPayments = await _apiService.GetPaymentsByActiveStateAsync(true);
                    var inactivePayments = await _apiService.GetPaymentsByActiveStateAsync(false);
                    var allPaymentsList = new List<PaymentResponseDto>();
                    if (allPayments != null) allPaymentsList.AddRange(allPayments);
                    if (inactivePayments != null) allPaymentsList.AddRange(inactivePayments);
                    
                    // Get all members to find IdMembro
                    var allMembers = await _apiService.GetAllMembersAsync();
                    var member = allMembers?.FirstOrDefault(m => m.IdUser == _existingUser.IdUser);
                    
                    if (member != null)
                    {
                        var lastPayment = allPaymentsList
                            .Where(p => p.IdMembro == member.IdMembro)
                            .OrderByDescending(p => p.MesReferente)
                            .ThenByDescending(p => p.DataRegisto)
                            .FirstOrDefault();
                        
                        if (lastPayment != null)
                        {
                            // Find and select the payment method in the ComboBox
                            MetodoPagamentoComboBox.SelectedItem = null; // Clear selection first
                            
                            foreach (ComboBoxItem item in MetodoPagamentoComboBox.Items)
                            {
                                if (item.Tag is MetodoPagamento metodo && Enum.TryParse<MetodoPagamento>(lastPayment.MetodoPagamento, out var lastMetodo) &&
    metodo == lastMetodo)
                                {
                                    MetodoPagamentoComboBox.SelectedItem = item;
                                    break;
                                }

                            }
                        }
                    }
                }
            }
            else
            {
                TipoComboBox.SelectedIndex = 1;
                if (!string.IsNullOrEmpty(_existingUser.Funcao))
                {
                    for (int i = 0; i < FuncaoComboBox.Items.Count; i++)
                    {
                        if (FuncaoComboBox.Items[i] is ComboBoxItem item &&
                            item.Content.ToString() == _existingUser.Funcao)
                        {
                            FuncaoComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        private async void TipoComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs? e)
        {
            if (TipoComboBox.SelectedIndex == 0) // Membro
            {
                MembroPanel.Visibility = Visibility.Visible;
                FuncionarioPanel.Visibility = Visibility.Collapsed;
                
                // Load subscriptions and payment methods when switching to member
                if (!_isEditMode && SubscricaoComboBox != null && MetodoPagamentoComboBox != null)
                {
                    await LoadSubscriptionsAndPaymentMethodsAsync();
                }
            }
            else // Funcionario
            {
                MembroPanel.Visibility = Visibility.Collapsed;
                FuncionarioPanel.Visibility = Visibility.Visible;
            }
        }

        private async Task LoadSubscriptionsAndPaymentMethodsAsync()
        {
            try
            {
                // Load subscriptions
                var subscriptions = await _apiService.GetSubscriptionsByStateAsync(true);
                if (subscriptions != null && SubscricaoComboBox != null)
                {
                    SubscricaoComboBox.ItemsSource = subscriptions;
                }

                // Load payment methods (enum)
                if (MetodoPagamentoComboBox != null)
                {
                    MetodoPagamentoComboBox.Items.Clear();
                    foreach (MetodoPagamento metodo in Enum.GetValues(typeof(MetodoPagamento)))
                    {
                        var displayName = FormatEnumName(metodo.ToString());
                        var item = new ComboBoxItem { Content = displayName, Tag = metodo };
                        MetodoPagamentoComboBox.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar subscrições e métodos de pagamento: {ex.Message}",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(NomeTextBox.Text) ||
                string.IsNullOrWhiteSpace(TelemovelTextBox.Text))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios.", 
                    "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate payment method for new members
            if (!_isEditMode && TipoComboBox.SelectedIndex == 0) // Criar novo membro
            {
                if (MetodoPagamentoComboBox == null || MetodoPagamentoComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Por favor, selecione um método de pagamento.", 
                        "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                if (_isEditMode && _existingUser != null)
                {
                    bool success = false;
                    string errorMessage = "";

                    if (TipoComboBox.SelectedIndex == 0) // Membro
                    {
                        // Get IdMembro from all members
                        var allMembers = await _apiService.GetAllMembersAsync();
                        var member = allMembers?.FirstOrDefault(m => m.IdUser == _existingUser.IdUser);
                        
                        if (member == null)
                        {
                            MessageBox.Show("Membro não encontrado.",
                                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var updateDto = new UpdateMemberDto
                        {
                            Nome = NomeTextBox.Text,
                            Telemovel = TelemovelTextBox.Text
                        };

                        if (DataNascimentoDatePicker.SelectedDate.HasValue)
                            updateDto.DataNascimento = DataNascimentoDatePicker.SelectedDate.Value;
                        if (SubscricaoComboBox != null && SubscricaoComboBox.SelectedItem is SubscriptionResponseDto selectedSub)
                            updateDto.IdSubscricao = selectedSub.IdSubscricao;

                        var (updateSuccess, updateError) = await _apiService.UpdateMemberAsync(member.IdMembro, updateDto);
                        success = updateSuccess;
                        errorMessage = updateError ?? "";

                        // Update payment method if changed or create payment if needed
                        if (success && MetodoPagamentoComboBox != null && MetodoPagamentoComboBox.SelectedItem is ComboBoxItem selectedPaymentMethod)
                        {
                            var newMetodoPagamento = (MetodoPagamento)selectedPaymentMethod.Tag;
                            
                            // Get the last payment for this member (refresh from API to get latest data)
                            var allPayments = await _apiService.GetPaymentsByActiveStateAsync(true);
                            var inactivePayments = await _apiService.GetPaymentsByActiveStateAsync(false);
                            var allPaymentsList = new List<PaymentResponseDto>();
                            if (allPayments != null) allPaymentsList.AddRange(allPayments);
                            if (inactivePayments != null) allPaymentsList.AddRange(inactivePayments);
                            
                            var lastPayment = allPaymentsList
                                .Where(p => p.IdMembro == member.IdMembro)
                                .OrderByDescending(p => p.MesReferente)
                                .ThenByDescending(p => p.DataRegisto)
                                .FirstOrDefault();

                            // Update payment method if payment exists
                            var selectedSubscription = SubscricaoComboBox.SelectedItem as SubscriptionResponseDto;

                            if (lastPayment != null)
                            {
                                if (Enum.TryParse<MetodoPagamento>(lastPayment.MetodoPagamento, out var lastMetodo))
                                {
                                    if (lastMetodo != newMetodoPagamento)
                                    {
                                        var paymentUpdateDto = new UpdatePaymentDto
                                        {
                                            MetodoPagamento = newMetodoPagamento,
                                            IdSubscricao = selectedSubscription?.IdSubscricao ?? lastPayment.IdSubscricao
                                        };

                                        var (paymentUpdateSuccess, paymentUpdateError) = await _apiService.UpdatePaymentAsync(lastPayment.IdPagamento, paymentUpdateDto);
                                    }
                                }
                            }


                            // If no payment exists but subscription is selected, try to create a payment
                            // Note: The PaymentService calculates MesReferente automatically and may reject if payment already exists
                            else if (SubscricaoComboBox != null)
                            {
                                try
                                {
                                    // Verify subscription is active before attempting to create payment
                                    if (!selectedSubscription.Ativo)
                                    {
                                        MessageBox.Show($"Dados do membro atualizados, mas não foi possível criar o pagamento: A subscrição selecionada está inativa.",
                                            "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    }
                                    else
                                    {
                                        // The PaymentService will calculate the MesReferente automatically based on current date
                                        // It will use current month if day <= 25, otherwise next month
                                        var paymentDto = new PaymentDto
                                        {
                                            IdMembro = member.IdMembro,
                                            IdSubscricao = selectedSubscription.IdSubscricao,
                                            MetodoPagamento = newMetodoPagamento,
                                            MesReferente = DateTime.UtcNow // Service will recalculate this
                                        };
                                        
                                        var (paymentCreateSuccess, paymentCreateError, createdPayment) = await _apiService.CreatePaymentAsync(paymentDto);
                                        if (!paymentCreateSuccess)
                                        {
                                            // Check if error is because payment already exists for this period
                                            var paymentErrorMessage = paymentCreateError ?? "Erro desconhecido";
                                            var errorLower = paymentErrorMessage.ToLower();
                                            
                                            if (errorLower.Contains("já existe") || errorLower.Contains("já existe um pagamento") || errorLower.Contains("payment already exists"))
                                            {
                                                // Payment already exists for this period, try to find and update it
                                                // Calculate the expected MesReferente (same logic as PaymentService)
                                                var hoje = DateTime.UtcNow;
                                                var expectedMesReferente = hoje.Day > 25
                                                    ? new DateTime(hoje.Year, hoje.Month, 1).AddMonths(1)
                                                    : new DateTime(hoje.Year, hoje.Month, 1);
                                                
                                                // Refresh payments list to get the latest data
                                                var refreshedActivePayments = await _apiService.GetPaymentsByActiveStateAsync(true);
                                                var refreshedInactivePayments = await _apiService.GetPaymentsByActiveStateAsync(false);
                                                var refreshedPaymentsList = new List<PaymentResponseDto>();
                                                if (refreshedActivePayments != null) refreshedPaymentsList.AddRange(refreshedActivePayments);
                                                if (refreshedInactivePayments != null) refreshedPaymentsList.AddRange(refreshedInactivePayments);
                                                
                                                // Try to find the payment for this member and subscription
                                                // First, try to find payment matching the expected period
                                                var paymentToUpdate = refreshedPaymentsList
                                                    .Where(p => p.IdMembro == member.IdMembro &&
                                                                p.Subscricao == FormatEnumName(selectedSubscription.Tipo.ToString()) &&
                                                                p.MesReferente.Year == expectedMesReferente.Year &&
                                                                p.MesReferente.Month == expectedMesReferente.Month &&
                                                                p.DataDesativacao == null) // Only active payments
                                                    .OrderByDescending(p => p.DataRegisto)
                                                    .FirstOrDefault();
                                                
                                                // If not found, try to find any active payment for this member and subscription
                                                if (paymentToUpdate == null)
                                                {
                                                    paymentToUpdate = refreshedPaymentsList
                                                        .Where(p => p.IdMembro == member.IdMembro &&
                                                                    p.Subscricao == FormatEnumName(selectedSubscription.Tipo.ToString()) &&
                                                                    p.DataDesativacao == null)
                                                        .OrderByDescending(p => p.MesReferente)
                                                        .ThenByDescending(p => p.DataRegisto)
                                                        .FirstOrDefault();
                                                }
                                                
                                                // If still not found, try any active payment for this member
                                                if (paymentToUpdate == null)
                                                {
                                                    paymentToUpdate = refreshedPaymentsList
                                                        .Where(p => p.IdMembro == member.IdMembro && p.DataDesativacao == null)
                                                        .OrderByDescending(p => p.MesReferente)
                                                        .ThenByDescending(p => p.DataRegisto)
                                                        .FirstOrDefault();
                                                }
                                                
                                                if (paymentToUpdate != null)
                                                {
                                                    var paymentUpdateDto = new UpdatePaymentDto
                                                    {
                                                        MetodoPagamento = newMetodoPagamento
                                                    };
                                                    
                                                    var (paymentMethodUpdateSuccess, paymentMethodUpdateError) = await _apiService.UpdatePaymentAsync(paymentToUpdate.IdPagamento, paymentUpdateDto);
                                                    if (!paymentMethodUpdateSuccess)
                                                    {
                                                        MessageBox.Show($"Dados do membro atualizados. Já existe um pagamento para este período. Erro ao atualizar método de pagamento: {paymentMethodUpdateError ?? "Erro desconhecido"}",
                                                            "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                                                    }
                                                    // If update succeeds, no message needed - it's expected behavior
                                                }
                                                else
                                                {
                                                    // Payment exists but we couldn't find it - this shouldn't happen but handle gracefully
                                                    MessageBox.Show($"Dados do membro atualizados. Já existe um pagamento ativo para este período, mas não foi possível encontrá-lo para atualizar o método de pagamento. Por favor, atualize manualmente na secção de Pagamentos.",
                                                        "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
                                                }
                                            }
                                            else
                                            {
                                                // Other error - show the actual error message with full details
                                                MessageBox.Show($"Dados do membro atualizados, mas houve um erro ao criar o pagamento:\n\n{paymentErrorMessage}\n\n" +
                                                    $"IdMembro: {member.IdMembro}\n" +
                                                    $"IdSubscricao: {selectedSubscription.IdSubscricao}\n" +
                                                    $"Método: {newMetodoPagamento}",
                                                    "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                                            }
                                        }
                                        // If payment creation succeeds, no message needed - it's expected behavior
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Dados do membro atualizados, mas houve um erro ao criar o pagamento:\n\n{ex.Message}\n\nTipo: {ex.GetType().Name}",
                                        "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                        }

                        // Update active status separately
                        if (success && AtivoCheckBox.IsChecked != _existingUser.Ativo)
                        {
                            var statusSuccess = await _apiService.ChangeUserActiveStatusAsync(_existingUser.IdUser, AtivoCheckBox.IsChecked ?? true);
                            if (!statusSuccess)
                            {
                                MessageBox.Show("Dados atualizados, mas houve um erro ao alterar o estado do utilizador.",
                                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                    else // Funcionario
                    {
                        // For employees, use UpdateEmployeeAsync
                        // First need to get IdFuncionario - for now use a workaround
                        var updateDto = new UpdateEmployeeDto
                        {
                            Nome = NomeTextBox.Text,
                            Telemovel = TelemovelTextBox.Text
                        };

                        if (FuncaoComboBox.SelectedItem is ComboBoxItem selectedItem)
                            updateDto.Funcao = selectedItem.Content.ToString();

                        // TODO: Get IdFuncionario from employee list
                        // For now, try using UserUpdateDto approach if available
                        var userUpdateDto = new UserUpdateDto
                        {
                            Nome = NomeTextBox.Text,
                            Telemovel = TelemovelTextBox.Text,
                            Ativo = AtivoCheckBox.IsChecked,
                            Funcao = updateDto.Funcao
                        };

                        // Try UpdateUserAsync first (may not exist)
                        success = await _apiService.UpdateUserAsync(_existingUser.IdUser, userUpdateDto);
                        if (!success)
                        {
                            errorMessage = "Erro ao atualizar funcionário. Endpoint pode não estar disponível.";
                        }
                    }

                    if (success)
                    {
                        MessageBox.Show("Utilizador atualizado com sucesso!", 
                            "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show($"Erro ao atualizar utilizador: {errorMessage}", 
                            "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Criar
                    var registerDto = new UserRegisterDto
                    {
                        Email = EmailTextBox.Text,
                        Tipo = TipoComboBox.SelectedIndex == 0 ? "Membro" : "Funcionario",
                        Nome = NomeTextBox.Text,
                        Telemovel = TelemovelTextBox.Text
                    };

                    if (TipoComboBox.SelectedIndex == 0) // Membro
                    {
                        if (DataNascimentoDatePicker.SelectedDate.HasValue)
                            registerDto.DataNascimento = DataNascimentoDatePicker.SelectedDate.Value;
                        if (SubscricaoComboBox != null && SubscricaoComboBox.SelectedItem is SubscriptionResponseDto selectedSub)
                            registerDto.IdSubscricao = selectedSub.IdSubscricao;
                        if (MetodoPagamentoComboBox != null && MetodoPagamentoComboBox.SelectedItem is ComboBoxItem selectedPaymentMethod)
                            registerDto.MetodoPagamento = (MetodoPagamento)selectedPaymentMethod.Tag;
                    }
                    else // Funcionario
                    {
                        if (FuncaoComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
                            registerDto.Funcao = selectedItem.Content.ToString();
                    }

                    var registerResult = await _apiService.RegisterUserAsync(registerDto);
                    if (registerResult.Success)
                    {
                        // If creating a member with subscription and payment method selected, try to create payment
                        // But don't show error if payment already exists for the period
                        

                        MessageBox.Show("Utilizador criado com sucesso!", 
                            "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        var errorMessage = registerResult.ErrorMessage ?? "Erro ao criar utilizador.";
                        MessageBox.Show(errorMessage, 
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
