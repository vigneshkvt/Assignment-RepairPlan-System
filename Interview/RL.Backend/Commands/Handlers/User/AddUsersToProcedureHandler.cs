using MediatR;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Commands.Handlers.User
{
    public class AddUsersToProcedureHandler : IRequestHandler<AddUserToProducer, ApiResponse<Unit>>
    {
        private readonly RLContext _context;

        public AddUsersToProcedureHandler(RLContext context)
        {
            _context = context;
        }



        public async Task<ApiResponse<Unit>> Handle(AddUserToProducer request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.UserId == null)
                    throw new ArgumentNullException(nameof(request.UserId));

                var existingAssignments = _context.UserProcedureAssignment.Where(x => x.ProcedureId == request.ProcedureId).ToList();

                if (existingAssignments.Any())
                {
                    _context.UserProcedureAssignment.RemoveRange(existingAssignments);
                    await _context.SaveChangesAsync();
                }

                foreach (var userId in request.UserId)
                {
                    var newProcedureAssignment = new UserProcedureAssignment
                    {
                        ProcedureId = request.ProcedureId,
                        UserId = userId,
                        CreateDate = DateTime.UtcNow,
                        UpdateDate = DateTime.UtcNow
                    };
                    _context.UserProcedureAssignment.Add(newProcedureAssignment);
                }

                await _context.SaveChangesAsync();

                return ApiResponse<Unit>.Succeed(new Unit());
            }
            catch (Exception e)
            {
                return ApiResponse<Unit>.Fail(e);
            }
        }
    }
}
