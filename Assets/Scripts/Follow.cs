using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0, 1, -6);
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private float playerZPosition = 0f; // Constant Z position for player
    private Transform player;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Follow script started");
        FindPlayer();
    }

    void FindPlayer()
    {
        Debug.Log("Attempting to find player...");
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player Ninja");
        if(playerObj == null)
        {
            Debug.LogError("Player Ninja not found! Make sure your player has the 'Player Ninja' tag.");
            // Try to find the player again in the next frame
            Invoke("FindPlayer", 0.1f);
        }
        else
        {
            player = playerObj.transform;
            // Set player's Z position to constant value
            Vector3 playerPos = player.position;
            playerPos.z = playerZPosition;
            player.position = playerPos;
            
            Debug.Log($"Found player at position: {player.position}");
            // Set initial camera position
            Vector3 initialPosition = player.position;
            initialPosition.z = offset.z; // Keep the camera's Z position constant
            transform.position = initialPosition + offset;
            Debug.Log($"Set camera position to: {transform.position}");
        }
    }

    void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("Player is null in LateUpdate");
            return;
        }

        // Freeze player's Z position
        Vector3 playerPos = player.position;
        playerPos.z = playerZPosition;
        player.position = playerPos;

        // Only update X and Y positions, keep Z constant
        Vector3 desiredPosition = new Vector3(
            player.position.x + offset.x,
            player.position.y + offset.y,
            offset.z
        );
        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        
        // Debug log every frame for now to help diagnose
        Debug.Log($"Camera following player. Player position: {player.position}, Camera position: {transform.position}");
    }
}
