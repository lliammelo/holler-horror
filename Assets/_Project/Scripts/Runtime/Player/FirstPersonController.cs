using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Player
{
    public enum LocomotionState { Idle, Walking, Sprinting }
    public enum Stance { Standing, Crouched }

    /// <summary>
    /// M0 first-person controller: walk / crouch / sprint with stamina, mouse look.
    /// Movement state and speed are exposed as the seam for the M2 noise-emission
    /// system (surface-based noise feeds entity hearing later).
    /// Input comes from an InputActionAsset (the project-wide InputSystem_Actions),
    /// actions: Player/Move, Player/Look, Player/Sprint, Player/Crouch.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerStamina))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField, Tooltip("If true, crouch input toggles; if false, crouch is held.")]
        private bool toggleCrouch = true;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float sprintSpeed = 6.2f;
        [SerializeField] private float crouchSpeed = 1.8f;
        [SerializeField, Tooltip("How quickly current velocity approaches target velocity.")]
        private float acceleration = 14f;
        [SerializeField] private float gravity = -22f;

        [Header("Stances")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchHeight = 1.05f;
        [SerializeField] private float stanceLerpSpeed = 10f;

        [Header("Look")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float lookSensitivity = 0.12f;
        [SerializeField] private float pitchClamp = 85f;
        [SerializeField, Tooltip("Camera height as a fraction of controller height.")]
        private float eyeHeightRatio = 0.92f;

        private CharacterController controller;
        private PlayerStamina stamina;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction sprintAction;
        private InputAction crouchAction;

        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float pitch;
        private bool crouchToggled;

        public LocomotionState Locomotion { get; private set; }
        public Stance CurrentStance { get; private set; } = Stance.Standing;
        public bool IsGrounded => controller.isGrounded;

        /// <summary>Horizontal speed in m/s — primary input for the future noise model (M2).</summary>
        public float CurrentSpeed => horizontalVelocity.magnitude;

        /// <summary>Current speed normalized against sprint speed, 0..1.</summary>
        public float CurrentSpeedNormalized => Mathf.Clamp01(CurrentSpeed / sprintSpeed);

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            stamina = GetComponent<PlayerStamina>();

            if (inputActions == null)
            {
                Debug.LogError($"{nameof(FirstPersonController)} on '{name}' has no InputActionAsset assigned.", this);
                enabled = false;
                return;
            }

            moveAction = inputActions.FindAction("Player/Move", throwIfNotFound: true);
            lookAction = inputActions.FindAction("Player/Look", throwIfNotFound: true);
            sprintAction = inputActions.FindAction("Player/Sprint", throwIfNotFound: true);
            crouchAction = inputActions.FindAction("Player/Crouch", throwIfNotFound: true);
        }

        private void OnEnable()
        {
            inputActions.FindActionMap("Player", throwIfNotFound: true).Enable();
            crouchAction.performed += OnCrouchPerformed;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            crouchAction.performed -= OnCrouchPerformed;
            inputActions.FindActionMap("Player").Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnCrouchPerformed(InputAction.CallbackContext _)
        {
            if (toggleCrouch)
                crouchToggled = !crouchToggled;
        }

        private void Update()
        {
            UpdateStance();
            UpdateMovement();
            UpdateLook();
        }

        private void UpdateStance()
        {
            bool wantsCrouch = toggleCrouch ? crouchToggled : crouchAction.IsPressed();

            if (CurrentStance == Stance.Crouched && !wantsCrouch && !CanStandUp())
                wantsCrouch = true; // blocked by ceiling, stay crouched

            CurrentStance = wantsCrouch ? Stance.Crouched : Stance.Standing;

            float targetHeight = CurrentStance == Stance.Crouched ? crouchHeight : standingHeight;
            if (!Mathf.Approximately(controller.height, targetHeight))
            {
                controller.height = Mathf.MoveTowards(controller.height, targetHeight, stanceLerpSpeed * Time.deltaTime);
                controller.center = new Vector3(0f, controller.height * 0.5f, 0f);
            }

            if (cameraPivot != null)
                cameraPivot.localPosition = new Vector3(0f, controller.height * eyeHeightRatio, 0f);
        }

        private bool CanStandUp()
        {
            Vector3 bottom = transform.position + Vector3.up * controller.radius;
            float castDistance = standingHeight - controller.radius * 2f;
            return !Physics.SphereCast(bottom, controller.radius * 0.95f, Vector3.up, out _,
                castDistance, ~0, QueryTriggerInteraction.Ignore);
        }

        private void UpdateMovement()
        {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            bool hasInput = moveInput.sqrMagnitude > 0.01f;

            bool wantsSprint = sprintAction.IsPressed()
                               && hasInput
                               && moveInput.y > 0.1f // no backwards/strafe-only sprinting
                               && CurrentStance == Stance.Standing
                               && stamina.CanSprint;

            float targetSpeed;
            if (!hasInput)
            {
                targetSpeed = 0f;
                Locomotion = LocomotionState.Idle;
            }
            else if (CurrentStance == Stance.Crouched)
            {
                targetSpeed = crouchSpeed;
                Locomotion = LocomotionState.Walking;
            }
            else if (wantsSprint)
            {
                targetSpeed = sprintSpeed;
                Locomotion = LocomotionState.Sprinting;
                stamina.DrainForSprint();
            }
            else
            {
                targetSpeed = walkSpeed;
                Locomotion = LocomotionState.Walking;
            }

            Vector3 wishDirection = transform.TransformDirection(new Vector3(moveInput.x, 0f, moveInput.y));
            if (wishDirection.sqrMagnitude > 1f)
                wishDirection.Normalize();

            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity, wishDirection * targetSpeed, acceleration * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            else
                verticalVelocity += gravity * Time.deltaTime;

            Vector3 motion = horizontalVelocity + Vector3.up * verticalVelocity;
            controller.Move(motion * Time.deltaTime);
        }

        private void UpdateLook()
        {
            if (cameraPivot == null)
                return;

            Vector2 lookInput = lookAction.ReadValue<Vector2>() * lookSensitivity;
            transform.Rotate(Vector3.up, lookInput.x);

            pitch = Mathf.Clamp(pitch - lookInput.y, -pitchClamp, pitchClamp);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
