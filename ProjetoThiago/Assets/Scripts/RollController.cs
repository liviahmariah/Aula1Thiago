using UnityEngine;
using UnityEngine.InputSystem;

// RollController: reads a Vector2 move action from the new Input System
// and applies force to the Rigidbody in world-space using AddForce.
[RequireComponent(typeof(Rigidbody))]
public class RollController : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Reference to the Move action (Vector2) from the Input System asset")] 
    public InputActionReference moveAction;

    [Header("Movement")]
    [Tooltip("Multiplier applied to the input to produce the force")]
    public float speed = 10f;

    [Tooltip("ForceMode used when applying force to the Rigidbody")]
    public ForceMode forceMode = ForceMode.Force;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("RollController requires a Rigidbody on the same GameObject.", this);
        }
    }

    void OnEnable()
    {
        if (moveAction?.action != null)
            moveAction.action.Enable();
    }

    void OnDisable()
    {
        if (moveAction?.action != null)
            moveAction.action.Disable();
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        if (moveAction == null || moveAction.action == null) return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        // Convert Vector2 (x, y) -> world-space Vector3 (x, 0, y)
        Vector3 force = new Vector3(input.x, 0f, input.y) * speed;

        // Apply force only when there is meaningful input to avoid tiny forces
        if (force.sqrMagnitude > 0f)
        {
            rb.AddForce(force, forceMode);
        }
    }
}

