using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FitControlAdmin.Helper;
using FitControlAdmin.Models;
using FitControlAdmin.Services;

namespace FitControlAdmin
{
    public partial class ScheduledClassesWindow : Window
    {
        private readonly ApiService _apiService;
        private int _currentPtId;

        public ScheduledClassesWindow(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            
            // Obter ID do PT logado
            var token = _apiService.CurrentAccessToken;
            if (!string.IsNullOrEmpty(token))
            {
                var idUserClaim = JwtHelper.GetClaim(token, "IdUser");
                if (int.TryParse(idUserClaim, out int idUser))
                {
                    _currentPtId = idUser;
                }
            }

            LoadScheduledClasses();
        }

        private async void LoadScheduledClasses()
        {
            try
            {
                var scheduledClasses = await _apiService.GetScheduledClassesByPTAsync(_currentPtId);
                if (scheduledClasses != null)
                {
                    // Filtrar apenas aulas das próximas 2 semanas
                    var today = DateTime.Today;
                    var twoWeeksFromNow = today.AddDays(14);
                    
                    var filteredClasses = scheduledClasses
                        .Where(c => c.DataAula.Date >= today && c.DataAula.Date <= twoWeeksFromNow)
                        .OrderBy(c => c.DataAula)
                        .ToList();

                    ScheduledClassesDataGrid.ItemsSource = filteredClasses;

                    if (filteredClasses.Count == 0)
                    {
                        MessageBox.Show("Não há aulas agendadas para as próximas 2 semanas.", "Informação", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Erro ao carregar aulas agendadas.", "Erro", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar aulas agendadas: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GenerateClassesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Isto irá gerar automaticamente aulas agendadas para as suas aulas recorrentes nas próximas 2 semanas.\n\nDeseja continuar?",
                    "Confirmar Geração",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var (success, errorMessage, aulasGeradas) = await _apiService.GenerateScheduledClassesForPTAsync(_currentPtId);
                    
                    if (success)
                    {
                        MessageBox.Show("Aulas geradas com sucesso!", "Sucesso", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadScheduledClasses();
                    }
                    else
                    {
                        MessageBox.Show($"Erro ao gerar aulas: {errorMessage}", "Erro", 
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

        private async void CreateScheduledClassButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obter lista de aulas do PT
                var classes = await _apiService.GetAllClassesAsync();
                if (classes == null || classes.Count == 0)
                {
                    MessageBox.Show("Não há aulas disponíveis para agendar.", "Informação", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Filtrar apenas aulas do PT atual e ativas
                var ptClasses = classes.Where(c => c.IdFuncionario == _currentPtId && c.DataDesativacao == null).ToList();
                
                if (ptClasses.Count == 0)
                {
                    MessageBox.Show("Você não tem aulas recorrentes criadas.", "Informação", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Criar diálogo para selecionar aula e data
                var dialog = new CreateScheduledClassDialog(ptClasses);
                if (dialog.ShowDialog() == true)
                {
                    var selectedClass = ptClasses.FirstOrDefault(c => c.IdAula == dialog.SelectedClassId);
                    if (selectedClass == null)
                    {
                        MessageBox.Show("Aula não encontrada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var dataSelecionada = dialog.SelectedDate;
                    var scheduleDto = new ScheduleClassDto
                    {
                        IdAula = dialog.SelectedClassId,
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
                        MessageBox.Show("Aula agendada com sucesso!", "Sucesso", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadScheduledClasses();
                    }
                    else
                    {
                        // Se foi erro 500/Internal, verificar se a aula foi criada (workaround para API que cria mas falha na resposta)
                        var isServerError = errorMessage != null && (errorMessage.Contains("Internal", StringComparison.OrdinalIgnoreCase) || errorMessage.Contains("500"));
                        if (isServerError)
                        {
                            var scheduledClasses = await _apiService.GetScheduledClassesByPTAsync(_currentPtId);
                            var foiCriada = scheduledClasses?.Any(sc => sc.IdAula == scheduleDto.IdAula && sc.DataAula.Date == scheduleDto.DataAula.Date) == true;
                            if (foiCriada)
                            {
                                MessageBox.Show("Aula agendada com sucesso!", "Sucesso", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadScheduledClasses();
                                return;
                            }
                        }

                        // Se "já existe aula" - pode ter sido criada na tentativa anterior
                        if (errorMessage != null && (errorMessage.Contains("já existe", StringComparison.OrdinalIgnoreCase) || errorMessage.Contains("existe aula")))
                        {
                            MessageBox.Show($"{errorMessage}\n\nA aula pode ter sido criada numa tentativa anterior. Verifique a lista de aulas agendadas.", "Aviso", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadScheduledClasses();
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

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadScheduledClasses();
        }

        private void MarkAttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is int idAulaMarcada)
                {
                    var attendanceWindow = new MarkAttendanceWindow(_apiService, idAulaMarcada);
                    attendanceWindow.ShowDialog();
                    
                    // Recarregar lista após marcar presenças
                    LoadScheduledClasses();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir janela de presenças: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteScheduledClassButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is int idAulaMarcada)
                {
                    var result = MessageBox.Show(
                        "Tem certeza que deseja eliminar esta aula agendada?\n\nTodas as reservas associadas serão canceladas.",
                        "Confirmar Eliminação",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        var (success, errorMessage) = await _apiService.CancelScheduledClassAsync(idAulaMarcada);
                        
                        if (success)
                        {
                            MessageBox.Show("Aula eliminada com sucesso!", "Sucesso", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadScheduledClasses();
                        }
                        else
                        {
                            MessageBox.Show($"Erro ao eliminar aula: {errorMessage}", "Erro", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
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
}
