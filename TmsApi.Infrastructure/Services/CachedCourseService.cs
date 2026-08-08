// using Microsoft.Extensions.Caching.Hybrid;
// using Microsoft.Extensions.Logging;
// using TmsApi.Application.DTOs;
// using TmsApi.Application.Interfaces;
// using TmsApi.Infrastructure.Caching;
// namespace TmsApi.Infrastructure.Services;
// public class CachedCourseService(
// HybridCache cache,
// ICourseService service,
// ILogger<CachedCourseService> logger)
// : ICachedCourseService
// {
// public async Task<CourseResponseDto> GetCourseAsync(string code, CancellationToken ct)
// {
// var key = CacheKeys.Course(code);
// var dbHit = false;
// var dto = await cache.GetOrCreateAsync(
// key,
// (service, code),
// async (state, token) =>
// {
// dbHit = true;
// logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
// var course = await state.service.GetByCodeAsync(state.code, token)
// ?? throw new Exception($"Course {state.code} not found.");
// // ?? throw new NotFoundException($"Course {state.code} not found.");
// return new CourseResponseDto(
// course.Id, course.Title, course.Code,
// course.MaxCapacity, course.Enrollments.Count);
// },
// tags: [CacheKeys.CoursesTag],
// cancellationToken: ct);
// if (!dbHit)
// logger.LogInformation("Cache HIT for {Key}", key);
// return dto;
// }
// public async Task<List<CourseResponseDto>> GetAllCoursesAsync(CancellationToken ct)
// {
// var key = CacheKeys.CoursesAll;
// var dbHit = false;
// var list = await cache.GetOrCreateAsync(
// key,
// service,
// async (state, token) =>
// {
// dbHit = true;
// logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
// var courses = await state.GetAllAsync(token);
// return courses.Select(c => new CourseResponseDto(
// c.Id, c.Title, c.Code,
// c.MaxCapacity, c.Enrollments.Count)).ToList();
// },
// tags: [CacheKeys.CoursesTag],
// cancellationToken: ct);
// if (!dbHit)
// logger.LogInformation("Cache HIT for {Key}", key);
// return list;
// }
// public async Task InvalidateCourseCacheAsync(CancellationToken ct)
// {
// logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.
// CoursesTag);
// await cache.RemoveByTagAsync(CacheKeys.CoursesTag, ct);
// }
// }

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Common.Exceptions;

// using TmsApi.Application.Common.Exceptions;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CachedCourseService(
    HybridCache cache,
    ICourseService service,
    ILogger<CachedCourseService> logger
) : ICachedCourseService
{
    public async Task<CourseResponseDto> GetCourseAsync(string code, CancellationToken ct)
    {
        var key = CacheKeys.Course(code);
        var dbHit = false;

        var dto = await cache.GetOrCreateAsync(
            key,
            (service, code),
            async (state, token) =>
            {
                dbHit = true;

                logger.LogInformation("Cache MISS for {Key}. Fetching course from database.", key);

                var course =
                    await state.service.GetByCodeAsync(state.code, token)
                    ?? throw new NotFoundException($"Course '{state.code}' was not found.");

                return new CourseResponseDto(
                    course.Id,
                    course.Code,
                    course.Title,
                    course.MaxCapacity,
                    course.Enrollments.Count
                );
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct
        );

        if (!dbHit)
        {
            logger.LogInformation("Cache HIT for {Key}", key);
        }

        return dto;
    }

    public async Task<List<CourseResponseDto>> GetAllCoursesAsync(CancellationToken ct)
    {
        var key = CacheKeys.CoursesAll;
        var dbHit = false;

        var courses = await cache.GetOrCreateAsync(
            key,
            service,
            async (state, token) =>
            {
                dbHit = true;

                logger.LogInformation(
                    "Cache MISS for {Key}. Fetching all courses from database.",
                    key
                );

                var entities = await state.GetAllAsync(token);

                return entities
                    .Select(c => new CourseResponseDto(
                        c.Id,
                        c.Code,
                        c.Title,
                        c.MaxCapacity,
                        c.Enrollments.Count
                    ))
                    .ToList();
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct
        );

        if (!dbHit)
        {
            logger.LogInformation("Cache HIT for {Key}", key);
        }

        return courses;
    }

    public async Task<PagedResponse<CourseResponseDto>> GetPagedCoursesAsync(
        PagedRequest request,
        CancellationToken ct
    )
    {
        var key =
            $"v2:courses:"
            + $"page:{request.Page}:"
            + $"size:{request.PageSize}:"
            + $"search:{request.Search ?? "none"}:"
            + $"sort:{request.OrderBy}:"
            + $"desc:{request.Descending}";

        var dbHit = false;

        var result = await cache.GetOrCreateAsync(
            key,
            (service, request),
            async (state, token) =>
            {
                dbHit = true;

                logger.LogInformation(
                    "Cache MISS for {Key}. Fetching paged courses from database.",
                    key
                );

                return await state.service.GetCoursesAsync(state.request, token);
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct
        );

        if (!dbHit)
        {
            logger.LogInformation("Cache HIT for {Key}", key);
        }

        return result;
    }

    public async Task InvalidateCourseCacheAsync(CancellationToken ct)
    {
        logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.CoursesTag);

        await cache.RemoveByTagAsync(CacheKeys.CoursesTag, ct);
    }
}
