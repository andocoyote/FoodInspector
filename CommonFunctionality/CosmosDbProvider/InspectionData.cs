using CommonFunctionality.CosmosDbProvider;

namespace CommonFunctionality.Model
{
    // This models the data retrieved from the inspection API and contains all of the available fields
    public class InspectionData : CosmosDbDocument
    {
        // Name: The name of the establishment
        public string Name { get; set; } = null;

        // Program_Identifier: Usually the name of the establishment
        public string Program_Identifier { get; set; } = null;

        // Inspection_Date: the date of the inspection
        public string Inspection_Date { get; set; } = null;

        // Description: identifies the risk category of the business
        // https://kingcounty.gov/depts/health/environmental-health/food-safety/inspection-system/food-safety-rating.aspx
        // There are three different risk categories within the General Food Service permit.
        //  Risk Category I businesses are primarily grab and go and do not prepare food on-site.
        //  Risk Category II: restaurants are moderate risk, they assemble food on-site but do not prepare food from scratch.
        //  Risk Category III: restaurants prepare food from scratch and are more complex, which puts them in a higher risk category.
        public string Description { get; set; } = null;

        // Address: the street address of the establishment
        public string Address { get; set; } = null;

        // City: the city of the establishment
        public string City { get; set; } = null;

        // Zip_Code: the zip code of the establishment
        public string Zip_Code { get; set; } = null;
        public string Phone { get; set; } = null;
        public string Longitude { get; set; } = null;
        public string Latitude { get; set; } = null;

        // Inspection_Business_Name: usually the name of the establishment
        public string Inspection_Business_Name { get; set; } = null;

        // Inspection_Type: the reason for the inspection
        //  Routine Inspection/Field Review
        //  Consultation/Education - Field
        //  Return Inspection
        public string Inspection_Type { get; set; } = null;

        // Inspection_Score: the sum of all violation points accrued during the inspection
        public string Inspection_Score { get; set; } = null;

        // Inspection_Result: the overall result of the inspection
        //  Most common: Satisfactory, Unsatisfactory, Complete (for Consultation/Education - Field)
        public string Inspection_Result { get; set; } = null;

        // Inspection_Closed_Business: true/false
        public string Inspection_Closed_Business { get; set; } = null;

        // Violation_Type: color coded violation type
        //  RED: violation is >= 10 points
        //  BLUE: violation is < 10 points
        public string Violation_Type { get; set; } = null;

        // Violation_Description: text description of the violation
        public string Violation_Description { get; set; } = null;

        // Violation_Points: the points for one specific violation
        // An establishment may have more than one violation listed during an inspection
        public string Violation_Points { get; set; } = null;

        // Business_Id: internal
        public string Business_Id { get; set; } = null;

        // Inspection_Serial_Num: internal
        public string Inspection_Serial_Num { get; set; } = null;

        // Violation_Record_ID: internal
        public string Violation_Record_ID { get; set; } = null;

        // Grade: not sure.
        // There doesn't seem to be any link between Unsatisfactory, RED, Inspection_Closed_Business, etc.
        public string Grade { get; set; } = null;

        // The inspector app is interested in:
        //  Inspection_Type == Routine Inspection/Field Review, Return Inspection
        //  where Inspection_Result == Satisfactory, Unsatisfactory
    }
}
