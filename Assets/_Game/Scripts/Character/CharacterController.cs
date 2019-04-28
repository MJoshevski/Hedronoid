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
    [Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 1f;
    [Range(1f, 4f)] [SerializeField] float m_AirborneGravityMultiplier = 1f;
    [SerializeField] float m_MoveSpeedMultiplier = 1f;

    Rigidbody m_Rigidbody;
    [ReadOnly] [SerializeField] bool m_IsGrounded;
    const float k_Half = 0.5f;
    float m_TurnAmount;
    float m_ForwardAmount;

    //
    //TODO: get it from gravity service
    Vector3 m_GroundNormal = Vector3.up;
    private static float m_Gravity = -9.81f;
    [SerializeField] private float m_GravityRotationMultiplier = 5f;
    /*[SerializeField]*/
    private Vector3 m_GravityPullVector;
    /*[SerializeField]*/
    private Vector3 m_GravityUp;
    /*[SerializeField]*/
    private Vector3 m_TotalPull;
    [SerializeField] List<GameObject> m_AllAtractors = new List<GameObject>();
    [SerializeField] private GameObject m_currentAttractor;
    //

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

        CharacterInput.Instance.OnPlatformSwitched -= PlatformSwitched;
        CharacterInput.Instance.OnPlatformSwitched += PlatformSwitched;

        //HACK: Remove this one we have gravity service
        m_GravityPullVector = new Vector3(0, m_AllAtractors[0].transform.up.y * -1, 0);
        m_currentAttractor = m_AllAtractors[0];
        //

        m_Rigidbody.useGravity = false;
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
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
        Vector3 extraGravityForce = (m_Gravity * m_GravityUp * m_AirborneGravityMultiplier) - (m_Gravity * m_GravityUp);
        m_Rigidbody.AddForce(extraGravityForce);
    }


    void HandleGroundedMovement(bool crouch, bool jump)
    {
        // check whether conditions are right to allow a jump:
        if (jump && !crouch)
        {
            // jump!
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_Rigidbody.transform.up.y * m_JumpPower, m_Rigidbody.velocity.z);
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

		// HACK (for different up-vectors)
        // we preserve the existing y part of the current velocity.
        if(m_GravityUp.x == 0) velocity.y = m_Rigidbody.velocity.y;
        else velocity.x = m_Rigidbody.velocity.x;

        m_Rigidbody.velocity = velocity;
    }


    void CheckGroundStatus()
    {
        m_IsGrounded = FeetCollider.IsColliding();

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
        //Apply adequate gravity
        m_Rigidbody.AddForce(m_GravityUp * m_Gravity * m_GravityMultiplier);

        //Apply adequate rotation
        if (m_Rigidbody.transform.up != m_currentAttractor.transform.up)
            m_Rigidbody.rotation = Quaternion.Slerp(
                m_Rigidbody.rotation, 
                m_currentAttractor.transform.rotation, 
                m_GravityRotationMultiplier * Time.deltaTime);

        //Debugging
        m_GravityUp = m_GravityPullVector.normalized * -1;
        m_TotalPull = m_GravityUp * m_Gravity * m_GravityMultiplier;

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

        m_Rigidbody.position = m_Rigidbody.position + move.normalized * distance;
    }

    private void PlatformSwitched(GravityDirections dir)
    {
        switch (dir)
        {
            case GravityDirections.DOWN:
                m_GravityPullVector = new Vector3(0, m_AllAtractors[0].transform.up.y * -1, 0).normalized;
                m_currentAttractor = m_AllAtractors[0];
                break;
            case GravityDirections.UP:
                m_GravityPullVector = new Vector3(0, m_AllAtractors[1].transform.up.y * -1, 0).normalized;
                m_currentAttractor = m_AllAtractors[1];
                break;
            case GravityDirections.LEFT:
                m_GravityPullVector = new Vector3(m_AllAtractors[2].transform.up.x * -1, 0, 0).normalized;
                m_currentAttractor = m_AllAtractors[2];
                break;
            case GravityDirections.RIGHT:
                m_GravityPullVector = new Vector3(m_AllAtractors[3].transform.up.x * -1, 0, 0).normalized;
                m_currentAttractor = m_AllAtractors[3];
                break;
        }
    }
}
