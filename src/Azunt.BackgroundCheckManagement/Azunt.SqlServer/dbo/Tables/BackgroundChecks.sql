-- [0][0] 백그라운드체크: BackgroundChecks 
CREATE TABLE [dbo].[BackgroundChecks] (
    [ID]                BIGINT IDENTITY(1,1) NOT NULL,
    [Active]            BIT NULL CONSTRAINT [DF_BackgroundChecks_Active] DEFAULT ((1)),
    [BackgroundCheckID] NVARCHAR(MAX) NULL,
    [BackgroundStatus]  NVARCHAR(MAX) NULL,
    [CompletedAt]       DATETIMEOFFSET(7) NULL,
    [CreatedAt]         DATETIMEOFFSET(7) NULL,
    [CreatedBy]         NVARCHAR(70) NULL,
    [EmployeeID]        BIGINT NULL,
    [FileName]          NVARCHAR(MAX) NULL,
    [InvestigationID]   BIGINT NULL,
    [PackageID]         NVARCHAR(MAX) NULL,
    [BillCodeID]        NVARCHAR(MAX) NULL,
    [Provider]          NVARCHAR(MAX) NULL,
    [ReportURL]         NVARCHAR(MAX) NULL,
    [Score]             NVARCHAR(MAX) NULL,
    [Status]            NVARCHAR(MAX) NULL,
    [UpdatedAt]         DATETIMEOFFSET(7) NULL,
    [VendorID]          BIGINT NULL,
    CONSTRAINT [PK_BackgroundChecks] PRIMARY KEY CLUSTERED ([ID] ASC)
);
