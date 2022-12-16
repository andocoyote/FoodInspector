CREATE PROCEDURE [dbo].[CreateTable_InspectionData]
AS
BEGIN
SET NOCOUNT ON

CREATE TABLE InspectionData (
    Name varchar(255),
    Program_Identifier varchar(255),
    Inspection_Date date,
    Description varchar(255),
    Address varchar(255),
    City varchar(255),
    Zip_Code varchar(255),
    Phone varchar(255),
    Longitude float,
    Latitude float,
    Inspection_Business_Name varchar(255),
    Inspection_Type varchar(255),
    Inspection_Score smallint,
    Inspection_Result varchar(255),
    Inspection_Closed_Business bit,
    Violation_Points smallint,
    Business_Id varchar(255),
    Inspection_Serial_Num varchar(255),
    Grade varchar(255)
);

END