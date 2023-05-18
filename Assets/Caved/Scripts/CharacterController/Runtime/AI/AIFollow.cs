using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIFollow : MonoBehaviour
{
    [field: SerializeField, HideInInspector] public NavMeshAgent agent { get; private set; }
    [field: SerializeField] public Transform followTransform { get; set; }
    [field: SerializeField] public float outOfRange { get; set; } = 5f;

    public bool isFollowing { get; private set; }

    private void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Reset()
    {
        InitializeComponents();
    }

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        UpdateFollowing();
    }

    private void UpdateFollowing()
    {
        if (followTransform == null)
        {
            return;
        }

        var delta = transform.position - followTransform.position;
        if (isFollowing)
        {
            if (delta.sqrMagnitude <= agent.stoppingDistance * agent.stoppingDistance)
            {
                isFollowing = false;
            }
        }
        else if (delta.sqrMagnitude > outOfRange * outOfRange)
        {
            isFollowing = true;
        }

        if (isFollowing)
        {
            agent.destination = followTransform.position;
        }
    }
}
