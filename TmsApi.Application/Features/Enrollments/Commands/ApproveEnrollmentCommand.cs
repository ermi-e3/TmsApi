using MediatR;

namespace TmsApi.Application.Enrollments.Commands;

public record ApproveEnrollmentCommand(int Id) : IRequest<bool>;
