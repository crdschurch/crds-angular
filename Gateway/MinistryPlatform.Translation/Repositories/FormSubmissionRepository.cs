using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using log4net;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Repositories
{
    public class FormSubmissionRepository : BaseRepository, IFormSubmissionRepository
    {
        private readonly int _formResponsePageId;
        private readonly int _formAnswerPageId;
        private readonly int _formFieldCustomPage;
        private readonly int _formResponseGoTripView;

        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly IDbConnection _dbConnection;       
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly ILog _logger;

        public FormSubmissionRepository(IMinistryPlatformService ministryPlatformService, IDbConnection dbConnection, IAuthenticationRepository authenticationService, IConfigurationWrapper configurationWrapper, IMinistryPlatformRestRepository ministryPlatformRest)
            : base(authenticationService,configurationWrapper)
        {
            _ministryPlatformService = ministryPlatformService;
            _dbConnection = dbConnection;
            _ministryPlatformRestRepository = ministryPlatformRest;            
     
            _configurationWrapper = configurationWrapper;

            _formResponsePageId = configurationWrapper.GetConfigIntValue("FormResponsePageId");
            _formAnswerPageId = configurationWrapper.GetConfigIntValue("FormAnswerPageId");
            _formFieldCustomPage = configurationWrapper.GetConfigIntValue("AllFormFieldsView");
            _formResponseGoTripView = configurationWrapper.GetConfigIntValue("GoTripFamilySignup");
        }

        public int GetFormFieldId(int crossroadsId)
        {
            var searchString = $",{crossroadsId}";
            var formFields = _ministryPlatformService.GetPageViewRecords(_formFieldCustomPage, ApiLogin(), searchString);

            var field = formFields.Single();
            var formFieldId = field.ToInt("Form_Field_ID");
            return formFieldId;
        }

        public List<MpFormField> GetFieldsForForm(int formId)
        {
            var searchString = $",,,,{formId}";
            var formFields = _ministryPlatformService.GetPageViewRecords(_formFieldCustomPage, ApiLogin(), searchString);

            return formFields.Select(formField => new MpFormField
            {
                CrossroadsId = formField.ToInt("CrossroadsId"),
                FieldLabel = formField.ToString("Field_Label"),
                FieldOrder = formField.ToInt("Field_Order"),
                FieldType = formField.ToString("Field_Type"),
                FormFieldId = formField.ToInt("Form_Field_ID"),
                FormId = formField.ToInt("Form_ID"),
                FormTitle = formField.ToString("Form_Title"),
                Required = formField.ToBool("Required")
            }).ToList();
        }

        public List<MpTripFormResponse> GetTripFormResponsesByRecordId(int recordId)
        {
            var command = CreateTripFormResponsesSqlCommandWithRecordId(recordId);
            return GetTripFormResponses(command);
        }

        public List<MpTripFormResponse> GetTripFormResponsesBySelectionId(int selectionId)
        {
            var command = CreateTripFormResponsesSqlCommandWithSelectionId(selectionId);
            return GetTripFormResponses(command);
        }
        private List<MpTripFormResponse> GetTripFormResponses(IDbCommand command)
        {
            var connection = _dbConnection;
            try
            {
                connection.Open();

                command.Connection = connection;
                var reader = command.ExecuteReader();
                var responses = new List<MpTripFormResponse>();
                while (reader.Read())
                {
                    var response = new MpTripFormResponse {ContactId = reader.GetInt32(reader.GetOrdinal("Contact_ID"))};
                    var donorId = SafeInt32(reader, "Donor_ID");
                    response.DonorId = donorId;
                    response.FundraisingGoal = SafeDecimal(reader,"Fundraising_Goal");
                    response.ParticipantId = reader.GetInt32(reader.GetOrdinal("Participant_ID"));
                    response.PledgeCampaignId = SafeInt32(reader, "Pledge_Campaign_ID");
                    response.EventId = SafeInt32(reader, "Event_ID");
                    response.DestinationId = reader.GetInt32(reader.GetOrdinal("Destination_ID"));

                    responses.Add(response);
                }
                return responses;
            }
            finally
            {
                connection.Close();
            }
        }

        private static decimal SafeDecimal(IDataRecord record, string fieldName)
        {
            var ordinal = record.GetOrdinal(fieldName);
            return !record.IsDBNull(ordinal) ? record.GetDecimal(ordinal) : 0;
        }

        private static int? SafeInt32(IDataRecord record, string fieldName)
        {
            var ordinal = record.GetOrdinal(fieldName);
            return !record.IsDBNull(ordinal) ? record.GetInt32(ordinal) : (int?)null;
        }

        private static IDbCommand CreateTripFormResponsesSqlCommandWithRecordId(int recordId)
        {
            const string query = @"SELECT fr.Contact_ID, fr.Pledge_Campaign_ID, pc.Event_ID, pc.Fundraising_Goal, p.Participant_ID, d.Donor_ID, pc.Destination_ID
                                  FROM [MinistryPlatform].[dbo].[Form_Responses] fr
                                  INNER JOIN [MinistryPlatform].[dbo].[Participants] p on fr.Contact_ID = p.Contact_ID
                                  LEFT OUTER JOIN [MinistryPlatform].[dbo].[Pledge_Campaigns] pc on fr.Pledge_Campaign_ID = pc.Pledge_Campaign_ID
                                  LEFT OUTER JOIN [MinistryPlatform].[dbo].[Donors] d on fr.Contact_ID = d.Contact_ID
                                  WHERE fr.Form_Response_ID = @recordId";

            using (IDbCommand command = new SqlCommand(string.Format(query)))
            {
                command.Parameters.Add(new SqlParameter("@recordId", recordId) { DbType = DbType.Int32 });
                command.CommandType = CommandType.Text;
                return command;
            }
        }

        private static IDbCommand CreateTripFormResponsesSqlCommandWithSelectionId(int selectionId)
        {
            const string query = @"SELECT fr.Contact_ID, fr.Pledge_Campaign_ID, pc.Event_ID, pc.Fundraising_Goal, p.Participant_ID, d.Donor_ID, pc.Destination_ID 
                                  FROM [MinistryPlatform].[dbo].[dp_Selected_Records] sr
                                  INNER JOIN [MinistryPlatform].[dbo].[Form_Responses] fr on sr.Record_ID = fr.Form_Response_ID
                                  INNER JOIN [MinistryPlatform].[dbo].[Participants] p on fr.Contact_ID = p.Contact_ID
                                  LEFT OUTER JOIN [MinistryPlatform].[dbo].[Pledge_Campaigns] pc on fr.Pledge_Campaign_ID = pc.Pledge_Campaign_ID
                                  LEFT OUTER JOIN [MinistryPlatform].[dbo].[Donors] d on fr.Contact_ID = d.Contact_ID
                                  WHERE sr.Selection_ID = @selectionId";

            using (IDbCommand command = new SqlCommand(string.Format(query)))
            {
                command.Parameters.Add(new SqlParameter("@selectionId", selectionId) { DbType = DbType.Int32 });
                command.CommandType = CommandType.Text;
                return command;
            }
        }

        public int SubmitFormResponse(MpFormResponse form)
        {
            try
            {
                var token = ApiLogin();
                var responseId = CreateOrUpdateFormResponse(form, token);
                foreach (var answer in form.FormAnswers)
                {
                    if (answer.Response == null) continue;
                    answer.FormResponseId = responseId;
                    CreateOrUpdateFormAnswer(answer, token);
                }
                return responseId;
            }
            catch
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    Error = (serializer, err) => err.ErrorContext.Handled = true
                };
                string json = JsonConvert.SerializeObject(form, settings);            
                _logger.Error($"Failure in SubmitFormResponse: {json}");
                throw ;
            }
           
        }

        public string GetFormResponseAnswer(int formId, int contactId, int formFieldId, int? eventId = null)
        {
            var apiToken = ApiLogin();
            const string selectColumns = "Response";

            var formResponseId = GetFormResponseIdForFormContact(formId, contactId, eventId);
            var formResponseAnswerId = GetFormResponseAnswerIdForFormFeildFormResponse(formResponseId,formFieldId);
            var responseAnswer =_ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Get<MpFormAnswer>(formResponseAnswerId, selectColumns);

            return responseAnswer == null ? string.Empty : responseAnswer.Response;
        }

        public DateTime? GetTripFormResponseByContactId(int contactId, int pledgeId)
        {
            var searchString = $",{contactId},,{pledgeId}";
            var signedUp = _ministryPlatformService.GetPageViewRecords(_formResponseGoTripView, ApiLogin(), searchString);
            if (signedUp.Count < 1)
            {
                return null;
            }
            return DateTime.Parse(signedUp.First()["Response_Date"].ToString());
        }

        public MpFormResponse GetFormResponse(int formId, int contactId, int? eventId = null)
        {
            var apiToken = ApiLogin();
            var searchString = $"Form_ID={formId} AND Contact_ID={contactId}";
            if (eventId != null)
            {
                searchString = $"{searchString} AND Event_ID={eventId}";
            }
            var response = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpFormResponse>(searchString).FirstOrDefault();
            if (response == null) throw new ApplicationException($"Form response with formId={formId}, contactId={contactId}, eventId={eventId} not found!");
            searchString = $"Form_Response_ID={response.FormResponseId}";
            response.FormAnswers = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpFormAnswer>(searchString);
            return response;
        }

        private int CreateOrUpdateFormResponse(MpFormResponse formResponse, string token)
        {
            var record = new Dictionary<string, object>
            {
                {"Form_ID", formResponse.FormId},
                {"Response_Date", DateTime.Now},
                {"Contact_ID", formResponse.ContactId},
                {"Opportunity_ID",  formResponse.OpportunityId },
                {"Opportunity_Response", formResponse.OpportunityResponseId},
                {"Pledge_Campaign_ID", formResponse.PledgeCampaignId},
                {"Event_ID", formResponse.EventId }
            };

            var formResponseId = _configurationWrapper.GetConfigIntValue("FormResponsePageId");
           
            // This code is shared by Trips, Camps, and Volunteer Application.  
            //
            // For trips:
            // we want to maintain separate form responses per trip.  We can distinguish
            // trips from other callers because PledgeCampaignId is required for trips.
            //
            // TODO: Currently, Camps is sharing form responses if the contact has
            // registered for multiple camps; this will likely need to change in the
            // future.
            var sb = new StringBuilder($"Contact_ID={formResponse.ContactId} AND Form_ID={formResponse.FormId}");
            if (formResponse.EventId != null) sb.Append($" AND Event_ID={formResponse.EventId}");
            if (formResponse.PledgeCampaignId != null) sb.Append($" AND Pledge_Campaign_ID={formResponse.PledgeCampaignId}");
            var filter = sb.ToString();
            const string selectColumns = "Form_Response_ID";
            var response = _ministryPlatformRestRepository.UsingAuthenticationToken(token).Search<MpFormResponse>(filter, selectColumns, null, true);
            var responseId = response?.FirstOrDefault()?.FormResponseId ?? 0;
            
            if (responseId == 0)
            {
                responseId = _ministryPlatformService.CreateRecord(formResponseId, record, token, true);
            }
            else
            {
                record.Add("Form_Response_ID", responseId);
                _ministryPlatformService.UpdateRecord(formResponseId, record, token);
            }
            
            return responseId;
        }

        private int GetFormResponseIdForFormContact(int formId, int contactId, int? eventId)
        {
            var apiToken = ApiLogin();
            var searchString = $"Contact_ID='{contactId}' AND Form_ID='{formId}'";
            if (eventId != null) searchString = $"{searchString} AND Event_ID={eventId}";
            const string selectColumns = "Form_Response_ID";

            var formResponse = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpFormResponse>(searchString, selectColumns, null, true).FirstOrDefault();

            return formResponse?.FormResponseId ?? 0;
        }

        private int GetFormResponseIdForFormContactAndPledgeCampaign(int formId, int contactId, int pledgeCampaignId)
        {
            var apiToken = ApiLogin();
            var searchString = $"Contact_ID='{contactId}' AND Form_ID='{formId}' AND Pledge_Campaign_ID='{pledgeCampaignId}'";
            const string selectColumns = "Form_Response_ID";

            var formResponse = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpFormResponse>(searchString, selectColumns, null, true).FirstOrDefault();

            return formResponse?.FormResponseId ?? 0;
        }

        private int GetFormResponseAnswerIdForFormFeildFormResponse(int formResponseId, int formFieldId)
        {
            var apiToken = ApiLogin();
            var searchString = $"Form_Response_ID='{formResponseId}' AND Form_Field_ID='{formFieldId}'";
            const string selectColumns = "Form_Response_Answer_ID";

            var formResponseAnswer = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpFormAnswer>(searchString, selectColumns, null, true).FirstOrDefault();

            return formResponseAnswer?.FormResponseAnswerId ?? 0;
        }

        private void CreateOrUpdateFormAnswer(MpFormAnswer answer, string token)
        {
            var formAnswer = new Dictionary<string, object>
            {
                {"Form_Response_ID", answer.FormResponseId},
                {"Form_Field_ID", answer.FieldId},
                {"Response", answer.Response},
                {"Opportunity_Response", answer.OpportunityResponseId},
                {"Event_Participant_ID", answer.EventParticipantId }
            };

            try
            {
                var responseAnswerId = GetFormResponseAnswerIdForFormFeildFormResponse(answer.FormResponseId, answer.FieldId);
                if (responseAnswerId == 0)
                {
                    _ministryPlatformService.CreateRecord(_formAnswerPageId, formAnswer, token, true);
                }
                else
                {
                    formAnswer.Add("Form_Response_Answer_ID",responseAnswerId);
                    _ministryPlatformService.UpdateRecord(_formAnswerPageId, formAnswer, token);
                }
                
            }
            catch (Exception exception)
            {
                throw new ApplicationException($"CreateFormAnswer failed.  Field Id: {answer.FieldId}", exception);
            }
        }
    }
}
