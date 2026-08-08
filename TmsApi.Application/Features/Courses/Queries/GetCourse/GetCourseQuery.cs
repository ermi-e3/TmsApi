using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Features.Courses.Queries.GetCourse;

public sealed record GetCourseQuery(string Code) : IRequest<CourseResponseDto>;
