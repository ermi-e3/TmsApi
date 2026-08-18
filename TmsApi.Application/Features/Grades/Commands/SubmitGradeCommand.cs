using MediatR;

namespace TmsApi.Application.Features.Grades.Commands;

public record SubmitGradeCommand(
    int StudentId,
    int AssessmentId,
    decimal Score
) : IRequest<SubmitGradeResponse>;

public record SubmitGradeResponse(
    int Id,
    bool Success
);