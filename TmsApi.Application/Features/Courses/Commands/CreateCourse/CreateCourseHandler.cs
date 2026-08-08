using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Features.Courses.Commands.CreateCourse;

public sealed class CreateCourseHandler(ICourseRepository repository, ICachedCourseService cache)
    : IRequestHandler<CreateCourseCommand, CourseResponseDto>
{
    public async Task<CourseResponseDto> Handle(CreateCourseCommand request, CancellationToken ct)
    {
        if (await repository.CodeExistsAsync(request.Code, ct))
        {
            throw new InvalidOperationException($"Course '{request.Code}' already exists.");
        }

        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity,
        };

        await repository.AddAsync(course, ct);

        await repository.SaveChangesAsync(ct);

        await cache.InvalidateCourseCacheAsync(ct);

        return new CourseResponseDto(course.Id, course.Code, course.Title, course.MaxCapacity, 0);
    }
}
