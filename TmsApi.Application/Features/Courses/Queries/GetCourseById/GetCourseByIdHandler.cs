using MediatR;
using TmsApi.Application.Common.Exceptions;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Features.Courses.Queries.GetCourseById;

public sealed class GetCourseByIdHandler(ICourseRepository repository)
    : IRequestHandler<GetCourseByIdQuery, CourseResponseDto>
{
    public async Task<CourseResponseDto> Handle(GetCourseByIdQuery request, CancellationToken ct)
    {
        return await repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Course '{request.Id}' was not found.");
    }
}
