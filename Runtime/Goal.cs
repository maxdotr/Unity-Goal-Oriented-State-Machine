// Ignore Spelling: offline

using System.Collections.Generic;

namespace GOSM
{
    /// <summary>
    /// <para>
    ///     This class describes a Goal for use in the Goal Oriented State Machine.
    ///     A goal can be thought of as a state in a state machine. Only one goal at a time in the GOSM will be performed at a time.
    ///     Each goal is performed as defined by multiple properties that help the <see cref="StateManager"/> decide which goal to execute. 
    /// </para>
    /// <para>
    ///     A goal is a group of <see cref="Action"/>s. When all actions are completed, the goal is successful. If an action fails,
    ///     the goal will complete the <see cref="Fail"/> action defined within the fail condition, then signal to the manager the goal has failed.
    /// </para>
    /// </summary>
    public class Goal
    {
        private LinkedList<Action> actions = new LinkedList<Action>();
        private LinkedListNode<Action> curr;

        /// <summary>
        /// The weight of a goal. A higher weight signal higher priority for the <see cref="StateManager"/>
        /// </summary>
        public int goalWeight { get; set; }

        /// <summary>
        /// If a goal is available to be executed. If a goal is offline, the <see cref="StateManager"/> will not execute it
        /// until it goes online again.
        /// </summary>
        public bool offline { get; set; }

        /// <summary>
        /// The status of a goal. If a goal is being executed <see cref="GoalFailed"/> will contain an empty string. Otherwise, it 
        /// will either contain "true" for a failure and a "false" for a success.
        /// </summary>
        public bool? GoalFailed { get; private set; }

        /// <summary>
        /// Defines whether this goal can be added back to the list of available goals when the initial attempt failed.
        /// </summary>
        public bool repeatableOnFail { get; }

        /// <summary>
        /// Defines whether this goal can be added back to the list of available goals when the initial attempt succeeded.
        /// </summary>
        public bool repeatableOnSuccess { get; }

        public delegate int ExecuteAction();
        public delegate bool PrerequisitesMet();
        private ExecuteAction executeAction;
        private PrerequisitesMet prerequisitesMet;

        /// <summary>
        /// The constructor for a Goal object. Pass in a linked list of actions, which defines the order in which actions have to be performed in for the
        /// particular goal, the prerequisites for a goal to be considered eligible for completion, the weight (the priority of a goal), if it can be repeatable on
        /// failure/success, and whether the goal is offline or not. 
        /// </summary>
        /// <param name="actions">A linked list of <see cref="Action"/>s in which order is defined for the goal to complete.</param>
        /// <param name="prerequisites">A delegate that returns a bool if the prerequisites are met for the goal to be executed.</param>
        /// <param name="goalWeight">The priority of a goal</param>
        /// <param name="repeatableOnFail">If this goal is repeatable on a failure</param>
        /// <param name="repeatableOnSuccess">If this goal is repeatable on a success</param>
        /// <param name="offline">If this goal can be executed at all.</param>
        public Goal(LinkedList<Action> actions, PrerequisitesMet prerequisites, int goalWeight, bool repeatableOnFail, bool repeatableOnSuccess, bool offline)
        {
            GoalFailed = null;
            this.actions = actions;
            curr = this.actions.First;
            prerequisitesMet = prerequisites;
            this.goalWeight = goalWeight;
            this.offline = offline;
            this.repeatableOnFail = repeatableOnFail;
            this.repeatableOnSuccess = repeatableOnSuccess;
        }

        /// <summary>
        /// Checks if the goal is available for execution. 
        /// </summary>
        /// <returns>True if the conditions are met, false otherwise.</returns>
        public bool GoalConditionsMet()
        {
            return prerequisitesMet();
        }

        /// <summary>
        /// Execute a step in an action of a goal.
        /// </summary>
        /// <returns></returns>
        public void ExecuteGoal()
        {
            executeAction = curr.Value.Execute;

            if (curr.Value.actionResult != 1) // Success case
            {
                executeAction();
            }

            if (curr.Value.failed == true)
            {
                GoalFailed = true;
            }

            else if (curr.Value.actionResult == 1) // If the current action succeeds
            {
                if (curr.Next == null)
                {
                    GoalFailed = false;
                }
                else
                {
                    curr = curr.Next;
                    ExecuteGoal();
                }
            }
        }

        /// <summary>
        /// Resets this goal's results.
        /// </summary>
        public void Reset()
        {
            GoalFailed = null;
            curr = actions.First;

            foreach (Action action in actions)
            {
                action.Reset();
            }
        }
    }
}