namespace Azunt.BackgroundCheckManagement.Models.Projections
{
    /// <summary>
    /// BackgroundCheckGraph 컬렉션을 래핑하는 그래프 목록 모델
    /// </summary>
    public class BackgroundCheckGraphList
    {
        public BackgroundCheckGraphList(IQueryable<BackgroundCheckGraph> backgroundchecks)
        {
            BackgroundChecks = backgroundchecks;
        }

        /// <summary>그래프 데이터 쿼리</summary>
        public IQueryable<BackgroundCheckGraph> BackgroundChecks { get; }
    }
}
