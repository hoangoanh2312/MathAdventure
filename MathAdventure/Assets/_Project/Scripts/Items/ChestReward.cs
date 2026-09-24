using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class ChestReward : MonoBehaviour
{
    [SerializeField] private GameStats gameStats;
    private bool isCollected;

    public void Configure(GameStats stats)
    {
        gameStats = stats;
    }

    public void Reveal()
    {
        if (isCollected) return;

        gameObject.SetActive(true);
    }

    public void ResetReward()
    {
        isCollected = false;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected || other.isTrigger) return;

        Rigidbody2D playerBody = other.attachedRigidbody;
        if (playerBody == null || playerBody.GetComponent<PlayerMovement>() == null) return;

        isCollected = true;
        gameStats?.AddGold(1);
        gameStats?.AddScore(10);
        Debug.Log("Chest reward collected");
        gameObject.SetActive(false);
    }
}
