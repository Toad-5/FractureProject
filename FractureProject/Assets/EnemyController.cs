using UnityEngine;

public enum EnemyState { Waiting, PreparingAttack, RushingPlayer, Attacking }

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Déplacement")]
    public float moveSpeed = 4f;
    public float stopDistance = 0.5f;
    
    [Header("attaque")]
    public float attackTriggerDistance = 1.5f;
    public float attackRange = 1f;
    public float hitRadius = 1f;
    public float attackDuration = 1f;
    public LayerMask playerLayer;
    
    public EnemyState currentState = EnemyState.Waiting; 
    
    private FightManager manager;
    private Rigidbody rb;
    private Vector3 currentDestination;
    
    private Vector3 facingDirection = new Vector3(1, 0, 1).normalized; 

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; 
    }

    public void Setup(FightManager combatManager)
    {
        manager = combatManager;
    }

    public void SetState(EnemyState newState)
    {
        currentState = newState;
    }

    public void SetTargetDestination(Vector3 targetPos)
    {
        currentDestination = targetPos;
    }

    void FixedUpdate()
    {
        if (currentState == EnemyState.Attacking) return;

        if (currentState == EnemyState.PreparingAttack || currentState == EnemyState.RushingPlayer)
        {
            float distToPlayer = Vector3.Distance(transform.position, manager.player.position);
            if (distToPlayer <= attackTriggerDistance)
            {
                PerformAttack();
                return;
            }
        }

        MoveTowardsDestination();
    }

    private void MoveTowardsDestination()
    {
        Vector3 directionToTarget = currentDestination - transform.position;
        directionToTarget.y = 0; 

        float distance = directionToTarget.magnitude;

        if (distance > stopDistance)
        {
            Vector3 moveDirection = directionToTarget.normalized;
            rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
            
            facingDirection = moveDirection;
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            
            Vector3 lookDir = manager.player.position - transform.position;
            lookDir.y = 0;
            if(lookDir != Vector3.zero) 
            {
                facingDirection = lookDir.normalized;
            }
        }
    }

    private void PerformAttack()
    {
        SetState(EnemyState.Attacking);
        
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
        
        Vector3 hitCenter = transform.position + (facingDirection * attackRange);
        hitCenter.y = transform.position.y;

        Collider[] hitPlayers = Physics.OverlapSphere(hitCenter, hitRadius, playerLayer);

        foreach (Collider playerHit in hitPlayers)
        {
            Debug.Log(gameObject.name + " a touché le joueur");
        }
        
        Invoke(nameof(EndAttack), attackDuration);
    }

    private void EndAttack()
    {
        manager.OnEnemyFinishedAttack(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 hitCenter = transform.position;

        if (Application.isPlaying)
        {
            hitCenter += facingDirection * attackRange;
        }
        else
        {
            hitCenter += transform.forward * attackRange;
        }

        Gizmos.DrawWireSphere(hitCenter, hitRadius);
    }
}