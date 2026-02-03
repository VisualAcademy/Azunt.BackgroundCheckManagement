namespace Azunt.BackgroundCheckManagement.Models.Projections
{
    /// <summary>
    /// BackgroundCheck 상태별 집계 결과를 표현하는 그래프용 모델
    /// </summary>
    public class BackgroundCheckGraph
    {
        /// <summary>백그라운드체크 상태 (NULL 가능 — DB 컬럼이 NULL 허용이므로)</summary>
        public string? BackgroundCheckStatus { get; set; }

        /// <summary>해당 상태의 건수</summary>
        public long Count { get; set; }
    }
}
