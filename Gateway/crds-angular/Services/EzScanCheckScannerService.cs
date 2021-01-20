using System;
using System.Collections.Generic;
using AutoMapper;
using crds_angular.DataAccess.Interfaces;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Services.Interfaces;
using log4net;
using MPServices = MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.Utilities.Extensions;
using MinistryPlatform.Translation.Models;

namespace crds_angular.Services
{
    public class EzScanCheckScannerService :  ICheckScannerService
    {
        private readonly ICheckScannerDao _checkScannerDao;
        private readonly IDonorService _donorService;
        private readonly ILog _logger = LogManager.GetLogger(typeof (EzScanCheckScannerService));
        private readonly MPServices.IDonorRepository _mpDonorService;
        private readonly IPaymentProcessorService _paymentService;

        private const int MinistryPlatformCheckNumberMaxLength = 15;
      
        public EzScanCheckScannerService(ICheckScannerDao checkScannerDao, IDonorService donorService, IPaymentProcessorService paymentService, MPServices.IDonorRepository mpDonorService)
        {
            _checkScannerDao = checkScannerDao;
            _donorService = donorService;
            _paymentService = paymentService;
            _mpDonorService = mpDonorService;
        }

        public List<CheckScannerBatch> GetBatches(bool onlyOpenBatches = true)
        {
            return (_checkScannerDao.GetBatches(onlyOpenBatches));
        }

        public List<CheckScannerCheck> GetChecksForBatch(string batchName)
        {
            return (_checkScannerDao.GetChecksForBatch(batchName));
        }

        public CheckScannerBatch UpdateBatchStatus(string batchName, BatchStatus newStatus)
        {
            return (_checkScannerDao.UpdateBatchStatus(batchName, newStatus));
        }

        public CheckScannerBatch CreateDonationsForBatch(CheckScannerBatch batchDetails)
        {
            var checks = _checkScannerDao.GetChecksForBatch(batchDetails.Name);
            foreach (var check in checks)
            {
                if (check.Exported)
                {
                    var previousError = string.IsNullOrWhiteSpace(check.Error) ? string.Empty : string.Format("Previous Error: {0}", check.Error);
                    var msg = string.Format("Not exporting check {0} on batch {1}, it was already exported. {2}", check.Id, batchDetails.Name, previousError);
                    _logger.Info(msg);
                    check.Error = msg;
                    batchDetails.ErrorChecks.Add(check);
                    continue;
                }

                try
                {
                    var contactDonor = CreateDonor(check);


                    var account = _mpDonorService.DecryptCheckValue(check.AccountNumber);
                    var routing = _mpDonorService.DecryptCheckValue(check.RoutingNumber);
                    var encryptedKey = _mpDonorService.CreateHashedAccountAndRoutingNumber(account, routing);


                    string donorAccountId = "";

                    if (contactDonor.Account.HasPaymentProcessorInfo() == false)
                    {
                        var stripeCustomer = _paymentService.CreateCustomer(null, contactDonor.DonorId + " Scanned Checks", contactDonor.Details?.EmailAddress, contactDonor.Details?.DisplayName);

                        var stripeCustomerSource = _paymentService.AddSourceToCustomer(stripeCustomer.id, contactDonor.Account.Token);

                        donorAccountId = _mpDonorService.CreateDonorAccount(null,
                                                                                routing,
                                                                                account.Right(4),
                                                                                encryptedKey,
                                                                                contactDonor.DonorId,
                                                                                stripeCustomerSource.id,
                                                                                stripeCustomer.id).ToString();

                        contactDonor.Account = new MpDonorAccount
                        {
                            DonorAccountId = int.Parse(donorAccountId),
                            ProcessorId = stripeCustomer.id,
                            ProcessorAccountId = stripeCustomerSource.id
                            
                        };
                    }
                    else
                    {
                        donorAccountId = contactDonor.Account.DonorAccountId.ToString();
                    }

                    //Always use the customer ID and source ID from the Donor Account, if it exists
                    var charge = _paymentService.ChargeCustomer(contactDonor.Account.ProcessorId, contactDonor.Account.ProcessorAccountId, check.Amount, contactDonor.DonorId, check.CheckNumber, contactDonor.Details?.EmailAddress, contactDonor.Details?.DisplayName);


                    var fee = charge.BalanceTransaction != null ? charge.BalanceTransaction.Fee : null;

                    // Mark the check as exported now, so we don't double-charge a community member.
                    // If the CreateDonationAndDistributionRecord fails, we'll still consider it exported, but
                    // it will be in error, and will have to be manually resolved.
                    check.Exported = true;
                 
                    var programId = batchDetails.ProgramId == null ? null : batchDetails.ProgramId + "";

                    var donationAndDistribution = new MpDonationAndDistributionRecord
                    {
                        DonationAmt = check.Amount,
                        FeeAmt = fee,
                        DonorId = contactDonor.DonorId,
                        ProgramId = programId,
                        ChargeId = charge.Id,
                        PymtType = "check",
                        ProcessorId = contactDonor.Account.ProcessorId,
                        SetupDate = check.CheckDate ?? (check.ScanDate ?? DateTime.Now),
                        RegisteredDonor = contactDonor.RegisteredUser,
                        DonorAcctId = donorAccountId,
                        CheckScannerBatchName = batchDetails.Name,
                        CheckNumber = (check.CheckNumber ?? string.Empty).TrimStart(' ', '0').Right(MinistryPlatformCheckNumberMaxLength)
                    };

                    var donationId = _mpDonorService.CreateDonationAndDistributionRecord(donationAndDistribution, false);

                    check.DonationId = donationId;

                    _checkScannerDao.UpdateCheckStatus(check.Id, true);

                    batchDetails.Checks.Add(check);
                }
                catch (Exception e)
                {
                    check.Error = e.ToString();
                    check.AccountNumber = _mpDonorService.DecryptCheckValue(check.AccountNumber);
                    check.RoutingNumber = _mpDonorService.DecryptCheckValue(check.RoutingNumber);
                    batchDetails.ErrorChecks.Add(check);
                    _checkScannerDao.UpdateCheckStatus(check.Id, check.Exported, check.Error);
                }
            }

            batchDetails.Status = BatchStatus.Exported;
            _checkScannerDao.UpdateBatchStatus(batchDetails.Name, batchDetails.Status);

            return (batchDetails);
        }
        
