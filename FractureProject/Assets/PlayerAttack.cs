using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 1.5f;
    public float hitRadius = 1f;
    public float attackDuration = 0.5f;
    public float attackCooldown = 1f;
    
    public LayerMask enemyLayer;

    private float nextAttackTime = 0f;
    private Player playerMovement;

    void Start()
    {
        playerMovement = GetComponent<Player>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time >= nextAttackTime && 
               (playerMovement.currentState == Player.States.Idle || playerMovement.currentState == Player.States.Walking))
            {
                PerformAttack();
            }
        }
    }

    private void PerformAttack()
    {
        playerMovement.ChangeState(Player.States.Attacking);
        nextAttackTime = Time.time + attackCooldown;

        Debug.Log("Le joueur attaque");

        Vector3 hitCenter = transform.position + (playerMovement.lastFacingDirection * attackRange);
        hitCenter.y = transform.position.y;

        Collider[] hitEnemies = Physics.OverlapSphere(hitCenter, hitRadius, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            Debug.Log("touché : " + enemy.name);
            
        }

        Invoke(nameof(EndAttack), attackDuration);
    }

    private void EndAttack()
    {
        if (playerMovement.currentState == Player.States.Attacking)
        {
            playerMovement.ChangeState(Player.States.Idle);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<Player>();

        if (playerMovement != null)
        {
            Gizmos.color = Color.red;
            Vector3 hitCenter = transform.position + (playerMovement.lastFacingDirection * attackRange);
            Gizmos.DrawWireSphere(hitCenter, hitRadius);
        }
    }
}