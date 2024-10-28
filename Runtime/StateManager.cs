using System.Collections.Generic;
using System.Diagnostics;
namespace GOSM
{
    /// <summary>
    /// <para>
    ///     This class describes the StateManager object of the Goal Oriented State Machine.
    ///     Designed to be used with Unity Game Engine, every time executed, the manager performs one iteration of one action as defined by your goals.
    ///     Within Unity, the FixedUpdate or Update method should determine how fast the goals are executed.
    /// </para>
    /// 
    /// <para>
    ///     This state machine was developed with the thought of Enemy AI. In this context, the GOSM works in the following manner:
    ///     <ol>
    ///         <li>An <see cref="Step"/> is a single step of the overall goal.</li>
    ///         <li>A <see cref="Goal"/> is a group of actions.</li> 
    ///     </ol>
    ///</para>
    ///<para>
    ///     The state manager will take a list of goals, determine the best goal to execute, execute the goal until failure or success, then
    ///     execute the next goal. For example. if an enemy would carry a sword an action would be to approach the player, another action would be to swing the sword,
    ///     then another action would be to regain distance. These actions would combine to a single goal. Another goal could be another attack pattern, changing positions, etc. 
    ///</para>
    ///<para>
    ///     On a success, every action defined in a goal was completed. The manager will then move to another goal.
    ///     Each goal has a <see cref="Fail"/> condition which determines that the goal should be restated/moved on from early.
    ///     Fails contain an uninterpretable action that will be completed in entirety before the goal will change.
    ///</para>
    /// </summary>
    public class StateManager
    {
        private List<Goal> goals;
        private Goal defaultGoal;
        public Goal currentGoal { get; set; }

        /// <summary>
        /// The name of the method the state machine is currently invoking.
        /// </summary>
        public string CurrentlyInvoking
        {
            get => currentGoal.CurrentlyInvoking;
        }

        /// <summary>
        /// Constructor for the StateManager object. Pass in a list of <see cref="Goal"/>s this instance of the StateManager should manage, and a
        /// default <see cref="Goal"/> that will run when no <see cref="Goal"/>s match any of the defined prerequisites in <see cref="Goal"/>. The default
        /// goal should have the lowest (the least important) weight of all passed in goals.
        /// </summary>
        /// <param name="goals">A list of goals to manage</param>
        /// <param name="defaultGoal">The default goal</param>
        public StateManager(List<Goal> goals, Goal defaultGoal)
        {
            this.goals = goals;
            this.defaultGoal = defaultGoal;
            currentGoal = defaultGoal;
        }

        /// <summary>
        /// Executes the current <see cref="Goal"/>.
        /// </summary>
        public void Execute()
        {
            GetGoal().ExecuteGoal();
        }

        /// <summary>
        /// Helper method that defines which goal <see cref="Execute"/> should run.
        /// </summary>
        /// <returns></returns>
        public Goal GetGoal(bool switchGoal = false)
        {
            if ((currentGoal.GoalFailed.HasValue &&
                ((currentGoal.GoalFailed.Value && !currentGoal.repeatableOnFail) ||
                 (!currentGoal.GoalFailed.Value && !currentGoal.repeatableOnSuccess))) || switchGoal)
            {
                currentGoal.Reset();

                Goal goalWithConditionsMet = defaultGoal;

                foreach (Goal goal in goals)
                {
                    if (goal.Equals(currentGoal))
                    {
                        continue;
                    }

                    if (goal.GoalConditionsMet() && !goal.offline && goal.goalWeight > goalWithConditionsMet.goalWeight)
                    {
                        goalWithConditionsMet = goal;
                    }
                }

                currentGoal = goalWithConditionsMet;
            }
            else if (currentGoal.GoalFailed.HasValue &&
                    ((currentGoal.GoalFailed.Value && currentGoal.repeatableOnFail) ||
                    (!currentGoal.GoalFailed.Value && currentGoal.repeatableOnSuccess)))
            {
                currentGoal.Reset();
                if (!currentGoal.GoalConditionsMet())
                {
                    currentGoal = GetGoal(true);
                }
            }

            return currentGoal;
        }


        public void ResetGoals()
        {
            foreach (Goal goal in goals)
            {
                goal.Reset();
            }
            currentGoal = defaultGoal;
        }
    }
}
