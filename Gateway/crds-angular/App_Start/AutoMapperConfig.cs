using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AutoMapper;
using crds_angular.Models.AwsCloudsearch;
using crds_angular.Models.Finder;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Models.Crossroads.Camp;
using crds_angular.Models.Crossroads.Events;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Crossroads.Opportunity;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Models.Crossroads.Serve;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Models.MailChimp;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Finder;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Models.Payments;
using MinistryPlatform.Translation.Models.Product;
using MpAddress = MinistryPlatform.Translation.Models.MpAddress;
using DonationStatus = crds_angular.Models.Crossroads.Stewardship.DonationStatus;
using MpEvent = MinistryPlatform.Translation.Models.MpEvent;
using MpGroup = MinistryPlatform.Translation.Models.MpGroup;
using MpInvitation = MinistryPlatform.Translation.Models.MpInvitation;
using MpResponse = MinistryPlatform.Translation.Models.MpResponse;
using crds_angular.Services;

namespace crds_angular.App_Start
{
    public static class AutoMapperConfig
    {
        public static void RegisterMappings()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<EventProfile>();
                cfg.AddProfile<ServeProfile>();
            });

            Mapper.CreateMap<Dictionary<string, object>, AccountInfo>()
                .ForMember(dest => dest.EmailNotifications,
                    opts => opts.MapFrom(src => src["Bulk_Email_Opt_Out"]));

            Mapper.CreateMap<SpPinDto, PinDto>()
                .ForMember(dest => dest.FirstName, opts => opts.MapFrom(src => src.First_Name))
                .ForMember(dest => dest.LastName, opts => opts.MapFrom(src => src.Last_Name))
                .ForMember(dest => dest.Contact_ID, opts => opts.MapFrom(src => src.Contact_ID))
                .ForMember(dest => dest.Participant_ID, opts => opts.MapFrom(src => src.Participant_ID))
                .ForMember(dest => dest.Host_Status_ID, opts => opts.MapFrom(src => src.Host_Status_ID))
                .ForMember(dest => dest.Gathering, opts => opts.MapFrom(src => src.Gathering))
                .ForMember(dest => dest.SiteName, opts => opts.MapFrom(src => src.Site_Name))
                .ForMember(dest => dest.Household_ID, opts => opts.MapFrom(src => src.Household_ID))
                .ForMember(dest => dest.Address,
                           opts => opts.MapFrom(src => new AddressDTO(
                                                    src.Address_Line_1,
                                                    src.Address_Line_2,
                                                    src.City,
                                                    src.State,
                                                    src.Postal_Code,
                                                    src.Longitude,
                                                    src.Latitude)))
                .ForMember(dest => dest.PinType, opts => opts.MapFrom(src => src.Pin_Type)); 

            Mapper.CreateMap<MpGroup, OpportunityGroup>()
                .ForMember(dest => dest.GroupId, opts => opts.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.EventTypeId, opts => opts.MapFrom(src => src.EventTypeId))
                .ForMember(dest => dest.Participants, opts => opts.MapFrom(src => src.Participants));

            Mapper.CreateMap<MpCongregation, Congregation>()
                .ForMember(dest => dest.CongregationId, opts => opts.MapFrom(src => src.CongregationId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.LocationId, opts => opts.MapFrom(src => src.LocationId));

            Mapper.CreateMap<Congregation, MpCongregation>()
                .ForMember(dest => dest.CongregationId, opts => opts.MapFrom(src => src.CongregationId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.LocationId, opts => opts.MapFrom(src => src.LocationId));

            Mapper.CreateMap<Invitation,MpInvitation > ()
                .ForMember(dest => dest.SourceId, opts => opts.MapFrom(src => src.SourceId))
                .ForMember(dest => dest.GroupRoleId, opts => opts.MapFrom(src => src.GroupRoleId))
                .ForMember(dest => dest.EmailAddress, opts => opts.MapFrom(src => src.EmailAddress))
                .ForMember(dest => dest.RecipientName, opts => opts.MapFrom(src => src.RecipientName))
                .ForMember(dest => dest.RequestDate, opts => opts.MapFrom(src => src.RequestDate))
                .ForMember(dest => dest.InvitationType, opts => opts.MapFrom(src => src.InvitationType));

            Mapper.CreateMap<MpInvitation, Invitation>()
                .ForMember(dest => dest.SourceId, opts => opts.MapFrom(src => src.SourceId))
                .ForMember(dest => dest.GroupRoleId, opts => opts.MapFrom(src => src.GroupRoleId))
                .ForMember(dest => dest.EmailAddress, opts => opts.MapFrom(src => src.EmailAddress))
                .ForMember(dest => dest.RecipientName, opts => opts.MapFrom(src => src.RecipientName))
                .ForMember(dest => dest.RequestDate, opts => opts.MapFrom(src => src.RequestDate))
                .ForMember(dest => dest.InvitationType, opts => opts.MapFrom(src => src.InvitationType));

            Mapper.CreateMap<MpGroupParticipant, OpportunityGroupParticipant>()
                .ForMember(dest => dest.ContactId, opts => opts.MapFrom(src => src.ContactId))
                .ForMember(dest => dest.GroupRoleId, opts => opts.MapFrom(src => src.GroupRoleId))
                .ForMember(dest => dest.GroupRoleTitle, opts => opts.MapFrom(src => src.GroupRoleTitle))
                .ForMember(dest => dest.LastName, opts => opts.MapFrom(src => src.LastName))
                .ForMember(dest => dest.NickName, opts => opts.MapFrom(src => src.NickName))
                .ForMember(dest => dest.ParticipantId, opts => opts.MapFrom(src => src.ParticipantId));

            Mapper.CreateMap<MpResponse, OpportunityResponseDto>()
                .ForMember(dest => dest.Closed, opts => opts.MapFrom(src => src.Closed))
                .ForMember(dest => dest.Comments, opts => opts.MapFrom(src => src.Comments))
                .ForMember(dest => dest.EventId, opts => opts.MapFrom(src => src.Event_ID))
                .ForMember(dest => dest.OpportunityId, opts => opts.MapFrom(src => src.Opportunity_ID))
                .ForMember(dest => dest.ParticipantId, opts => opts.MapFrom(src => src.Participant_ID))
                .ForMember(dest => dest.ResponseDate, opts => opts.MapFrom(src => src.Response_Date))
                .ForMember(dest => dest.ResponseId, opts => opts.MapFrom(src => src.Response_ID))
                .ForMember(dest => dest.ResponseResultId, opts => opts.MapFrom(src => src.Response_Result_ID));

            Mapper.CreateMap<MpDonationBatch, DonationBatchDTO>()
                .ForMember(dest => dest.BatchName, opts => opts.MapFrom(src => src.BatchName))
                .ForMember(dest => dest.ProcessorTransferId, opts => opts.MapFrom(src => src.ProcessorTransferId))
                .ForMember(dest => dest.DepositId, opts => opts.MapFrom(src => src.DepositId))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id));

            Mapper.CreateMap<Dictionary<string, object>, MpDonationBatch>()
                .ForMember(dest => dest.BatchName, opts => opts.MapFrom(src => src.ToString("Batch_Name")))
                .ForMember(dest => dest.ProcessorTransferId, opts => opts.MapFrom(src => src.ToString("Processor_Transfer_ID")))
                .ForMember(dest => dest.DepositId, opts => opts.MapFrom(src => src.ToNullableInt("Deposit_ID", false)))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ContainsKey("dp_RecordID") ? src.ToInt("dp_RecordID", false) : src.ToInt("Batch_ID", false)));

            Mapper.CreateMap<Dictionary<string, object>, MpProgram>()
                .ForMember(dest => dest.ProgramId, opts => opts.MapFrom(src => src.ToInt("Program_ID", false)))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.ToString("Program_Name")))
                .ForMember(dest => dest.ProgramType, opts => opts.MapFrom(src => src.ToInt("Program_Type_ID", false)))
                .ForMember(dest => dest.CommunicationTemplateId, opts => opts.MapFrom(src => src.ContainsKey("Communication_ID") ? src.ToNullableInt("Communication_ID", false) : (int?)null))
                .ForMember(dest => dest.AllowRecurringGiving, opts => opts.MapFrom(src => src.ToBool("Allow_Recurring_Giving", false)));

            Mapper.CreateMap<MpProgram, ProgramDTO>()
                .ForMember(dest => dest.ProgramType, opts => opts.MapFrom(src => src.ProgramType))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.CommunicationTemplateId, opts => opts.MapFrom(src => src.CommunicationTemplateId))
                .ForMember(dest => dest.ProgramId, opts => opts.MapFrom(src => src.ProgramId));

            Mapper.CreateMap<MpDeposit, DepositDTO>();

            Mapper.CreateMap<Dictionary<string, object>, MpDeposit>()
                .ForMember(dest => dest.DepositDateTime, opts => opts.MapFrom(src => src.ToDate("Deposit_Date", false)))
                .ForMember(dest => dest.DepositName, opts => opts.MapFrom(src => src.ToString("Deposit_Name")))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ToInt("Deposit_ID", false)))
                .ForMember(dest => dest.DepositTotalAmount, opts => opts.MapFrom(src => src.ContainsKey("Deposit_Total") ? src["Deposit_Total"] as decimal? : 0))
                .ForMember(dest => dest.BatchCount, opts => opts.MapFrom(src => src.ToInt("Batch_Count", false)))
                .ForMember(dest => dest.Exported, opts => opts.MapFrom(src => src.ToBool("Exported", false)))
                .ForMember(dest => dest.ProcessorTransferId, opts => opts.MapFrom(src => src.ToString("Processor_Transfer_ID")));

            Mapper.CreateMap<MpGPExportDatum, GPExportDatumDTO>()
                .ForMember(dest => dest.DocumentNumber, opts => opts.MapFrom(src => src.DocumentNumber))
                .ForMember(dest => dest.DocumentDescription, opts => opts.MapFrom(src => src.BatchName))
                .ForMember(dest => dest.BatchId, opts => opts.MapFrom(src => src.BatchName))
                .ForMember(dest => dest.ContributionDate, opts => opts.MapFrom(src => src.DonationDate.ToString("MM/dd/yyyy")))
                .ForMember(dest => dest.SettlementDate, opts => opts.MapFrom(src => src.DepositDate.ToString("MM/dd/yyyy")))
                .ForMember(dest => dest.ContributionAmount, opts => opts.MapFrom(src => src.DonationAmount.ToString()))
                .ForMember(dest => dest.ReceivablesAccount, opts => opts.MapFrom(src => src.ReceivableAccount))
                .ForMember(dest => dest.DistributionAmount, opts => opts.MapFrom(src => src.Amount.ToString()))
                .ForMember(dest => dest.CashAccount, opts => opts.MapFrom(src => (src.ScholarshipPaymentTypeId == src.PaymentTypeId ? src.ScholarshipExpenseAccount : src.CashAccount)))
                .ForMember(dest => dest.DistributionReference, opts => opts.MapFrom(src => src.DistributionReference));

            Mapper.CreateMap<MpDonation, DonationDTO>()
                .ForMember(dest => dest.Amount, opts => opts.MapFrom(src => src.donationAmt))
                .ForMember(dest => dest.DonationDate, opts => opts.MapFrom(src => src.donationDate))
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => src.donationStatus))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.donationId))
                .ForMember(dest => dest.Distributions, opts => opts.MapFrom(src => src.Distributions))
                .ForMember(dest => dest.IncludeOnGivingHistory, opts => opts.MapFrom(src => src.IncludeOnGivingHistory))
                .ForMember(dest => dest.IncludeOnPrintedStatement, opts => opts.MapFrom(src => src.IncludeOnPrintedStatement))
                .ForMember(dest => dest.AccountingCompanyName, opts => opts.MapFrom(src => src.AccountingCompanyName))
                .ForMember(dest => dest.AccountingCompanyIncludeOnPrintedStatement, opts => opts.MapFrom(src => src.AccountingCompanyIncludeOnPrintedStatement))
                .ForMember(dest => dest.Notes, opts => opts.MapFrom(src => src.donationNotes))
                .AfterMap((src, dest) =>
                {
                    dest.Source = new DonationSourceDTO
                    {
                        SourceType =
                            ((src.softCreditDonorId != 0)
                                ? PaymentType.SoftCredit
                                : (System.Enum.IsDefined(typeof (PaymentType), src.paymentTypeId) ? (PaymentType) src.paymentTypeId : PaymentType.Other)),
                        PaymentProcessorId = src.transactionCode,
                        AccountHolderName = ((src.softCreditDonorId != 0) ? src.donorDisplayName : null),
                        CheckNumber = src.itemNumber
                        
                    };

                    if (src.donationAmt < 0 )
                    {
                        dest.Status = src.donationStatus == 1 ? DonationStatus.Pending : DonationStatus.Refunded;
                    }
                  
                });
                
            Mapper.CreateMap<MpContactDonor, EZScanDonorDetails>()
                .ForMember(dest => dest.DisplayName, opts => opts.MapFrom(src => src.Details.DisplayName))
                .ForMember(dest => dest.Address, opts => opts.MapFrom(src => src.Details.Address));
           
            Mapper.CreateMap<MpDonationDistribution, DonationDistributionDTO>()
                .ForMember(dest => dest.Amount, opts => opts.MapFrom(src => src.donationDistributionAmt))
                .ForMember(dest => dest.ProgramName, opts => opts.MapFrom(src => src.donationDistributionProgram));

            Mapper.CreateMap<MpMyContact, Person>()
                .ForMember(dest => dest.ContactId, opts => opts.MapFrom(src => src.Contact_ID))
                .ForMember(dest => dest.EmailAddress, opts => opts.MapFrom(src => src.Email_Address))
                .ForMember(dest => dest.NickName, opts => opts.MapFrom(src => src.Nickname))
                .ForMember(dest => dest.FirstName, opts => opts.MapFrom(src => src.First_Name))
                .ForMember(dest => dest.MiddleName, opts => opts.MapFrom(src => src.Middle_Name))
                .ForMember(dest => dest.LastName, opts => opts.MapFrom(src => src.Last_Name))
                .ForMember(dest => dest.MaidenName, opts => opts.MapFrom(src => src.Maiden_Name))
                .ForMember(dest => dest.MobilePhone, opts => opts.MapFrom(src => src.Mobile_Phone))
                .ForMember(dest => dest.MobileCarrierId, opts => opts.MapFrom(src => src.Mobile_Carrier))
                .ForMember(dest => dest.DateOfBirth, opts => opts.MapFrom(src => src.Date_Of_Birth))
                .ForMember(dest => dest.MaritalStatusId, opts => opts.MapFrom(src => src.Marital_Status_ID))
                .ForMember(dest => dest.GenderId, opts => opts.MapFrom(src => src.Gender_ID))
                .ForMember(dest => dest.EmployerName, opts => opts.MapFrom(src => src.Employer_Name))
                .ForMember(dest => dest.AddressLine1, opts => opts.MapFrom(src => src.Address_Line_1))
                .ForMember(dest => dest.AddressLine2, opts => opts.MapFrom(src => src.Address_Line_2))
                .ForMember(dest => dest.City, opts => opts.MapFrom(src => src.City))
                .ForMember(dest => dest.State, opts => opts.MapFrom(src => src.State))
                .ForMember(dest => dest.PostalCode, opts => opts.MapFrom(src => src.Postal_Code))
                .ForMember(dest => dest.ParticipantStartDate, opts => opts.MapFrom(src => src.Participant_Start_Date))
                .ForMember(dest => dest.AttendanceStartDate, opts => opts.MapFrom(src => src.Attendance_Start_Date))                
                .ForMember(dest => dest.ForeignCountry, opts => opts.MapFrom(src => src.Foreign_Country))
                .ForMember(dest => dest.HomePhone, opts => opts.MapFrom(src => src.Home_Phone))
                .ForMember(dest => dest.CongregationId, opts => opts.MapFrom(src => src.Congregation_ID))
                .ForMember(dest => dest.HouseholdId, opts => opts.MapFrom(src => src.Household_ID))
                .ForMember(dest => dest.HouseholdName, opts => opts.MapFrom(src => src.Household_Name))
                .ForMember(dest => dest.AddressId, opts => opts.MapFrom(src => src.Address_ID))
                .ForMember(dest => dest.Age, opts => opts.MapFrom(src => src.Age))
                .ForMember(dest => dest.PassportExpiration, opts=> opts.MapFrom(src => src.Passport_Expiration != null ? String.Format("{0:MM/dd/yyyy}", src.Passport_Expiration) : null))
                .ForMember(dest => dest.PassportNumber, opts => opts.MapFrom(src => src.Passport_Number))
                .ForMember(dest => dest.PassportFirstname, opts => opts.MapFrom(src => src.Passport_Firstname))
                .ForMember(dest => dest.PassportLastname, opts => opts.MapFrom(src => src.Passport_Lastname))
                .ForMember(dest => dest.PassportMiddlename, opts => opts.MapFrom(src => src.Passport_Middlename))
                .ForMember(dest => dest.PassportCountry, opts => opts.MapFrom(src => src.Passport_Country));

            Mapper.CreateMap<MpEvent, EventToolDto>()
                .ForMember(dest => dest.CongregationId, opts => opts.MapFrom(src => src.CongregationId))
                .ForMember(dest => dest.EndDateTime, opts => opts.MapFrom(src => src.EventEndDate))
                .ForMember(dest => dest.EventTypeId, opts => opts.MapFrom(src => src.EventType))
                .ForMember(dest => dest.ProgramId, opts => opts.MapFrom(src => src.ProgramId))
                .ForMember(dest => dest.ReminderDaysId, opts => opts.MapFrom(src => src.ReminderDaysPriorId))
                .ForMember(dest => dest.StartDateTime, opts => opts.MapFrom(src => src.EventStartDate))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.EventTitle))
                .ForMember(dest => dest.SendReminder, opts => opts.MapFrom(src => src.SendReminder))
                .ForMember(dest => dest.DonationBatchTool, opts => opts.MapFrom(src => src.DonationBatchTool))
                .ForMember(dest => dest.ContactId, opts => opts.MapFrom(src => src.PrimaryContactId))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.MinutesSetup, opts => opts.MapFrom(src => src.MinutesSetup))
                .ForMember(dest => dest.MinutesTeardown, opts => opts.MapFrom(src => src.MinutesTeardown))
                .ForMember(dest => dest.MeetingInstructions, opts => opts.MapFrom(src => src.MeetingInstructions))
                .ForMember(dest => dest.ParticipantsExpected, opts => opts.MapFrom(src => src.ParticipantsExpected));

            Mapper.CreateMap<GroupDTO, FinderGatheringDto>()
                .ForMember(dest => dest.PrimaryContact, opts => opts.MapFrom(src => src.ContactId))
                .ForMember(dest => dest.Address, opts => opts.MapFrom(src => src.Address))
                .ForMember(dest => dest.AvailableOnline, opts => opts.MapFrom(src => src.AvailableOnline))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.GroupName))
                .ForMember(dest => dest.GroupType, opts => opts.MapFrom(src => src.GroupTypeId));

            Mapper.CreateMap<FinderGatheringDto, GroupDTO>()
                .ForMember(dest => dest.ContactId, opts => opts.MapFrom(src => src.PrimaryContact))
                .ForMember(dest => dest.Address, opts => opts.MapFrom(src => src.Address))
                .ForMember(dest => dest.GroupName, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.GroupTypeId, opts => opts.MapFrom(src => src.GroupType));
                

            Mapper.CreateMap<EventToolDto, MpEvent>()
                .ForMember(dest => dest.CongregationId, opts => opts.MapFrom(src => src.CongregationId))
                .ForMember(dest => dest.EventEndDate, opts => opts.MapFrom(src => src.EndDateTime))
                .ForMember(dest => dest.EventType, opts => opts.MapFrom(src => src.EventTypeId))
                .ForMember(dest => dest.ProgramId, opts => opts.MapFrom(src => src.ProgramId))
                .ForMember(dest => dest.ReminderDaysPriorId, opts => opts.MapFrom(src => src.ReminderDaysId))
                .ForMember(dest => dest.EventStartDate, opts => opts.MapFrom(src => src.StartDateTime))
                .ForMember(dest => dest.EventTitle, opts => opts.MapFrom(src => src.Title))
                .ForMember(dest => dest.SendReminder, opts => opts.MapFrom(src => src.SendReminder))
                .ForMember(dest => dest.DonationBatchTool, opts => opts.MapFrom(src => src.DonationBatchTool))
                .ForMember(dest => dest.PrimaryContactId, opts => opts.MapFrom(src => src.ContactId))
                .ForMember(dest => dest.ParticipantsExpected, opts => opts.MapFrom(src => src.ParticipantsExpected))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.MinutesSetup, opts => opts.MapFrom(src => src.MinutesSetup))
                .ForMember(dest => dest.MinutesTeardown, opts => opts.MapFrom(src => src.MinutesTeardown))
                .ForMember(dest => dest.MeetingInstructions, opts => opts.MapFrom(src => src.MeetingInstructions));

            Mapper.CreateMap<MpRecurringGift, RecurringGiftDto>()
                .ForMember(dest => dest.EmailAddress, opts => opts.MapFrom(src => src.RecurringGiftId))
                .ForMember(dest => dest.DonorID, opts => opts.MapFrom(src => src.DonorID))
                .ForMember(dest => dest.EmailAddress, opts => opts.MapFrom(src => src.EmailAddress))
                .ForMember(dest => dest.PlanInterval, opts => opts.MapFrom(src => src.Frequency.IndexOf("Monthly", StringComparison.Ordinal) >= 0 ? PlanInterval.Monthly : PlanInterval.Weekly))
                .ForMember(dest => dest.Recurrence, opts => opts.MapFrom(src => src.Recurrence))
                .ForMember(dest => dest.StartDate, opts => opts.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opts => opts.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.PlanAmount, opts => opts.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Program, opts => opts.MapFrom(src => src.ProgramID))
                .ForMember(dest => dest.ProgramName, opts => opts.MapFrom(src => src.ProgramName))
                .ForMember(dest => dest.CongregationName, opts => opts.MapFrom(src => src.CongregationName))
                .ForMember(dest => dest.SubscriptionID, opts => opts.MapFrom(src => src.SubscriptionID))
                .AfterMap((src, dest) =>
                {
                    dest.Source = new DonationSourceDTO
                    {
                        SourceType = (int)AccountType.Checking == src.AccountTypeID || (int)AccountType.Savings == src.AccountTypeID ? PaymentType.Bank : PaymentType.CreditCard,
                        AccountNumberLast4 = src.AccountNumberLast4,
                        // Have to remove space to match to enum for things like American Express which needs to be AmericanExpress
                        CardType = src.InstitutionName.Equals("Bank") ? (CreditCardType?) null : (CreditCardType)System.Enum.Parse(typeof(CreditCardType), Regex.Replace(src.InstitutionName, @"\s+", "")),
                        ProcessorAccountId = src.ProcessorAccountId,
                        PaymentProcessorId = src.ProcessorId
                    };
                    try
                    {
                        // Have to remove space to match to enum for things like American Express which needs to be AmericanExpress
                        dest.Source.CardType = (CreditCardType)System.Enum.Parse(typeof(CreditCardType), Regex.Replace(src.InstitutionName, @"\s+", ""));
                    }
                    catch { }
                });

            Mapper.CreateMap<MpPledge, PledgeDto>()
                .ForMember(dest => dest.PledgeCampaign, opts => opts.MapFrom(src => src.CampaignName))
                .ForMember(dest => dest.TotalPledge, opts => opts.MapFrom(src => src.PledgeTotal))
                .ForMember(dest => dest.CampaignStartDate, opts => opts.MapFrom(src => src.CampaignStartDate.ToString("MMMM d, yyyy")))
                .ForMember(dest => dest.CampaignEndDate, opts => opts.MapFrom(src => src.CampaignEndDate.ToString("MMMM d, yyyy")))
                .ForMember(dest => dest.CampaignTypeId, opts => opts.MapFrom(src => src.CampaignTypeId))
                .ForMember(dest => dest.CampaignTypeName, opts => opts.MapFrom(src => src.CampaignTypeName));

            Mapper.CreateMap<MpDonorStatement, DonorStatementDTO>();
            Mapper.CreateMap<DonorStatementDTO, MpDonorStatement>();

            Mapper.CreateMap<MpEvent, Event>()
                .ForMember(dest => dest.EventId, opts => opts.MapFrom(src => src.EventId))
                .ForMember(dest => dest.name, opts => opts.MapFrom(src => src.EventTitle))
                .ForMember(dest => dest.location, opts => opts.MapFrom(src => src.Congregation))
                .ForMember(dest => dest.time, opts => opts.MapFrom(src => src.EventStartDate.ToString("h:mm")))
                .ForMember(dest => dest.meridian, opts => opts.MapFrom(src => src.EventStartDate.ToString("tt")))
                .ForMember(dest => dest.StartDate, opts => opts.MapFrom(src => src.EventStartDate))
                .ForMember(dest => dest.EndDate, opts => opts.MapFrom(src => src.EventEndDate));

            Mapper.CreateMap<BulkEmailSubscriberOptDTO, MpBulkEmailSubscriberOpt>();
            Mapper.CreateMap<MpBulkEmailSubscriberOpt, BulkEmailSubscriberOptDTO>();

            Mapper.CreateMap<MpObjectAttribute, ObjectAttributeDTO>();
            Mapper.CreateMap<MpObjectAttributeType, ObjectAttributeTypeDTO>();

            Mapper.CreateMap<MpObjectAttribute, ObjectSingleAttributeDTO>()
                .ForMember(dest => dest.Value, opts => opts.MapFrom(src => src));
            Mapper.CreateMap<MpObjectAttribute, AttributeDTO>();

            Mapper.CreateMap<MpGroupSearchResultDto, GroupDTO>()
                .ForMember(dest => dest.GroupName, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.GroupTypeId, opts => opts.MapFrom(src => src.GroupType))
                .IncludeBase<MpGroup, GroupDTO>();

            Mapper.CreateMap<MpGroup, GroupDTO>()
                .ForMember(dest => dest.GroupName, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.ContactId, opts => opts.MapFrom(src => src.PrimaryContact))
                .ForMember(dest => dest.GroupTypeId, opts => opts.MapFrom(src => src.GroupType));

            Mapper.CreateMap<GroupDTO, MpGroup>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.GroupName))
                .ForMember(dest => dest.GroupType, opts => opts.MapFrom(src => src.GroupTypeId));
                
            Mapper.CreateMap<MpGroupSearchResult, GroupDTO>()
                .ForMember(dest => dest.GroupName, opts => opts.MapFrom(src => src.Name));

            Mapper.CreateMap<MpLocation, LocationDTO>()
                .ForMember(dest => dest.Address,
                           opts => opts.MapFrom(src => new AddressDTO(
                                                    src.AddressLine1,
                                                    src.AddressLine2,
                                                    src.City,
                                                    src.State,
                                                    src.Zip,
                                                    src.Longitude,
                                                    src.Latitude)))
                .ForMember(dest => dest.LocationId, opts => opts.MapFrom(src => src.LocationId))
                .ForMember(dest => dest.LocationName, opts => opts.MapFrom(src => src.LocationName))
                .ForMember(dest => dest.ImageUrl, opts => opts.MapFrom(src => src.ImageUrl));

            Mapper.CreateMap<MpAddress, AddressDTO>()
                .ForMember(dest => dest.AddressLine1, opts => opts.MapFrom(src => src.Address_Line_1))
                .ForMember(dest => dest.AddressLine2, opts => opts.MapFrom(src => src.Address_Line_2))
                .ForMember(dest => dest.PostalCode, opts => opts.MapFrom(src => src.Postal_Code))
                .ForMember(dest => dest.ForeignCountry, opts => opts.MapFrom(src => src.Foreign_Country))
                .ForMember(dest => dest.AddressID, opts => opts.MapFrom(src => src.Address_ID))
                .ForMember(dest => dest.Longitude, opts => opts.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.Latitude, opts => opts.MapFrom(src => src.Latitude));

            Mapper.CreateMap<AddressDTO, MpAddress>()
                .ForMember(dest => dest.Address_Line_1, opts => opts.MapFrom(src => src.AddressLine1))
                .ForMember(dest => dest.Address_Line_2, opts => opts.MapFrom(src => src.AddressLine2))
                .ForMember(dest => dest.Postal_Code, opts => opts.MapFrom(src => src.PostalCode))
                .ForMember(dest => dest.Foreign_Country, opts => opts.MapFrom(src => src.ForeignCountry))
                .ForMember(dest => dest.Address_ID, opts => opts.MapFrom(src => src.AddressID))
                .ForMember(dest => dest.Longitude, opts => opts.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.Latitude, opts => opts.MapFrom(src => src.Latitude));

            Mapper.CreateMap<MpGroupParticipant, GroupParticipantDTO>();
            Mapper.CreateMap<GroupParticipantDTO, MpGroupParticipant>();

            Mapper.CreateMap<MpInquiry, Inquiry>();
            Mapper.CreateMap<ObjectAttributeDTO, MpAttribute>();
            Mapper.CreateMap<MpAttribute, ObjectAttributeDTO>();
            Mapper.CreateMap<MpAttribute, AttributeDTO>()
                .ForMember(dest => dest.CategoryId, opts => opts.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.AttributeId, opts => opts.MapFrom(src => src.AttributeId))
                .ForMember(dest => dest.AttributeTypeId, opts => opts.MapFrom(src => src.AttributeTypeId))
                .ForMember(dest => dest.Category, opts => opts.MapFrom(src => src.Category))
                .ForMember(dest => dest.CategoryDescription, opts => opts.MapFrom(src => src.CategoryDescription))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.SortOrder, opts => opts.MapFrom(src => src.SortOrder));

            Mapper.CreateMap<AttributeDTO, MpAttribute>()
                .ForMember(dest => dest.CategoryId, opts => opts.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.AttributeId, opts => opts.MapFrom(src => src.AttributeId))
                .ForMember(dest => dest.AttributeTypeId, opts => opts.MapFrom(src => src.AttributeTypeId))
                .ForMember(dest => dest.Category, opts => opts.MapFrom(src => src.Category))
                .ForMember(dest => dest.CategoryDescription, opts => opts.MapFrom(src => src.CategoryDescription))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.SortOrder, opts => opts.MapFrom(src => src.SortOrder));

            Mapper.CreateMap<RsvpMember, MpRsvpMember>();
            Mapper.CreateMap<MpRsvpMember, RsvpMember>();
            Mapper.CreateMap<PinDto, FinderPinDto>();
            Mapper.CreateMap<FinderPinDto, PinDto>();
            Mapper.CreateMap<AwsConnectDto,MpConnectAws>();
            Mapper.CreateMap<MpConnectAws, AwsConnectDto>()
                .ForMember(dest => dest.LatLong,
                           opts => opts.MapFrom(
                               src => (src.Latitude == null || src.Longitude == null) ? "0 , 0" : $"{src.Latitude} , {src.Longitude}"))
                .ForMember(dest => dest.GroupAgeRange,
                           opts => opts.MapFrom(
                               src => src.GroupAgeRange.Split(',')))
                .ForMember(dest => dest.GroupCategory,
                           opts => opts.MapFrom(
                               src => src.GroupCategory.Split(',')))
                .ForMember(dest => dest.GroupMeetingTime,
                           opts => opts.MapFrom(
                               src => new AwsTimeHelper().ConvertTimeToUtcString(src.GroupMeetingTime)));

            Mapper.CreateMap<PinDto, AwsConnectDto>()
                .ForMember(dest => dest.FirstName, opts => opts.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opts => opts.MapFrom(src => src.LastName))
                .ForMember(dest => dest.AddressId, opts => opts.MapFrom(src => src.Address.AddressID))
                .ForMember(dest => dest.City, opts => opts.MapFrom(src => src.Address.City))
                .ForMember(dest => dest.State, opts => opts.MapFrom(src => src.Address.State))
                .ForMember(dest => dest.Zip, opts => opts.MapFrom(src => src.Address.PostalCode))
                .ForMember(dest => dest.ContactId, opts => opts.MapFrom(src => src.Contact_ID))
                .ForMember(dest => dest.HouseholdId, opts => opts.MapFrom(src => src.Household_ID))
                .ForMember(dest => dest.PinType, opts => opts.MapFrom(src => src.PinType))
                .ForMember(dest => dest.HostStatus, opts => opts.MapFrom(src => src.Host_Status_ID))
                .ForMember(dest => dest.ParticipantId, opts => opts.MapFrom(src => src.Participant_ID))
                .ForMember(dest => dest.LatLong, opts => opts.MapFrom(
                   src => (src.Address.Latitude == null || src.Address.Longitude == null) ? "0 , 0" : 
                                                                                            $"{src.Address.Latitude} , {src.Address.Longitude}"));


            Mapper.CreateMap<GroupDTO, PinDto>()
                .ForMember(dest => dest.Contact_ID, opts => opts.MapFrom(src => src.ContactId))
                .ForMember(dest => dest.PinType, opt => opt.UseValue<PinType>(PinType.GATHERING));

            Mapper.CreateMap<ConnectCommunicationDto, MpConnectCommunication>();
            Mapper.CreateMap<MpConnectCommunication, ConnectCommunicationDto>();

            Mapper.CreateMap<FinderGroupDto, GroupDTO>();
            Mapper.CreateMap<GroupDTO, FinderGroupDto>();

            Mapper.CreateMap<MpSU2SOpportunity, ServeOpportunity>();
            Mapper.CreateMap<ServeOpportunity, MpSU2SOpportunity>();
            Mapper.CreateMap<MpInvoiceDetail, InvoiceDetailDTO>();
            Mapper.CreateMap<InvoiceDetailDTO, MpInvoiceDetail>();
            Mapper.CreateMap<MpProduct, ProductDTO>();
            Mapper.CreateMap<ProductDTO, MpProduct>();
            Mapper.CreateMap<MpAttributeCategory, AttributeCategoryDTO>()
                .ForMember(dest => dest.CategoryId, opts => opts.MapFrom(src => src.Attribute_Category_ID))
                .ForMember(dest => dest.AttributeCategory, opts => opts.MapFrom(src => src.Attribute_Category))
                .ForMember(dest => dest.Attribute, opts => opts.MapFrom(src => src.Attribute))
                .ForMember(dest => dest.RequiresActiveAttribute, opts => opts.MapFrom(src => src.Requires_Active_Attribute))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.ExampleText, opts => opts.MapFrom(src => src.Example_Text));
            Mapper.CreateMap<AttributeCategoryDTO, MpAttributeCategory>()
                .ForMember(dest => dest.Attribute_Category_ID, opts => opts.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Attribute_Category, opts => opts.MapFrom(src => src.AttributeCategory))
                .ForMember(dest => dest.Attribute, opts => opts.MapFrom(src => src.Attribute))
                .ForMember(dest => dest.Requires_Active_Attribute, opts => opts.MapFrom(src => src.RequiresActiveAttribute))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.Example_Text, opts => opts.MapFrom(src => src.ExampleText));
        }
    }
}