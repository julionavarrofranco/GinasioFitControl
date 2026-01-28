using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FitControlAdmin.Models;
using FitControlAdmin.Services;

namespace FitControlAdmin
{
    public partial class MarkAttendanceWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly int _idAulaMarcada;
        private ClassAttendanceDto? _attendanceData;

        public MarkAttendanceWindow(ApiService apiService, int idAulaMarcada)
        {
            InitializeComponent();
            _apiService = apiService;
            _idAulaMarcada = idAulaMarcada;
            
            LoadAttendanceData();
        }

        private async void LoadAttendanceData()
        {
            try
            {
                _attendanceData = await _apiService.GetScheduledClassForAttendanceAsync(_idAulaMarcada);
                
                if (_attendanceData != null)
                {
                    // Preencher informações da aula
                    ClassNameTextBlock.Text = _attendanceData.NomeAula;
                    DateTextBlock.Text = _attendanceData.DataAula.ToString("dd/MM/yyyy");
                    TimeTextBlock.Text = $"{_attendanceData.HoraInicio:hh\\:mm} - {_attendanceData.HoraFim:hh\\:mm}";
                    CapacityTextBlock.Text = $"{_attendanceData.TotalReservas}/{_attendanceData.Capacidade}";

                    // Verificar se está lotada
                    if (_attendanceData.TotalReservas >= _attendanceData.Capacidade)
                    {
                        CapacityTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                    }

                    // Preencher lista de membros
                    if (_attendanceData.Reservas != null && _attendanceData.Reservas.Count > 0)
                    {
                        // Apenas mostrar membros que não cancelaram
                        var activeMembros = _attendanceData.Reservas
                            .Where(r => r.Presenca != Presenca.Cancelado)
                            .ToList();

                        MembersListBox.ItemsSource = activeMembros;

                        if (activeMembros.Count == 0)
                        {
                            MessageBox.Show("Não há membros com reserva ativa para esta aula.", "Informação", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Não há membros inscritos nesta aula.", "Informação", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Erro ao carregar dados da aula.", "Erro", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (_attendanceData?.Reservas != null)
            {
                foreach (var membro in _attendanceData.Reservas.Where(r => r.Presenca != Presenca.Cancelado))
                {
                    membro.Presente = true;
                }
                MembersListBox.Items.Refresh();
            }
        }

        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (_attendanceData?.Reservas != null)
            {
                foreach (var membro in _attendanceData.Reservas.Where(r => r.Presenca != Presenca.Cancelado))
                {
                    membro.Presente = false;
                }
                MembersListBox.Items.Refresh();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_attendanceData?.Reservas == null)
                {
                    MessageBox.Show("Nenhum dado para guardar.", "Aviso", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Obter lista de IDs dos membros presentes
                var idsPresentes = _attendanceData.Reservas
                    .Where(r => r.Presente && r.Presenca != Presenca.Cancelado)
                    .Select(r => r.IdMembro)
                    .ToList();

                // Marcar presenças na API
                var (success, errorMessage) = await _apiService.MarkAttendanceAsync(_idAulaMarcada, idsPresentes);
                
                if (success)
                {
                    MessageBox.Show($"Presenças guardadas com sucesso!\n\nPresentes: {idsPresentes.Count}\nFaltaram: {_attendanceData.Reservas.Count(r => r.Presenca != Presenca.Cancelado) - idsPresentes.Count}", 
                        "Sucesso", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show($"Erro ao guardar presenças: {errorMessage}", "Erro", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
