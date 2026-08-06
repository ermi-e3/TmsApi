using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Commands;

public class ApproveEnrollmentHandler(IEnrollmentService enrollmentService)
    : IRequestHandler<ApproveEnrollmentCommand, bool>
{
    public async Task<bool> Handle(ApproveEnrollmentCommand request, CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdAsync(request.Id, ct);

        if (enrollment is null)
            return false;

        await enrollmentService.ApproveAsync(request.Id, ct);

        return true;
    }
}
