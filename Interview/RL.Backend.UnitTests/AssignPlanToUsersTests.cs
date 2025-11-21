using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using RL.Backend.Commands;
using RL.Backend.Commands.Handlers.Plans;
using RL.Backend.Exceptions;
using RL.Data;

namespace RL.Backend.UnitTests;

[TestClass]
public class AssignPlanToUsersTests
{
    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(int.MinValue)]
    public async Task AssignPlanToUsersTests_InvalidPlanId_ReturnsBadRequest(int planId)
    {
        //Given
        var context = new Mock<RLContext>();
        var sut = new AssignPlanToUsersCommandHandler(context.Object);
        var request = new AssignPlanToUsersCommand()
        {
            PlanId = planId,
            ProcedureId = 1,
            UserIds = new List<int> { 1 }
        };
        //When
        var result = await sut.Handle(request, new CancellationToken());

        //Then
        result.Exception.Should().BeOfType(typeof(BadRequestException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(int.MinValue)]
    public async Task AssignPlanToUsersTests_InvalidProcedureId_ReturnsBadRequest(int procedureId)
    {
        //Given
        var context = new Mock<RLContext>();
        var sut = new AssignPlanToUsersCommandHandler(context.Object);
        var request = new AssignPlanToUsersCommand()
        {
            PlanId = 1,
            ProcedureId = procedureId,
            UserIds = new List<int> { 1 }
        };
        //When
        var result = await sut.Handle(request, new CancellationToken());

        //Then
        result.Exception.Should().BeOfType(typeof(BadRequestException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(19)]
    [DataRow(35)]
    public async Task AssignPlanToUsersTests_PlanProcedureIdNotFound_ReturnsNotFound(int planId)
    {
        //Given
        var context = DbContextHelper.CreateContext();
        var sut = new AssignPlanToUsersCommandHandler(context);
        var request = new AssignPlanToUsersCommand()
        {
            PlanId = planId,
            ProcedureId = 1,
            UserIds = new List<int> { 1 }
        };

        context.Plans.Add(new Data.DataModels.Plan
        {
            PlanId = planId + 1
        });
        await context.SaveChangesAsync();

        //When
        var result = await sut.Handle(request, new CancellationToken());

        //Then
        result.Exception.Should().BeOfType(typeof(NotFoundException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1, 1, 8)]
    [DataRow(2, 2, 9)]
    [DataRow(3, 3, 10)]
    public async Task AssignPlanToUsersTests_UserIdNotFound_ReturnsNotFound(int planId, int procedureId, int userId)
    {
        //Given
        var context = DbContextHelper.CreateContext();
        var sut = new AssignPlanToUsersCommandHandler(context);
        var request = new AssignPlanToUsersCommand()
        {
            PlanId = 1,
            ProcedureId = 1,
            UserIds = new List<int> { userId }
        };

        context.Plans.Add(new Data.DataModels.Plan
        {
            PlanId = planId + 1
        });
        context.Procedures.Add(new Data.DataModels.Procedure
        {
            ProcedureId = procedureId + 1,
            ProcedureTitle = "Test Procedure"
        });
        context.PlanProcedures.Add(new Data.DataModels.PlanProcedure
        {
            ProcedureId = procedureId + 1,
            PlanId = planId + 1
        });
        await context.SaveChangesAsync();

        //When
        var result = await sut.Handle(request, new CancellationToken());

        //Then
        result.Exception.Should().BeOfType(typeof(NotFoundException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1, 1,1)]
    [DataRow(19, 1010,2)]
    [DataRow(35, 69,3)]
    public async Task AssignPlanToUsersTests_PlanAssignToUsers_ReturnsSuccess(int planId, int procedureId, int userId)
    {
        //Given
        var context = DbContextHelper.CreateContext();
        var sut = new AssignPlanToUsersCommandHandler(context);
        var request = new AssignPlanToUsersCommand()
        {
            PlanId = planId,
            ProcedureId = procedureId,
            UserIds = new List<int> { userId }
        };

        context.Plans.Add(new Data.DataModels.Plan
        {
            PlanId = planId
        });
        context.Procedures.Add(new Data.DataModels.Procedure
        {
            ProcedureId = procedureId,
            ProcedureTitle = "Test Procedure"
        });
        context.PlanProcedures.Add(new Data.DataModels.PlanProcedure
        {
            ProcedureId = procedureId,
            PlanId = planId 
        });
        context.Users.Add(new Data.DataModels.User
        {
            UserId = userId,
            Name = "Test User" 
        });
        await context.SaveChangesAsync();

        //When
        var result = await sut.Handle(request, new CancellationToken());

        //Then
        var dbPlanProcedure = await context.PlanProcedures.FirstOrDefaultAsync(pp => pp.PlanId == planId && pp.ProcedureId == procedureId);

        dbPlanProcedure.Should().NotBeNull();

        result.Value.Should().BeOfType(typeof(Unit));
        result.Succeeded.Should().BeTrue();
    }
}