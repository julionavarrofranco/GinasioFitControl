using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System;
using System.Windows;

namespace FitControlAdmin.Views
{
    public partial class CreatePhysicalEvaluationWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly int _idMembro;
        private readonly int _idFuncionario;
        private readonly int _idAvaliacao;

        public CreatePhysicalEvaluationWindow(ApiService apiService, int idMembro, int idFuncionario, int idAvaliacao)
        {
            InitializeComponent();
            _apiService = apiService;
            _idMembro = idMembro;
            _idFuncionario = idFuncionario;
            _idAvaliacao = idAvaliacao;
            PesoTextBox.TextChanged += (s, e) => UpdateImc();
            AlturaTextBox.TextChanged += (s, e) => UpdateImc();
        }

        private void UpdateImc()
        {
            if (decimal.TryParse(PesoTextBox.Text?.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal peso) &&
                decimal.TryParse(AlturaTextBox.Text?.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal altura) &&
                altura > 0)
            {
                decimal imc = peso / (altura * altura);
                ImcTextBox.Text = imc.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            }
            else
                ImcTextBox.Text = "";
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Validação
            if (string.IsNullOrWhiteSpace(PesoTextBox.Text) || !decimal.TryParse(PesoTextBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal peso) || peso <= 0)
            {
                MessageBox.Show("Por favor, insira um peso válido.", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(AlturaTextBox.Text) || !decimal.TryParse(AlturaTextBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal altura) || altura <= 0)
            {
                MessageBox.Show("Por favor, insira uma altura válida (em metros).", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal imc = peso / (altura * altura);

            if (string.IsNullOrWhiteSpace(MassaMuscularTextBox.Text) || !decimal.TryParse(MassaMuscularTextBox.Text, out decimal massaMuscular))
            {
                MessageBox.Show("Por favor, insira uma massa muscular válida.", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(MassaGordaTextBox.Text) || !decimal.TryParse(MassaGordaTextBox.Text, out decimal massaGorda))
            {
                MessageBox.Show("Por favor, insira uma massa gorda válida.", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Confirmar reserva apenas ao completar a avaliação
                var confirmResult = await _apiService.ConfirmReservationAsync(_idMembro, _idAvaliacao, _idFuncionario);
                if (!confirmResult.Success)
                {
                    MessageBox.Show(confirmResult.ErrorMessage ?? "Erro ao confirmar reserva.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Criar DTO para marcar presença e completar avaliação
                var markAttendanceDto = new MarkAttendanceDto
                {
                    Presente = true,
                    IdFuncionario = _idFuncionario,
                    Peso = peso,
                    Altura = altura,
                    Imc = imc,
                    MassaMuscular = massaMuscular,
                    MassaGorda = massaGorda,
                    Observacoes = ObservacoesTextBox.Text
                };

                // Marcar presença e completar avaliação
                var (success, errorMessage) = await _apiService.MarkAttendanceAsync(_idMembro, _idAvaliacao, markAttendanceDto);
                if (success)
                {
                    MessageBox.Show("Avaliação física completada com sucesso!", 
                        "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(errorMessage ?? "Erro ao completar avaliação física.", 
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
