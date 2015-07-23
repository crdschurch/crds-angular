﻿using System.Collections.Generic;
using crds_angular.Models.Crossroads.Stewardship;

namespace crds_angular.Services.Interfaces
{
    public interface IPaymentService
    {
        string CreateCustomer(string customerToken);
        StripeCharge ChargeCustomer(string customerToken, int amount, int donorId);
        string UpdateCustomerDescription(string customerToken, int donorId);
        SourceData UpdateCustomerSource(string customerToken, string cardToken);
        SourceData GetDefaultSource(string customerToken);
        List<StripeCharge> GetChargesForTransfer(string transferId);
    }
}
