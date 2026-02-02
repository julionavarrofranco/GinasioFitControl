using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FitControlAdmin
{
    public partial class MemberEvaluationHistoryWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly MemberDto _member;

        public MemberEvaluationHistoryWindow(ApiService apiService, MemberDto member)
        {
            InitializeComponent();
            _apiService = apiService;
            _member = member;

            Title = $"Histórico de Avaliações Físicas - {member.Nome}";
            MemberInfoText.Text = $"Membro: {member.Nome} | Email: {member.Email}";

            LoadMemberHistory();
        }

        private async void LoadMemberHistory()
        {
            try
            {
                var evaluations = await _apiService.GetAllEvaluationsForMemberAsync(_member.IdMembro);
                if (evaluations != null && evaluations.Count > 0)
                {
                    EvaluationsDataGrid.ItemsSource = evaluations.OrderByDescending(e => e.DataAvaliacao);
                }
                else
                {
                    EvaluationsDataGrid.ItemsSource = new List<PhysicalEvaluationResponseDto>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar histórico: {ex.Message}",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
