using System;
using UnityEngine;
public class EnemyChecks : MonoBehaviour
{
    [SerializeField] Transform tf2;
    [SerializeField] Transform tf;
    [SerializeField] LayerMask layerMask;


    [SerializeField] float maxAngle;
    [SerializeField] float innerRadius;
    [SerializeField] float maxDistance;

    [SerializeField][ReadOnlyAtribute] public bool visible = false;
    [SerializeField][ReadOnlyAtribute] float angle = 0;
    [SerializeField][ReadOnlyAtribute] Vector2 tf2Dir;



    void Update()
    {
        tf2Dir = -tf2.up;
        visible = false;
        

        float distance = Vector3.Distance(tf.position, tf2.position);

        if (distance <= maxDistance)
        {
            Vector3 direction = (tf2.position - tf.position).normalized;
            RaycastHit hit;
            Boolean ray = Physics.Raycast(tf.position, direction, out hit, maxDistance, layerMask);
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

}