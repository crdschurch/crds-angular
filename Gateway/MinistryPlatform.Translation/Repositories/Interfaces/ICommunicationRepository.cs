using System;
using System.Collections.Generic;
using System.Net.Mail;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface ICommunicationRepository
    {
        MpCommunicationPreferences GetPreferences(String token, int userId);
        bool SetEmailSMSPreferences(String token, Dictionary<string, object> prefs);
        bool SetMailPreferences(string token, Dictionary<string, object> prefs);
        int SendMessage(MpCommunication communication, bool isDraft = false);
        MpDirectEmailCommunication GetDirectEmail(int communicationId);
        bool UpdateDirectEmailMessagesToSent(int communicationId, int communicationMessageId);
        MpMessageTemplate GetTemplate(int templateId);
        MpCommunication GetTemplateAsCommunication(int templateId,                                                 
                                                 int toContactId,
                                                 string toEmailAddress,
                                                 Dictionary<string, object> mergeData);
        MpCommunication GetTemplateAsCommunication(int templateId,
                                                 int fromContactId,
                                                 string fromEmailAddress,
                                                 int replyContactId,
                                                 string replyEmailAddress,
                                                 int toContactId,
                                                 string toEmailAddress,
                                                 Dictionary<string, object> mergeData);

        string ParseTemplateBody(string templateBody, Dictionary<string, object> record);
        int GetUserIdFromContactId(string token, int contactId);
        int GetUserIdFromContactId(int contactId);
        string GetEmailFromContactId(int contactId);
        SmtpClient GetSmtpCredentials();
    }
}