        public EZScanDonorDetails GetContactDonorForCheck(string accountNumber, string routingNumber)
        {
            var account = _mpDonorService.DecryptCheckValue(accountNumber);
            var routing = _mpDonorService.DecryptCheckValue(routingNumber);
            var encryptedKey = _mpDonorService.CreateHashedAccountAndRoutingNumber(account, routing);
            return (Mapper.Map<MpContactDonor, EZScanDonorDetails>(_donorService.GetContactDonorForCheckAccount(encryptedKey)));
            
        }

        public MpContactDonor CreateDonor(CheckScannerCheck checkDetails)
        {
            MpContactDonor mpContactDonorById = null;
            // If scanned check has a donor id, try to use it to lookup the donor
            if (checkDetails.DonorId != null && checkDetails.DonorId > 0)
            {
                mpContactDonorById = _donorService.GetContactDonorForDonorId(checkDetails.DonorId.Value);
            }

            // Get the MpContactDonor and info based off of the account and routing number to see if we need to create a new one
            var contactDonorByAccount = _donorService.GetContactDonorForDonorAccount(checkDetails.AccountNumber, checkDetails.RoutingNumber) ?? new MpContactDonor();

            // if find by mpContact donor id is used then mpContact donor found by id matches mpContact donor 
            // found by account and account has stripe token
            if (mpContactDonorById != null && mpContactDonorById.ContactId == contactDonorByAccount.ContactId &&
                contactDonorByAccount.Account.HasPaymentProcessorInfo())
            {
                return contactDonorByAccount;
            }
            // if find by mpContact donor id is not used then mpContact donor 
            // found by account has stripe token
            else if (mpContactDonorById == null && contactDonorByAccount.Account != null && contactDonorByAccount.Account.HasPaymentProcessorInfo())
            {
                return contactDonorByAccount;
            }

            var contactDonor = mpContactDonorById ?? contactDonorByAccount;

            var account = _mpDonorService.DecryptCheckValue(checkDetails.AccountNumber);
            var routing = _mpDonorService.DecryptCheckValue(checkDetails.RoutingNumber);

            var token = _paymentService.CreateToken(account, routing, checkDetails.Name1);
            var encryptedKey = _mpDonorService.CreateHashedAccountAndRoutingNumber(account, routing);

            contactDonor.Details = new MpContactDetails
            {
                DisplayName = checkDetails.Name1,
                Address = new MpPostalAddress
                {
                    Line1 = checkDetails.Address.Line1,
                    Line2 = checkDetails.Address.Line2,
                    City = checkDetails.Address.City,
                    State = checkDetails.Address.State,
                    PostalCode = checkDetails.Address.PostalCode
                }
            };

            var newDonor = _donorService.CreateOrUpdateContactDonor(contactDonor, string.Empty,string.Empty, string.Empty, string.Empty, null, DateTime.Now);

            newDonor.Account = new MpDonorAccount
            {
                AccountNumber = account,
                RoutingNumber = routing,
                Type = AccountType.Checking,
                EncryptedAccount = encryptedKey,
                Token = token.Id
            };

            return newDonor;
        }
    }
}