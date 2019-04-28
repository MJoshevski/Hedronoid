using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class CharacterController : MonoSingleton<CharacterController>
{
    [Header("Refs")]
    [SerializeField]
    BoxCollider BoxCollider;

    [SerializeField]
    LayerCollider FeetCollider;

    public Rigidbody Rigidbody;

    [Header("Settings")]
    [SerializeField] float m_MovingTurnSpeed = 360;
    [SerializeField] float m_StationaryTurnSpeed = 180;
    [Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 1f;
    [Range(1f, 4f)] [SerializeField] float m_AirborneGravityMultiplier = 1f;
    [SerializeField] float m_MoveSpeedMultiplier = 1f;

    const float k_Half = 0.5f;
    float m_TurnAmount;
    float m_ForwardAmount;
    [SerializeField] private float m_GravityRotationMultiplier = 5f;

    float _colliderHeight;
    Vector3 _boxColliderCenter;
    bool m_Crouching;

    [SerializeField]
    LayerMask m_GroundLayer;
    Vector3 targetVelocity;
    bool grounded;
    Vector3 groundNormal;
    Vector3 velocity;
    RaycastHit[] hitBuffer = new RaycastHit[16];
    List<RaycastHit> hitBufferList = new List<RaycastHit>(16);

    [SerializeField] bool _isGrounded;
    public bool IsGrounded { get { return _isGrounded; } set { _isGrounded = value; } }

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        _colliderHeight = BoxCollider.size.y;
        _boxColliderCenter = BoxCollider.center;

        Rigidbody.useGravity = false;
        Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }


    public void Move(Vector3 move, bool crouch)
    {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, GravityService.Instance.GravityUp);
        m_TurnAmount = Mathf.Atan2(move.x, move.z);
        m_ForwardAmount = move.z;

        ApplyExtraTurnRotation();

        // control and velocity handling is different when grounded and airborne:
        if (!IsGrounded)
        {
            HandleAirborneMovement();
        }

        ScaleCapsuleForCrouching(crouch);
        PreventStandingInLowHeadroom();

        MoveForward();
    }


    void ScaleCapsuleForCrouching(bool crouch)
    {
        if (IsGrounded && crouch)
        {
            if (m_Crouching) return;
            var size = BoxCollider.size;
            size.y = size.y / 2;
            BoxCollider.size = size;
            BoxCollider.center = BoxCollider.center / 2f;

            m_Crouching = true;
        }
        else
        {
            var halfWidth = BoxCollider.size.x / 2f;
            Ray crouchRay = new Ray(Rigidbody.position + Vector3.up * halfWidth * k_Half, Vector3.up);
            float crouchRayLength = _colliderHeight - halfWidth * k_Half;
            if (Physics.SphereCast(crouchRay, halfWidth * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_Crouching = true;
                return;
            }
            var size = BoxCollider.size;
            size.y = _colliderHeight;
            BoxCollider.size = size;
            BoxCollider.center = _boxColliderCenter;
            m_Crouching = false;
        }
    }

    void PreventStandingInLowHeadroom()
    {
        // prevent standing up in crouch-only zones
        if (!m_Crouching)
        {
            var quarterWidth = BoxCollider.size.x / 2f * k_Half;

            Ray crouchRay = new Ray(Rigidbody.position + Vector3.up * quarterWidth, Vector3.up);
            float crouchRayLength = _colliderHeight - quarterWidth;
            if (Physics.SphereCast(crouchRay, quarterWidth, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_Crouching = true;
            }
        }
    }

    void HandleAirborneMovement()
    {
        var gravity = GravityService.Instance.Gravity;
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (gravity * GravityService.Instance.GravityUp * m_AirborneGravityMultiplier) - (gravity * GravityService.Instance.GravityUp);
        Rigidbody.AddForce(extraGravityForce);
    }

    void ApplyExtraTurnRotation()
    {
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
        transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
    }

    void MoveForward()
    {
        var velocity = transform.forward * m_ForwardAmount * m_MoveSpeedMultiplier * Time.deltaTime;

        // HACK (for different up-vectors)
        // we preserve the existing y part of the current velocity.
        if (GravityService.Instance.GravityUp.x == 0)
            velocity.y = Rigidbody.velocity.y;
        else
            velocity.x = Rigidbody.velocity.x;

        Rigidbody.velocity = velocity;
    }


    void CheckGroundStatus()
    {
        IsGrounded = FeetCollider.IsColliding();

    }

    public float minGroundNormalY = .65f;

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;

    void Update()
    {
        targetVelocity = Vector2.zero;
    }

    void FixedUpdate()
    {
        var gravityService = GravityService.Instance;
        //Apply adequate gravity
        Rigidbody.AddForce(gravityService.GravityUp * gravityService.Gravity * m_GravityMultiplier);

        //Apply adequate rotation
        if (Rigidbody.transform.up != gravityService.GravityUp)
            Rigidbody.rotation = Quaternion.Slerp(
                Rigidbody.rotation,
                gravityService.GravityRotation,
                m_GravityRotationMultiplier * Time.deltaTime);

        //Debugging
        // m_GravityService.Instance.GravityUp = m_GravityPullVector.normalized * -1;
        // m_TotalPull = m_GravityService.Instance.GravityUp * m_Gravity * m_GravityMultiplier;

        //velocity += gravityModifier * Physics.gravity * Time.deltaTime;
        velocity.x = targetVelocity.x;

        grounded = false;

        Vector3 deltaPosition = velocity * Time.deltaTime;

        Vector3 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

        Vector3 move = moveAlongGround * deltaPosition.x;

        Movement(move, false);

        move = Vector2.up * deltaPosition.y;

        Movement(move, true);
    }

    void Movement(Vector3 move, bool yMovement)
    {
        float distance = move.magnitude;

        var moveRay = new Ray(transform.position, move);

        if (distance > minMoveDistance)
        {
            int count = Physics.SphereCastNonAlloc(moveRay, distance + shellRadius, hitBuffer, m_GroundLayer);

            hitBufferList.Clear();
            for (int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]);
            }

            for (int i = 0; i < hitBufferList.Count; i++)
            {
                Vector3 currentNormal = hitBufferList[i].normal;
                if (currentNormal.y > minGroundNormalY)
                {
                    grounded = true;
                    if (yMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                float projection = Vector2.Dot(velocity, currentNormal);
                if (projection < 0)
                {
                    velocity = velocity - projection * currentNormal;
                }

                float modifiedDistance = hitBufferList[i].distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }


        }

        Rigidbody.position = Rigidbody.position + move.normalized * distance;
    }
}
