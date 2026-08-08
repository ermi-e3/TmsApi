using MediatR;
using TmsApi.Application.Common.Exceptions;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Features.Courses.Commands.UpdateCourse;

// public sealed class UpdateCourseHandler(ICourseRepository repository, ICachedCourseService cache)
//     : IRequestHandler<UpdateCourseCommand, CourseResponseDto>
// {
//     public async Task<CourseResponseDto> Handle(UpdateCourseCommand request, CancellationToken ct)
//     {
//         var course =
//             await repository.GetByIdAsync(request.Id, ct)
//             ?? throw new NotFoundException($"Course '{request.Id}' was not found.");

//         if (!course.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase))
//         {
//             if (await repository.CodeExistsAsync(request.Code, ct))
//             {
//                 throw new InvalidOperationException(
//                     $"Course code '{request.Code}' already exists."
//                 );
//             }
//         }

//         course.Code = request.Code;
//         course.Title = request.Title;
//         course.MaxCapacity = request.MaxCapacity;

//         await repository.SaveChangesAsync(ct);

//         await cache.InvalidateCourseCacheAsync(ct);

//         return new CourseResponseDto(
//             course.Id,
//             course.Code,
//             course.Title,
//             course.MaxCapacity,
//             course.Enrollments.Count
//         );
//     }
// }

// public class UpdateCourseHandler(ICourseRepository repository, ICachedCourseService cachedService)
//     : IRequestHandler<UpdateCourseCommand, bool>
// {
//     public async Task<bool> Handle(UpdateCourseCommand command, CancellationToken ct)
//     {
//         var course =
//             await repository.GetByIdAsync(request.Id, ct)
//             ?? throw new NotFoundException($"Course '{request.Id}' was not found.");

//         course.Code = request.Code;
//         course.Title = request.Title;
//         course.MaxCapacity = request.MaxCapacity;

//         await repository.UpdateAsync(course, ct);
//         await repository.SaveChangesAsync(ct);

//         await cache.InvalidateCourseCacheAsync(ct);
//     }
// }

public class UpdateCourseHandler(ICourseService service, ICachedCourseService cachedService)
    : IRequestHandler<UpdateCourseCommand, bool>
{
    public async Task<bool> Handle(UpdateCourseCommand command, CancellationToken ct)
    {
        await service.UpdateAsync(command, ct);

        await cachedService.InvalidateCourseCacheAsync(ct);

        return true;
    }
}


// 1..50 | ForEach-Object -Parallel { Invoke-RestMethod http://localhost:5022/api/v2/courses | Out-Null } -ThrottleLimit 50


