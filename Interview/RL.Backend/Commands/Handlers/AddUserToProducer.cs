using MediatR;
using RL.Backend.Models;

namespace RL.Backend.Commands
{
    public class AddUserToProducer : IRequest<ApiResponse<Unit>>
    {
        public List<int>? UserId { get; set; }
        public int ProcedureId { get; set; }
    }
}
