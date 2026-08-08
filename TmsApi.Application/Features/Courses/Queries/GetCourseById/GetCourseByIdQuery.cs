using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Features.Courses.Queries.GetCourseById;

public sealed record GetCourseByIdQuery(int Id)
    : IRequest<CourseResponseDto>;