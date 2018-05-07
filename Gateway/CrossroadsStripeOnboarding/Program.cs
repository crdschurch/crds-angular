using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using Crossroads.Utilities.Services;
using CrossroadsStripeOnboarding.Models;
using CrossroadsStripeOnboarding.Models.Json;
using CrossroadsStripeOnboarding.Services;
using log4net;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Crossroads.Web.Common.Configuration;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace CrossroadsStripeOnboarding
{
    public class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

        public static void Main(string[] args)
        {
            var container = new UnityContainer();
            CrossroadsWebCommonConfig.Register(container);

            var unitySections = new[] { "unity" };

            foreach (var section in unitySections.Select(sectionName => (UnityConfigurationSection)ConfigurationManager.GetSection(sectionName)))
            {
                container.LoadConfiguration(section);
            }

            TlsHelper.AllowTls12();

            var program = container.Resolve<Program>();
            program.run(args);
        }

        private readonly StripePlansAndSubscriptions _stripePlansAndSubscriptions;
        private readonly VerifyStripeSubscriptions _verifyStripeSubscriptions;
        private readonly ListStripeSubscriptions _listStripeSubscriptions;
        private readonly FixSubscriptionCycle _fixSubscriptionCycle;

        public Program(StripePlansAndSubscriptions stripePlansAndSubscriptions, VerifyStripeSubscriptions verifyStripeSubscriptions, ListStripeSubscriptions listStripeSubscriptions, FixSubscriptionCycle fixSubscriptionCycle)
        {
            _stripePlansAndSubscriptions = stripePlansAndSubscriptions;
            _verifyStripeSubscriptions = verifyStripeSubscriptions;
            _listStripeSubscriptions = listStripeSubscriptions;
            _fixSubscriptionCycle = fixSubscriptionCycle;
        }

        public void run(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                logger.Error("Invalid Arguments.");
                logger.Error(options.GetUsage());
                Environment.Exit(1);
            }

            if (options.VerifyMode)
            {
                logger.Info("Running in verify mode");
                _verifyStripeSubscriptions.Verify();
                Environment.Exit(0);
            }
            else if (options.ListSubscriptionsFileName != null && !string.IsNullOrWhiteSpace(options.ListSubscriptionsFileName))
            {
                logger.Info("Running in List Subscriptions mode");
                _listStripeSubscriptions.ListSubscriptions(options.ListSubscriptionsFileName);
                Environment.Exit(0);
            }
            else if (options.FixMode)
            {
                logger.Info("Running in Fix Subscription Cycle mode");
                _fixSubscriptionCycle.Fix();
                Environment.Exit(0);
            }

            LoadAndImportFile();
            CreateStripePlansAndSubscriptions();
        }

        private void LoadAndImportFile()
        {
            var result = new KeyValuePair<Messages, StripeJsonExport>(Messages.NotRun, null);

            while (result.Key != Messages.ImportFileSuccess && result.Key != Messages.SkipImportProcess)
            {
                Console.WriteLine("Enter the exports file path/location to import.  Otherwise press S to skip this step or X to close the program: ");
                result = LoadExportFile.ReadFile(Console.ReadLine());

                switch (result.Key)
                {
                    case Messages.ReadFileSuccess:
                        result = LoadExportFile.ImportFile(result.Value);

                        if (result.Key == Messages.ImportFileSuccess)
                        {
                            Console.WriteLine("The file was processed successfully.");
                        }
                        break;
                    case Messages.FileNameRequired:
                    case Messages.FileNotFound:
                        Console.WriteLine("Please enter a valid file.");
                        break;
                    case Messages.FileContainsInvalidData:
                        Console.WriteLine("The file contains invalid data please investigate.");
                        break;
                    case Messages.Close:
                        Environment.Exit(0);
                        break;
                }
            }
        }

        private void CreateStripePlansAndSubscriptions()
        {
            var result = Messages.NotRun;

            while (result != Messages.PlansAndSubscriptionsSuccess)
            {
                Console.WriteLine("To create Stripe Plans and Subscritions press any key to continue or X to close the Program: ");
                var command = Console.ReadLine();

                if (command != null && command.Trim().Length != 0 && command.Equals("X"))
                {
                    Environment.Exit(0);
                }

                result = _stripePlansAndSubscriptions.generate();

                if (result != Messages.PlansAndSubscriptionsSuccess)
                {
                    Console.WriteLine("An error occured while creating stripe plans and subscriptions.");
                }
            }


            Console.WriteLine("The file was processed successfully. Press any key to close.");
            Console.ReadKey();
        }

        public class Options
        {
            [Option('V', "verify", Required = false, DefaultValue = false, MutuallyExclusiveSet = "OpMode",
              HelpText = "Execute in verification mode - by default will run in execute mode")]
            public bool VerifyMode { get; set; }

            [Option('L', "list", Required = false, DefaultValue = null, MutuallyExclusiveSet = "OpMode",
              HelpText = "Execute in List Subscriptions mode, specifying the filename - by default will run in execute mode")]
            public string ListSubscriptionsFileName { get; set; }

            [Option('F', "fix", Required = false, DefaultValue = null, MutuallyExclusiveSet = "OpMode",
              HelpText = "Execute in \"Fix Subscription Cycle\" mode, to make subscriptions bill on the correct day - by default will run in execute mode")]
            public bool FixMode { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }
    }
}