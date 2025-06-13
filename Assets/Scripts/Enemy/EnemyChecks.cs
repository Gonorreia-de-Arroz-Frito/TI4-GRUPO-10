using System.Collections.Generic;
using UnityEngine;
public class EnemyChecks : MonoBehaviour
{
    [SerializeField] Transform tf2;
    [SerializeField] Transform tf;
    [SerializeField] LayerMask layerMask;
    [SerializeField] graphPath path;


    [SerializeField] float maxAngle;
    [SerializeField] float innerRadius;
    [SerializeField] float maxDistance;
    [SerializeField] int foodGraphDistance;
    [SerializeField] float playerDetectionRange;
    

    [SerializeField][ReadOnlyAtribute] float angle = 0;
    [SerializeField][ReadOnlyAtribute] Vector2 tf2Dir;
    [SerializeField] public bool doCheckForVisibility;
    [SerializeField][ReadOnlyAtribute] public bool visible = false;
    [SerializeField] public bool doCheckForFood;
    [SerializeField][ReadOnlyAtribute] public bool foundFood = false;
    [SerializeField] public bool doCheckForPlayer;
    [SerializeField][ReadOnlyAtribute] public bool foundPlayer = false;


    [Header("Debug")]
    [SerializeField] bool fPlayerGizmos = true;

    


    void Update()
    {
        if (doCheckForVisibility)
            visibilityCheck();
        if (doCheckForFood)
            checkForFood();
        if (doCheckForPlayer)
            checkForPlayer();
    }

    

    void visibilityCheck()
    {
        tf2Dir = -tf2.up;
        visible = false;

        float distance = Vector3.Distance(tf.position, tf2.position);

        if (distance <= maxDistance)
        {
            Vector3 direction = (tf2.position - tf.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(tf.position, direction, maxDistance, layerMask);
            if (!hit.collider)
            {
                if (distance <= innerRadius)
                {
                    visible = true;
                }

                angle = Vector3.Angle(-tf2.up, direction);
                if (angle <= maxAngle)
                {
                    visible = true;
                }
            }
        }
    }

    void checkForFood()
    {
        foundFood = false;
        List<Vertex> vertices = path.getVerticesAtExactRange(path.FindClosestVertex(transform.position).id, foodGraphDistance);
        foreach (Vertex v in vertices)
        {
            if (v.zoneType == ZoneType.Food)
            {
                foundFood = true;
                return;
            }
        }
    }

    void checkForPlayer()
    {
        foundPlayer = false;
        Vector3 tPos = tf.position;
        tPos.z = 0;

        float distance = Vector3.Distance(tPos, tf2.position);

        Debug.Log($"tPos: {tPos}, tf2.position: {tf2.position}");

        if (distance <= playerDetectionRange)
        {
            Vector3 direction = (tf2.position - tPos).normalized;
            RaycastHit2D hit = Physics2D.Raycast(tPos, direction, distance, layerMask);
            if (!hit.collider)
            {
                foundPlayer = true;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (fPlayerGizmos)
        {
            if (tf != null && tf2 != null)
            {
                float distance = Vector3.Distance(tf.position, tf2.position);
                if (distance <= playerDetectionRange)
                {
                    Vector3 direction = (tf2.position - tf.position).normalized;
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(tf.position, tf.position + direction * distance);
                }
            }
        }
    }
}