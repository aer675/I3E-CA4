using UnityEngine;
using UnityEngine.AI;

public class ChasingAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;  // Drag Player here in Inspector
    private NavMeshAgent agent;
    
    [Header("Vision Settings")]
    public float visionRange = 15f;  // How far the AI can "see"
    
    [Header("Movement")]
    public float stoppingDistance = 0.5f;
    
    // Internal state
    private Vector3 startPosition;
    private enum AIState { Idle, Chasing, Returning }
    private AIState currentState = AIState.Idle;
    
    void Start()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        
        // Store the starting position (for returning later)
        startPosition = transform.position;
        
        // If player not assigned, find it
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
        // Validate
        if (agent == null)
        {
            Debug.LogError("ChasingAI: NavMeshAgent not found on this GameObject!");
        }
        if (player == null)
        {
            Debug.LogError("ChasingAI: Player not found! Make sure Player is tagged 'Player'.");
        }
    }
    
    void Update()
    {
        if (player == null || agent == null) return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool playerInVision = distanceToPlayer < visionRange;
        
        // State machine
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdleState(playerInVision, distanceToPlayer);
                break;
                
            case AIState.Chasing:
                HandleChasingState(playerInVision, distanceToPlayer);
                break;
                
            case AIState.Returning:
                HandleReturningState();
                break;
        }
    }
    
    void HandleIdleState(bool playerInVision, float distanceToPlayer)
    {
        // Stop moving
        agent.SetDestination(transform.position);
        
        // If player enters vision, start chasing
        if (playerInVision)
        {
            currentState = AIState.Chasing;
            Debug.Log($"AI: Player detected! Distance: {distanceToPlayer:F1}");
        }
    }
    
    void HandleChasingState(bool playerInVision, float distanceToPlayer)
    {
        // Always move toward player
        agent.SetDestination(player.position);
        
        // If player leaves vision range, start returning
        if (!playerInVision)
        {
            currentState = AIState.Returning;
            Debug.Log("AI: Player lost! Returning to start position...");
        }
    }
    
    void HandleReturningState()
    {
        // Move back to start position
        agent.SetDestination(startPosition);
        
        // Check if we've reached the start position
        if (!agent.hasPath || agent.remainingDistance < stoppingDistance)
        {
            currentState = AIState.Idle;
            Debug.Log("AI: Back at start position. Idle.");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        
        // Draw vision range as a wire sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        
        // Draw line to player
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, player.position);
    }
}