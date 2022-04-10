using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;


public class MovementAndAnimationController : MonoBehaviour
{
    //declare reference variables
    XRIDefaultInputActions playerInputAsset;
    CharacterController characterController;
    Animator animator;
    

    //variables to store player input valeus
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    bool isMovementPressed;
    bool isRunPressed;

    //constants
    float rotationFactorPerFrame = 1.0f;
    float runMultiplier = 3.0f;
    int zero = 0;

    //gravity variables
    float gravity = -9.8f;
    float groundedGravity = -0.05f;


    //Jump variables
    bool isJumpPressed = false;
    float initialJumpVelocity;
    float maxJumpHeight = 1.25f;
    float maxJumpTime = 0.75f;
    bool isJumping = false;
    int isJumpingHash;
    int jumpCountHash;
    bool isJumpAnimating;
    int jumpCount = 0;

    //Dictionary of jump velocities
    Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>();

    //Dictionary of jump gravities
    Dictionary<int, float> jumpGravities = new Dictionary<int, float>(); 

    Coroutine currentJumpResetRoutine = null;


    private void Awake()
    {
        //initialize input actions
        playerInputAsset = new XRIDefaultInputActions();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();


        //set the parameter hash references
        isJumpingHash = Animator.StringToHash("isJumping");
        jumpCountHash = Animator.StringToHash("jumpCount");

        //set the player input callbacks
        playerInputAsset.XRILeftHand.Move.started += onMovementInput;
        playerInputAsset.XRILeftHand.Move.canceled += onMovementInput;
        playerInputAsset.XRILeftHand.Move.performed += onMovementInput;
        playerInputAsset.XRILeftHand.Run.started += onRun;
        playerInputAsset.XRILeftHand.Run.canceled += onRun;
        playerInputAsset.XRILeftHand.Jump.started += onJump;
        playerInputAsset.XRILeftHand.Jump.canceled += onJump;
        playerInputAsset.XRIRightHand.Jump.started += onJump;
        playerInputAsset.XRIRightHand.Jump.canceled += onJump;


        setupJumpVariables();


    }

    void setupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight / MathF.Pow(timeToApex, 2));
        initialJumpVelocity = (2 * maxJumpHeight / timeToApex);
        float secondJumpGravity = (-2 * (maxJumpHeight +1)) / Mathf.Pow((timeToApex * 1.25f), 2);
        float secondJumpInitialVelocity = (2 * (maxJumpHeight + 1)) / (timeToApex * 1.25f);
        float thirdJUmpGravity = (-2 * (maxJumpHeight + 2)) / Mathf.Pow((timeToApex * 1.5f), 2);
        float thirdJumpInitialVelocity = (2 * (maxJumpHeight + 2)) / (timeToApex * 1.5f);

        //Assiginig jump velocities to jump dictionary
        initialJumpVelocities.Add(1, initialJumpVelocity);
        initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        //Assigning jump gravities to gravity dictionary
        jumpGravities.Add(0, gravity);
        jumpGravities.Add(1, gravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJUmpGravity);


    }

    void handleJump()
    {
        if (!isJumping && characterController.isGrounded && isJumpPressed)
        {
            if(jumpCount <3 && currentJumpResetRoutine != null)
            {
                StopCoroutine(currentJumpResetRoutine);
            }
            animator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;
            isJumping = true;
            jumpCount += 1;
            animator.SetInteger(jumpCountHash, jumpCount);
            currentMovement.y = initialJumpVelocities[jumpCount] * 0.5f;
            currentRunMovement.y = initialJumpVelocities[jumpCount] * 0.5f;
        }
        else if (!isJumpPressed && isJumping && characterController.isGrounded)
        {
            isJumping = false;
        }
    }

    IEnumerator jumpResetRoutine()
    {
        //Timer for resetting the jump
        yield return new WaitForSeconds(0.5f);
        jumpCount = 0;
    }

    void onJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
        //Debug.Log("Jump is being read y'all");
    }

    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void handleRotation()
    {
        Vector3 positionToLookAt;
        //the change in position our character should point to
        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;
        //the current rotation of our character
        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed)
        {
            //creates a new rotation based on where the player is currently pressing
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame);
        }
    }


    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
       // currentRunMovement.x = currentMovementInput.x * runMultiplier;
       // currentRunMovement.z = currentMovementInput.y * runMultiplier;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;

        var device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out var controllerRotation)) {
            currentMovement = Quaternion.FromToRotation(controllerRotation * Vector3.up, Vector3.up) * controllerRotation * currentMovement;
        }

        currentRunMovement = currentMovement * runMultiplier;

    }

    void handlleAnimation()
    {
        bool isWalking = animator.GetBool("isWalking");
        bool isRunning = animator.GetBool("isRunning");

        if (isMovementPressed && !isWalking)
        {
            animator.SetBool("isWalking", true);
        }

        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool("isWalking", false);
        }

        if ((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool("isRunning", true);
        } else if ((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool("isRunning", false);
        }


    }

    void handleGravity()
    {
        bool isFalling = currentMovement.y <= 0.0f || !isJumpPressed;
        float fallMultiplier = 2.0f;
        if (characterController.isGrounded)
        {
            if (isJumpAnimating)
            {
                animator.SetBool(isJumpingHash, false);
                isJumpAnimating = false;
                currentJumpResetRoutine = StartCoroutine(jumpResetRoutine());
                if (jumpCount == 3)
                {
                    jumpCount = 0;
                    animator.SetInteger(jumpCountHash, jumpCount);
                }
            }
            currentMovement.y = groundedGravity;
            currentRunMovement.y = groundedGravity;
        } 
        else if(isFalling) 
        {
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (jumpGravities[jumpCount] * fallMultiplier * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            currentMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;
        }
        else
        {
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (jumpGravities[jumpCount] * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            currentMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;
        }
    }
    // Update is called once per frame
    void Update()
    {
        handleRotation();
        handlleAnimation();

        if (isRunPressed)
        {
            characterController.Move(currentRunMovement * Time.deltaTime);
        }
        else
        {
            characterController.Move(currentMovement * Time.deltaTime);
        }
        handleGravity();
        handleJump();


    }


    private void Start()
    {
        XRSettings.eyeTextureResolutionScale = 1.2f;
    }


    private void OnEnable()
    {
        playerInputAsset.XRILeftHand.Enable();
        playerInputAsset.XRIRightHand.Enable();

    }
    private void OnDisable()
    {
        playerInputAsset.XRILeftHand.Disable();
        playerInputAsset.XRIRightHand.Disable();
    }
}
