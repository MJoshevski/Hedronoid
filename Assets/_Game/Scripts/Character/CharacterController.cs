using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CharacterController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField]
    BoxCollider BoxCollider;

    [SerializeField]
    LayerCollider FeetCollider;

    [Header("Settings")]

    [SerializeField] float m_MovingTurnSpeed = 360;
    [SerializeField] float m_StationaryTurnSpeed = 180;
    [SerializeField] float m_JumpPower = 12f;
    [Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 2f;
    [SerializeField] float m_MoveSpeedMultiplier = 1f;

    Rigidbody m_Rigidbody;
    [ReadOnly] [SerializeField] bool m_IsGrounded;
    const float k_Half = 0.5f;
    float m_TurnAmount;
    float m_ForwardAmount;
    //TODO: get it from gravity service
    Vector3 m_GroundNormal = Vector3.up;
    float _colliderHeight;
    Vector3 _boxColliderCenter;
    bool m_Crouching;

    [SerializeField]
    protected LayerMask m_GroundLayer;
    protected Vector3 targetVelocity;
    protected bool grounded;
    protected Vector3 groundNormal;
    protected Vector3 velocity;
    protected RaycastHit[] hitBuffer = new RaycastHit[16];
    protected List<RaycastHit> hitBufferList = new List<RaycastHit>(16);



    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        _colliderHeight = BoxCollider.size.y;
        _boxColliderCenter = BoxCollider.center;

        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }


    public void Move(Vector3 move, bool crouch, bool jump)
    {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        m_TurnAmount = Mathf.Atan2(move.x, move.z);
        m_ForwardAmount = move.z;

        ApplyExtraTurnRotation();

        // control and velocity handling is different when grounded and airborne:
        if (m_IsGrounded)
        {
            HandleGroundedMovement(crouch, jump);
        }
        else
        {
            HandleAirborneMovement();
        }

        ScaleCapsuleForCrouching(crouch);
        PreventStandingInLowHeadroom();

        MoveForward();
    }


    void ScaleCapsuleForCrouching(bool crouch)
    {
        if (m_IsGrounded && crouch)
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
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * halfWidth * k_Half, Vector3.up);
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

            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * quarterWidth, Vector3.up);
            float crouchRayLength = _colliderHeight - quarterWidth;
            if (Physics.SphereCast(crouchRay, quarterWidth, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_Crouching = true;
            }
        }
    }

    void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
        m_Rigidbody.AddForce(extraGravityForce);
    }


    void HandleGroundedMovement(bool crouch, bool jump)
    {
        // check whether conditions are right to allow a jump:
        if (jump && !crouch)
        {
            // jump!
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
            m_IsGrounded = false;
        }
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
        // we preserve the existing y part of the current velocity.
        velocity.y = m_Rigidbody.velocity.y;
        m_Rigidbody.velocity = velocity;
    }


    void CheckGroundStatus()
    {
        m_IsGrounded = FeetCollider.IsColliding();

    }

    public float minGroundNormalY = .65f;
    public float gravityModifier = 1f;



    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;

    void Update()
    {
        targetVelocity = Vector2.zero;
    }

    void FixedUpdate()
    {
        velocity += gravityModifier * Physics.gravity * Time.deltaTime;
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

        m_Rigidbody.position = m_Rigidbody.position + move.normalized * distance;
    }
}
