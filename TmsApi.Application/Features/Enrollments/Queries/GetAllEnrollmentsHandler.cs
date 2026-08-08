using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Queries;

public class GetAllEnrollmentsHandler(IEnrollmentService enrollmentService)
    : IRequestHandler<GetAllEnrollmentsQuery, IEnumerable<EnrollmentResponseDto>>
{
    public async Task<IEnumerable<EnrollmentResponseDto>> Handle(
        GetAllEnrollmentsQuery request,
        CancellationToken ct
    )
    {
        return await enrollmentService.GetAllAsync(ct);
    }
}
