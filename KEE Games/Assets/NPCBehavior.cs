using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCBehavior : MonoBehaviour
{
    public float detectionRadius = 10f;
    public float avoidDistance = 3f;
    public float patrolRadius = 20f;
    public float patrolPointWaitTime = 2f;
    public float minAFKTime = 2f;
    public float maxAFKTime = 8f;

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private bool isAFK = false;
    private int idleAnimationIndex = 0; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing.");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animator component is missing.");
            return;
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        StartCoroutine(UpdateTargets());
        StartCoroutine(PatrolRoutine());
        StartCoroutine(AFKRoutine());
    }

    void Update()
    {
        if (agent == null || animator == null) return;
        if (!isAFK)
        {
            if (agent.velocity.magnitude > 0.1f)
            {
                animator.Play("locom_m_jogging_30f");
            }
            else
            {
                PlayNextIdleAnimation();
            }
        }
    }

    IEnumerator UpdateTargets()
    {
        while (true)
        {
            AvoidClosestThreat();
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (!isAFK)
            {
                Vector3 randomPoint = RandomNavSphere(transform.position, patrolRadius);
                agent.SetDestination(randomPoint);

                float waitTime = patrolPointWaitTime;
                while (agent.pathPending || agent.remainingDistance > 0.5f)
                {
                    waitTime -= Time.deltaTime;
                    if (waitTime <= 0)
                        break;
                    yield return null;
                }

                yield return new WaitForSeconds(patrolPointWaitTime);
            }
            else
            {
                yield return null; 
            }
        }
    }

    IEnumerator AFKRoutine()
    {
        while (true)
        {
            float afkDuration = Random.Range(minAFKTime, maxAFKTime);
            yield return new WaitForSeconds(Random.Range(5f, 15f));

            isAFK = true;
            agent.isStopped = true;
            PlayNextIdleAnimation();
            yield return new WaitForSeconds(afkDuration);

            isAFK = false;
            agent.isStopped = false;
        }
    }

    void PlayNextIdleAnimation()
    {
        switch (idleAnimationIndex)
        {
            case 0:
                animator.Play("Exercise_warmingUp_170f");
                break;
            case 1:
                animator.Play("idle_phoneTalking_180f");
                break;
            case 2:
                animator.Play("idle_selfcheck_1_300f");
                break;
        }
        idleAnimationIndex = (idleAnimationIndex + 1) % 3;
    }

    void AvoidClosestThreat()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        Transform closestThreat = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player") || hitCollider.CompareTag("Vehicle"))
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestThreat = hitCollider.transform;
                }
            }
        }

        if (closestThreat != null)
        {
            Vector3 directionAwayFromThreat = transform.position - closestThreat.position;
            Vector3 newDestination = transform.position + directionAwayFromThreat.normalized * avoidDistance;
            agent.SetDestination(newDestination);
        }
    }

    Vector3 RandomNavSphere(Vector3 origin, float distance)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, distance, -1);

        return navHit.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
}
