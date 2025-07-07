using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Azunt.BackgroundCheckManagement
{
    /// <summary>
    /// BackgroundChecks 테이블과 매핑되는 백그라운드체크 엔터티
    /// </summary>
    [Table("BackgroundChecks")]
    public class BackgroundCheck
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>활성 여부 (기본값: true)</summary>
        public bool? Active { get; set; }

        /// <summary>백그라운드체크 고유 Id</summary>
        public string? BackgroundCheckId { get; set; }

        /// <summary>검사 상태</summary>
        public string? BackgroundStatus { get; set; }

        /// <summary>검사 완료 시각</summary>
        public DateTimeOffset? CompletedAt { get; set; }

        /// <summary>요청 생성 시각</summary>
        public DateTimeOffset? CreatedAt { get; set; }

        /// <summary>요청 생성자</summary>
        [StringLength(70)]
        public string? CreatedBy { get; set; }

        /// <summary>직원 Id</summary>
        public long? EmployeeId { get; set; }

        /// <summary>파일 이름</summary>
        public string? FileName { get; set; }

        /// <summary>수사 Id</summary>
        public long? InvestigationId { get; set; }

        /// <summary>패키지 Id</summary>
        public string? PackageId { get; set; }

        /// <summary>청구 코드 Id</summary>
        public string? BillCodeId { get; set; }

        /// <summary>검사 제공자</summary>
        public string? Provider { get; set; }

        /// <summary>보고서 URL</summary>
        public string? ReportUrl { get; set; }

        /// <summary>검사 점수</summary>
        public string? Score { get; set; }

        /// <summary>검사 요청 상태</summary>
        public string? Status { get; set; }

        /// <summary>최종 업데이트 시각</summary>
        public DateTimeOffset? UpdatedAt { get; set; }

        /// <summary>벤더 Id</summary>
        public long? VendorId { get; set; }
    }
}
