﻿using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models.Childcare;

namespace MinistryPlatform.Translation.Services.Interfaces
{
    public interface IChildcareRequestService
    {
        int CreateChildcareRequest(ChildcareRequest request);

        ChildcareRequestEmail GetChildcareRequest(int childcareRequestId, string token);

        ChildcareRequest GetChildcareRequestForReview(int childcareRequestId);

        void DecisionChildcareRequest(int childcareRequestId, int requestStatusId, ChildcareRequest childcareRequest);
        void CreateChildcareRequestDates(int childcareRequestId, ChildcareRequest mpRequest, string token);       
        List<ChildcareRequestDate> GetChildcareRequestDates(int childcareRequestId);
        List<ChildcareRequestDate> GetChildcareRequestDatesForReview(int childcareRequestId);        
        Dictionary<int, int> FindChildcareEvents(int childcareRequestId, List<ChildcareRequestDate> requestedDates);        
        void DecisionChildcareRequestDate(int childcareRequestDateId, bool decision);
        ChildcareRequestDate GetChildcareRequestDates(int childcareRequestId, DateTime date, string token);
    }
}
