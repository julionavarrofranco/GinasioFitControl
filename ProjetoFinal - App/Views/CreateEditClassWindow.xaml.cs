using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace FitControlAdmin.Views
{
    public partial class CreateEditClassWindow : Window, INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly AulaResponseDto? _existingClass;

        private string _titleText = "Criar Aula";
        private string _saveButtonText = "Criar";

        private string _nome = "";
        private string _selectedDiaSemana = "Segunda";
        private string _horaInicioText = "";
        private string _horaFimText = "";
        private string _capacidadeText = "";
        private int? _selectedFuncionarioId;

        private List<FuncionarioDto> _funcionarios = new List<FuncionarioDto>();

        public CreateEditClassWindow(ApiService apiService, AulaResponseDto? existingClass = null)
        {
            InitializeComponent();
            _apiService = apiService;
            _existingClass = existingClass;
            DataContext = this;

            if (existingClass != null)
            {
                Title = "Editar Aula";
                LoadExistingClass(existingClass);
            }
            else
                Title = "Criar Aula";

            LoadFuncionariosAsync();
        }

        #region Properties

        public string TitleText
        {
            get => _titleText;
            set { _titleText = value; OnPropertyChanged(); }
        }

        public string SaveButtonText
        {
            get => _saveButtonText;
            set { _saveButtonText = value; OnPropertyChanged(); }
        }

        public string Nome
        {
            get => _nome;
            set { _nome = value; OnPropertyChanged(); }
        }

        public string SelectedDiaSemana
        {
            get => _selectedDiaSemana;
            set { _selectedDiaSemana = value; OnPropertyChanged(); }
        }

        public string HoraInicioText
        {
            get => _horaInicioText;
            set { _horaInicioText = value; OnPropertyChanged(); }
        }

        public string HoraFimText
        {
            get => _horaFimText;
            set { _horaFimText = value; OnPropertyChanged(); }
        }

        public string CapacidadeText
        {
            get => _capacidadeText;
            set { _capacidadeText = value; OnPropertyChanged(); }
        }

        public int? SelectedFuncionarioId
        {
            get => _selectedFuncionarioId;
            set { _selectedFuncionarioId = value; OnPropertyChanged(); }
        }

        public List<FuncionarioDto> Funcionarios
        {
            get => _funcionarios;
            set { _funcionarios = value; OnPropertyChanged(); }
        }

        #endregion

        #region Private Methods

        private void LoadExistingClass(AulaResponseDto existingClass)
        {
            TitleText = "Editar Aula";
            SaveButtonText = "Salvar";

            Nome = existingClass.Nome;
            SelectedDiaSemana = existingClass.DiaSemana switch
            {
                DiaSemana.Domingo => "Domingo",
                DiaSemana.Segunda => "Segunda",
                DiaSemana.Terca => "Terca",
                DiaSemana.Quarta => "Quarta",
                DiaSemana.Quinta => "Quinta",
                DiaSemana.Sexta => "Sexta",
                DiaSemana.Sabado => "Sabado",
                
                _ => "Segunda"
            };
            HoraInicioText = existingClass.HoraInicio.ToString(@"hh\:mm");
            HoraFimText = existingClass.HoraFim.ToString(@"hh\:mm");
            CapacidadeText = existingClass.Capacidade.ToString();
            SelectedFuncionarioId = existingClass.IdFuncionario;
        }

        private async void LoadFuncionariosAsync()
        {
            try
            {
                var employees = await _apiService.GetAllEmployeesAsync();
                if (employees != null)
                {
                    // Apenas PTs podem ser instrutores de aula
                    Funcionarios = employees
                        .Where(e => e.Funcao == Funcao.PT)
                        .Select(e => new FuncionarioDto
                        {
                            IdUser = e.IdUser,
                            IdFuncionario = e.IdFuncionario,
                            Nome = e.Nome,
                            Email = e.Email,
                            Telemovel = e.Telemovel,
                            Funcao = e.Funcao
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar funcionários: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(Nome))
            {
                MessageBox.Show("Por favor, insira o nome da aula.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(SelectedDiaSemana))
            {
                MessageBox.Show("Por favor, selecione o dia da semana.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!TimeSpan.TryParse(HoraInicioText, out _))
            {
                MessageBox.Show("Por favor, insira uma hora de início válida (formato HH:mm).", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!TimeSpan.TryParse(HoraFimText, out _))
            {
                MessageBox.Show("Por favor, insira uma hora de fim válida (formato HH:mm).", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(CapacidadeText, out int capacidade) || capacidade <= 0)
            {
                MessageBox.Show("Por favor, insira uma capacidade válida (número maior que 0).", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private async void SaveClassAsync()
        {
            try
            {
                if (_existingClass != null)
                {
                    var updateDto = new UpdateClassDto
                    {
                        Nome = Nome,
                        DiaSemana = SelectedDiaSemana switch
                        {
                            "Domingo" => DiaSemana.Domingo,
                            "Segunda" => DiaSemana.Segunda,
                            "Terca" => DiaSemana.Terca,
                            "Quarta" => DiaSemana.Quarta,
                            "Quinta" => DiaSemana.Quinta,
                            "Sexta" => DiaSemana.Sexta,
                            "Sabado" => DiaSemana.Sabado,                            
                            _ => DiaSemana.Segunda
                        },
                        HoraInicio = TimeSpan.Parse(HoraInicioText),
                        HoraFim = TimeSpan.Parse(HoraFimText),
                        Capacidade = int.Parse(CapacidadeText),
                        IdFuncionario = SelectedFuncionarioId
                    };

                    System.Diagnostics.Debug.WriteLine($"SaveClassAsync: Updating class {Nome} with DiaSemana: {updateDto.DiaSemana}");
                    var (success, errorMessage) = await _apiService.UpdateClassAsync(_existingClass.IdAula, updateDto);
                    if (success)
                    {
                        MessageBox.Show("Aula atualizada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(errorMessage ?? "Erro ao atualizar aula.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var createDto = new AulaDto
                    {
                        Nome = Nome,
                        DiaSemana = SelectedDiaSemana switch
                        {
                            "Domingo" => DiaSemana.Domingo,
                            "Segunda" => DiaSemana.Segunda,
                            "Terca" => DiaSemana.Terca,
                            "Quarta" => DiaSemana.Quarta,
                            "Quinta" => DiaSemana.Quinta,
                            "Sexta" => DiaSemana.Sexta,
                            "Sabado" => DiaSemana.Sabado,                            
                            _ => DiaSemana.Segunda
                        },
                        HoraInicio = TimeSpan.Parse(HoraInicioText),
                        HoraFim = TimeSpan.Parse(HoraFimText),
                        Capacidade = int.Parse(CapacidadeText),
                        IdFuncionario = SelectedFuncionarioId
                    };

                    System.Diagnostics.Debug.WriteLine($"SaveClassAsync: Creating class {Nome} with DiaSemana: {createDto.DiaSemana}, HoraInicio: {createDto.HoraInicio}, HoraFim: {createDto.HoraFim}");
                    var (success, errorMessage, newClass) = await _apiService.CreateClassAsync(createDto);
                    if (success)
                    {
                        System.Diagnostics.Debug.WriteLine($"SaveClassAsync: Class created successfully with ID: {newClass?.IdAula ?? 0}");
                        MessageBox.Show("Aula criada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"SaveClassAsync: Failed to create class: {errorMessage}");
                        MessageBox.Show(errorMessage ?? "Erro ao criar aula.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveClassAsync: Exception - {ex.Message}");
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Event Handlers

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                SaveClassAsync();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Window Events

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        #endregion
    }
}