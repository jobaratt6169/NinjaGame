using UnityEngine;

public class PlayerAttackTrigger : MonoBehaviour
{
    private PlayerStateMachine playerStateMachine;
    private bool isAttacking = false;
    private float attackCooldown = 0.5f;
    private float lastAttackTime = -1f;

    void Start()
    {
        // Get the PlayerStateMachine from the parent object
        playerStateMachine = GetComponentInParent<PlayerStateMachine>();
        if (playerStateMachine == null)
        {
            Debug.LogError("PlayerStateMachine not found on parent object!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit an enemy and if we're in an attacking state
        if (other.CompareTag("Enemy") && isAttacking)
        {
            // Get the enemy component
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Stab the enemy
                enemy.TakeDamage(1); // You can adjust the damage amount
                Debug.Log("Enemy stabbed!");
            }
        }
    }

    // Call this method when the player starts attacking
    public void StartAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            Debug.Log("Attack started");
        }
    }

    // Call this method when the player stops attacking
    public void StopAttack()
    {
        isAttacking = false;
        Debug.Log("Attack stopped");
    }
} 