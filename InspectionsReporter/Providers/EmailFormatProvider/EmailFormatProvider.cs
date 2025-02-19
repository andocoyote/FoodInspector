using FoodInspectorModels;
using System.Text;

namespace InspectionsReporter.Providers.EmailFormatProvider
{
    public static class EmailFormatProvider
    {
        public static string? GenerateHtmlTable(EstablishmentRecommendations recommendations)
        {
            if (recommendations == null ||
                recommendations.Recommended == null ||
                recommendations.Unrecommended == null)
            {
                return null;
            }

            var sb = new StringBuilder();
            sb.Append("<html><head>");
            sb.Append("<style>");
            sb.Append("h2.recommended { background-color: green; color: white; padding: 10px; }");
            sb.Append("h2.unrecommended { background-color: red; color: white; padding: 10px; }");
            sb.Append("table { border-collapse: collapse; width: 100%; text-align: left; }");
            sb.Append("th, td { border: 1px solid black; padding: 8px; }");
            sb.Append("th { background-color: #f2f2f2; }");
            sb.Append("</style>");
            sb.Append("</head><body>");

            sb.Append("<h2 class='recommended'>Recommended</h2>");
            sb.Append(GenerateTable(recommendations.Recommended));

            sb.Append("<h2 class='unrecommended'>Unrecommended</h2>");
            sb.Append(GenerateTable(recommendations.Unrecommended));

            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string GenerateTable(List<InspectionRecordAggregated> inspections)
        {
            if (inspections == null || inspections.Count == 0)
                return "<p>No data available.</p>";

            var sb = new StringBuilder();
            sb.Append("<table border='1' style='border-collapse:collapse; width:100%; text-align:left;'>");
            sb.Append("<tr>");
            sb.Append("<th>Program Identifier</th><th>Inspection Date</th>");
            sb.Append("<th>Address</th><th>City</th><th>Inspection Type</th>");
            sb.Append("<th>Score</th><th>Result</th><th>Violations</th>");
            sb.Append("</tr>");

            foreach (var inspection in inspections)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{inspection.ProgramIdentifier}</td>");
                sb.Append($"<td>{inspection.InspectionDate.ToString()}</td>");
                sb.Append($"<td>{inspection.Address}</td>");
                sb.Append($"<td>{inspection.City}</td>");
                sb.Append($"<td>{inspection.InspectionType}</td>");
                sb.Append($"<td>{inspection.InspectionScore}</td>");
                sb.Append($"<td>{inspection.InspectionResult}</td>");
                sb.Append($"<td>{GenerateViolations(inspection.Violations)}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</table>");
            return sb.ToString();
        }

        private static string GenerateViolations(List<Violation> violations)
        {
            if (violations == null || violations.Count == 0)
                return "None";

            var sb = new StringBuilder();
            sb.Append("<ul>");
            foreach (var violation in violations)
            {
                sb.Append($"<li>{violation.ViolationType}: {violation.ViolationDescription} (Points: {violation.ViolationPoints})</li>");
            }
            sb.Append("</ul>");
            return sb.ToString();
        }
    }
}
