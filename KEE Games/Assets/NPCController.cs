using UnityEngine;

public class NPCController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float maxYPosition = 1f;
    public float idleTimeMin = 2f;
    public float idleTimeMax = 10f;
    public float afkProbability = 0.1f;
    public float rotationSpeed = 5f;
    public float raycastDistance = 0.5f; 
    public GameObject detectionSphere; 
    public Transform player; 
    public float detectionRadius = 5f; 
    private Vector3 randomDirection;
    private float idleTimer;
    private bool isIdle = false;

    private Animator animator;
    private int idleAnimationIndex = 0;

    void Start()
    {
        SetRandomDirection();
        SetRandomIdleTime();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component is missing.");
        }
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) < detectionRadius)
        {
            isIdle = false;
        }

        if (!isIdle)
        {
            MoveNPC();
            PlayMovementAnimation();
        }
        else
        {
            IdleNPC();
            PlayIdleAnimation();
        }

        LimitYPosition();
    }

    void MoveNPC()
    {
        if (IsObstacleInFront())
        {
            ChangeDirection();
        }

        transform.position += randomDirection * moveSpeed * Time.deltaTime;

        if (randomDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(randomDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
        {
            if (Random.value < afkProbability)
            {
                isIdle = true; 
                SetRandomIdleTime();
            }
            else
            {
                SetRandomDirection();
                SetRandomIdleTime();
            }
        }
    }

    void IdleNPC()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
        {
            isIdle = false;
            SetRandomDirection();
        }
    }

    void LimitYPosition()
    {
        if (transform.position.y > maxYPosition)
        {
            transform.position = new Vector3(transform.position.x, maxYPosition, transform.position.z);
        }
    }

    void SetRandomDirection()
    {
        randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
    }

    void SetRandomIdleTime()
    {
        idleTimer = Random.Range(idleTimeMin, idleTimeMax);
    }

    private bool IsObstacleInFront()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, randomDirection, out hit, raycastDistance))
        {
            if (hit.collider != null)
            {
                return true; 
            }
        }
        return false; 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider is BoxCollider || collision.collider is CapsuleCollider)
        {
            ChangeDirection(); 
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider is BoxCollider || collision.collider is CapsuleCollider)
        {
            ChangeDirection(); 
        }
    }

    private void ChangeDirection()
    {
        SetRandomDirection();
    }

    void PlayMovementAnimation()
    {
        animator.SetBool("isMoving", randomDirection != Vector3.zero);
    }

    void PlayIdleAnimation()
    {

        switch (idleAnimationIndex)
        {
            case 0:
                animator.SetTrigger("PlayWarmingUp"); 
                break;
            case 1:
                animator.SetTrigger("PlayPhoneTalking"); 
                break;
            case 2:
                animator.SetTrigger("PlaySelfCheck"); 
                break;
        }
        idleAnimationIndex = (idleAnimationIndex + 1) % 3;
    }
}
