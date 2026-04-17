using AutoMapper;
using DShop2024.Areas.Admin.Models.Task;
using DShop2024.Models;
using Microsoft.IdentityModel.Tokens;


namespace DShop2024.AutoMapper
{
    public class AssignmentMapper : Profile
    {
        public AssignmentMapper()
        {
            CreateMap<AssignmentModel, EditAssignmentRequest>()
                  .ForMember(pv => pv.EmployeeName, p => p.MapFrom(p => p.Employee.UserName))
                  .ForMember(pv => pv.EmployeeAvatar, p => p.MapFrom(p => p.Employee.Avatar))
                .ForMember(pv => pv.Date, p => p.MapFrom(p => getDate(p.AssignedDate)))
                .ForMember(pv => pv.TimeFrom, p => p.MapFrom(p => getTimespan(p.AssignedDate)))
                .ForMember(pv => pv.TimeTo, p => p.MapFrom(p => getTimespan(p.Deadline)));

            CreateMap<EditAssignmentRequest, AssignmentModel>()
                    .ForMember(pv => pv.AssignedDate, p => p.MapFrom(p => addTimeSpan(p.Date, p.TimeFrom)))
                    .ForMember(pv => pv.Deadline, p => p.MapFrom(p => addTimeSpan(p.Date, p.TimeTo)));

        }
        private static DateTime addTimeSpan(string date, TimeSpan time)
        {
            if (String.IsNullOrEmpty(date)) return DateTime.Now;
            var datetime = DateTime.Parse(date);
            var timeFrom = datetime.Date.Add(time);
            return timeFrom;
        }
        private static TimeSpan getTimespan(DateTime datetime)
        {
            TimeSpan time = datetime.TimeOfDay;
            return time;
        }
        private static string getDate(DateTime datetime)
        {
            return datetime.Date.ToString("yyyy-MM-dd");
        }
    }
}
