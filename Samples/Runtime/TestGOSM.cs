using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOSM;
using UnityEngine.AI;

public class TestGOSM : MonoBehaviour
{
    public NavMeshAgent agent;
    public bool walkPointSet;
    public bool touched;
    public Vector3 walkPoint;

    public Fail walkAroundFail;
    public Action walkAroundAction;
    public Goal walkAroundGoal;
    private LinkedList<Action> walkAroundGoalActionList = new LinkedList<Action>();
    private List<Goal> walkAroundGoalList = new List<Goal>();

   public StateManager stateManager;
    void Start()
    {
        touched = false;

        ///Defining goals
        walkAroundFail = new Fail(RunAway);
        walkAroundAction = new Action(WalkRandomly, walkAroundFail);
        walkAroundGoalActionList.AddFirst(walkAroundAction);

        // Initialize the goal list before adding the goal
        walkAroundGoalList = new List<Goal>();
        walkAroundGoal = new Goal(walkAroundGoalActionList, () => true, 0, true, true, false);
        walkAroundGoalList.Add(walkAroundGoal);

        stateManager = new StateManager(walkAroundGoalList, walkAroundGoal);
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        stateManager.Execute();
    }

    public int WalkRandomly() 
    {
        if (touched) 
        {
            Debug.Log("Goal WalkRandomly failed after touching entity!");
            return 0;
        }

        if (!walkPointSet) 
        {
            walkPoint = RandomPosition();
            walkPointSet = true;
        }

        Vector3 distanceToDestination = transform.position - walkPoint;

        if(walkPointSet) 
        {
            agent.SetDestination(walkPoint);
            if(distanceToDestination.magnitude < 1f) 
            {
                walkPointSet = false;
                return 1;
            }
            else
            {
                return -1;
            }
        }

        return -1;

    }

    public int RunAway() 
    {
        if (!walkPointSet)
        {
            agent.speed += 5f;
            walkPoint = RunPoint();
            walkPointSet = true;
        }
        
        Vector3 distanceToDestination = transform.position - walkPoint;
        agent.SetDestination(walkPoint);
        if (distanceToDestination.magnitude < 1f)
        {
            agent.speed -= 5f;
            touched = false;
            walkPointSet = false;
            Debug.Log("Finished fail action, run away!");
            return 1;
        }
        else
        {
            return -1;
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<BoxCollider>() != null) 
        {
            touched = true;
            walkPointSet = false;
        }
    }
    public Vector3 RunPoint()
    {
        float z = transform.position.z + 10;
        float x = transform.position.x + 10;
        return new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z); 
    }

    public Vector3 RandomPosition()
    {
        float z = Random.Range(-10, 10);
        float x = Random.Range(-10,10);
        return new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);
    }
}
