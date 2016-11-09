﻿using System;
using System.Collections.Generic;
using System.Linq;
using MinistryPlatform.Translation.Models.Payments;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IProductRepository _productRepository;

        public InvoiceRepository(IMinistryPlatformRestRepository ministryPlatformRest, IApiUserRepository apiUserRepository, IProductRepository productRepository)
        {
            _ministryPlatformRest = ministryPlatformRest;
            _apiUserRepository = apiUserRepository;
            _productRepository = productRepository;
        }
        public bool InvoiceExists(int invoiceId)
        {
           return GetInvoice(invoiceId) != null;
        }

        public void SetInvoiceStatus(int invoiceId, int statusId)
        {
            var dict = new Dictionary<string, object> {{"Invoice_ID", invoiceId}, {"Invoice_Status_ID", statusId}};

            var update = new List<Dictionary<string, object>> {dict};

            var apiToken = _apiUserRepository.GetToken();
            _ministryPlatformRest.UsingAuthenticationToken(apiToken).Put("Invoices",update);
        }

        public MpInvoice GetInvoice(int invoiceId)
        {
            var apiToken = _apiUserRepository.GetToken();
            return _ministryPlatformRest.UsingAuthenticationToken(apiToken).Get<MpInvoice>(invoiceId);
        }

        public MpInvoiceDetail GetInvoiceDetailForInvoice(int invoiceId)
        {
            var filter = new Dictionary<string, object> {{"Invoice_ID", invoiceId}};

            var apiToken = _apiUserRepository.GetToken();
            return _ministryPlatformRest.UsingAuthenticationToken(apiToken).Get<MpInvoiceDetail>("Invoice_Detail", filter).FirstOrDefault();
        }

        public bool CreateInvoiceAndDetail(int productId, int? productOptionPriceId, int purchaserContactId, int recipientContactId)
        {
            var product = _productRepository.GetProduct(productId);
            var productOptionPrice = productOptionPriceId != null ?_productRepository.GetProductOptionPrice((int)productOptionPriceId).OptionPrice : 0;

            var invoice = new MpNestedInvoiceDetail
            {
                Invoice = new MpInvoice()
                {
                    InvoiceDate = DateTime.Now,
                    InvoiceTotal = product.BasePrice + productOptionPrice,
                    PurchaserContactId = purchaserContactId,
                    InvoiceStatusId = 1
                },
                ProductId = productId,
                Quantity = 1,
                ProductOptionPriceId = productOptionPriceId,
                LineTotal = product.BasePrice + productOptionPrice,
                RecipientContactId = recipientContactId
            };
            var apiToken = _apiUserRepository.GetToken();
            return _ministryPlatformRest.UsingAuthenticationToken(apiToken).Post(new List<MpNestedInvoiceDetail>(new List<MpNestedInvoiceDetail> { invoice })) == 200;
        }
    }
}
