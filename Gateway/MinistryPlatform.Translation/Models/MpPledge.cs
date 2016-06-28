﻿using System;

namespace MinistryPlatform.Translation.Models
{
    public class MpPledge
    {
        public int PledgeId { get; set; }
        public int PledgeCampaignId { get; set; }
        public int DonorId { get; set; }
        public int PledgeStatusId { get; set; }
        public string PledgeStatus { get; set; }
        public string CampaignName { get; set; }
        public decimal PledgeTotal { get; set; }
        public decimal PledgeDonations { get; set; }
        public DateTime CampaignStartDate { get; set; }
        public DateTime CampaignEndDate { get; set; }
        public int CampaignTypeId { get; set; }
        public string CampaignTypeName { get; set; }
    }
}