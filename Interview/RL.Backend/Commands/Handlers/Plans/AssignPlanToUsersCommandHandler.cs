using MediatR;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Commands.Handlers.Plans;

public class AssignPlanToUsersCommandHandler : IRequestHandler<AssignPlanToUsersCommand, ApiResponse<Unit>>
{
    private readonly RLContext _context;

    public AssignPlanToUsersCommandHandler(RLContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<Unit>> Handle(AssignPlanToUsersCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate
            if (request.PlanId < 1)
                return ApiResponse<Unit>.Fail(new BadRequestException("Invalid PlanId"));

            if (request.ProcedureId < 1)
                return ApiResponse<Unit>.Fail(new BadRequestException("Invalid ProcedureId"));

            // Load plan procedure
            var planProcedure = await _context.PlanProcedures
                .Include(pp => pp.AssignedUsers)
                .FirstOrDefaultAsync(pp =>
                    pp.PlanId == request.PlanId &&
                    pp.ProcedureId == request.ProcedureId);

            if (planProcedure is null)
                return ApiResponse<Unit>.Fail(
                    new NotFoundException("PlanProcedure not found. Add procedure first.")
                );

            // Load valid users
            var users = await _context.Users
                .Where(u => request.UserIds.Contains(u.UserId))
                .ToListAsync();

            // Validate invalid userIds
            var foundIds = users.Select(x => x.UserId).ToHashSet();
            var invalidIds = request.UserIds.Where(id => !foundIds.Contains(id)).ToList();

            if (invalidIds.Any())
                return ApiResponse<Unit>.Fail(
                    new NotFoundException($"UserIds not found: {string.Join(", ", invalidIds)}")
                );

            // Remove old users
            planProcedure.AssignedUsers.Clear();

            // Add new users
            foreach (var user in users)
            {
                planProcedure.AssignedUsers.Add(new PlanProcedureUser
                {
                    PlanId = request.PlanId,
                    ProcedureId = request.ProcedureId,
                    UserId = user.UserId,
                    AssignedDate = DateTime.UtcNow,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            
            return ApiResponse<Unit>.Succeed(new Unit());
        }
        catch (Exception e)
        {
            return ApiResponse<Unit>.Fail(e);
        }
    }
}