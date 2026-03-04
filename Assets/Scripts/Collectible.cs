using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int scoreValue = 10; // optional
    public int coinCountValue = 1; // how many coins this object counts as

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
        {
            // increment score (optional)
            GameManager.Instance.AddScore(scoreValue);
            // increment coin count that determines level change
            GameManager.Instance.AddCoin(coinCountValue);
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is null when collecting coin.");
        }

        // remove coin
        Destroy(gameObject);
    }
}
