﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CrossroadsStripeOnboarding.Models.Json
{
    public class StripeJsonExport
    {
        [JsonProperty("old_customer_id")]
        public Dictionary<string, StripeJsonCustomer> CustomersMap;
    }

    public class StripeJsonCustomer
    {   
        [JsonProperty("id")]
        public string NewCustomerId { get; set; }

        [JsonProperty("cards")]
        public Dictionary<string, StripeJsonCard> CardsMap;

        [JsonProperty("banks")]
        public Dictionary<string, StripeJsonBank> BanksMap;
    }

    public class StripeJsonAccount
    {
        public AccountType Type { get; set; }

        [JsonProperty("id")]
        public string NewAccountId { get; set; }

        [JsonProperty("fingerprint")]
        public string Fingerprint { get; set; }

        [JsonProperty("last4")]
        public string Last4 { get; set; }

        [JsonProperty("exp_month")]
        public int ExpMonth { get; set; }

        [JsonProperty("exp_year")]
        public int ExpYear { get; set; }

        [JsonProperty("brand")]
        public string Institution { get; set; }
    }

    public class StripeJsonCard : StripeJsonAccount
    {
        public StripeJsonCard()
        {
            Type = AccountType.CreditCard;
        }
        
    }

    public class StripeJsonBank : StripeJsonAccount
    {
        public StripeJsonBank()
        {
            Type = AccountType.Bank;
        }

    }
}
