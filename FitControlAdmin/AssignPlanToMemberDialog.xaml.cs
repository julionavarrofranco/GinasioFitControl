using System;
using System.Collections.Generic;
using System.Windows;
using FitControlAdmin.Models;
using FitControlAdmin.Services;

namespace FitControlAdmin
{
    public partial class AssignPlanToMemberDialog : Window
    {
        private readonly ApiService _apiService;
        private readonly int _idPlano;

        public AssignPlanToMemberDialog(List<MemberDto> members, int idPlano, ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            _idPlano = idPlano;

            MemberComboBox.ItemsSource = members;
        }

        private async void AssignButton_Click(object sender, RoutedEventArgs e)
        {
            if (MemberComboBox.SelectedValue == null)
            {
                MessageBox.Show("Selecione um membro.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var idMembro = (int)MemberComboBox.SelectedValue;

            try
            {
                var (success, errorMessage) = await _apiService.AssignTrainingPlanToMemberAsync(idMembro, _idPlano);
                if (success)
                {
                    MessageBox.Show("Plano atribuído ao membro com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(errorMessage ?? "Erro ao atribuir plano.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
