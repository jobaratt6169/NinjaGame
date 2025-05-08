using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float doubleJumpForce = 4f;
    
    private bool isGrounded;
    private bool canDoubleJump;
    private int jumpCount = 0;
    private const int MAX_JUMPS = 2;
    
    private void Awake()
    {
        // Ensure the player has the correct tag
        gameObject.tag = "Player Ninja";
        
        // Get or add required components
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Add a collider if none exists
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<CapsuleCollider2D>();
        }

        // Validate components
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing on Player!");
            enabled = false;
            return;
        }
    }
    
    
    
    private void Update()
    {
        if (rb == null) return;
        
        HandleMovement();
        HandleJumping();
    }
    
    private void HandleMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        
        // Flip sprite based on movement direction
        if (moveInput != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = moveInput < 0;
        }
    }
    
    private void HandleJumping()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                // First jump
                PerformJump(jumpForce);
                jumpCount = 1;
            }
            else if (jumpCount < MAX_JUMPS)
            {
                // Double jump
                PerformJump(doubleJumpForce);
                jumpCount++;
            }
        }
    }
    
    private void PerformJump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0; // Reset jump count when landing
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
} 