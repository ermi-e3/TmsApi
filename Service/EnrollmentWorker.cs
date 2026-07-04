using Microsoft.Extensions.DependencyInjection;

namespace TmsApi.Services;

public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
{
    // public async Task ProcessBatch()
    // {
    //     using var scope = scopeFactory.CreateScope();

    //     var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

    //     var enrollment = await enrollmentService.EnrollAsync("S-001", "CS-101");

    //     Console.WriteLine($"Created enrollment {enrollment.Id}");
    // }
}
