﻿using MinistryPlatform.Translation.Models.Attributes;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Contact_Attributes")]
    public class MpContactAttribute
    {   
        public int Contact_ID { get; set; }
        public int Attribute_ID { get; set; }
    }
}