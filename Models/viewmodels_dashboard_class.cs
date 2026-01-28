using System;
using System.Collections.Generic;

namespace TTFWebsite.Models
{
    public class DashboardViewModel
    {
        public MemberProfileViewModel Profile { get; set; } = new();
        public List<Reservation> UpcomingReservations { get; set; } = new();
        public PhysicalAssessment? RecentAssessment { get; set; }
        public List<Class> AvailableClassesToday { get; set; } = new();
        public TrainingPlanViewModel TrainingPlan { get; set; } = new TrainingPlanViewModel();
    }

    public class ClassesViewModel
    {
        public List<Class> AvailableClasses { get; set; } = new();
        public List<Reservation> UserReservations { get; set; } = new();
    }
}
