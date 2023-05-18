using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AIFollow))]
[RequireComponent(typeof(BotCharacterController))]
public class BotAI : MonoBehaviour
{
    private const float turnMultiplier = 1f / 180f;

    [field: SerializeField, HideInInspector] public AIFollow aiFollow { get; private set; }
    [field: SerializeField, HideInInspector] public BotCharacterController bot { get; private set; }

    public NavMeshAgent agent => aiFollow.agent;

    [Header("Walk")]
    public float walkSpeed = 0.925f;

    [Header("Run")]
    public float runDistance = 14f;
    public float runSpeed = 1f;

    private bool isRunning;

    private void InitializeComponents()
    {
        aiFollow = GetComponent<AIFollow>();
        bot = GetComponent<BotCharacterController>();
        agent.updatePosition = false;
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
        var deltaSteeringTarget = agent.steeringTarget - transform.position;
        var targetRotation = Quaternion.LookRotation(deltaSteeringTarget);
        var turnAngle = Vector3.SignedAngle(bot.motor.TransientRotation * bot.motor.CharacterForward, targetRotation * bot.motor.CharacterForward, bot.motor.CharacterUp);

        if (!aiFollow.isFollowing)
        {
            bot.animator.SetFloat(bot.move.parameterName, 0f);
            isRunning = false;
        }
        else if (isRunning)
        {
            bot.move.value = runSpeed;
        }
        else
        {
            bot.move.value = walkSpeed;
            isRunning = agent.remainingDistance >= runDistance;
        }

        // Update animation parameters
        bot.isMoving.value = aiFollow.isFollowing;
        bot.turn.value = turnAngle * turnMultiplier;

        // Pull agent towards character
        agent.nextPosition = transform.position;
    }
}
