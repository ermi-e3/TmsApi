// using MediatR;
// using TmsApi.Application.DTOs;

// namespace TmsApi.Application.Features.Courses.Commands.UpdateCourse;

// public sealed record UpdateCourseCommand(int Id, string Code, string Title, int MaxCapacity)
//     : IRequest<CourseResponseDto>;

using MediatR;

namespace TmsApi.Application.Features.Courses.Commands.UpdateCourse;

public sealed record UpdateCourseCommand(int Id, string Code, string Title, int MaxCapacity)
    : IRequest<bool>;
