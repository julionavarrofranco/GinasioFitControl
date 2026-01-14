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
                TitleText.Text = "Editar Utilizador";
                LoadUserData();
            }
            else if (!string.IsNullOrEmpty(preSelectedTipo))
            {
                // Pre-select the tipo when creating a new user
                if (preSelectedTipo == "Membro")
                {
                    TipoComboBox.SelectedIndex = 0;
                }
                else if (preSelectedTipo == "Funcionario")
                {
                    TipoComboBox.SelectedIndex = 1;
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
                                if (item.Tag is MetodoPagamento metodo && metodo == lastPayment.MetodoPagamento)
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

                        // Update payment method if changed
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
                            
                            // Update payment method if it changed or if payment exists
                            if (lastPayment != null)
                            {
                                if (lastPayment.MetodoPagamento != newMetodoPagamento)
                                {
                                    var paymentUpdateDto = new UpdatePaymentDto
                                    {
                                        MetodoPagamento = newMetodoPagamento
                                    };
                                    
                                    var (paymentUpdateSuccess, paymentUpdateError) = await _apiService.UpdatePaymentAsync(lastPayment.IdPagamento, paymentUpdateDto);
                                    if (!paymentUpdateSuccess)
                                    {
                                        MessageBox.Show($"Dados do membro atualizados, mas houve um erro ao atualizar o método de pagamento: {paymentUpdateError ?? "Erro desconhecido"}",
                                            "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    }
                                }
                            }
                            // If no payment exists, we could create one, but that's probably not desired here
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

