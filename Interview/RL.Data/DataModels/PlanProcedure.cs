using System.ComponentModel.DataAnnotations;
using RL.Data.DataModels.Common;
using System.Text.Json.Serialization;

namespace RL.Data.DataModels;

public class PlanProcedure : IChangeTrackable
{
    public int PlanId { get; set; }
    public int ProcedureId { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }

    // Navigation
    public virtual Plan Plan { get; set; }
    public virtual Procedure Procedure { get; set; }
    public ICollection<PlanProcedureUser> AssignedUsers { get; set; } = new List<PlanProcedureUser>();
}