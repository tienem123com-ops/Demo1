using UnityEngine;

public class HitboxDetection : DetectionBase
{
    [Header("Hitbox Shape")]
    public Vector3 halfExtents = new Vector3(0.5f,0.5f,0.5f);
    public Vector3 offset;

    [Header("Layer")]
    public LayerMask layerToCheck;

    [Header("Debug")]
    public bool drawGizmos = true;
    public Color gizmosColor = Color.red;

    private readonly Collider[] _hits = new Collider[32];

    public void CheckCollision()
    {
        Vector3 center = transform.position + transform.rotation * offset;

        int count = Physics.OverlapBoxNonAlloc(
            center,
            halfExtents,
            _hits,
            transform.rotation,
            layerToCheck
        );

        for (int i = 0; i < count; i++)
        {
            var col = _hits[i];

            CollisionEnterEvent?.Invoke(col.gameObject);
            PositionEnterEvent?.Invoke(col.ClosestPoint(center));
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = new Color(gizmosColor.r, gizmosColor.g, gizmosColor.b, 0.6f);

        Vector3 center = transform.position + transform.rotation * offset;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);

        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);

        Gizmos.matrix = oldMatrix;
    }
}