using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class XRCharacter : MonoBehaviour
{
    // Input values
    public float speed = 5.0f;

    // References
    public Transform head = null;
    public Transform mesh = null;
    public XRController controller = null;

    // Components
    private Animator animator = null;
    private CharacterController character = null;


    // Values
    private Vector3 currentDirection = Vector3.zero;

    private void Awake()
    {
        // Collect our components
        animator = GetComponentInChildren<Animator>();
        character = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // If the controller is enabled, check for input
        if (controller.enableInputActions)
            CheckForMovement(controller.inputDevice);
    }

    private void CheckForMovement(InputDevice device)
    {
        // Look for input, and potential value
        if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystickDirection))
        {
            // Sets character direction, also factoring head
            CalculateDirection(joystickDirection);

            // Apply character direction, and speed
            MoveCharacter();

            // Orient the character mesh seperately
            OrientMesh();

            // Animate blend tree
            Animate();
        }
    }

    private void CalculateDirection(Vector2 joystickDirection)
    {
        // Joystick direction
        Vector3 newDirection = new Vector3(joystickDirection.x, 0, joystickDirection.y);

        // Look rotate
        Vector3 headRotation = new Vector3(0, transform.eulerAngles.y, 0);

        // Rotate our joystick direction using the rotation of the head
        currentDirection = Quaternion.Euler(headRotation) * newDirection;


    }

    private void MoveCharacter()
    {
        // Figure out how much we should move
        Vector3 movement = currentDirection * speed;

        // Use simple move to include gravity, frame independent by default
        character.SimpleMove(movement);
    }

    private void OrientMesh()
    {
        // Set the direction the character should look, only with input
        if (currentDirection != Vector3.zero)
            mesh.transform.forward = currentDirection;
    }

    private void Animate()
    {
        // Blend between walk/run using length, which is a value between 0 - 1
        float blend = currentDirection.magnitude;
        animator.SetFloat("Move", blend);
    }
}
