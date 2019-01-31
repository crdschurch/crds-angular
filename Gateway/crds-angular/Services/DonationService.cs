using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using crds_angular.Models.Crossroads.Stewardship;
using MPServices=MinistryPlatform.Translation.Repositories.Interfaces;
using crds_angular.Services.Interfaces;
using crds_angular.Util;
using Crossroads.Utilities;
using Crossroads.Utilities.Interfaces;
using log4net;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Models;
using Newtonsoft.Json;
using DonationStatus = crds_angular.Models.Crossroads.Stewardship.DonationStatus;
using PaymentType = crds_angular.Models.Crossroads.Stewardship.PaymentType;
using Constants = Crossroads.Utilities.Constants;

namespace crds_angular.Services
{
    public class DonationService: IDonationService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (DonationService));

        private readonly MPServices.IDonationRepository _mpDonationRepository;
        private readonly MPServices.IDonorRepository _mpDonorRepository;
        private readonly IPaymentProcessorService _paymentService;
        private readonly MPServices.IContactRepository _contactRepository;
        private readonly int _statementTypeFamily;
        private readonly int _bankErrorRefundDonorId;

        public DonationService(MPServices.IDonationRepository mpDonationRepository, MPServices.IDonorRepository mpDonorRepository, IPaymentProcessorService paymentService, MPServices.IContactRepository contactRepository, IConfigurationWrapper config)
        {
            _mpDonationRepository = mpDonationRepository;
            _mpDonorRepository = mpDonorRepository;
            _paymentService = paymentService;
            _contactRepository = contactRepository;
            _statementTypeFamily = config.GetConfigIntValue("DonorStatementTypeFamily");
            _bankErrorRefundDonorId = config.GetConfigIntValue("DonorIdForBankErrorRefund");
        }

        public DonationDTO GetDonationByProcessorPaymentId(string processorPaymentId)
        {
            var d = _mpDonationRepository.GetDonationByProcessorPaymentId(processorPaymentId);
            if (d == null)
            {
                return (null);
            }

            var donation = new DonationDTO
            {
                Amount = d.donationAmt,
                Id = d.donationId + "",
                BatchId = d.batchId,
                Status = (DonationStatus)d.donationStatus
            };
            return (donation);
        }

        public int UpdateDonationStatus(int donationId, int statusId, DateTime? statusDate, string statusNote = null)
        {
            return(_mpDonationRepository.UpdateDonationStatus(donationId, statusId, statusDate ?? DateTime.Now, statusNote));
        }

        public int UpdateDonationStatus(string processorPaymentId, int statusId, DateTime? statusDate, string statusNote = null)
        {
            return(_mpDonationRepository.UpdateDonationStatus(processorPaymentId, statusId, statusDate ?? DateTime.Now, statusNote));
        }

        public DonationBatchDTO CreateDonationBatch(DonationBatchDTO batch)
        {
            var batchId = _mpDonationRepository.CreateDonationBatch(batch.BatchName, batch.SetupDateTime, batch.BatchTotalAmount,batch.ItemCount, batch.BatchEntryType, batch.DepositId, batch.FinalizedDateTime, batch.ProcessorTransferId);

            batch.Id = batchId;

            foreach (var donation in batch.Donations)
            {
                _mpDonationRepository.AddDonationToBatch(batchId, int.Parse(donation.Id));
            }

            return (batch);
        }

        public DepositDTO GetDepositByProcessorTransferId(string processorTransferId)
        {
            return (Mapper.Map<MpDeposit, DepositDTO>(_mpDonationRepository.GetDepositByProcessorTransferId(processorTransferId)));
        }

        public DonationBatchDTO GetDonationBatch(int batchId)
        {
            return (Mapper.Map<MpDonationBatch, DonationBatchDTO>(_mpDonationRepository.GetDonationBatch(batchId)));
        }

        public DonationsDTO GetDonationsForAuthenticatedUser(string userToken, string donationYear = null, int? limit = null, bool? softCredit = null, bool? includeRecurring = true)
        {
            var donations = _mpDonorRepository.GetDonationsForAuthenticatedUser(userToken, softCredit, donationYear, includeRecurring);            
            return (PostProcessDonations(donations, limit));
        }

        public DonationYearsDTO GetDonationYearsForAuthenticatedUser(string userToken)
        {
            var donations = _mpDonorRepository.GetDonationsForAuthenticatedUser(userToken, null, null);

            var years = new HashSet<string>();
            if (donations != null && donations.Any())
            {
                years.UnionWith(donations.Select(d => d.donationDate.Year.ToString()));
            }

            var donationYears = new DonationYearsDTO();
            donationYears.AvailableDonationYears.AddRange(years.ToList());

            return (donationYears);
        }

        public DonationsDTO GetDonationsForDonor(int donorId, string donationYear = null, bool softCredit = false)
        {
            var donor = _mpDonorRepository.GetEmailViaDonorId(donorId);
            return (GetDonationsForDonor(donor, donationYear, softCredit));
        }

        public DonationYearsDTO GetDonationYearsForDonor(int donorId)
        {
            var donor = _mpDonorRepository.GetEmailViaDonorId(donorId);
            return (GetDonationYearsForDonor(donor));
        }

        private DonationsDTO GetDonationsForDonor(MpContactDonor donor, string donationYear = null, bool softCredit = false)
        {
            var donorIds = GetDonorIdsForDonor(donor);

            var donations = softCredit ? _mpDonorRepository.GetSoftCreditDonations(donorIds, donationYear) : _mpDonorRepository.GetDonations(donorIds, donationYear);
            return (PostProcessDonations(donations));
        }

        private DonationsDTO PostProcessDonations(List<MpDonation> donations, int? limit = null)
        {
            if (donations == null || donations.Count == 0)
            {
                return (null);
            }

            var response = donations.Select(Mapper.Map<DonationDTO>).ToList();
            var donationsResponse = NormalizeDonations(response, limit);

            return (donationsResponse);
        }

        private DonationsDTO NormalizeDonations(IList<DonationDTO> donations, int? limit = null)
        {
            foreach (var donation in donations)
            {
                if (donation.Source.SourceType != PaymentType.SoftCredit)
                {
                    var charge = GetStripeCharge(donation);
                    SetDonationSource(donation, charge);
                }

                ConfirmRefundCorrect(donation);
            }

            donations = OrderDonations(donations, limit);
            donations = LimitDonations(donations, limit);


            var donationsResponse = new DonationsDTO();

            donationsResponse.Donations.AddRange(donations);
            donationsResponse.BeginningDonationDate = donationsResponse.Donations.First().DonationDate;
            donationsResponse.EndingDonationDate = donationsResponse.Donations.Last().DonationDate;

            return donationsResponse;
        }

        private StripeCharge GetStripeCharge(DonationDTO donation)
        {
            if (string.IsNullOrWhiteSpace(donation.Source.PaymentProcessorId))
            {
                return null;
            }

            // If it is a positive amount, it means it's a Charge, otherwise it's a Refund
            if (donation.Amount >= 0)
            {
                return _paymentService.GetCharge(donation.Source.PaymentProcessorId);
            }
            
            var refund = _paymentService.GetRefund(donation.Source.PaymentProcessorId);
            if (refund != null && refund.Charge != null)
            {
               return refund.Charge;
            }

            return null;
        }

        public void SetDonationSource(DonationDTO donation, StripeCharge charge)
        {
            if (donation.Source.SourceType == PaymentType.Cash)
            {
                donation.Source.AccountHolderName = "cash";
            }
            else if (charge != null && charge.Source != null)
            {
                donation.Source.AccountNumberLast4 = charge.Source.AccountNumberLast4;

                if (donation.Source.SourceType != PaymentType.CreditCard || charge.Source.Brand == null)
                {
                    return;
                }
                switch (charge.Source.Brand)
                {
                    case CardBrand.AmericanExpress:
                        donation.Source.CardType = CreditCardType.AmericanExpress;
                        break;
                    case CardBrand.Discover:
                        donation.Source.CardType = CreditCardType.Discover;
                        break;
                    case CardBrand.MasterCard:
                        donation.Source.CardType = CreditCardType.MasterCard;
                        break;
                    case CardBrand.Visa:
                        donation.Source.CardType = CreditCardType.Visa;
                        break;
                    default:
                        donation.Source.CardType = null;
                        break;
                }
            }
        }

        private void ConfirmRefundCorrect(DonationDTO donation)
        {
            // Refund amount should already be negative (when the original donation was reversed), but negative-ify it just in case
            if (donation.Status != DonationStatus.Refunded || donation.Amount <= 0)
            {
                return;
            }

            donation.Amount *= -1;
            donation.Distributions.All(dist =>
            {
                dist.Amount *= -1;
                return (true);
            });
        }

        private IList<DonationDTO> OrderDonations(IList<DonationDTO> donations, int? limit = null)
        {
            return (limit == null)
                ? donations.OrderBy(donation => donation.DonationDate).ToList()
                : donations.OrderByDescending(donation => donation.DonationDate).ToList();
        }

        private IList<DonationDTO> LimitDonations(IList<DonationDTO> donations, int? limit = null)
        {
            //limit is on the donation & distribution level
            if (limit != null)
            {
                var numDistributions = 0;
                var limitedDonations = new List<DonationDTO>();

                foreach (var donation in donations)
                {
                    numDistributions += donation.Distributions.Count;

                    // There are too many distributions so some need to be removed
                    if (numDistributions > limit)
                    {
                        var numToRemove = numDistributions - (int)limit;
                        var removeStartIndex = donation.Distributions.Count - numToRemove;

                        donation.Distributions.RemoveRange(removeStartIndex, numToRemove);
                    }

                    limitedDonations.Add(donation);
                    
                    // if we have hit the limit break the loop
                    if (numDistributions >= limit)
                    {
                        break;
                    }
            
                }


                donations = limitedDonations;
            }

            return donations;
        }

        private DonationYearsDTO GetDonationYearsForDonor(MpContactDonor donor)
        {
            var donorIds = GetDonorIdsForDonor(donor);
            var donations = _mpDonorRepository.GetDonations(donorIds, null);
            var softCreditDonations = _mpDonorRepository.GetSoftCreditDonations(donorIds);

            var years = new HashSet<string>();
            if (softCreditDonations != null && softCreditDonations.Any())
            {
                years.UnionWith(softCreditDonations.Select(d => d.donationDate.Year.ToString()));
            }
            if (donations != null && donations.Any())
            {
                years.UnionWith(donations.Select(d => d.donationDate.Year.ToString()));
            }

            var donationYears = new DonationYearsDTO();
            donationYears.AvailableDonationYears.AddRange(years.ToList());

            return (donationYears);
        }

        private IEnumerable<int> GetDonorIdsForDonor(MpContactDonor donor)
        {
            var donorIds = new HashSet<int>();
            if (donor.ExistingDonor)
            {
                donorIds.Add(donor.DonorId);
            }

            if (donor.StatementTypeId != _statementTypeFamily || !donor.HasDetails)
            {
                return (donorIds);
            }

            var household = _contactRepository.GetHouseholdFamilyMembers(donor.Details.HouseholdId);
            if (household == null || !household.Any())
            {
                return (donorIds);
            }

            foreach (var member in household)
            {
                if(member.StatementTypeId.HasValue && member.StatementTypeId == _statementTypeFamily && member.DonorId.HasValue)
                {
                    donorIds.Add(member.DonorId.Value);
                }
            }

            return (donorIds);
        }

        public DonationBatchDTO GetDonationBatchByDepositId(int depositId)
        {
            return (Mapper.Map<MpDonationBatch, DonationBatchDTO>(_mpDonationRepository.GetDonationBatchByDepositId(depositId)));
        }

        public List<DepositDTO> GetSelectedDonationBatches(int selectionId)
        {
            var selectedDeposits = _mpDonationRepository.GetSelectedDonationBatches(selectionId);
            var deposits = new List<DepositDTO>();

            foreach (var deposit in selectedDeposits)
            {
                deposits.Add(Mapper.Map<MpDeposit, DepositDTO>(deposit));
            }

            return deposits;
        }

        public DepositDTO GetDepositById(int depositId)
        {
            return (Mapper.Map<MpDeposit, DepositDTO>(_mpDonationRepository.GetDepositById(depositId)));
        }

        public void ProcessDeclineEmail(string processorPaymentId)
        {
            _mpDonationRepository.ProcessDeclineEmail(processorPaymentId);
        }

        public DepositDTO CreateDeposit(DepositDTO deposit)
        {
            deposit.Id = _mpDonationRepository.CreateDeposit(deposit.DepositName, deposit.DepositTotalAmount, deposit.DepositAmount, deposit.ProcessorFeeTotal, deposit.DepositDateTime,
                deposit.AccountNumber, deposit.BatchCount, deposit.Exported, deposit.Notes, deposit.ProcessorTransferId);
            
            return (deposit);

        }

        public void CreatePaymentProcessorEventError(StripeEvent stripeEvent, StripeEventResponseDTO stripeEventResponse)
        {
            _mpDonationRepository.CreatePaymentProcessorEventError(stripeEvent.Created, stripeEvent.Id, stripeEvent.Type, JsonConvert.SerializeObject(stripeEvent, Formatting.Indented), JsonConvert.SerializeObject(stripeEventResponse, Formatting.Indented));
        }


        public List<GPExportDatumDTO> GetGpExport(int depositId)
        {
            var gpExportData = _mpDonationRepository.GetGpExport(depositId);
            return gpExportData.Select(Mapper.Map<MpGPExportDatum, GPExportDatumDTO>).ToList();
        }

        public MemoryStream CreateGPExport(int selectionId, int depositId)
        {
            var gpExport = GetGpExport(depositId);
            var stream = new MemoryStream();
            CSV.Create(gpExport, GPExportDatumDTO.Headers, stream, "\t");
            UpdateDepositToExported(selectionId, depositId);

            return stream;
        }

        private void UpdateDepositToExported(int selectionId, int depositId)
        {
            _mpDonationRepository.UpdateDepositToExported(selectionId, depositId);
        }

        public List<DepositDTO> GenerateGPExportFileNames(int selectionId)
        {
            var deposits = GetSelectedDonationBatches(selectionId);

            foreach (var deposit in deposits)
            {
                deposit.ExportFileName = GPExportFileName(deposit);
            }

            return deposits;
        }

        public void SendMessageToDonor(int donorId, int donationDistributionId, int fromContactId, string body, string tripName)
        {
            _mpDonationRepository.SendMessageToDonor(donorId, donationDistributionId, fromContactId, body, tripName);
        }

        public string GPExportFileName(DepositDTO deposit)
        {            
            var date = DateTime.Today.ToString("yyMMdd");            
            var depositName = deposit.DepositName.Replace(" ", "_");
            return string.Format("XRDReceivables-{0}_{1}.txt", depositName, date);
        }

        public int? CreateDonationForBankAccountErrorRefund(StripeRefund refund)
        {
            if (refund.Data[0].BalanceTransaction == null || !"payment_failure_refund".Equals(refund.Data[0].BalanceTransaction.Type))
            {
                _logger.Error(string.Format("Balance transaction was not set, or was not a payment_failure_refund for refund ID {0}", refund.Data[0].Id));
                return (null);
            }

            if (refund.Data[0].Charge == null || string.IsNullOrWhiteSpace(refund.Data[0].Charge.Id))
            {
                _logger.Error(string.Format("No associated Charge for Refund {0}", refund.Data[0].Id));
                return (null);
            }

            MpDonation donation;
            try
            {
                donation = _mpDonationRepository.GetDonationByProcessorPaymentId(refund.Data[0].Charge.Id, true);
            }
            catch (DonationNotFoundException)
            {
                _logger.Error(string.Format("No Donation with payment processor ID {0} in MP for Refund {1}", refund.Data[0].Charge.Id, refund.Data[0].Id));
                return (null);
            }

            var donationAndDist = new MpDonationAndDistributionRecord
            {
                Anonymous = false,
                ChargeId = refund.Data[0].Id,
                CheckNumber = null,
                CheckScannerBatchName = null,
                DonationAmt = -(int.Parse(refund.Data[0].Amount) / Constants.StripeDecimalConversionValue),
                DonationStatus = (int)DonationStatus.Declined,
                DonorAcctId = string.Empty,
                DonorId = _bankErrorRefundDonorId,
                FeeAmt = refund.Data[0].BalanceTransaction.Fee,
                PledgeId = null,
                RecurringGift = false,
                ProcessorId = string.Empty,
                ProgramId = donation.Distributions[0].donationDistributionProgram,
                PymtType = MinistryPlatform.Translation.Enum.PaymentType.GetPaymentType(donation.paymentTypeId).name,
                RecurringGiftId = null,
                RegisteredDonor = false,
                SetupDate = refund.Data[0].BalanceTransaction.Created,
                Notes = string.Format("Reversed from DonationID {0}", donation.donationId)
            };

            foreach (var distribution in donation.Distributions)
            {
                donationAndDist.Distributions.Add(new MpDonationDistribution
                {
                    donationDistributionAmt = -distribution.donationDistributionAmt,
                    donationDistributionProgram = distribution.donationDistributionProgram,
                    PledgeId = distribution.PledgeId
                });
            }

            // Create the refund donation and distribution(s), but do NOT send email
            return(_mpDonorRepository.CreateDonationAndDistributionRecord(donationAndDist, false));
        }

        public int? CreateDonationForInvoice(StripeInvoice invoice)
        {
            if (string.IsNullOrWhiteSpace(invoice.Charge) || invoice.Amount <= 0)
            {
                _logger.Info(string.Format("No charge or amount on invoice {0} for subscription {1} - this is likely a trial-period donation, skipping", invoice.Id, invoice.Subscription));
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug(string.Format("Invoice: {0}", JsonConvert.SerializeObject(invoice)));
                }
                return (null);
            }

            // Make sure we don't create duplicate donations for the same charge (could happen if we get a transfer and a invoice.payment_succeeded events near each other
            try
            {
                var donation = _mpDonationRepository.GetDonationByProcessorPaymentId(invoice.Charge);
                if (donation != null)
                {
                    _logger.Info(string.Format("Donation already located for charge id {0}, not creating duplicate (existing donation {1})", invoice.Charge, donation.donationId));
                    return (donation.donationId);
                }
            }
            catch (DonationNotFoundException)
            {
                _logger.Info(string.Format("Donation not located for charge id {0}, this is expected", invoice.Charge));
            }

            var charge = _paymentService.GetCharge(invoice.Charge);
            var createDonation = _mpDonorRepository.GetRecurringGiftForSubscription(invoice.Subscription, charge.ProcessorId);
            _mpDonorRepository.UpdateRecurringGiftFailureCount(createDonation.RecurringGiftId.Value, Constants.ResetFailCount);

            var donationStatus = charge.Status == "succeeded" ? DonationStatus.Succeeded : DonationStatus.Pending;
            var fee = charge.BalanceTransaction != null ? charge.BalanceTransaction.Fee : null;
            var amount = charge.Amount / Constants.StripeDecimalConversionValue;

            var donationAndDistribution = new MpDonationAndDistributionRecord
            {
                DonationAmt = amount,
                FeeAmt = fee,
                DonorId = createDonation.DonorId,
                ProgramId = createDonation.ProgramId,
                ChargeId = invoice.Charge,
                PymtType = createDonation.PaymentType,
                ProcessorId = invoice.Customer,
                SetupDate = invoice.Date,
                RegisteredDonor = true,
                RecurringGift = true,
                RecurringGiftId = createDonation.RecurringGiftId,
                DonorAcctId = createDonation.DonorAccountId.HasValue ? createDonation.DonorAccountId.ToString() : null,
                DonationStatus = (int)donationStatus
            };

            return (_mpDonorRepository.CreateDonationAndDistributionRecord(donationAndDistribution, false));
        }
        
    }
}
