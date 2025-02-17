using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GOSM.Step;

namespace GOSM
{
    /// <summary>
    /// <para>
    ///     This class describes failure behavior for use in the <see cref="Step"/>, <see cref="Goal"/>, and <see cref="StateManager"/> for the Goal Oriented State Machine.
    ///     A failure action is uninterrupted and is executed when a particular action fails.
    /// </para>
    /// </summary>
    public class Fail : IMetaDataProvider
    {
        public string heldMethod { get; private set; }
        public delegate int ExecuteFail();
        ExecuteFail executeFail;
        int FailResult = 0;

        /// <summary>
        /// Method name of the current action
        /// </summary>
        public string HeldMethod
        {
            get
            {
                return executeFail.Method.Name;
            }
        }

        /// <summary>
        /// Constructor for the fail object. Please pass in a delegate that returns an int, <see cref="FailResult"/>.
        /// </summary>
        /// <param name="Fail"></param>
        public Fail(ExecuteFail Fail)
        {
            this.executeFail = Fail;
            heldMethod = Fail.Method.Name;
        }

        /// <summary>
        /// Execute a step in the defined fail action.
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            FailResult = executeFail();
            if (FailResult == 1) // Success case
            {
                return 1;
            }

            return 0; // Failure case
        }
    }
}