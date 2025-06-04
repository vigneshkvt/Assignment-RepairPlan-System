using FluentValidation;
using RL.Backend.Commands;

namespace RL.Backend.Validators
{
    public class AddUserToProducerValidator : AbstractValidator<AddUserToProducer>
    {
        public AddUserToProducerValidator()
        {
            RuleFor(x => x.ProcedureId)
                .NotNull()
                .WithMessage("ProcedureId cannot be null.")
                .GreaterThan(0)
                .WithMessage("Please provide a valid ProcedureId ");

            RuleFor(x => x.UserId)
                .NotNull()
                .WithMessage("UserId list cannot be null.")
                .Must(userIds => userIds != null && userIds.Any())
                .WithMessage("UserId list must contain at least one user.");
        }
    }
}