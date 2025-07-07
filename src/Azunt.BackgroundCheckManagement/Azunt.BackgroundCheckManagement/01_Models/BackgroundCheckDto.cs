using System;

namespace Azunt.BackgroundCheckManagement
{
    /// <summary>
    /// BackgroundCheck 엔터티의 요약 정보용 DTO 클래스입니다.
    /// 주로 목록 조회, API 응답, 그리드 출력 등에 사용됩니다.
    /// </summary>
    public class BackgroundCheckDto
    {
        public long Id { get; set; }

        /// <summary>백그라운드체크 고유 Id</summary>
        public string? BackgroundCheckId { get; set; }

        /// <summary>검사 상태</summary>
        public string? BackgroundStatus { get; set; }

        /// <summary>검사 완료 시각</summary>
        public DateTimeOffset? CompletedAt { get; set; }

        /// <summary>요청 생성자</summary>
        public string? CreatedBy { get; set; }

        /// <summary>요청 생성 시각</summary>
        public DateTimeOffset? CreatedAt { get; set; }

        /// <summary>파일 이름</summary>
        public string? FileName { get; set; }

        /// <summary>청구 코드 Id</summary>
        public string? BillCodeId { get; set; }

        /// <summary>검사 제공자</summary>
        public string? Provider { get; set; }

        /// <summary>검사 점수</summary>
        public string? Score { get; set; }

        /// <summary>검사 요청 상태</summary>
        public string? Status { get; set; }

        /// <summary>보고서 URL</summary>
        public string? ReportUrl { get; set; }
    }
}
