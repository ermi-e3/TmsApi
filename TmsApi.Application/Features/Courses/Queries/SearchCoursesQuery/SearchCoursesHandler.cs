using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Features.Courses.Queries.SearchCourses;

public sealed class SearchCoursesHandler(ICourseRepository repository)
    : IRequestHandler<SearchCoursesQuery, IReadOnlyList<CourseResponseDto>>
{
    public async Task<IReadOnlyList<CourseResponseDto>> Handle(
        SearchCoursesQuery query,
        CancellationToken ct
    )
    {
        var courses = await repository.SearchAsync(query.Term, ct);

        return courses
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count
            ))
            .ToList();
    }
}
