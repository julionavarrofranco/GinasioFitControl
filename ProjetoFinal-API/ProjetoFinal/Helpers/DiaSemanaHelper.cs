namespace ProjetoFinal.Helpers
{
    public static class DiaSemanaHelper
    {
        // Converte DayOfWeek para DiaSemana usando switch expression
        public static Models.DiaSemana FromDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => Models.DiaSemana.Domingo,
                DayOfWeek.Monday => Models.DiaSemana.Segunda,
                DayOfWeek.Tuesday => Models.DiaSemana.Terca,
                DayOfWeek.Wednesday => Models.DiaSemana.Quarta,
                DayOfWeek.Thursday => Models.DiaSemana.Quinta,
                DayOfWeek.Friday => Models.DiaSemana.Sexta,
                DayOfWeek.Saturday => Models.DiaSemana.Sabado,
                _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null)
            };
        }
    }
}
