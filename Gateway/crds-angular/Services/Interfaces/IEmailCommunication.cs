using System.Net.Mail;
using crds_angular.Models.Crossroads;

namespace crds_angular.Services.Interfaces
{
    public interface IEmailCommunication
    {
        void SendEmail(EmailCommunicationDTO email);
        void SendEmail(CommunicationDTO emailData);
        DirectEmailCommunication GetDirectEmailMessage(int communicationId);
        bool UpdateDirectEmailMessagesToSent(int communicationId, int communicationMessageId);
        SmtpClient GetSmtpCredentials();
    }
}