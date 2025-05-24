import React, { useEffect, useState } from "react";
import ReactSelect from "react-select";
import {
    addUsersToProducer,
    getUsersForprocedure
} from "../../../api/api";

const PlanProcedureItem = ({ procedure, users }) => {
    const [selectedUsers, setSelectedUsers] = useState([]);

    const handleAssignUserToProcedure = async (e) => {
        setSelectedUsers(e);
        let userids = [];
        userids = e.map(c => c.value);
        await addUsersToProducer(userids, procedure.procedureId);
    };

    useEffect(() => {

        const fetchUsers = async () => {
            try {
                const users = await getUsersForprocedure(procedure.procedureId);

                const userOptions = users.map((u) => ({
                    label: u.name,
                    value: u.userId
                }));

                setSelectedUsers(userOptions);
            } catch (error) {
                console.error('Error fetching users:', error);
            }
        };

        fetchUsers();
    }, [procedure.procedureId]);

    return (
        <div className="py-2">
            <div>
                {procedure.procedureTitle}
            </div>

            <ReactSelect
                className="mt-2"
                placeholder="Select User to Assign"
                isMulti={true}
                options={users}
                value={selectedUsers}
                onChange={(e) => handleAssignUserToProcedure(e)}
            />
        </div>
    );
};

export default PlanProcedureItem;
