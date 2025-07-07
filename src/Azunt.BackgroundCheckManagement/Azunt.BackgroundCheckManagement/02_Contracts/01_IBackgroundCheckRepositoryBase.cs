using Azunt.Repositories;

namespace Azunt.BackgroundCheckManagement;

/// <summary>
/// 기본 CRUD 작업을 위한 BackgroundCheck 전용 저장소 인터페이스
/// </summary>
public interface IBackgroundCheckRepositoryBase : IRepositoryBase<BackgroundCheck, long>
{
}