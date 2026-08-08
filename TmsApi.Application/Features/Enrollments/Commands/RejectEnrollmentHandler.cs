using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Commands;

public class RejectEnrollmentHandler(IEnrollmentService enrollmentService)
    : IRequestHandler<RejectEnrollmentCommand, bool>
{
    public async Task<bool> Handle(RejectEnrollmentCommand request, CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdAsync(request.Id, ct);

        if (enrollment is null)
            return false;

        await enrollmentService.RejectAsync(request.Id, ct);

        return true;
    }
}
