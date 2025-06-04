using FluentValidation;
using FluentValidation.Results;
using MediatR;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Commands.Handlers.User
{
    public class AddUsersToProcedureHandler : IRequestHandler<AddUserToProducer, ApiResponse<Unit>>
    {
        private readonly RLContext _context;
        private readonly IValidator<AddUserToProducer> _validator;

        public AddUsersToProcedureHandler(RLContext context, IValidator<AddUserToProducer> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<ApiResponse<Unit>> Handle(AddUserToProducer request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate the request
                var validationResult = await ValidateRequestAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return ApiResponse<Unit>.Fail(new Exception(FormatValidationErrors(validationResult)));
                }

                // Remove existing assignments
                await RemoveExistingAssignmentsAsync(request.ProcedureId);

                // Add new assignments
                if (request.UserId == null || !request.UserId.Any())
                {
                    throw new ArgumentException("UserId cannot be null or empty.");
                }

                await AddNewAssignmentsAsync(request.UserId, request.ProcedureId);

                return ApiResponse<Unit>.Succeed(new Unit());
            }
            catch (Exception e)
            {
                return ApiResponse<Unit>.Fail(e);
            }
        }

        private async Task<ValidationResult> ValidateRequestAsync(AddUserToProducer request, CancellationToken cancellationToken)
        {
            return await _validator.ValidateAsync(request, cancellationToken);
        }

        private string FormatValidationErrors(ValidationResult validationResult)
        {
            return string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
        }

        private async Task RemoveExistingAssignmentsAsync(int procedureId)
        {
            var existingAssignments = _context.UserProcedureAssignment
                .Where(x => x.ProcedureId == procedureId)
                .ToList();

            if (existingAssignments.Any())
            {
                _context.UserProcedureAssignment.RemoveRange(existingAssignments);
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddNewAssignmentsAsync(IEnumerable<int> userIds, int procedureId)
        {
            foreach (var userId in userIds)
            {
                var newProcedureAssignment = new UserProcedureAssignment
                {
                    ProcedureId = procedureId,
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };
                _context.UserProcedureAssignment.Add(newProcedureAssignment);
            }

            await _context.SaveChangesAsync();
        }
    }
}