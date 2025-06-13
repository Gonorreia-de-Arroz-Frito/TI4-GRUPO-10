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

    void Awake()
    {
        // Ensure all required fields are initialized before Update runs
        planner = new GOAPPlanner();
        currentState = new Dictionary<string, bool>
        {
            {"hasWeapon", false},
            {"playerInRange", false},
            {"playerAttacked", false}
        };

        goal = new Dictionary<string, bool>
        {
            {"playerAttacked", true}
        };

        actionList = new List<GAction>
        {
            new GAction(
                "GetWeapon",
                new Dictionary<string, bool> { {"hasWeapon", false} },
                new Dictionary<string, bool> { {"hasWeapon", true} },
                () => Debug.Log("Pegando arma")
            ),

            new GAction(
                "ApproachPlayer",
                new Dictionary<string, bool> { {"playerInRange", false} },
                new Dictionary<string, bool> { {"playerInRange", true} },
                () => Debug.Log("Chegando perto do jogador")
            ),

            new GAction(
                "AttackPlayer",
                new Dictionary<string, bool> {
                    {"hasWeapon", true},
                    {"playerInRange", true}
                },
                new Dictionary<string, bool> { {"playerAttacked", true} },
                () => Debug.Log("Atacando jogador")
            )
        };
    }

    void Start()
    {
        // Optionally, you can leave this empty or use it for other initialization
    }

    void Update()
    {
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