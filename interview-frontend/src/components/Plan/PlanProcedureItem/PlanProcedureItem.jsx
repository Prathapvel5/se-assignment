import { useEffect, useState } from "react";
import ReactSelect from "react-select";
import { assignPlanToUsers } from "../../../api/api";

const PlanProcedureItem = ({ planProcedure, users }) => {
  const [selectedUsers, setSelectedUsers] = useState(null);
  const handleAssignUserToProcedure = async (procedureId, planId, users) => {
    const userIds = users.map((opt) => opt.value);
    setSelectedUsers(users);
    await assignPlanToUsers(procedureId, planId, userIds);
  };

  useEffect(() => {
    if (!planProcedure || !planProcedure.assignedUsers || !users) return;
    const userOptions = planProcedure.assignedUsers
      .map((assigned) => {
        const user = users.find((u) => u.value === assigned.userId);
        if (user) {
          return user;
        }
        return null;
      })
      .filter(Boolean);

    setSelectedUsers(userOptions);
  }, [users, planProcedure]);

  return (
    <div className="py-2">
      <div>{planProcedure.procedure.procedureTitle}</div>

      <ReactSelect
        className="mt-2"
        placeholder="Select User to Assign"
        isMulti={true}
        options={users}
        value={selectedUsers}
        onChange={(e) =>
          handleAssignUserToProcedure(
            planProcedure.procedureId,
            planProcedure.planId,
            e
          )
        }
      />
    </div>
  );
};

export default PlanProcedureItem;
