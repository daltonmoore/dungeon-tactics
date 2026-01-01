using UnityEngine;

public class PositionDirectionTracker : MonoBehaviour
{
    private Vector3 previousPosition;
    
    [field: SerializeField]
    public Vector3 currentMoveDirection { get; private set; }

    void Start()
    {
        previousPosition = transform.position;
    }

    void Update()
    {
        // Calculate the movement delta in this frame
        Vector3 deltaMovement = transform.position - previousPosition;

        // If the object moved, normalize the delta to get the direction
        if (deltaMovement.magnitude > 0)
        {
            currentMoveDirection = deltaMovement.normalized;
        }
        else
        {
            currentMoveDirection = Vector3.zero;
        }

        // Update the previous position for the next frame
        previousPosition = transform.position;

        // Example usage: Debug the direction
        // Debug.Log("Movement Direction: " + currentMoveDirection);
    }
}

