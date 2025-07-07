using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Azunt.BackgroundCheckManagement.Models.Configurations
{
    /// <summary>
    /// BackgroundCheck 엔터티와 BackgroundChecks 테이블 간 매핑 설정 클래스
    /// </summary>
    public class BackgroundCheckConfiguration : IEntityTypeConfiguration<BackgroundCheck>
    {
        public void Configure(EntityTypeBuilder<BackgroundCheck> builder)
        {
            // 매핑할 테이블명 지정
            builder.ToTable("BackgroundChecks");

            // 기본 키 및 IDENTITY 설정
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                   .ValueGeneratedOnAdd();

            // 기본값: CreatedAt = 현재 시각
            builder.Property(e => e.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            // CreatedBy: 최대 길이 제한
            builder.Property(e => e.CreatedBy)
                   .HasMaxLength(70);

            // FileName: (예시로 255 지정, 필요시 조정 가능)
            builder.Property(e => e.FileName)
                   .HasMaxLength(255);

            // BackgroundCheckId, PackageId, BillCodeId, Provider, ReportUrl, Score, Status
            // NVARCHAR(MAX) 필드는 명시적으로 설정하지 않아도 됨

            // 기타 설정이 필요한 필드가 생기면 여기에 추가
        }
    }
}
