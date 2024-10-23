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
    public class Step : IMetaDataProvider
    {
        public delegate int ExecuteStep();
        ExecuteStep executeStep;

        // Save base action when a fail condition starts
        ExecuteStep executeActionHolder;

        /// <summary>
        /// Method name of the current action
        /// </summary>
        public string HeldMethod
        {
            get
            {
                return executeStep.Method.Name;
            }
        }

        /// <summary>
        /// Represents if the action has failed. Ongoing actions contain the empty string. 
        /// Successful actions contain "false" and otherwise "true"
        /// </summary>
        public bool? failed { get; set; }

        /// <summary>
        /// Action result. -1 if ongoing, 0 if failed, 1 if successful. 
        /// </summary>
        public int stepResult { get; set; }

        private Fail fail;

        /// <summary>
        /// Constructor for the action object. Pass in a void method that returns an <see cref="stepResult"/> and a <see cref="Fail"/> object
        /// that defines what happens when an object fails.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="fail"></param>
        public Step(ExecuteStep action, Fail fail)
        {
            executeStep = action;
            executeActionHolder = action;
            this.fail = fail;
        }

        /// <summary>
        /// Execute an iteration of the action.
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            if (stepResult == 0) // Fail condition
            {
                executeStep = fail.Execute;
                failed = true;
            }
            else if (stepResult == 1) // Success condition
            {
                failed = false;
            }

            return stepResult = executeStep();
        }

        /// <summary>
        /// Reset the results of an action.
        /// </summary>
        public void Reset()
        {
            failed = null;
            stepResult = -1; // Reset to ongoing state
            executeStep = executeActionHolder;
        }
    }
}
