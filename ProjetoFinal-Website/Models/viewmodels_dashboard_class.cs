using System;
using System.Collections.Generic;
using TTFWebsite.Models.DTOs;

namespace TTFWebsite.Models
{
    public class DashboardViewModel
    {
        public MemberProfileViewModel Profile { get; set; } = new();
        public List<ClassReservationDto> UpcomingReservations { get; set; } = new();
        public PhysicalAssessment? RecentAssessment { get; set; }
        public List<ScheduleClassDto> AvailableClassesToday { get; set; } = new();
        public TrainingPlanViewModel TrainingPlan { get; set; } = new TrainingPlanViewModel();
    }

    public class ClassesViewModel
    {
        public List<ScheduleClassDto> AvailableClasses { get; set; } = new();
        public List<ClassReservationDto> UserReservations { get; set; } = new();
    }
}
