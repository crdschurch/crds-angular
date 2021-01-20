using System;
using crds_angular.Models.Crossroads.Stewardship;

namespace crds_angular.Services.Interfaces
{
    public interface IStripeEventService
    {
        void ChargeSucceeded(DateTime? eventTimestamp, StripeCharge charge);
        void ChargeFailed(DateTime? eventTimestamp, StripeCharge charge);
        TransferPaidResponseDTO TransferPaid(DateTime? eventTimestamp, StripeTransfer transfer);
        StripeEventResponseDTO ProcessStripeEvent(StripeEvent stripeEvent);
        void RecordFailedEvent(StripeEvent stripeEvent, StripeEventResponseDTO stripeEventResponse);
    }
}
