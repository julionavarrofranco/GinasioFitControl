using System;
using TTFWebsite.Models;

namespace TTFWebsite.ViewModels
{
    public enum Presenca
    {
        Reservado,
        Presente,
        Cancelado,
        Faltou
    }

    public class PhysicalAssessmentViewModel
    {
        public PhysicalAssessment? LatestAssessment { get; set; }
        public Reservation? NextReservation { get; set; }
        public Presenca? ReservationState { get; set; } // opcional, derivado de IsCancelled
    }
}
