import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import {
  addProcedureToPlan,
  getPlanProcedures,
  getProcedures,
  getUsers,
  RemoveAllUsersForSelectedProcedure
} from "../../api/api";
import Layout from '../Layout/Layout';
import ProcedureItem from "./ProcedureItem/ProcedureItem";
import PlanProcedureItem from "./PlanProcedureItem/PlanProcedureItem";

const Plan = () => {
  let { id } = useParams();
  const [procedures, setProcedures] = useState([]);
  const [planProcedures, setPlanProcedures] = useState([]);
  const [users, setUsers] = useState([]);

  useEffect(() => {
    (async () => {
      try {
        const procedures = await getProcedures();
        const planProcedures = await getPlanProcedures(id);
        const users = await getUsers();
  
        const userOptions = users.map((u) => ({ label: u.name, value: u.userId }));
  
        setUsers(userOptions);
        setProcedures(procedures);
        setPlanProcedures(planProcedures);
      } catch (error) {
        console.error("Error fetching data:", error);
        alert("An error occurred while fetching data. Please try again later.");
      }
    })();
  }, [id]);

  const handleAddProcedureToPlan = async (procedure) => {
    const hasProcedureInPlan = planProcedures.some((p) => p.procedureId === procedure.procedureId);
    if (hasProcedureInPlan) return;
  
    try {
      await addProcedureToPlan(id, procedure.procedureId);
      setPlanProcedures((prevState) => [
        ...prevState,
        {
          planId: id,
          procedureId: procedure.procedureId,
          procedure: {
            procedureId: procedure.procedureId,
            procedureTitle: procedure.procedureTitle,
          },
        },
      ]);
    } catch (error) {
      console.error("Error adding procedure to plan:", error);
      alert("An error occurred while adding the procedure to the plan. Please try again.");
    }
  };

  const removeAllUsers = async () => {
    try {
      const result = await RemoveAllUsersForSelectedProcedure();
      if (result && result.deletedCount > 0) {
        window.location.reload();
      }
      alert(`${result.message}`);
    } catch (error) {
      console.error("Error removing all users:", error);
      alert("An error occurred while removing all users. Please try again.");
    }
  };

  return (
    <Layout>
      <div className="container pt-4">
        <div className="d-flex justify-content-center">
          <h2>OEC Interview Frontend</h2>
        </div>
        <div className="row mt-4">
          <div className="col">
            <div className="card shadow">
              <h5 className="card-header">Repair Plan</h5>
              <div className="card-body">
                <div className="row">
                  <div className="col">
                    <h4>Procedures</h4>
                    <div>
                      {procedures.map((p) => (
                        <ProcedureItem
                          key={p.procedureId}
                          procedure={p}
                          handleAddProcedureToPlan={handleAddProcedureToPlan}
                          planProcedures={planProcedures}
                        />
                      ))}
                    </div>
                  </div>
                  <div className="col">
                    <div style={{display:"flex", gap:"16rem"}}>
                    <h4>Added to Plan</h4>
                    <button style={{height:"2rem"}} onClick={removeAllUsers}>Remove All User</button>
                    </div>
                    <div>
                      {planProcedures.map((p) => (
                        <PlanProcedureItem
                          key={p.procedure.procedureId}
                          procedure={p.procedure}
                          users={users}
                        />
                      ))}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default Plan;
