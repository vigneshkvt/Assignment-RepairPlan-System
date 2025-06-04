using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using RL.Backend.Commands;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;
using System.Data.Entity;

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
        public IActionResult Get(int procedureId)
        {
            var userIDs = _context.UserProcedureAssignment
                .Where(c => c.ProcedureId == procedureId)
                .ToList();

            if (userIDs.Any())
            {
                var userIds = new List<User>();
                foreach (var userProcedureAssignment in userIDs)
                {
                    var user = _context.Users
                        .Where(c => c.UserId == userProcedureAssignment.UserId)
                        .FirstOrDefault();
                    if (user != null)
                        userIds.Add(user);
                }

                return Ok(userIds);
            }
            else
            {
                return Ok(Enumerable.Empty<User>());
            }
        }
        [HttpPost("AddRemoveUsersToPlan")]
        public async Task<IActionResult> AddRemoveUsersToPlan(AddUserToProducer command, CancellationToken token)
        {
            var response = await _mediator.Send(command, token);

            return response.ToActionResult();
        }

        [HttpDelete("RemoveAllUser")]
        public  IActionResult RemoveAllUser()
        {
            var planWithProcedures =  _context.Plans
                                    .OrderByDescending(c => c.PlanId)
                                    .Include(c => c.PlanProcedures)
                                    .ToList();

            var planProcedure = planWithProcedures.FirstOrDefault();

            if (planProcedure == null)
            {
                return BadRequest("No procedure IDs found in PlanProcedures.");
            }


            var procedureIds =  _context.PlanProcedures
                .Where(c => c.PlanId == planProcedure.PlanId)
                .Select(c => c.ProcedureId)
                .ToList();

            if (!procedureIds.Any())
            {
                return Ok(new
                {
                    Message = "No Users Selected",
                    DeletedCount = 0
                });
            }

            var assignmentsToDelete =  _context.UserProcedureAssignment
                .Where(x => procedureIds.Contains(x.ProcedureId))
                .ToList(); 

            if (!assignmentsToDelete.Any())
            {
                return Ok(new
                {
                    Message = "No Users Selected",
                    DeletedCount = 0
                });
            }

            _context.UserProcedureAssignment.RemoveRange(assignmentsToDelete);
            _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Users removed successfully.",
                DeletedCount = assignmentsToDelete.Count
            });
        }
    }
}
