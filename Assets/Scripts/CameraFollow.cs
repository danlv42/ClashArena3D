using UnityEngine;

public sealed class CameraFollow : MonoBehaviour
{
    private Transform target;

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float desiredX = Mathf.Max(0f, target.position.x + 2.0f);
        Vector3 desired = new Vector3(desiredX, 1.0f, -10f);
        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * 5f);
    }
}
