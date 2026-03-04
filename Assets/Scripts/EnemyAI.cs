using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack, ReturnToPatrol }
    public State currentState = State.Idle;

    [Header("Patrol")]
    public Transform[] waypoints;
    int waypointIndex = 0;

    [Header("Detection")]
    public Transform player;
    public float detectionRange = 10f;
    public float chaseLostRange = 12f;

    [Header("Attack")]
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    bool canAttack = true;

    [Header("Animation")]
    public Animator animator;

    NavMeshAgent agent;
    Vector3 lastPatrolPos;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        lastPatrolPos = transform.position;

        if (animator == null) animator = GetComponent<Animator>();

        currentState = State.Idle;
        StartCoroutine(StateRoutine());
    }

    IEnumerator StateRoutine()
    {
        while (true)
        {
            switch (currentState)
            {
                case State.Idle: yield return IdleState(); break;
                case State.Patrol: yield return PatrolState(); break;
                case State.Chase: yield return ChaseState(); break;
                case State.Attack: yield return AttackState(); break;
                case State.ReturnToPatrol: yield return ReturnState(); break;
            }
            yield return null;
        }
    }

    // ---------------- STATES ----------------
    IEnumerator IdleState()
    {
        agent.isStopped = true;
        PlayIdleAnimation();


        float idleTime = Random.Range(1f, 2.5f);
        float t = 0f;
        while (t < idleTime)
        {
            if (PlayerInDetection())
            {
                currentState = State.Chase;
                yield break;
            }
            t += Time.deltaTime;
            yield return null;
        }

        agent.isStopped = false;
        GoToWaypoint();
        currentState = State.Patrol;
    }

    IEnumerator PatrolState()
    {
        agent.isStopped = false;
        PlayPatrolAnimation();

        while (currentState == State.Patrol)
        {
            if (PlayerInDetection())
            {
                currentState = State.Chase;
                yield break;
            }

            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                GoToNextWaypoint();
            }
            yield return null;
        }
    }

    IEnumerator ChaseState()
    {
        agent.isStopped = false;
        PlayChaseAnimation();

        while (currentState == State.Chase)
        {
            if (player == null) { currentState = State.ReturnToPatrol; yield break; }

            agent.SetDestination(player.position);

            float dist = Vector3.Distance(transform.position, player.position);

            if (dist <= attackRange)
            {
                agent.isStopped = true;
                currentState = State.Attack;
                yield break;
            }

            if (dist > chaseLostRange)
            {
                currentState = State.ReturnToPatrol;
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator AttackState()
    {
        while (currentState == State.Attack)
        {
            if (player == null) { currentState = State.ReturnToPatrol; yield break; }

            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > attackRange)
            {
                agent.isStopped = false;
                currentState = State.Chase;
                yield break;
            }

            if (canAttack)
            {
                StartCoroutine(DoAttack());
            }

            yield return null;
        }
    }

    IEnumerator DoAttack()
    {
        canAttack = false;
        PlayAttackAnimation();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    IEnumerator ReturnState()
    {
        agent.isStopped = false;
        GoToWaypointClosest();
        StopChaseAnimation();
        PlayPatrolAnimation();

        while (agent.pathPending || agent.remainingDistance > 0.5f)
        {
            if (PlayerInDetection())
            {
                currentState = State.Chase;
                yield break;
            }
            yield return null;
        }

        currentState = State.Patrol;
    }

    // ---------------- HELPERS ----------------
    bool PlayerInDetection()
    {
        if (player == null) return false;
        float d = Vector3.Distance(transform.position, player.position);
        return d <= detectionRange;
    }

    void GoToWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        agent.SetDestination(waypoints[waypointIndex].position);
    }

    void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
        agent.SetDestination(waypoints[waypointIndex].position);
    }

    void GoToWaypointClosest()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        int closest = 0;
        float best = float.MaxValue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, waypoints[i].position);
            if (dist < best)
            {
                best = dist;
                closest = i;
            }
        }
        waypointIndex = closest;
        agent.SetDestination(waypoints[waypointIndex].position);
    }

    // ---------------- ANIMATIONS ----------------
    void PlayIdleAnimation()
    {
        if (animator == null) return;
        animator.SetBool("isPatrolling", false);
        animator.SetBool("isChasing", false);
        animator.ResetTrigger("isAttacking");
        animator.SetTrigger("isIdle");
    }

    void PlayPatrolAnimation()
    {
        if (animator == null) return;
        animator.SetBool("isPatrolling", true);
        animator.SetBool("isChasing", false);
        animator.ResetTrigger("isIdle");
        animator.ResetTrigger("isAttacking");
    }

    void PlayChaseAnimation()
    {
        if (animator == null) return;
        animator.SetBool("isChasing", true);
        animator.SetBool("isPatrolling", false);
        animator.ResetTrigger("isIdle");
        animator.ResetTrigger("isAttacking");
    }

    void StopChaseAnimation()
    {
        if (animator == null) return;
        animator.SetBool("isChasing", false);
    }

    void PlayAttackAnimation()
    {
        if (animator == null) return;
        animator.SetTrigger("isAttacking");
        animator.SetBool("isPatrolling", false);
        animator.SetBool("isChasing", false);
        animator.ResetTrigger("isIdle");
    }
}
