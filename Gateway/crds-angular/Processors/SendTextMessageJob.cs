﻿
using System.Text.RegularExpressions;
using System.Web.UI;
using crds_angular.Models.Crossroads;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Quartz;

namespace crds_angular.Processors
{
    public class SendTextMessageJob : IJob
    {
        private readonly ITextCommunicationService _textCommunicationService;
        private readonly ICommunicationRepository _communicationService;

        public SendTextMessageJob(ITextCommunicationService textCommunicationService, ICommunicationRepository communicationService)
        {
            _textCommunicationService = textCommunicationService;
            _communicationService = communicationService;
        }

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            TextCommunicationDto dto = (TextCommunicationDto)dataMap.Get("dto");
            MpMessageTemplate template = _communicationService.GetTemplate(dto.TemplateId);
            string htmlBody =_communicationService.ParseTemplateBody(template.Body, dto.MergeData);
            Regex regex = new Regex("<[^>]*>");
            string textBody = regex.Replace(htmlBody, "");
            _textCommunicationService.SendTextMessage(dto.ToPhoneNumber, textBody);
        }
    }
}