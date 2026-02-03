-- [0][0] 백그라운드체크: BackgroundChecks
CREATE TABLE [dbo].[BackgroundChecks] (
    -- 기본 키
    [ID]                BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,

    -- 활성 여부
    [Active]            BIT NULL DEFAULT (1),

    -- 외부 식별자 / 코드
    [BackgroundCheckID] NVARCHAR(MAX) NULL,
    [InvestigationID]   BIGINT NULL,
    [PackageID]         NVARCHAR(MAX) NULL,
    [BillCodeID]        NVARCHAR(MAX) NULL,

    -- 상태 / 점수
    [BackgroundStatus]  NVARCHAR(MAX) NULL,
    [Status]            NVARCHAR(MAX) NULL,
    [Score]             NVARCHAR(MAX) NULL,

    -- 파일 / 리포트 정보
    [FileName]          NVARCHAR(MAX) NULL,
    [ReportURL]         NVARCHAR(MAX) NULL,
    [Provider]          NVARCHAR(MAX) NULL,

    -- 관계 키
    [EmployeeID]        BIGINT NULL,
    [VendorID]          BIGINT NULL,

    -- 감사(로그) 정보
    [CreatedAt]         DATETIMEOFFSET(7) NULL,
    [CreatedBy]         NVARCHAR(70) NULL,
    [CompletedAt]       DATETIMEOFFSET(7) NULL,
    [UpdatedAt]         DATETIMEOFFSET(7) NULL
);
