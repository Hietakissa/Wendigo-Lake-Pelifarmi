using System.Collections;
using HietakissaUtils;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] float speed = 4.5f;
    [SerializeField] float runMultiplier = 1.5f;
    [SerializeField] float jumpForce = 3.5f;
    [SerializeField] float crouchHeight = 0.6f;
    [SerializeField] float crouchMultiplier = 0.6f;
    [SerializeField] float airMultiplier = 0.5f;

    [Header("QOL / Input")]
    [SerializeField] float stepHeight = 0.3f;
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBufferTime = 0.2f;
    [SerializeField] bool allowHoldingJump;
    [SerializeField] bool keepCrouchPressedAirborne = true;
    [SerializeField] float movementLerpSpeed = 15f;

    [Header("Physics")]
    [SerializeField] float gravity = 1f;
    [SerializeField] float fallMultiplier = 1.5f;
    [SerializeField] float drag = 2f;
    [SerializeField] bool applyVerticalDrag;
    [SerializeField] float pushForce = 450f;

    CharacterController cc;

    Vector2 inputVector;
    float horizontal, vertical;
    Vector3 moveDir;
    Vector3 velocity;
    Vector3 actualVelocity;
    Vector3 lastPos;

    const float GRAVITY = 9.81f;

    [Header("Casts")]
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] float sphereCastRadius = 0.35f;
    [SerializeField] float sphereCastOffset = 0.03f;
    [SerializeField] float rayCastOffset = 0.03f;

    RaycastHit sphereCast;
    RaycastHit groundRay;

    bool isGrounded;
    bool lastGrounded;

    bool isCrouching;
    bool crouchKeyHeld;
    bool isSprinting;

    bool coyote;
    bool jumpBuffer;
    float timeInAir;
    Coroutine jumpBufferCoroutine;


    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }
    
    void Update()
    {
        isGrounded = Physics.SphereCast(transform.position + Vector3.up * (sphereCastRadius + sphereCastOffset), sphereCastRadius, Vector3.down, out sphereCast, sphereCastOffset * 2, whatIsGround);
        Physics.Raycast(transform.position + Vector3.up * rayCastOffset, Vector3.down, out groundRay, rayCastOffset * 2, whatIsGround);

        if (isGrounded && !lastGrounded) StartGround();
        else if (!isGrounded && lastGrounded) StartAir();

        if (isCrouching) cc.stepOffset = stepHeight * crouchHeight * 0.7f;
        else if (isGrounded) cc.stepOffset = stepHeight;
        else cc.stepOffset = 0;

        HandlePhysics();
        GetInput();
        HandleMovement();

        HandleCrouching();
        HandleJumping();

        if (velocity.y > 0f && Physics.SphereCast(transform.position, sphereCastRadius, Vector3.up, out RaycastHit hit, 1.8f - sphereCastRadius * 0.9f, whatIsGround)) velocity.y = 0f;


        lastGrounded = isGrounded;


        void HandlePhysics()
        {
            HandleDrag();
            HandleGravity();

            void HandleDrag()
            {
                //terrible implementation, technically works, will leave it at that for now
                //the implementation isn't the greatest, so it slows down at an odd rate, should probably multiply it with 
                //Mathf.Max(1f, velocity.Magnitude()) or something similar

                //Vector3 tempVelocity = velocity;
                //tempVelocity.y = 0f;
                //if (tempVelocity.magnitude < 0.1f && velocity.y == 0f) velocity = Vector3.zero;
                //else velocity -= tempVelocity.normalized * drag * Time.deltaTime;

                //velocity = velocity - Vector3.Cross(velocity, velocity) * 0.5f * drag;

                if (applyVerticalDrag) velocity = velocity * Mathf.Max(0.2f, (1 - Time.deltaTime * drag));
                else
                {
                    float beforeYVelocity = velocity.y;
                    velocity = velocity * Mathf.Max(0.2f, (1 - Time.deltaTime * drag));
                    velocity.y = beforeYVelocity;
                }
            }

            void HandleGravity()
            {
                velocity += Vector3.down * CalculateGravityMagnitude();
                if (isGrounded) velocity.y = Mathf.Max(-5f, velocity.y);
            }
        }



        void GetInput()
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            inputVector = new Vector2(horizontal, vertical).normalized;
        }

        void HandleMovement()
        {
            MoveDir();

            lastPos = transform.position;
            cc.Move((moveDir + velocity) * Time.deltaTime);

            actualVelocity = (transform.position - lastPos) / Time.deltaTime;


            void MoveDir()
            {
                //TODO somehow orient vector away from the slope

                isSprinting = !isCrouching && Input.GetKey(KeyCode.LeftShift) && Vector3.Dot(inputVector, Vector2.up) >= 0.4f;

                Vector3 newMoveDir;
                if (OnSteepGround()) newMoveDir = Vector3.ProjectOnPlane(Vector3.down, groundRay.normal);
                else newMoveDir = (transform.forward * vertical + transform.right * horizontal).normalized;
                newMoveDir *= speed * GetSpeedMultiplier();

                float effectiveAirMultiplier = (isGrounded ? 1 : airMultiplier);
                float stopMultiplier = (Mathf.Approximately(horizontal + vertical, 0f) ? 2f : 1f);
                moveDir = Vector3.Lerp(moveDir, newMoveDir, movementLerpSpeed * stopMultiplier * effectiveAirMultiplier * Time.deltaTime);
            }
        }

        void HandleCrouching()
        {
            KeyCode crouchKey;

            if (Application.platform == RuntimePlatform.WebGLPlayer) crouchKey = KeyCode.C;
            else crouchKey = KeyCode.LeftControl;

            if (Input.GetKeyDown(crouchKey) || Input.GetKeyDown(KeyCode.C)) crouchKeyHeld = true;
            else if (Input.GetKeyUp(crouchKey) || Input.GetKeyUp(KeyCode.C)) crouchKeyHeld = false;


            if (isSprinting) return;


            if (!isGrounded)
            {
                if (isCrouching) EndCrouch();
                return;
            }


            if (crouchKeyHeld && isCrouching) return;

            if (crouchKeyHeld && !isCrouching)
            {
                StartCrouch();
            }
            else if (isCrouching)
            {
                if (!Physics.SphereCast(transform.position + Vector3.up * (sphereCastRadius + sphereCastOffset), sphereCastRadius, Vector3.up, out RaycastHit hit, 2f - sphereCastRadius * 2 - sphereCastOffset, whatIsGround))
                {
                    EndCrouch();
                }
            }
        }

        void HandleJumping()
        {
            HandleCoyote();

            if (Input.GetKeyDown(KeyCode.Space) || (allowHoldingJump && Input.GetKey(KeyCode.Space)))
            {
                if (CanJump()) Jump();
                else if (!allowHoldingJump)
                {
                    StopJumpBuffer();
                    jumpBufferCoroutine = StartCoroutine(SetJumpBufferCoroutine());
                }
            }
            else if (jumpBuffer)
            {
                if (CanJump()) Jump();
            }

            //bool CanJump() => (coyote || (isGrounded && !OnSteepGround()) && !isCrouching);
            bool CanJump() => (coyote || (isGrounded && !OnSteepGround()));

            void Jump()
            {
                if (isCrouching) EndCrouch();

                velocity.y = jumpForce;

                coyote = false;

                StopJumpBuffer();
            }

            void HandleCoyote()
            {
                if (lastGrounded && !isGrounded && velocity.y <= 0f) coyote = true;
                else if (!isGrounded)
                {
                    timeInAir += Time.deltaTime;

                    if (timeInAir > coyoteTime) coyote = false;
                }
                else
                {
                    timeInAir = 0f;
                    coyote = false;
                }
            }

            void StopJumpBuffer()
            {
                if (jumpBufferCoroutine != null) StopCoroutine(jumpBufferCoroutine);
                jumpBuffer = false;
            }

            IEnumerator SetJumpBufferCoroutine()
            {
                jumpBuffer = true;
                yield return new WaitForSeconds(jumpBufferTime);
                jumpBuffer = false;
            }
        }


        void StartCrouch()
        {
            isCrouching = true;

            cc.enabled = false;
            transform.localScale = new Vector3(1f, crouchHeight, 1f);
            cc.enabled = true;
        }
        void EndCrouch()
        {
            isCrouching = false;

            cc.enabled = false;
            transform.localScale = new Vector3(1f, 1f, 1f);
            cc.enabled = true;
        }


        void StartAir()
        {
            // Check if there's ground close enough -> if so snap to it and stay on the ground, for walking down stairs
            if (velocity.y < 0f && Physics.SphereCast(transform.position + Vector3.up * (sphereCastRadius + sphereCastOffset), sphereCastRadius, Vector3.down, out sphereCast, sphereCastOffset + cc.stepOffset * 0.5f, whatIsGround))
            {
                Teleport(transform.position.SetY(sphereCast.point.y));
                return;
            }

            if (velocity.y < 0) velocity.y = 0f;

            if (!keepCrouchPressedAirborne) crouchKeyHeld = false;
        }

        void StartGround()
        {

        }
    }


    bool OnSteepGround()
    {
        return GetGroundAngle() > cc.slopeLimit;
    }

    float GetGroundAngle()
    {
        float raycastAngle = Vector3.Angle(groundRay.normal, Vector3.up);
        float spherecastAngle = Vector3.Angle(sphereCast.normal, Vector3.up);

        return Mathf.Min(raycastAngle, spherecastAngle);
    }

    float GetSpeedMultiplier()
    {
        if (isCrouching) return crouchMultiplier;
        else if (isSprinting) return runMultiplier;
        else return 1;
    }

    float CalculateGravityMagnitude() => GRAVITY * gravity * (velocity.y < 0 ? fallMultiplier : 1) * Time.deltaTime;

    public void AddKnockback(Vector3 knockbackForce)
    {
        velocity += knockbackForce;
    }

    public void Teleport(Vector3 position)
    {
        cc.enabled = false;
        transform.position = position;
        cc.enabled = true;
    }


    public bool IsCrouching()
    {
        return isCrouching;
    }

    public bool IsSprinting()
    {
        return isSprinting;
    }

    public Vector3 GetMoveDir() => moveDir;
    //public Vector3 GetVelocity() => velocity;
    public Vector3 GetVelocity() => actualVelocity;


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.rigidbody)
        {
            if (hit.moveDirection.y < -0.3f || (moveDir + velocity).magnitude < 0.3f) return;

            float totalDistanceToMove = (moveDir + velocity).magnitude * Time.deltaTime;
            float leftOverDistance = totalDistanceToMove - hit.moveLength;
            float leftOverForce = leftOverDistance * speed * GetSpeedMultiplier();

            Vector3 velocityDir = new Vector3(hit.moveDirection.x, Mathf.Clamp(hit.moveDirection.y, 0f, 5f), hit.moveDirection.z).normalized;
            Vector3 velocityDir2 = Maf.Direction(transform.position, hit.point.SetY(transform.position.y));
            Vector3 velocityDir3 = (velocityDir + velocityDir2) * 0.5f;

            Debug.DrawRay(hit.point, velocityDir3 * leftOverForce * pushForce, Color.blue, 1.2f);
            //hit.rigidbody.AddForceAtPosition(velocityDir * leftOverForce * pushForce * Time.deltaTime, hit.point);
            hit.rigidbody.AddForce(velocityDir3 * leftOverForce * pushForce);
            //AddKnockback(-(velocityDir * leftOverForce * pushForce * Time.deltaTime) * 0.00002f);
            //hit.rigidbody.velocity = velocityDir * leftOverDistance * pushForce;
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, moveDir);

        if (Physics.SphereCast(transform.position + Vector3.up * (sphereCastRadius + sphereCastOffset), sphereCastRadius, Vector3.down, out sphereCast, sphereCastOffset * 2, whatIsGround))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x, sphereCast.point.y, transform.position.z) + Vector3.up * sphereCastRadius, sphereCastRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(sphereCast.point, 0.1f);
        }
        else
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position + Vector3.down * (sphereCastOffset - sphereCastRadius), sphereCastRadius);
        }
    }
}
