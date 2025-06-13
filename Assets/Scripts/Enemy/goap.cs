using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using UnityEngine;

public class GAction : IEquatable<GAction>
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

    public bool Equals(GAction other)
    {
        if (other == null) return false;
        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as GAction);
    }

    public override int GetHashCode()
    {
        return Name != null ? Name.GetHashCode() : 0;
    }

    public bool IsApplicable(Dictionary<string, bool> state)
    {
        foreach (var cond in Preconditions)
        {
            if (!state.ContainsKey(cond.Key) || state[cond.Key] != cond.Value)
            {
                Debug.Log($"GOAP: Action '{Name}' not applicable. Missing or mismatched precondition: {cond.Key} (required: {cond.Value}, actual: {(state.ContainsKey(cond.Key) ? state[cond.Key].ToString() : "not present")})");
                return false;
            }
        }
        Debug.Log($"GOAP: Action '{Name}' is applicable.");
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
        int step = 0;

        while (open.Count > 0)
        {
            Node node = open.Dequeue();
            Debug.Log($"GOAP Step {step++}: State = [{string.Join(", ", node.State.Select(kv => $"{kv.Key}:{kv.Value}"))}], Plan = [{string.Join(" -> ", node.Plan.Select(a => a.Name))}]");

            if (GoalSatisfied(goalState, node.State))
            {
                Debug.Log("GOAP: Goal satisfied!");
                return node.Plan;
            }

            foreach (var action in actions)
            {

                if (!node.Plan.Contains(action) && action.IsApplicable(node.State))
                {
                    var newState = action.ApplyEffects(node.State);
                    var newPlan = new List<GAction>(node.Plan) { action };
                    Debug.Log($"GOAP: Considering action '{action.Name}' -> New State: [{string.Join(", ", newState.Select(kv => $"{kv.Key}:{kv.Value}"))}]");
                    Debug.Log($"GOAP: Enqueuing Node with State = [{string.Join(", ", newState.Select(kv => $"{kv.Key}:{kv.Value}"))}], Plan = [{string.Join(" -> ", newPlan.Select(a => a.Name))}]");
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
    Dictionary<string, bool> prevState = new Dictionary<string, bool>();
    Dictionary<string, bool> currentState = new Dictionary<string, bool>();
    Dictionary<string, bool> nextState = new Dictionary<string, bool>();

    List<GAction> actionList;
    Dictionary<string, bool> goal;

    List<GAction> plannedActions;
    public int planPos = 0;

    GOAPPlanner planner;
    bool plan = true;
    bool executePlan = true;

    [SerializeField] EnemyMovement enemyMovement;
    [SerializeField] EnemyChecks enemyChecks;
    [SerializeField] Atributes atributes;

    [Header("Debug State Overrides")]
    public bool debugFoundFood;
    public bool debugOnFood;
    public bool debugLowHealth;
    public bool debugHungry;
    public bool debugConfort;

    void Awake()
    {
        // Ensure all required fields are initialized before Update runs
        planner = new GOAPPlanner();
        currentState = new Dictionary<string, bool>
        {
            {"foundFood", false},
            {"onFood", false},
            {"lowHealth", false},
            {"foundPlayer", false},
            { "hungry", false},
            { "fullHunger", true},
            {"confort", false},
            {"eating", false}
        };

        goal = new Dictionary<string, bool>
        {
            {"confort", true}
        };

        actionList = new List<GAction>
        {
            new GAction(
                "Patrol for food",
                new Dictionary<string, bool> { },
                new Dictionary<string, bool> { {"foundFood", true}},
                () => {
                    enemyMovement.SetModePatrol();
                }
            ),
            new GAction(
                "Patrol for player",
                new Dictionary<string, bool> { },
                new Dictionary<string, bool> { {"foundPlayer", true}},
                () => {
                    enemyMovement.SetModePatrol();
                }
            ),
            new GAction(
                "Search for Food",
                new Dictionary<string, bool> { {"foundFood", true} },
                new Dictionary<string, bool> { {"onFood", true} },
                () => {
                    enemyMovement.SetModeGoToFood();
                }
            ),

            new GAction(
                "AtackPlayer",
                new Dictionary<string, bool> {{"foundPlayer", true}, {"lowHealth", false}},
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
                new Dictionary<string, bool> {{"onFood", true}, { "foundPlayer", false} },
                new Dictionary<string, bool> {{"hungry", false}},
                () => enemyMovement.setModeEating()
            ),

            new GAction(
                "completly eat",
                new Dictionary<string, bool> {{"onFood", true}, { "hungry", false}, {"fullHunger", false}, {"foundPlayer", false}},
                new Dictionary<string, bool> {{"fullHunger", true}},
                () => enemyMovement.setModeEating()
            ),

            new GAction(
                "rest full",
                new Dictionary<string, bool> {{"fullHunger", true}},
                new Dictionary<string, bool> {{"confort", true}},
                () => enemyMovement.setModeGoToHome()
            ),

            new GAction(
                "rest",
                new Dictionary<string, bool> {{"hungry", false},{"eating", false}},
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
        // Use serialized debug values if set, otherwise use actual logic
        currentState = new Dictionary<string, bool>
        {
            {"foundFood", enemyChecks.foundFood},
            {"onFood", enemyMovement.path.FindClosestVertex(transform.position).zoneType == ZoneType.Food},
            {"lowHealth", atributes.getHealth() < (atributes.getMaxHealth() / 3)},
            {"foundPlayer", enemyChecks.foundPlayer},
            {"fullHunger", enemyMovement.getFome() <= 10},
            { "hungry", enemyMovement.getFome() >= (enemyMovement.getMaxFood() / 3)},
            {"confort", false},
            {"eating", enemyMovement.eating}
        };

        debugFoundFood = enemyChecks.foundFood;
        debugOnFood = enemyMovement.path.FindClosestVertex(transform.position).zoneType == ZoneType.Food;
        debugLowHealth = atributes.getHealth() < (atributes.getMaxHealth() / 3);
        debugHungry = enemyMovement.getFome() >= (enemyMovement.getMaxFood() / 3);
        debugConfort = false;

    }

    bool AreDictionariesEqual(
    Dictionary<string, bool> dict1,
    Dictionary<string, bool> dict2)
    {
        if (dict1.Count != dict2.Count)
        {
            Debug.Log("Size dif");
            return false;
        }

        foreach (var dc in dict1)
        {
            if (dc.Value != dict2[dc.Key])
            {
                Debug.Log(dc.Value + " - " + dict2[dc.Key]);
                return false;
            }
        }
        return true;
    }



    void Update()
    {
        translateToState();

        // Debug currentState and prevState
        //Debug.Log("Current State: " + string.Join(", ", currentState.Select(kvp => $"{kvp.Key}:{kvp.Value}")));
        //Debug.Log("Prev State: " + (prevState == null ? "null" : string.Join(", ", prevState.Select(kvp => $"{kvp.Key}:{kvp.Value}"))));
        if (!AreDictionariesEqual(currentState, prevState))
        {
            //Debug.Log("State Changed");
            if (AreDictionariesEqual(currentState, nextState) && planPos < plannedActions.Count)
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
            if (plannedActions == null)
            {
                Debug.LogWarning("GOAP: No plan could be found for the current state and goal.");
            }
            else
            {
                Debug.Log("GOAP: Plan found: " + string.Join(" -> ", plannedActions.Select(a => a.Name)));
            }
            if (plannedActions != null && plannedActions.Count > 0)
            {
                executePlan = true;
                planPos = 0;
            }
            plan = false;
        }
        if (executePlan)
        {
            prevState = new Dictionary<string, bool>(currentState);
            nextState = new Dictionary<string, bool>(plannedActions[planPos].ApplyEffects(currentState));
            plannedActions[planPos].Execute();
            executePlan = false;
        }
    }
}