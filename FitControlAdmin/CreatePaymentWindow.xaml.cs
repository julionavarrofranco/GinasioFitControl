using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FitControlAdmin
{
    public partial class CreatePaymentWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly int? _preSelectedMemberId;
        private readonly PaymentResponseDto? _paymentToEdit;
        private readonly MemberDto? _memberInfo;
        private readonly bool _isEditMode;

        public CreatePaymentWindow(ApiService apiService, int? preSelectedMemberId = null, PaymentResponseDto? paymentToEdit = null, MemberDto? memberInfo = null)
        {
            InitializeComponent();
            _apiService = apiService;
            _preSelectedMemberId = preSelectedMemberId;
            _paymentToEdit = paymentToEdit;
            _memberInfo = memberInfo;
            _isEditMode = paymentToEdit != null;

            // Set title based on mode
            // If opened from member list (has memberInfo and preSelectedMemberId), always show "Editar Pagamento"
            if (_isEditMode || (preSelectedMemberId.HasValue && memberInfo != null))
            {
                Title = "Editar Pagamento";
                TitleText.Text = "Editar Pagamento";
                ConfirmButton.Content = "Confirmar";
            }
            else
            {
                Title = "Criar Pagamento";
                TitleText.Text = "Criar Pagamento";
                ConfirmButton.Content = "Criar";
            }
            
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                // Carregar membros e subscrições
                var membersTask = _apiService.GetAllMembersAsync();
                var subscriptionsTask = _apiService.GetSubscriptionsByStateAsync(true);

                await System.Threading.Tasks.Task.WhenAll(membersTask, subscriptionsTask);

                var members = await membersTask;
                var subscriptions = await subscriptionsTask;

                // Popular ComboBox de membros
                if (members != null && members.Count > 0)
                {
                    MembroComboBox.ItemsSource = members;
                    MembroComboBox.SelectedValuePath = "IdMembro";
                    
                    if (_isEditMode && _paymentToEdit != null)
                    {
                        // Pre-select member from payment being edited
                        MembroComboBox.SelectedValue = _paymentToEdit.IdMembro;
                        MembroComboBox.IsEnabled = false; // Disable selection when editing
                    }
                    else if (_preSelectedMemberId.HasValue)
                    {
                        // Pre-select member if provided
                        MembroComboBox.SelectedValue = _preSelectedMemberId.Value;
                        MembroComboBox.IsEnabled = false; // Disable selection if pre-selected
                    }
                }
                else
                {
                    MessageBox.Show("Não há membros disponíveis.", "Aviso", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // Popular ComboBox de subscrições
                if (subscriptions != null && subscriptions.Count > 0)
                {
                    SubscricaoComboBox.ItemsSource = subscriptions;
                    SubscricaoComboBox.SelectedValuePath = "IdSubscricao";
                    
                    if (_isEditMode && _paymentToEdit != null)
                    {
                        // Pre-select subscription from payment being edited
                        var selectedSub = subscriptions.FirstOrDefault(s => FormatEnumName(s.Tipo.ToString()) == _paymentToEdit.Subscricao);
                        if (selectedSub != null)
                        {
                            SubscricaoComboBox.SelectedValue = selectedSub.IdSubscricao;
                        }
                        // Allow subscription change when editing (API now supports it)
                    }
                    else if (_memberInfo != null && !string.IsNullOrEmpty(_memberInfo.Subscricao))
                    {
                        // Pre-select subscription from member info (when creating new payment)
                        var memberSubscription = subscriptions.FirstOrDefault(s => s.Nome == _memberInfo.Subscricao);
                        if (memberSubscription != null)
                        {
                            SubscricaoComboBox.SelectedValue = memberSubscription.IdSubscricao;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Não há subscrições ativas disponíveis.", "Aviso", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // Popular ComboBox de métodos de pagamento
                var metodosList = new System.Collections.Generic.List<object>();
                foreach (MetodoPagamento metodo in Enum.GetValues(typeof(MetodoPagamento)))
                {
                    metodosList.Add(new { DisplayName = FormatEnumName(metodo.ToString()), Value = metodo });
                }
                MetodoPagamentoComboBox.ItemsSource = metodosList;
                
                // Pre-select payment method
                if (_isEditMode && _paymentToEdit != null)
                {
                    // Pre-select payment method from payment being edited
                    var selectedMetodo = metodosList.FirstOrDefault(m => ((dynamic)m).Value.ToString() == _paymentToEdit.MetodoPagamento);
                    if (selectedMetodo != null)
                        MetodoPagamentoComboBox.SelectedItem = selectedMetodo;
                }
                else if (_memberInfo != null)
                {
                    // When creating new payment or editing without existing payment, try to get last payment method from member's payments
                    // Search both active and inactive payments to find the most recent payment method used
                    var activePaymentsTask = _apiService.GetPaymentsByActiveStateAsync(true);
                    var inactivePaymentsTask = _apiService.GetPaymentsByActiveStateAsync(false);
                    await System.Threading.Tasks.Task.WhenAll(activePaymentsTask, inactivePaymentsTask);
                    
                    var activePayments = await activePaymentsTask;
                    var inactivePayments = await inactivePaymentsTask;
                    
                    var allMemberPayments = new List<PaymentResponseDto>();
                    if (activePayments != null)
                        allMemberPayments.AddRange(activePayments.Where(p => p.IdMembro == _memberInfo.IdMembro));
                    if (inactivePayments != null)
                        allMemberPayments.AddRange(inactivePayments.Where(p => p.IdMembro == _memberInfo.IdMembro));
                    
                    var lastPayment = allMemberPayments
                        .OrderByDescending(p => p.MesReferente)
                        .ThenByDescending(p => p.DataRegisto)
                        .FirstOrDefault();
                    
                    if (lastPayment != null)
                    {
                        // Pre-select payment method from last payment
                        var selectedMetodo = metodosList.FirstOrDefault(m => ((dynamic)m).Value == lastPayment.MetodoPagamento);
                        if (selectedMetodo != null)
                            MetodoPagamentoComboBox.SelectedItem = selectedMetodo;
                    }
                }

                // Definir mês referente
                if (_isEditMode && _paymentToEdit != null)
                {
                    // Use month from payment being edited
                    MesReferenteDatePicker.SelectedDate = _paymentToEdit.MesReferente;
                    MesReferenteDatePicker.IsEnabled = false; // Disable month change when editing
                }
                else
                {
                    // Definir mês referente padrão (primeiro dia do mês atual ou próximo)
                    var hoje = DateTime.Now;
                    var mesReferente = hoje.Day > 25
                        ? new DateTime(hoje.Year, hoje.Month, 1).AddMonths(1)
                        : new DateTime(hoje.Year, hoje.Month, 1);
                    
                    MesReferenteDatePicker.SelectedDate = mesReferente;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}", 
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Validação
            if (MembroComboBox.SelectedValue == null)
            {
                MessageBox.Show("Por favor, selecione um membro.", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SubscricaoComboBox.SelectedValue == null)
            {
                MessageBox.Show("Por favor, selecione uma subscrição.", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MetodoPagamentoComboBox.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecione um método de pagamento.", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MesReferenteDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Por favor, selecione o mês referente.", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Verificar se estamos editando um pagamento existente
                // Verificar novamente se há pagamento para editar (pode ter mudado após LoadData)
                bool shouldUpdate = _paymentToEdit != null && _paymentToEdit.IdPagamento > 0;

                if (shouldUpdate)
                {
                    // Modo edição - atualizar pagamento existente
                    // Método de pagamento e subscrição podem ser editados (MesReferente não pode ser alterado)

                    // Obter valores selecionados
                    dynamic metodoItem = MetodoPagamentoComboBox.SelectedItem;
                    var metodoPagamento = (MetodoPagamento)metodoItem.Value;
                    var idSubscricao = (int)SubscricaoComboBox.SelectedValue;

                    // Carregar subscrições para verificar mudança
                    var subscriptions = await _apiService.GetSubscriptionsByStateAsync(true);
                    var currentSubscription = subscriptions?.FirstOrDefault(s => FormatEnumName(s.Tipo.ToString()) == _paymentToEdit.Subscricao);

                    // Verificar se houve alteração
                    var metodoChanged = metodoPagamento.ToString() != _paymentToEdit.MetodoPagamento;
                    var subscricaoChanged = currentSubscription == null || idSubscricao != currentSubscription.IdSubscricao;

                    if (!metodoChanged && !subscricaoChanged)
                    {
                        MessageBox.Show("Nenhuma alteração foi realizada.",
                            "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Criar DTO de atualização
                    var updateDto = new UpdatePaymentDto();
                    if (metodoChanged)
                        updateDto.MetodoPagamento = metodoPagamento;
                    if (subscricaoChanged)
                        updateDto.IdSubscricao = idSubscricao;

                    // Debug: Log what we're updating
                    System.Diagnostics.Debug.WriteLine($"ConfirmButton_Click: Updating payment {_paymentToEdit.IdPagamento}");
                    System.Diagnostics.Debug.WriteLine($"ConfirmButton_Click: MetodoChanged={metodoChanged}, SubscricaoChanged={subscricaoChanged}");
                    System.Diagnostics.Debug.WriteLine($"ConfirmButton_Click: New MetodoPagamento={metodoPagamento}, New IdSubscricao={idSubscricao}");

                    // Atualizar pagamento
                    var (success, errorMessage) = await _apiService.UpdatePaymentAsync(_paymentToEdit.IdPagamento, updateDto);
                    if (success)
                    {
                        // Debug: Log successful update
                        System.Diagnostics.Debug.WriteLine("ConfirmButton_Click: Payment update successful");

                        MessageBox.Show("Pagamento atualizado com sucesso!",
                            "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        // Debug: Log failed update
                        System.Diagnostics.Debug.WriteLine($"ConfirmButton_Click: Payment update failed: {errorMessage}");

                        // Mensagem de erro específica para atualização
                        var errorMsg = errorMessage ?? "Erro ao atualizar pagamento.";
                        // Garantir que a mensagem indica que é um erro de atualização
                        if (!errorMsg.Contains("atualizar") && !errorMsg.Contains("Atualizar"))
                        {
                            errorMsg = "Não é possível atualizar: " + errorMsg;
                        }
                        MessageBox.Show(errorMsg,
                            "Erro ao Atualizar", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Criar novo pagamento (quando não há pagamento existente para editar)
                    // Obter valores selecionados
                    var idMembro = (int)MembroComboBox.SelectedValue;
                    var idSubscricao = (int)SubscricaoComboBox.SelectedValue;
                    
                    // Obter método de pagamento
                    dynamic metodoItem = MetodoPagamentoComboBox.SelectedItem;
                    var metodoPagamento = (MetodoPagamento)metodoItem.Value;
                    
                    // Normalizar mês referente (primeiro dia do mês)
                    var mesReferente = MesReferenteDatePicker.SelectedDate.Value;
                    var mesReferenteNormalizado = new DateTime(mesReferente.Year, mesReferente.Month, 1);

                    // Criar DTO
                    var paymentDto = new PaymentDto
                    {
                        IdMembro = idMembro,
                        IdSubscricao = idSubscricao,
                        MetodoPagamento = metodoPagamento,
                        MesReferente = mesReferenteNormalizado
                    };

                    // Criar pagamento
                    var (success, errorMessage, payment) = await _apiService.CreatePaymentAsync(paymentDto);
                    if (success && payment != null)
                    {
                        MessageBox.Show("Pagamento criado com sucesso!", 
                            "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(errorMessage ?? "Erro ao criar pagamento.", 
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
