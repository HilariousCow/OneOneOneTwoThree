using UnityEngine;
using System.Collections;

public class FollowTransform : MonoBehaviour
{

    private Transform _followTarget;

    void LateUpdate()
    {
        Vector3 targetPosition = Vector3.zero;
        if(_followTarget != null)
        {
            targetPosition = _followTarget.transform.position;
        }

        Vector3 delta = targetPosition - transform.position;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition,
                                                 Time.deltaTime*(delta.magnitude*2.0f + 1f));
    }


    public void SetTarget(Transform target)
    {
        _followTarget = target;
    }
}
