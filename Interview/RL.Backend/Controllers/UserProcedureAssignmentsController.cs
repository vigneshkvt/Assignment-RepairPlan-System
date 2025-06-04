using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using RL.Backend.Commands;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace RL.Backend.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class UserProcedureAssignmentsController : Controller
    {
        private readonly ILogger<UsersController> _logger;
        private readonly RLContext _context;
        private readonly IMediator _mediator;

        public UserProcedureAssignmentsController(ILogger<UsersController> logger, RLContext context, IMediator mediator)
        {
            _logger = logger;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet("GetUsersForProcedure/{procedureId}")]
        [EnableQuery]
        public async Task<IActionResult> GetUsersForProcedure(int procedureId)
        {
            var userProcedureAssignments = await _context.UserProcedureAssignment
                .Where(c => c.ProcedureId == procedureId)
                .ToListAsync();

            if (!userProcedureAssignments.Any())
            {
                // Return empty list if no assignments found
                return Ok(Enumerable.Empty<User>());
            }

            var userIds = new List<User>();

            foreach (var assignment in userProcedureAssignments)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == assignment.UserId);

                if (user != null)
                {
                    userIds.Add(user);
                }
            }

            return Ok(userIds);
        }


        [HttpPost("AddRemoveUsersToPlan")]
        public async Task<IActionResult> AddRemoveUsersToPlan(AddUserToProducer command, CancellationToken token)
        {
            var response = await _mediator.Send(command, token);

            return response.ToActionResult();
        }

        [HttpDelete("RemoveAllUser")]
        public async Task<IActionResult> RemoveAllUser()
        {
            var planWithProcedures = await _context.Plans
                                    .OrderByDescending(c => c.PlanId)
                                    .Include(c => c.PlanProcedures)
                                    .ToListAsync();

            var planProcedure = planWithProcedures.FirstOrDefault();

            if (planProcedure == null)
            {
                return BadRequest("No procedure IDs found in PlanProcedures.");
            }

            var procedureIds = await _context.PlanProcedures
                .Where(c => c.PlanId == planProcedure.PlanId)
                .Select(c => c.ProcedureId)
                .ToListAsync();

            if (!procedureIds.Any())
            {
                return Ok(new
                {
                    Message = "No Users Selected",
                    DeletedCount = 0
                });
            }

            var assignmentsToDelete = await _context.UserProcedureAssignment
                .Where(x => procedureIds.Contains(x.ProcedureId))
                .ToListAsync();

            if (!assignmentsToDelete.Any())
            {
                return Ok(new
                {
                    Message = "No Users Selected",
                    DeletedCount = 0
                });
            }

            _context.UserProcedureAssignment.RemoveRange(assignmentsToDelete);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Users removed successfully.",
                DeletedCount = assignmentsToDelete.Count
            });
        }
    }
}
