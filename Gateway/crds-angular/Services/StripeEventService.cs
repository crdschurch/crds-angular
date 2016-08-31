﻿using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Controllers.API;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Services.Interfaces;
using log4net;
using System.Text;
using Crossroads.Utilities;
using Crossroads.Utilities.Interfaces;
using MinistryPlatform.Translation.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DonationStatus = crds_angular.Models.Crossroads.Stewardship.DonationStatus;

namespace crds_angular.Services
{

    public class StripeEventService : IStripeEventService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(StripeEventController));
        private readonly IPaymentService _paymentService;
        private readonly IDonationService _donationService;
        private readonly MinistryPlatform.Translation.Repositories.Interfaces.IDonorRepository _mpDonorService;
        private readonly int _donationStatusDeclined;
        private readonly int _donationStatusDeposited;
        private readonly int _donationStatusSucceeded;
        private readonly int _batchEntryTypePaymentProcessor;

        // This value is used when creating the batch name for exporting to GP.  It must be 15 characters or less.
        private const string BatchNameDateFormat = @"\M\PyyyyMMddHHmm";
       
        public StripeEventService(IPaymentService paymentService, IDonationService donationService, MinistryPlatform.Translation.Repositories.Interfaces.IDonorRepository mpDonorService, IConfigurationWrapper configuration)
        {
            _paymentService = paymentService;
            _donationService = donationService;
            _mpDonorService = mpDonorService;

            _donationStatusDeclined = configuration.GetConfigIntValue("DonationStatusDeclined");
            _donationStatusDeposited = configuration.GetConfigIntValue("DonationStatusDeposited");
            _donationStatusSucceeded = configuration.GetConfigIntValue("DonationStatusSucceeded");
            _batchEntryTypePaymentProcessor = configuration.GetConfigIntValue("BatchEntryTypePaymentProcessor");
        }

        public void ChargeSucceeded(DateTime? eventTimestamp, StripeCharge charge)
        {
            _logger.Debug("Processing charge.succeeded event for charge id " + charge.Id);
            _donationService.UpdateDonationStatus(charge.Id, _donationStatusSucceeded, eventTimestamp);
        }

        public void ChargeFailed(DateTime? eventTimestamp, StripeCharge charge)
        {
            _logger.Debug("Processing charge.failed event for charge id " + charge.Id);
            var notes = string.Format("{0}: {1}", charge.FailureCode ?? "No Stripe Failure Code", charge.FailureMessage ?? "No Stripe Failure Message");
            _donationService.UpdateDonationStatus(charge.Id, _donationStatusDeclined, eventTimestamp, notes);
            _donationService.ProcessDeclineEmail(charge.Id);

            // Create a refund if it is a bank account failure
            if (charge.Source != null && "bank_account".Equals(charge.Source.Object) && charge.Refunds != null && charge.Refunds.Data != null && charge.Refunds.Data.Any())
            {
                var refundData = _paymentService.GetRefund(charge.Refunds.Data[0].Id);
                _donationService.CreateDonationForBankAccountErrorRefund(new StripeRefund { Data = new List<StripeRefundData> { refundData } } );
            }
        }

        public void InvoicePaymentSucceeded(DateTime? eventTimestamp, StripeInvoice invoice)
        {
            _logger.Debug(string.Format("Processing invoice.payment_succeeded event for subscription id {0}", invoice.Subscription));
            _donationService.CreateDonationForInvoice(invoice);
        }

        private void InvoicePaymentFailed(DateTime? created, StripeInvoice invoice)
        {
            _mpDonorService.ProcessRecurringGiftDecline(invoice.Subscription);
            var gift = _mpDonorService.GetRecurringGiftForSubscription(invoice.Subscription);
           
            if (gift.ConsecutiveFailureCount > 2)
            {
                var subscription = _paymentService.CancelSubscription(gift.StripeCustomerId, gift.SubscriptionId);
                _paymentService.CancelPlan(subscription.Plan.Id);
                _mpDonorService.CancelRecurringGift(gift.RecurringGiftId.Value);
            }
        }

        public TransferPaidResponseDTO TransferPaid(DateTime? eventTimestamp, StripeTransfer transfer)
        {
            _logger.Debug("Processing transfer.paid event for transfer id " + transfer.Id);

            var response = new TransferPaidResponseDTO();
            
            // Don't process this transfer if we already have a batch for the same transfer id
            var existingBatch = _donationService.GetDonationBatchByProcessorTransferId(transfer.Id);
            if (existingBatch != null)
            {
                var msg = string.Format("Batch {0} already created for transfer {1}", existingBatch.Id, existingBatch.ProcessorTransferId);
                _logger.Debug(msg);
                response.TotalTransactionCount = 0;
                response.Message = msg;
                response.Exception = new ApplicationException(msg);
                return (response);
            }

            // Don't process this transfer if we can't find any charges for the transfer
            var charges = _paymentService.GetChargesForTransfer(transfer.Id);
            if (charges == null || charges.Count <= 0)
            {
                var msg = "No charges found for transfer: " + transfer.Id;
                _logger.Debug(msg);
                response.TotalTransactionCount = 0;
                response.Message = msg;
                response.Exception = new ApplicationException(msg);
                return (response);
            }

            var now = DateTime.Now;
            var batchName = now.ToString(BatchNameDateFormat);

            var batch = new DonationBatchDTO()
            {
                BatchName = batchName,
                SetupDateTime = now,
                BatchTotalAmount = 0,
                ItemCount = 0,
                BatchEntryType = _batchEntryTypePaymentProcessor,
                FinalizedDateTime = now,
                DepositId = null,
                ProcessorTransferId = transfer.Id
            };

            response.TotalTransactionCount = charges.Count;
            _logger.Debug(string.Format("{0} charges to update for transfer {1}", charges.Count, transfer.Id));

            // Sort charges so we process refunds for payments in the same transfer after the actual payment is processed
            var sortedCharges = charges.OrderBy(charge => charge.Type);

            foreach (var charge in sortedCharges)
            {
                try
                {
                    var paymentId = charge.Id;
                    StripeRefund refund = null;
                    if ("refund".Equals(charge.Type)) // Credit Card Refund
                    {
                        refund = _paymentService.GetChargeRefund(charge.Id);
                        paymentId = refund.Data[0].Id;
                    }
                    else if ("payment_refund".Equals(charge.Type)) // Bank Account Refund
                    {
                        var refundData = _paymentService.GetRefund(charge.Id);
                        refund = new StripeRefund
                        {
                            Data = new List<StripeRefundData>
                            {
                                refundData
                            }
                        };
                    }

                    DonationDTO donation;
                    try
                    {
                        donation = _donationService.GetDonationByProcessorPaymentId(paymentId);
                    }
                    catch (DonationNotFoundException e)
                    {
                        donation = HandleDonationNotFoundException(transfer, refund, paymentId, e, charge);
                    }
                    
                    if (donation.BatchId != null)
                    {
                        var b = _donationService.GetDonationBatch(donation.BatchId.Value);
                        if (string.IsNullOrWhiteSpace(b.ProcessorTransferId))
                        {
                            // If this donation exists on another batch that does not have a Stripe transfer ID, we'll move it to our batch instead
                            var msg = string.Format("Charge {0} already exists on batch {1}, moving to new batch", charge.Id, b.Id);
                            _logger.Debug(msg);
                        } 
                        else 
                        {
                            // If this donation exists on another batch that has a Stripe transfer ID, skip it
                            var msg = string.Format("Charge {0} already exists on batch {1} with transfer id {2}", charge.Id, b.Id, b.ProcessorTransferId);
                            _logger.Debug(msg);
                            response.FailedUpdates.Add(new KeyValuePair<string, string>(charge.Id, msg));
                            continue;
                        }
                    }

                    if (donation.Status != DonationStatus.Declined && donation.Status != DonationStatus.Refunded)
                    {
                        _logger.Debug(string.Format("Updating charge id {0} to Deposited status", charge.Id));
                        _donationService.UpdateDonationStatus(int.Parse(donation.Id), _donationStatusDeposited, eventTimestamp);
                    }
                    else
                    {
                        _logger.Debug(string.Format("Not updating charge id {0} to Deposited status - it was already {1}",
                                                    charge.Id,
                                                    System.Enum.GetName(typeof (DonationStatus), donation.Status)));
                    }
                    response.SuccessfulUpdates.Add(charge.Id);
                    batch.ItemCount++;
                    batch.BatchTotalAmount += (charge.Amount /Constants.StripeDecimalConversionValue);
                    batch.Donations.Add(new DonationDTO { Id = donation.Id, Amount = charge.Amount, Fee = charge.Fee });
                }
                catch (Exception e)
                {
                    _logger.Warn("Error updating charge " + charge, e);
                    response.FailedUpdates.Add(new KeyValuePair<string, string>(charge.Id, e.Message));
                }
            }

            if (response.FailedUpdates.Count > 0)
            {
                response.Exception = new ApplicationException("Could not update all charges to 'deposited' status, see message for details");
            }

            var stripeTotalFees = batch.Donations.Sum(f => f.Fee);
            
            // Create the deposit
            var deposit = new DepositDTO
            {
                // Account number must be non-null, and non-empty; using a single space to fulfill this requirement
                AccountNumber = " ",
                BatchCount = 1,
                DepositDateTime = now,
                DepositName = batchName,
                // This is the amount from Stripe - will show out of balance if does not match batch total above
                DepositTotalAmount = ((transfer.Amount / Constants.StripeDecimalConversionValue) + (stripeTotalFees / Constants.StripeDecimalConversionValue)),
                ProcessorFeeTotal = stripeTotalFees / Constants.StripeDecimalConversionValue,
                DepositAmount = transfer.Amount /Constants.StripeDecimalConversionValue,
                Exported = false,
                Notes = null,
                ProcessorTransferId = transfer.Id
            };
            try
            {
                response.Deposit = _donationService.CreateDeposit(deposit);
            }
            catch (Exception e)
            {
                _logger.Error("Failed to create batch deposit", e);
                throw;
            }

            // Create the batch, with the above deposit id
            batch.DepositId = response.Deposit.Id;
            try
            {
                response.Batch = _donationService.CreateDonationBatch(batch);
            }
            catch (Exception e)
            {
                _logger.Error("Failed to create donation batch", e);
                throw;
            }

            return (response);
        }

        private DonationDTO HandleDonationNotFoundException(StripeTransfer transfer, StripeRefund refund, string paymentId, DonationNotFoundException e, StripeCharge charge)
        {
            DonationDTO donation;
            if (refund != null)
            {
                _logger.Warn(string.Format("Payment not found for refund {0} on transfer {1} - may be a refund due to a bank account error", paymentId, transfer.Id));
                // If this is a refund that doesn't exist in MP, create it, assuming it is a refund due to a bank account error (NSF, etc)
                if (_donationService.CreateDonationForBankAccountErrorRefund(refund) != null)
                {
                    donation = _donationService.GetDonationByProcessorPaymentId(paymentId);
                    _logger.Debug(string.Format("Updating charge id {0} to Declined status", charge.Id));
                    _donationService.UpdateDonationStatus(refund.Data[0].ChargeId, _donationStatusDeclined, refund.Data[0].BalanceTransaction.Created);
                }
                else
                {
                    _logger.Error(string.Format("Payment not found for refund {0} on transfer {1}, probably not a bank account error", paymentId, transfer.Id), e);
                    // ReSharper disable once PossibleIntendedRethrow
                    throw e;
                }
            }
            else
            {
                _logger.Warn(string.Format("Payment not found for charge {0} on transfer {1} - may be an ACH recurring gift that has not yet processed", paymentId, transfer.Id));
                var stripeCharge = _paymentService.GetCharge(charge.Id);
                if (stripeCharge != null && stripeCharge.Source != null && "bank_account".Equals(stripeCharge.Source.Object) && stripeCharge.HasInvoice())
                {
                    // We're making an assumption that if an ACH payment is included in a transfer, 
                    // and if we don't have the corresponding Donation in our system yet, that
                    // this is a mistake.  For instance, events delivered out of order from Stripe, and
                    // we received the transfer.paid before the invoice.payment_succeeded.
                    // In this scenario, we will go ahead and create the donation.
                    if (_donationService.CreateDonationForInvoice(stripeCharge.Invoice) != null)
                    {
                        _logger.Debug(string.Format("Creating donation for recurring gift payment {0}", charge.Id));
                        donation = _donationService.GetDonationByProcessorPaymentId(paymentId);
                    }
                    else
                    {
                        _logger.Error(string.Format("Donation not found for charge {0} on transfer {1}, and failed to create a donation", charge.Id, transfer.Id), e);
                        // ReSharper disable once PossibleIntendedRethrow
                        throw e;
                    }
                }
                else
                {
                    _logger.Error(string.Format("Donation not found for charge {0} on transfer {1}, charge does not appear to be related to an ACH recurring gift", charge.Id, transfer.Id));
                    // ReSharper disable once PossibleIntendedRethrow
                    throw e;
                }
            }
            return donation;
        }

        public StripeEventResponseDTO ProcessStripeEvent(StripeEvent stripeEvent)
        {
            StripeEventResponseDTO response = null;
            try
            {
                switch (stripeEvent.Type)
                {
                    case "charge.succeeded":
                        ChargeSucceeded(stripeEvent.Created, ParseStripeEvent<StripeCharge>(stripeEvent.Data));
                        break;
                    case "charge.failed":
                        ChargeFailed(stripeEvent.Created, ParseStripeEvent<StripeCharge>(stripeEvent.Data));
                        break;
                    case "transfer.paid":
                        response = TransferPaid(stripeEvent.Created, ParseStripeEvent<StripeTransfer>(stripeEvent.Data));
                        break;
                    case "invoice.payment_succeeded":
                        InvoicePaymentSucceeded(stripeEvent.Created, ParseStripeEvent<StripeInvoice>(stripeEvent.Data));
                        break;
                    case "invoice.payment_failed":
                        InvoicePaymentFailed(stripeEvent.Created, ParseStripeEvent<StripeInvoice>(stripeEvent.Data));
                        break;
                    default:
                        _logger.Debug("Ignoring event " + stripeEvent.Type);
                        break;
                }
                if (response != null && response.Exception != null)
                {
                    RecordFailedEvent(stripeEvent, response);
                }
            }
            catch (Exception e)
            {
                response = new StripeEventResponseDTO
                {
                    Exception = new ApplicationException("Problem processing Stripe event", e)
                };
                RecordFailedEvent(stripeEvent, response);
                throw;
            }
            return (response);
        }

        public void RecordFailedEvent(StripeEvent stripeEvent, StripeEventResponseDTO stripeEventResponse)
        {
            try
            {
                _donationService.CreatePaymentProcessorEventError(stripeEvent, stripeEventResponse);
            }
            catch (Exception e)
            {
                _logger.Error("Error writing event to failure log", e);
            }
        }

        private static T ParseStripeEvent<T>(StripeEventData data)
        {
            var jObject = data != null && data.Object != null ? data.Object as JObject : null;
            return jObject != null ? JsonConvert.DeserializeObject<T>(jObject.ToString()) : (default(T));
        }
    }

    // ReSharper disable once InconsistentNaming
    public class StripeEventResponseDTO
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("exception")]
        public ApplicationException Exception { get; set; }
    }

    // ReSharper disable once InconsistentNaming
    public class TransferPaidResponseDTO : StripeEventResponseDTO
    {
        [JsonProperty("transaction_count")]
        public int TotalTransactionCount { get; set; }

        [JsonProperty("successful_updates")]
        public List<string> SuccessfulUpdates { get { return (_successfulUpdates); } }
        private readonly List<string> _successfulUpdates = new List<string>();

        [JsonProperty("failed_updates")]
        public List<KeyValuePair<string, string>> FailedUpdates { get { return (_failedUpdates); } }
        private readonly List<KeyValuePair<string, string>> _failedUpdates = new List<KeyValuePair<string, string>>();

        [JsonProperty("donation_batch")]
        public DonationBatchDTO Batch;

        [JsonProperty("deposit")]
        public DepositDTO Deposit;
    }
}