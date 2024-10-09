using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOSM
{
    /// <summary>
    /// <para>
    ///     This class defines an action for use in a <see cref="Goal"/> and <see cref="StateManager"/> for the Goal Oriented State Machine.
    ///     Each action represents one step towards completing a goal. Each action has a <see cref="Fail"/> object attached, which defines an
    ///     uninterpretable action when an action has failed for the StateManager to proceed in execution.
    /// </para>
    /// </summary>
    public class Action
    {
        public delegate int ExecuteAction();

        ExecuteAction executeAction;

        // Save base action when a fail condition starts
        ExecuteAction executeActionHolder;

        /// <summary>
        /// Represents if the action has failed. Ongoing actions contain the empty string. 
        /// Successful actions contain "false" and otherwise "true"
        /// </summary>
        public bool? failed { get; set; }

        /// <summary>
        /// Action result. -1 if ongoing, 0 if failed, 1 if successful. 
        /// </summary>
        public int actionResult { get; set; }

        private Fail fail;

        /// <summary>
        /// Constructor for the action object. Pass in a void method that returns an <see cref="actionResult"/> and a <see cref="Fail"/> object
        /// that defines what happens when an object fails.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="fail"></param>
        public Action(ExecuteAction action, Fail fail)
        {
            executeAction = action;
            executeActionHolder = action;
            this.fail = fail;
        }

        /// <summary>
        /// Execute an iteration of the action.
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            if (actionResult == 0) // Fail condition
            {
                executeAction = fail.Execute;
                failed = true;
            }
            else if (actionResult == 1) // Success condition
            {
                failed = false;
            }

            return actionResult = executeAction();
        }

        /// <summary>
        /// Reset the results of an action.
        /// </summary>
        public void Reset()
        {
            failed = null;
            actionResult = -1; // Reset to ongoing state
            executeAction = executeActionHolder;
        }
    }
}
