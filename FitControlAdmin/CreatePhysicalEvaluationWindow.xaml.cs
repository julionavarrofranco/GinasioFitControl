using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System;
using System.Windows;

namespace FitControlAdmin
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
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Validação
            if (string.IsNullOrWhiteSpace(PesoTextBox.Text) || !decimal.TryParse(PesoTextBox.Text, out decimal peso))
            {
                MessageBox.Show("Por favor, insira um peso válido.", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(AlturaTextBox.Text) || !decimal.TryParse(AlturaTextBox.Text, out decimal altura))
            {
                MessageBox.Show("Por favor, insira uma altura válida.", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ImcTextBox.Text) || !decimal.TryParse(ImcTextBox.Text, out decimal imc))
            {
                MessageBox.Show("Por favor, insira um IMC válido.", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                // Criar DTO para marcar presença e completar a avaliação
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

                // Marcar presença e completar a avaliação
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
