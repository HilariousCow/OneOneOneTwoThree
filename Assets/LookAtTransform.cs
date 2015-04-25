using UnityEngine;
using System.Collections;

public class LookAtTransform : MonoBehaviour {

    private Transform _followTarget;

    void LateUpdate()
    {
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.zero - transform.position, transform.up);
        if (_followTarget != null)
        {
            targetRotation = Quaternion.LookRotation(_followTarget.transform.position - transform.position, transform.up);
        }

        Quaternion delta = Quaternion.Inverse(transform.rotation)*targetRotation;

        float angle = Quaternion.Angle(transform.rotation, targetRotation);
        if (angle > 0.0f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                                                          (angle + 1f)*Time.deltaTime*1f);
        }
    
    }


    public void SetTarget(Transform target)
    {
        _followTarget = target;
    }

	
}
