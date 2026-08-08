using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Features.Courses.Queries.SearchCourses;

public sealed record SearchCoursesQuery(string? Term) : IRequest<IReadOnlyList<CourseResponseDto>>;
