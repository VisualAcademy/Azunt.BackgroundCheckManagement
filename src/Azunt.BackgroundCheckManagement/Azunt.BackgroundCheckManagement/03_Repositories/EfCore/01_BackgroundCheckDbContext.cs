using Azunt.BackgroundCheckManagement.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Azunt.BackgroundCheckManagement
{
    /// <summary>
    /// BackgroundCheck 전용 DbContext.
    /// BackgroundCheckConfiguration을 통해 테이블 및 속성 매핑을 구성합니다.
    /// </summary>
    public class BackgroundCheckDbContext : DbContext
    {
        public BackgroundCheckDbContext(DbContextOptions<BackgroundCheckDbContext> options)
            : base(options)
        {
            // 기본적으로 NoTracking 모드 설정 (조회 성능 향상)
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        /// <summary>
        /// 모델 매핑 설정 - 구성 클래스 적용
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BackgroundCheckConfiguration());
        }

        /// <summary>
        /// BackgroundChecks 테이블에 대한 DbSet
        /// </summary>
        public DbSet<BackgroundCheck> BackgroundChecks { get; set; } = null!;
    }
}
