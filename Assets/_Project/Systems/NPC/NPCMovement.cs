using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NPCMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    private Action onDestinationReached;

    private bool isWalking;

    public bool IsWalking => isWalking;

    public bool HasReachedDestination =>
        !agent.pathPending &&
        agent.remainingDistance <= agent.stoppingDistance;

    private static readonly int IsWalkingHash =
        Animator.StringToHash("IsWalking");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Walks the NPC to the destination Transform.
    /// </summary>
    public void WalkTo(
        Transform destination,
        Action onReached = null)
    {
        if (destination == null)
        {
            Debug.LogWarning($"{name}: Walk destination is null.");
            return;
        }

        WalkTo(destination.position, onReached);
    }

    /// <summary>
    /// Walks the NPC to the destination world position.
    /// </summary>
    public void WalkTo(
        Vector3 destination,
        Action onReached = null)
    {
        onDestinationReached = onReached;

        isWalking = true;

        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(destination);

        animator.SetBool(IsWalkingHash, true);
    }

    /// <summary>
    /// Stops the NPC immediately.
    /// </summary>
    public void StopWalking()
    {
        if (!isWalking)
            return;

        isWalking = false;

        agent.isStopped = true;
        agent.ResetPath();

        animator.SetBool(IsWalkingHash, false);

        onDestinationReached?.Invoke();
        onDestinationReached = null;
    }

    private void Update()
    {
        if (!isWalking)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance > agent.stoppingDistance)
            return;

        // Wait until the agent has actually stopped moving.
        if (agent.hasPath &&
            agent.velocity.sqrMagnitude > 0.01f)
        {
            return;
        }

        StopWalking();
    }

    /// <summary>
    /// Instantly moves the NPC to the given position.
    /// </summary>
    public void Warp(Vector3 position)
    {
        agent.Warp(position);
    }

    public void FaceTarget(Transform target)
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}