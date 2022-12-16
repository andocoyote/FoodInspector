CREATE PROCEDURE [dbo].[Insert_InspectionData]
	@Name varchar(255),
    @Program_Identifier varchar(255),
    @Inspection_Date date,
    @Description varchar(255),
    @Address varchar(255),
    @City varchar(255),
    @Zip_Code varchar(255),
    @Phone varchar(255),
    @Longitude float,
    @Latitude float,
    @Inspection_Business_Name varchar(255),
    @Inspection_Type varchar(255),
    @Inspection_Score smallint,
    @Inspection_Result varchar(255),
    @Inspection_Closed_Business bit,
    @Violation_Points smallint,
    @Business_Id varchar(255),
    @Inspection_Serial_Num varchar(255),
    @Grade varchar(255)
AS
BEGIN
	INSERT INTO InspectionData
	(
		Name,
		Program_Identifier,
		Inspection_Date,
		Description,
		Address,
		City,
		Zip_Code,
		Phone,
		Longitude,
		Latitude,
		Inspection_Business_Name,
		Inspection_Type,
		Inspection_Score,
		Inspection_Result,
		Inspection_Closed_Business,
		Violation_Points,
		Business_Id,
		Inspection_Serial_Num,
		Grade
	)
	VALUES
	(
		@Name,
		@Program_Identifier,
		@Inspection_Date,
		@Description,
		@Address,
		@City,
		@Zip_Code,
		@Phone,
		@Longitude,
		@Latitude,
		@Inspection_Business_Name,
		@Inspection_Type,
		@Inspection_Score,
		@Inspection_Result,
		@Inspection_Closed_Business,
		@Violation_Points,
		@Business_Id,
		@Inspection_Serial_Num,
		@Grade
	)
END