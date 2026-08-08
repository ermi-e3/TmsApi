using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Features.Courses.Queries.GetCourses;

public sealed class GetCoursesHandler(ICachedCourseService cache)
    : IRequestHandler<GetCoursesQuery, PagedResponse<CourseResponseDto>>
{
    public async Task<PagedResponse<CourseResponseDto>> Handle(
        GetCoursesQuery request,
        CancellationToken ct
    )
    {
        return await cache.GetPagedCoursesAsync(request.Request, ct);
    }
}
