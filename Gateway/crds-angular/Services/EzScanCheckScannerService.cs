﻿using System;
using System.Collections.Generic;
using crds_angular.DataAccess.Interfaces;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Services.Interfaces;
using log4net;
using MinistryPlatform.Models;
using MPServices=MinistryPlatform.Translation.Services.Interfaces;

namespace crds_angular.Services
{
    public class EzScanCheckScannerService : ICheckScannerService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (EzScanCheckScannerService));

        private readonly ICheckScannerDao _checkScannerDao;
        private readonly IDonorService _donorService;
        private readonly IPaymentService _paymentService;
        private readonly MPServices.IDonorService _mpDonorService;

        public EzScanCheckScannerService(ICheckScannerDao checkScannerDao, IDonorService donorService, IPaymentService paymentService, MPServices.IDonorService mpDonorService)
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
                    var contactDonor = _donorService.GetContactDonorForDonorAccount(check.AccountNumber, check.RoutingNumber) ?? new ContactDonor();
                    if (!contactDonor.HasPaymentProcessorRecord)
                    {
                        var token = _paymentService.CreateToken(check.AccountNumber, check.RoutingNumber);
                        contactDonor.Details = new ContactDetails
                        {
                            DisplayName = check.Name1,
                            Address = new PostalAddress
                            {
                                Line1 = check.Address.Line1,
                                Line2 = check.Address.Line2,
                                City = check.Address.City,
                                State = check.Address.State,
                                PostalCode = check.Address.PostalCode
                            }
                        };
                        contactDonor.Account = new DonorAccount
                        {
                            AccountNumber = check.AccountNumber,
                            RoutingNumber = check.RoutingNumber,
                            Type = AccountType.Checking
                        };

                        contactDonor = _donorService.CreateOrUpdateContactDonor(contactDonor, string.Empty, token, DateTime.Now);
                    }

                    var charge = _paymentService.ChargeCustomer(contactDonor.ProcessorId, (int) (check.Amount), contactDonor.DonorId);
                    var fee = charge.BalanceTransaction != null ? charge.BalanceTransaction.Fee : null;

                    // Mark the check as exported now, so we don't double-charge a community member.
                    // If the CreateDonationAndDistributionRecord fails, we'll still consider it exported, but
                    // it will be in error, and will have to be manually resolved.
                    check.Exported = true;

                    var programId = batchDetails.ProgramId == null ? null : batchDetails.ProgramId + "";

                    var donationId = _mpDonorService.CreateDonationAndDistributionRecord((int) (check.Amount),
                                                                                         fee,
                                                                                         contactDonor.DonorId,
                                                                                         programId,
                                                                                         charge.Id,
                                                                                         "check",
                                                                                         contactDonor.ProcessorId,
                                                                                         check.CheckDate ?? (check.ScanDate ?? DateTime.Now),
                                                                                         contactDonor.RegisteredUser, batchDetails.Name);

                    check.DonationId = donationId;

                    _checkScannerDao.UpdateCheckStatus(check.Id, true);

                    batchDetails.Checks.Add(check);
                }
                catch (Exception e)
                {
                    check.Error = e.ToString();
                    batchDetails.ErrorChecks.Add(check);
                    _checkScannerDao.UpdateCheckStatus(check.Id, check.Exported, check.Error);
                }
            }

            batchDetails.Status = BatchStatus.Exported;
            _checkScannerDao.UpdateBatchStatus(batchDetails.Name, batchDetails.Status);

            return (batchDetails);
        }

        public ContactDetails GetContactDonorForCheck(string encryptedKey)
        {
            var contactDetails = _donorService.GetContactDonorForCheckAccount(encryptedKey);
            //return _mpDonorService.GetContactDonorForCheckAccount(encryptedKey); 
            //checkfor null
            return contactDetails;
        }

       
    }
}