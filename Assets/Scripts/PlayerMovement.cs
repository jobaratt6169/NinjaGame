using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float doubleJumpForce = 4f;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool canDoubleJump;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Update()
    {
        // Handle horizontal movement
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector3(moveInput * moveSpeed, rb.linearVelocity.y, 0);
        
        // Handle jumping
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                // First jump
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0);
                canDoubleJump = true;
            }
            else if (canDoubleJump)
            {
                // Double jump
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, doubleJumpForce, 0);
                canDoubleJump = false;
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if we're touching the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            canDoubleJump = false;
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        // Check if we've left the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
} 