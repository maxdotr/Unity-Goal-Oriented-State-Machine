# Goal-Oriented State Machine (GOSM) for Unity

This project implements a Goal-Oriented State Machine (GOSM) using Unity, aimed at handling complex state-based AI behaviors such as navigation, decision-making, and action prioritization. It was developed primarily with enemy AI in mind but can be extended to any other state-driven entity.

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Key Components](#key-components)
   - [Action](#action)
   - [Fail](#fail)
   - [Goal](#goal)
   - [StateManager](#statemanager)
4. [Usage](#usage)
   - [Installation](#installation)
   - [How to Implement Your Own Actions](#how-to-implement-your-own-actions)
   - [Defining Goals](#defining-goals)
   - [Creating a StateManager](#creating-a-statemanager)
   - [Collision Handling Example](#collision-handling-example)
5. [Example Script - TestGOSM](#example-script---testgosm)
6. [Customizing the GOSM](#customizing-the-gosm)
7. [Troubleshooting](#troubleshooting)
8. [Extending the GOSM](#extending-the-gosm)

## Overview

The GOSM in this project enables intelligent agents (such as NPCs or enemies) to follow a set of actions to achieve certain goals. Each goal consists of one or more actions, and each action can either succeed, fail, or continue (i.e., still in progress). In cases where an action fails, a corresponding "Fail" action is executed. This machine is controlled by a state manager that selects which goal to execute based on the current conditions.

## Architecture

The GOSM is composed of four key components:

| Component    | Description                                                    |
|--------------|----------------------------------------------------------------|
| Action       | Defines an individual step or behavior within a goal.          |
| Fail         | Represents a recovery action executed when an action fails.    |
| Goal         | A collection of actions that represent a high-level objective. |
| StateManager | Manages the selection and execution of goals.                  |

## Key Components

### Action

An `Action` is a unit of work in the state machine. Actions are part of goals and define individual behaviors, such as moving to a specific point, attacking a target, etc.

- **Status Codes:**
  - `-1`: Ongoing (the action is not complete yet)
  - `0`: Failed (the action has failed)
  - `1`: Success (the action has completed successfully)

```csharp
public class Action
{
    public delegate int ExecuteAction();
    public string failed { get; set; } // Whether the action failed or not ("true" or "false")
    public int actionResult { get; set; } // Action result (-1: Ongoing, 0: Fail, 1: Success)

    public Action(ExecuteAction action, Fail fail)
    {
        // Constructor to initialize the action and its fail condition
    }

    public int Execute()
    {
        // Executes the action and returns the result
    }

    public void Reset()
    {
        // Resets the action to its default state (ongoing)
    }
}
```

### Fail

A `Fail` object defines an action to perform when an associated action fails. For example, if an agent is moving towards a target and encounters an obstacle, a fail action could trigger the agent to run away or change direction.

```csharp
public class Fail
{
    public delegate int ExecuteFail();

    public Fail(ExecuteFail fail)
    {
        // Constructor to define the failure behavior
    }

    public int Execute()
    {
        // Execute the fail action
    }
}
```

### Goal

A `Goal` is a collection of actions that represent an overarching task for an agent, like patrolling an area, attacking a player, or escaping danger. Each goal can be repeatable on success or failure and is prioritized based on a weight system.

```csharp
public class Goal
{
    public LinkedList<Action> actions;
    public string GoalFailed { get; private set; } // Status of the goal ("true" if failed, "false" if successful)
    public int goalWeight { get; set; } // Determines the priority of this goal
    public bool offline { get; set; } // Whether the goal is currently active

    public Goal(LinkedList<Action> actions, PrerequisitesMet prerequisites, int goalWeight, bool repeatableOnFail, bool repeatableOnSuccess, bool offline)
    {
        // Constructor for initializing the goal
    }

    public bool GoalConditionsMet()
    {
        // Checks if the goal can be executed based on its prerequisites
    }

    public void ExecuteGoal()
    {
        // Executes the goal by running its actions sequentially
    }

    public void Reset()
    {
        // Resets the goal and its actions
    }
}
```

### StateManager

The `StateManager` oversees the entire system, executing goals and transitioning between them based on priority and current conditions. It selects goals to execute, monitors their progress, and handles goal failures.

```csharp
public class StateManager
{
    public StateManager(List<Goal> goals, Goal defaultGoal)
    {
        // Initializes the state manager with a list of goals and a default fallback goal
    }

    public void Execute()
    {
        // Executes the current goal
    }
}
```

## Usage

### Installation

The easiest way to use this GOSM is to add this repository as a package with Unity's package manager:
- Window-> Package Manager -> `+` button -> add package from `git` url -> `https://github.com/maxdotr/Unity-Goal-Oriented-State-Machine.git`

You can also manually add this package by cloning it to the packages folder of your project.

In scripts using the GOSM, be sure to specify this by using the namespace GOSM:
```csharp
using GOSM;
```

### How to Implement Your Own Actions

To define a custom action, create a method that returns an integer status code based on the result of the action (`-1` for ongoing, `0` for failure, `1` for success). Then, instantiate the `Action` class with your method.

```csharp
public Action walkAroundAction;
public Fail walkAroundFail;
```
```csharp
public int WalkRandomly()
{
    // Custom logic for walking randomly
    return statusCode;
}

public int RunAway()
{
    return statusCode;
}
```
```csharp
walkAroundFail = new Fail(RunAway);
walkAroundAction = new Action(WalkRandomly, WalkAroundFail);
```

### Defining Goals

A `Goal` consists of a list of actions, and the goal succeeds when all actions are completed. To define a goal:

1. Create a list of actions.
2. Define any prerequisites (conditions for execution).
3. Pass them to the `Goal` constructor with associated parameters:
- PrerequisitesMet: A delegate to a method that returns a bool. The goal will only be available for execution when both this delegate returns true and when online.
- GoalWeight: The weight of the goal. Higher weights (higher in integer, 99 > 1), signal to the StateManager the priority for execution of available goals.
- RepeatableOnFail: If, when failed and after the fail action is completed, this goal can rejoin goals available for execution.
- RepeatableOnSuccess: If, when completed successfully, the goal can rejoin goals available for execution.
- Offline: Whether this goal is available for execution. This can be changed at any time after initialization. 

```csharp
LinkedList<Action> goalActions = new LinkedList<Action>();
goalActions.AddLast(walkAroundAction);

Goal walkAroundGoal = new Goal(goalActions, () => true, 1, true, true, false);
```

### Creating a StateManager

To manage the execution of goals, instantiate the `StateManager` with a list of goals and a default fallback goal.

```csharp
StateManager stateManager = new StateManager(goalList, defaultGoal);
```

### Collision Handling Example

In the example, the agent will set a goal to move randomly, but if it collides with an object, the `WalkRandomly` action fails, triggering the `RunAway` fail action.

```csharp
void OnCollisionEnter(Collision collision)
{
    if(collision.gameObject.GetComponent<BoxCollider>() != null) 
    {
        touched = true;  // Set the condition for failure
    }
}
```

## Example Script - TestGOSM

`TestGOSM.cs` is an example implementation of the GOSM system, where an agent walks randomly but runs away if it touches another entity.

```csharp
public class TestGOSM : MonoBehaviour
{
    public NavMeshAgent agent;
    public Fail walkAroundFail;
    public Action walkAroundAction;
    public Goal walkAroundGoal;
    public StateManager stateManager;

    void Start()
    {
        walkAroundFail = new Fail(RunAway);
        walkAroundAction = new Action(WalkRandomly, walkAroundFail);
        // Define goals and state manager
    }

    void Update()
    {
        stateManager.Execute(); // Executes goals each frame
    }

    // Define actions like WalkRandomly and RunAway here
}
```

## Customizing the GOSM

- **Adding More Goals:** You can add additional goals (like attacking, defending) by defining new actions and fail conditions.
- **Changing Priorities:** Modify `goalWeight` to change the priority of goals based on your AI's behavior.
- **More Complex Prerequisites:** Use the `GoalConditionsMet` delegate to define custom conditions under which goals can be executed.

## Troubleshooting

1. **Why is an action executed multiple times after failing?**  
   Ensure the failure is handled properly within the action and that the state manager moves to a new goal or resets the failed goal.
   
2. **Why are actions not transitioning correctly?**  
   Check if the `GoalConditionsMet` or `actionResult` is being correctly handled in your `Action` and `Goal` classes.

## Extending the GOSM

This framework is flexible and can be extended to more complex behaviors, including multi-agent systems, dynamic goal reassignment, and environmental interaction. You can add more sophisticated AI behavior by adding new action methods and fail conditions as needed.