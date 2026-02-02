namespace ProjetoFinal.Helpers
{
    /// <summary>
    /// Converte DayOfWeek (C#: Sunday=0, Monday=1, ...) para DiaSemana (Segunda=0, Terca=1, ... Domingo=6).
    /// O cast direto (DiaSemana)DayOfWeek falha porque os enums têm ordens diferentes.
    /// </summary>
    public static class DiaSemanaHelper
    {
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
