using FoodInspectorModels;

namespace CommonFunctionality.CosmosDbProvider
{
    public class CosmosDbWriteDocument : InspectionRecordAggregated
    {
        public CosmosDbWriteDocument() { }
        public CosmosDbWriteDocument(InspectionRecordAggregated inspectionRecordAggregated)
        {
            this.ProgramIdentifier = inspectionRecordAggregated.ProgramIdentifier;
            this.Name = inspectionRecordAggregated.Name;
            this.InspectionDate = inspectionRecordAggregated.InspectionDate;
            this.Description = inspectionRecordAggregated.Description;
            this.Address = inspectionRecordAggregated.Address;
            this.City = inspectionRecordAggregated.City;
            this.ZipCode = inspectionRecordAggregated.ZipCode;
            this.InspectionBusinessName = inspectionRecordAggregated.InspectionBusinessName;
            this.InspectionType = inspectionRecordAggregated.InspectionType;
            this.InspectionScore = inspectionRecordAggregated.InspectionScore;
            this.InspectionResult = inspectionRecordAggregated.InspectionResult;
            this.InspectionClosedBusiness = inspectionRecordAggregated.InspectionClosedBusiness;
            this.Violations = inspectionRecordAggregated.Violations;
            this.InspectionSerialNum = inspectionRecordAggregated.InspectionSerialNum;
        }

        public string id { get; set; } = string.Empty;
    }
}
