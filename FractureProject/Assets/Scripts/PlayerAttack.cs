using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 5;
    public int currentHealth;
    public int attackDamage = 1;

    [Header("Combat")]
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
        currentHealth = maxHealth;
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

        Player.instance.locked = true;
        
        Vector3 hitCenter = transform.position + (playerMovement.lastFacingDirection * attackRange);
        hitCenter.y = transform.position.y;

        Collider[] hitEnemies = Physics.OverlapSphere(hitCenter, hitRadius, enemyLayer);

        foreach (Collider enemyCollider in hitEnemies)
        {
            EnemyController enemy = enemyCollider.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage, transform.position); 
            }
        }

        Invoke(nameof(EndAttack), attackDuration);
    }

    [Header("Hit du Joueur")]
    public float hitRecoveryTime = 0.5f;

    public void TakeDamage(int damage, Vector3 attackerPosition)
    {
        currentHealth -= damage;
        Debug.Log("Le joueur a pris " + damage + " dégâts ! Vie : " + currentHealth);
        
        Player.instance.locked = true;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        playerMovement.ChangeState(Player.States.Hit);
        
        Invoke(nameof(RecoverFromHit), hitRecoveryTime);
    }

    private void RecoverFromHit()
    {
        if (playerMovement.currentState == Player.States.Hit)
        {
            Player.instance.locked = false;
            playerMovement.ChangeState(Player.States.Idle);
        }
    }

    private void Die()
    {
        Debug.Log("GAME OVER : Le joueur est mort !");
        playerMovement.ChangeState(Player.States.Down);
        Player.instance.locked = true;
    }

    private void EndAttack()
    {
        if (playerMovement.currentState == Player.States.Attacking)
        {
            Player.instance.locked = false;
            playerMovement.ChangeState(Player.States.Idle);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerMovement == null) playerMovement = GetComponent<Player>();

        if (playerMovement != null)
        {
            Gizmos.color = Color.red;
            Vector3 hitCenter = transform.position + (playerMovement.lastFacingDirection * attackRange);
            Gizmos.DrawWireSphere(hitCenter, hitRadius);
        }
    }
}