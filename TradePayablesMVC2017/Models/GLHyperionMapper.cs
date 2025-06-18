using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // For ReadOnlyDictionary

namespace TradePayablesMVC2017.Models
{

    public static class GLHyperionMapper
    {
        public class GLAccountMapping
        {
            public string HyperionCode { get; }
            public string DueStatus { get; }
            public string BilledStatus { get; }

            public GLAccountMapping(string hyperionCode, string dueStatus, string billedStatus)
            {
                HyperionCode = hyperionCode;
                DueStatus = dueStatus;
                BilledStatus = billedStatus;
            }
        }

        // The static read-only dictionary
        public static ReadOnlyDictionary<string, GLAccountMapping> GLCodeHyperionMap { get; }

        // Static constructor for initialization
        static GLHyperionMapper()
        {
            var tempMap = new Dictionary<string, GLAccountMapping>
        {
            // Example Mappings:
            // G/L Account  , New GLAccountMapping(Hyperion Code, Due Status, Billed Status)
            { "14012", new GLAccountMapping("2D190100", "NOT DUE", "UNBILLED") },
            { "14005", new GLAccountMapping("2D190100", "DUE", "UNBILLED") },
            { "14006", new GLAccountMapping("2D190100", "DUE", "UNBILLED") },
            { "14705", new GLAccountMapping("N/A", "DUE", "BILLED") },
            { "14021", new GLAccountMapping("N/A", "NOT DUE", "UNBILLED") },
            { "14711", new GLAccountMapping("2D190300", "DUE", "BILLED") },
            { "14701", new GLAccountMapping("2D190300", "DUE", "BILLED") },
            { "14703", new GLAccountMapping("2D190300", "NOT DUE", "BILLED") },
            { "14712", new GLAccountMapping("2D190300", "DUE", "BILLED") },
            { "14704", new GLAccountMapping("2D190300", "NOT DUE", "BILLED") },
            { "22006", new GLAccountMapping("2D190100", "DUE", "BILLED") },
            { "14708", new GLAccountMapping("2D190300", "NOT DUE", "BILLED") },
            { "14722", new GLAccountMapping("2D190300", "DUE", "BILLED") },
            { "23051", new GLAccountMapping("N/A", "DUE", "BILLED") },
            { "23057", new GLAccountMapping("N/A", "DUE", "BILLED") },
            { "15715", new GLAccountMapping("2D190200", "DUE", "BILLED") },
            { "14724", new GLAccountMapping("N/A", "DUE", "BILLED") },
            { "14702", new GLAccountMapping("2D190300", "NOT DUE", "BILLED") },
            { "14721", new GLAccountMapping("2D190300", "DUE", "BILLED") },
            { "14622", new GLAccountMapping("2D190200", "DUE", "BILLED") },
            { "23141", new GLAccountMapping("N/A", "DUE", "BILLED") },
            { "14620", new GLAccountMapping("2D190200", "DUE", "BILLED") },
            { "14621", new GLAccountMapping("2D190200", "DUE", "BILLED") },
            { "14624", new GLAccountMapping("2D190200", "DUE", "BILLED") },
            { "22113", new GLAccountMapping("2D190300", "DUE", "BILLED") },
            { "23059", new GLAccountMapping("N/A", "DUE", "BILLED") },
            { "23054", new GLAccountMapping("2D190300", "DUE", "BILLED") },
            { "14007", new GLAccountMapping("N/A", "DUE", "UNBILLED") },
            { "14611", new GLAccountMapping("2D190200", "DUE", "BILLED") },
            { "14710", new GLAccountMapping("N/A", "DUE", "BILLED") },
            { "14618", new GLAccountMapping("2D190200", "DUE", "BILLED") }
            // Add all your GL code mappings here
        };

            GLCodeHyperionMap = new ReadOnlyDictionary<string, GLAccountMapping>(tempMap);
        }

        // Method to retrieve the mapping details
        public static GLAccountMapping GetMapping(string glAccount)
        {
            GLAccountMapping defaultReturnValue = new GLAccountMapping("N/A", "N/A", "N/A");
            if (GLCodeHyperionMap.TryGetValue(glAccount, out GLAccountMapping mapping))
            {
                return mapping;
            }
            return defaultReturnValue; // Return null if not found, or throw a KeyNotFoundException, or return a default/empty mapping object
        }


        public static GLAccountMapping ProcessGlAccount(string glAccount)
        {
            GLAccountMapping mapping = GetMapping(glAccount);
            return mapping;
        }

    }
}