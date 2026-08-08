using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Features.Courses.Queries.GetCourses;

public sealed record GetCoursesQuery(PagedRequest Request)
    : IRequest<PagedResponse<CourseResponseDto>>;
