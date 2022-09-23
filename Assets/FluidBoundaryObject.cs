using UnityEngine;

public class FluidBoundaryObject : MonoBehaviour
{
    [SerializeField] private Bounds bounds;

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + bounds.center,bounds.size);
    }
}
