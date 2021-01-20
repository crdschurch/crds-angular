using System;
using System.Collections.Generic;
using System.IO;
using CrossroadsStripeOnboarding.Models;
using CrossroadsStripeOnboarding.Models.Json;
using Newtonsoft.Json;

namespace CrossroadsStripeOnboarding.Services
{
    public class LoadExportFile
    {
        private static readonly Random Random = new Random();
        public static KeyValuePair<Messages, StripeJsonExport> ReadFile(string file)
        {
            var msg = ValidateIsFile(file);
            return msg == Messages.FileFound ? LoadFileToJson(file) : new KeyValuePair<Messages, StripeJsonExport>(msg, null);
        }

        private static Messages ValidateIsFile(string file)
        {
            if (file == null || file.Trim().Length == 0)
            {
                return Messages.FileNameRequired;
            }

            if (file.Equals("S"))
            {
                return Messages.SkipImportProcess;
            }

            if (file.Equals("X"))
            {
                return Messages.Close;
            }

            return File.Exists(file) ? Messages.FileFound : Messages.FileNotFound;
        }

        private static KeyValuePair<Messages, StripeJsonExport> LoadFileToJson(string file)
        {
            var json = JsonConvert.DeserializeObject<Dictionary<string, StripeJsonCustomer>>(File.ReadAllText(@file));
            return new KeyValuePair<Messages, StripeJsonExport>(Messages.ReadFileSuccess, new StripeJsonExport(json)); 
        }

        public static KeyValuePair<Messages, StripeJsonExport> ImportFile(StripeJsonExport jsonExport)
        {
            using (var db = new StripeOnboardingContext())
            {
                DumbDatabaseContent(db);
                ImportCustomerAndAccounts(db, jsonExport.CustomersMap);
            }

            return new KeyValuePair<Messages, StripeJsonExport>(Messages.ImportFileSuccess, jsonExport);
        }

        private static void DumbDatabaseContent(StripeOnboardingContext db)
        {
            db.Database.ExecuteSqlCommand("DELETE FROM [StripeAccounts]; DELETE FROM [StripeCustomers];");
        }

        private static void ImportCustomerAndAccounts(StripeOnboardingContext db, Dictionary<string, StripeJsonCustomer> customersMap)
        {
            foreach (var customerDetails in customersMap)
            {
                var customer = ImportCustomer(db, customerDetails.Value);
                ImportAccounts(customerDetails.Value, customer);
                db.SaveChanges();
            }
        }

        private static StripeCustomer ImportCustomer(StripeOnboardingContext db, StripeJsonCustomer customerDetails)
        {
            var customer = new StripeCustomer
            {
                CustomerId = customerDetails.NewCustomerId,
                ExternalPersonId = customerDetails.OldCustomerId,
                Imported = false,
            };


            db.StripeCustomers.Add(customer);
            return customer;
        }

        private static void ImportAccounts(StripeJsonCustomer customerDetails, StripeCustomer customer)
        {
            if (customerDetails.BanksMap != null)
            {
                foreach (var bankDetails in customerDetails.BanksMap)
                {
                    ImportAccount(bankDetails.Key, bankDetails.Value, customer);
                }
            }

            if (customerDetails.CardsMap != null)
            {
                foreach (var cardDetails in customerDetails.CardsMap)
                {
                    ImportAccount(cardDetails.Key, cardDetails.Value, customer);
                }
            }
        }

        private static void ImportAccount(string oldAccountId, StripeJsonAccount accountDetails, StripeCustomer customer)
        {
            var account = new StripeAccount
            {
                Type = accountDetails.Type,
                OldCardId = oldAccountId,
                NewCardId = accountDetails.NewAccountId,
                Fingerprint = accountDetails.Fingerprint,
                Last4 = accountDetails.Last4,
                ExpMonth = accountDetails.ExpMonth,
                ExpYear = accountDetails.ExpYear,
                Institution = accountDetails.Institution,
            };
            
            customer.StripeAccounts.Add(account);
        }
    }
}
