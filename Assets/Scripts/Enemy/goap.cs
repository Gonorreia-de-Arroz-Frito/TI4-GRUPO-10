using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using UnityEngine;

public class GAction
{
    public string Name;
    public Dictionary<string, bool> Preconditions;
    public Dictionary<string, bool> Effects;
    public Action Execute;

    public GAction(string name,
        Dictionary<string, bool> preconditions,
        Dictionary<string, bool> effects,
        Action execute)
    {
        Name = name;
        Preconditions = preconditions;
        Effects = effects;
        Execute = execute;
    }

    public bool IsApplicable(Dictionary<string, bool> state)
    {
        foreach (var cond in Preconditions)
        {
            if (!state.ContainsKey(cond.Key) || state[cond.Key] != cond.Value)
                return false;
        }
        return true;
    }

    public Dictionary<string, bool> ApplyEffects(Dictionary<string, bool> state)
    {
        var newState = new Dictionary<string, bool>(state);
        foreach (var effect in Effects)
            newState[effect.Key] = effect.Value;
        return newState;
    }
}

public class GOAPPlanner
{
    private class Node
    {
        public Dictionary<string, bool> State;
        public List<GAction> Plan;

        public Node(Dictionary<string, bool> state, List<GAction> plan)
        {
            State = state;
            Plan = plan;
        }
    }

    public List<GAction> Plan(List<GAction> actions,
        Dictionary<string, bool> currentState,
        Dictionary<string, bool> goalState)
    {
        Queue<Node> open = new Queue<Node>();
        open.Enqueue(new Node(currentState, new List<GAction>()));

        while (open.Count > 0)
        {
            Node node = open.Dequeue();

            if (GoalSatisfied(goalState, node.State))
                return node.Plan;

            foreach (var action in actions)
            {
                if (!node.Plan.Contains(action) && action.IsApplicable(node.State))
                {
                    var newState = action.ApplyEffects(node.State);
                    var newPlan = new List<GAction>(node.Plan) { action };
                    open.Enqueue(new Node(newState, newPlan));
                }
            }
        }

        return null;
    }

    private bool GoalSatisfied(Dictionary<string, bool> goal, Dictionary<string, bool> state)
    {
        foreach (var kvp in goal)
        {
            if (!state.ContainsKey(kvp.Key) || state[kvp.Key] != kvp.Value)
                return false;
        }
        return true;
    }
}



// - - - MONOBEHAVIOUR - - - //
public class goap : MonoBehaviour
{
    Dictionary<string, bool> prevState;
    Dictionary<string, bool> currentState;
    Dictionary<string, bool> nextState;

    List<GAction> actionList;
    Dictionary<string, bool> goal;

    List<GAction> plannedActions;
    int planPos = 0;

    GOAPPlanner planner;
    bool plan = true;
    bool executePlan = true;

    [SerializeField] EnemyMovement enemyMovement;
    [SerializeField] EnemyChecks enemyChecks;
    [SerializeField] Atributes atributes;

    void Awake()
    {
        // Ensure all required fields are initialized before Update runs
        planner = new GOAPPlanner();
        currentState = new Dictionary<string, bool>
        {
            {"foundFood", false},
            {"foundFood", false},
            {"lowHealth", false},
            {"hungry", false},
            {"confort", false}
        };

        goal = new Dictionary<string, bool>
        {
            {"confort", true}
        };

        actionList = new List<GAction>
        {
            new GAction(
                "Search for Food",
                new Dictionary<string, bool> { {"foundFood", false} },
                new Dictionary<string, bool> { {"onFood", true} },
                () => {
                    enemyMovement.SetModeGoToFood();
                }
            ),

            new GAction(
                "AtackPlayer",
                new Dictionary<string, bool> {{"foundPlayer", false}, {"lowHealth", false}},
                new Dictionary<string, bool> {{"foundFood", true}},
                () => enemyMovement.SetModeGoToPlayer()
            ),

            new GAction(
                "GoHome",
                new Dictionary<string, bool> {{"foundPlayer", true}, {"lowHealth", true}},
                new Dictionary<string, bool> {{"foundPlayer", false}},
                () => {
                    enemyMovement.setModeGoToHome();
                }
            ),

            new GAction(
                "eat",
                new Dictionary<string, bool> {{"onFood", true}},
                new Dictionary<string, bool> {{"hungry", false}},
                () => Debug.Log("Eat")
            ),

            new GAction(
                "rest",
                new Dictionary<string, bool> {{"hungry", false}},
                new Dictionary<string, bool> {{"confort", true}},
                () => enemyMovement.setModeGoToHome()
            ),
        };
    }

    void Start()
    {
        // Optionally, you can leave this empty or use it for other initialization
    }

    void translateToState()
    {
        currentState = new Dictionary<string, bool>
        {
            {"foundFood", enemyChecks.foundFood},
            {"onFood", enemyMovement.path.FindClosestVertex(transform.position).zoneType == ZoneType.Food},
            {"lowHealth", atributes.getHealth() < (atributes.getMaxHealth() / 3)},
            {"hungry", false},
            {"confort", false}
        };

    }

    void Update()
    {
        translateToState();

        if (currentState != prevState)
        {
            if (currentState == nextState && planPos < plannedActions.Count)
            {
                planPos++;
                executePlan = true;
            }
            else
            {
                plan = true;
            }
        }

        if (plan)
        {
            if (actionList == null || currentState == null || goal == null)
            {
                Debug.Log("is null");
            }
            plannedActions = planner.Plan(actionList, currentState, goal);
            if (plannedActions != null && plannedActions.Count > 0)
            {
                executePlan = true;
                planPos = 0;
            }
        }
        if (executePlan)
        {
            prevState = currentState;
            nextState = plannedActions[planPos].ApplyEffects(currentState);
            plannedActions[planPos].Execute();
        }
    }
}