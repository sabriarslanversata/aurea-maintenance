using Aurea.Maintenance.Debugger.Common;
using CIS.Element.Billing;
using Csla.Validation;
using System;

namespace Aurea.Maintenance.Debugger.Spark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var clientConfiguration = Utility.SetSecurity(Utility.BillingAdminDEV, Utility.Clients["SPK"]);
            SimulateCreateProduct();
        }

        private static void SimulateCreateProduct()
        {
            // Create new product with factory
            Product product = Product.New();

            // Set properties with test values
            product.CSPId = 1;
            product.CSPName = "SPK";
            product.DivisionCode = "";
            product.LdcCode = null;
            product.ServiceType = "E";
            product.ConsumptionUnitID = null;
            product.CustomerType = "R";
            product.IsDefault = false;
            product.BeginDate = DateTime.Parse("2017-4-25");
            product.EndDate = DateTime.MinValue;
            product.ProductCode = "123";
            product.Description = "hgyg hbh hu h";
            product.PriceDescription = "sales tax";
            product.ProductName = "p name";
            product.PlanTypeId = 1;
            product.PlanType = "Fixed";
            product.RolloverProductId = 10859;
            product.RolloverProductName = "C_CAMB_E_D2D_FIXED_BROWN_NOMSF_ETF_12";
            product.CreditScoreThreshold = 89;
            product.MonthlyChargeId = 3;
            product.TDSPTemplateID = 10;

            // Clear up
            product.EDIRateClasses.Clear();
            product.ClearProductEmail();
            product.Promotions.Clear();


            // Validation checkpoint to simulate step-1
            if (!product.IsValid)
            {
                //See if one of the validation error is because of a duplicate product
                foreach (BrokenRule rule in product.BrokenRulesCollection)
                {
                    if (rule.Property == "ProductCode")
                    {
                        Console.WriteLine($"Validation error on {rule.Description}");
                    }
                }
            }

            


            // Save product
            product.Save();
        }
    }
}
