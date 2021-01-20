using MinistryPlatform.Translation.Models;

namespace crds_angular.Models.Crossroads.Serve
{
    public class ServeProfile : AutoMapper.Profile
    {
        protected override void Configure()
        {
            
            AutoMapper.Mapper.CreateMap<MPServeReminder, ServeReminder>()
                 .ForMember(dest => dest.OpportunityTitle, opts => opts.MapFrom(src => src.Opportunity_Title))
                .ForMember(dest => dest.OpportunityContactId, opts => opts.MapFrom(src => src.Opportunity_Contact_Id))
                .ForMember(dest => dest.OpportunityEmailAddress, opts => opts.MapFrom(src => src.Opportunity_Email_Address))
                .ForMember(dest => dest.EventEndDate, opts => opts.MapFrom(src => src.Event_End_Date))
                .ForMember(dest => dest.EventStartDate, opts => opts.MapFrom(src => src.Event_Start_Date))
                .ForMember(dest => dest.EventTitle, opts => opts.MapFrom(src => src.Event_Title))
                .ForMember(dest => dest.SignedupContactId, opts => opts.MapFrom(src => src.Signedup_Contact_Id))
                .ForMember(dest => dest.SignedupEmailAddress, opts => opts.MapFrom(src => src.Signedup_Email_Address))
                .ForMember(dest => dest.TemplateId, opts => opts.MapFrom(src => src.Template_Id))
                .ForMember(dest => dest.ShiftStart, opts => opts.MapFrom(src => src.Shift_Start))
                .ForMember(dest => dest.ShiftEnd, opts => opts.MapFrom(src => src.Shift_End))
                ;
        }
    }
}
