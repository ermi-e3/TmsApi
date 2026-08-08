using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Features.Courses.Commands.CreateCourse;

public sealed record CreateCourseCommand(string Code, string Title, int MaxCapacity)
    : IRequest<CourseResponseDto>;
