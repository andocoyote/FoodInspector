using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodInspector.KeyVaultProvider
{
    public static class KeyVaultSecretNames
    {
        public static string ServicePrincipalClientID { get; } = "FoodInspector-ServicePrincipal-ClientID";
        public static string ServicePrincipalTokenSecret { get; } = "FoodInspector-ServicePrincipal-TokenSecret";
        public static string ServicePrincipalTenantID { get; } = "FoodInspector-ServicePrincipal-TenantID";
        public static string ServicePrincipalAppIDURI { get; } = "FoodInspector-ServicePrincipal-AppIDURI";
        public static string FoodEstablishmentInspectionDataAppToken { get; } = "AppToken-King-County-Food-Establishment-Inspection-Data";
    }
}
