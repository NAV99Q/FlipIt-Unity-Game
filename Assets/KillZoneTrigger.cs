using UnityEngine;

public class KillZoneTrigger : MonoBehaviour
{
    // This runs automatically the frame any collider touches this object
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Print absolutely everything that touches the floor to trace the issue
        Debug.Log("Physical touch registered! Object name: " + other.gameObject.name + " | Tag: " + other.tag);

        // Check if the object belongs to our player character setup
        if (other.gameObject.name == "player" || other.CompareTag("Player") || other.gameObject.transform.root.name == "player")
        {
            Debug.Log("Valid player fell out of bounds! Launching Game Over Canvas...");

            // Find our active manager object directly in the scene
            playercontrolsys manager = FindFirstObjectByType<playercontrolsys>();
            if (manager != null)
            {
                manager.TriggerGameOver();
            }
            else
            {
                Debug.LogError("Error: Could not locate the 'playercontrolsys' manager script in your hierarchy!");
            }
        }
    }
}
