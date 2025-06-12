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

    [SerializeField][ReadOnlyAtribute] public bool visible = false;
    [SerializeField][ReadOnlyAtribute] float angle = 0;
    [SerializeField][ReadOnlyAtribute] Vector2 tf2Dir;
    [SerializeField][ReadOnlyAtribute] public bool foundFood = false;



    void Update()
    {
        checkForFood();
        visibilityCheck();

    }
    void visibilityCheck()
    {
        tf2Dir = -tf2.up;
        visible = false;


        float distance = Vector3.Distance(tf.position, tf2.position);

        if (distance <= maxDistance)
        {
            Vector3 direction = (tf2.position - tf.position).normalized;
            RaycastHit hit;
            bool ray = Physics.Raycast(tf.position, direction, out hit, maxDistance, layerMask);
            if (!ray)
            {

                Debug.Log("gabriel");
                if (distance <= innerRadius)
                {
                    visible = true;

                }

                angle = Vector3.Angle(-tf2.up, (tf2.position - tf.position).normalized);
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

}