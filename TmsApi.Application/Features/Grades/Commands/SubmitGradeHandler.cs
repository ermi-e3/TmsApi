// using MediatR;
// using TmsApi.Application.Interfaces;
// using TmsApi.Domain.Entities;
// using TmsApi.Infrastructure.Persistence.Repositories;

// namespace TmsApi.Application.Features.Grades.Commands;

// public class SubmitGradeCommandHandler(
//     IGradeRepository gradeRepository,
//     TmsDbContext db)
//     : IRequestHandler<SubmitGradeCommand, SubmitGradeResponse>
// {
//     public async Task<SubmitGradeResponse> Handle(
//         SubmitGradeCommand command,
//         CancellationToken ct)
//     {
//         var assessment = await db.Assessments
//             .FirstOrDefaultAsync(
//                 a => a.Id == command.AssessmentId,
//                 ct);

//         if (assessment is null)
//             throw new KeyNotFoundException(
//                 $"Assessment {command.AssessmentId} was not found.");

//         if (command.Score < 0 || command.Score > assessment.MaxScore)
//             throw new ArgumentException(
//                 $"Score must be between 0 and {assessment.MaxScore}.");

//         var studentExists = await db.Students
//             .AnyAsync(s => s.Id == command.StudentId, ct);

//         if (!studentExists)
//             throw new KeyNotFoundException(
//                 $"Student {command.StudentId} was not found.");

//         var existing = await gradeRepository
//             .GetByStudentAndAssessmentAsync(
//                 command.StudentId,
//                 command.AssessmentId,
//                 ct);

//         if (existing is not null)
//             throw new InvalidOperationException(
//                 "This student already has a grade for this assessment.");

//         var grade = new Grade
//         {
//             StudentId = command.StudentId,
//             AssessmentId = command.AssessmentId,
//             Score = command.Score,
//             GradedAt = DateTime.UtcNow
//         };

//         await gradeRepository.AddAsync(grade, ct);

//         return new SubmitGradeResponse(
//             grade.Id,
//             true);
//     }
// }

using MediatR;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Features.Grades.Commands;

public class SubmitGradeCommandHandler(
    IGradeRepository gradeRepository,
    IAssessmentRepository assessmentRepository,
    IStudentRepository studentRepository
) : IRequestHandler<SubmitGradeCommand, SubmitGradeResponse>
{
    public async Task<SubmitGradeResponse> Handle(SubmitGradeCommand command, CancellationToken ct)
    {
        var assessment = await assessmentRepository.GetByIdAsync(command.AssessmentId, ct);

        if (assessment is null)
            throw new KeyNotFoundException($"Assessment {command.AssessmentId} was not found.");

        if (command.Score < 0 || command.Score > assessment.MaxScore)
        {
            throw new ArgumentException($"Score must be between 0 and {assessment.MaxScore}.");
        }

        var studentExists = await studentRepository.ExistsAsync(command.StudentId, ct);

        if (!studentExists)
            throw new KeyNotFoundException($"Student {command.StudentId} was not found.");

        var existing = await gradeRepository.GetByStudentAndAssessmentAsync(
            command.StudentId,
            command.AssessmentId,
            ct
        );

        if (existing is not null)
            throw new InvalidOperationException(
                "This student already has a grade for this assessment."
            );

        var grade = new Grade
        {
            StudentId = command.StudentId,
            AssessmentId = command.AssessmentId,
            Score = command.Score,
            GradedAt = DateTime.UtcNow,
        };

        await gradeRepository.AddAsync(grade, ct);

        return new SubmitGradeResponse(grade.Id, true);
    }
}
