using MediatR;

namespace TmsApi.Application.Enrollments.Commands;

public record RejectEnrollmentCommand(int Id) : IRequest<bool>;
