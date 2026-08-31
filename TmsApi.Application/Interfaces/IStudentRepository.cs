namespace TmsApi.Application.Interfaces;

public interface IStudentRepository
{
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
