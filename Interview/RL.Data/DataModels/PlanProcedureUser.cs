using RL.Data.DataModels.Common;

namespace RL.Data.DataModels;

public class PlanProcedureUser : IChangeTrackable
{
    public int PlanId { get; set; }
    public int ProcedureId { get; set; }
    public int UserId { get; set; }

    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual PlanProcedure PlanProcedure { get; set; }
    public virtual User User { get; set; }
}