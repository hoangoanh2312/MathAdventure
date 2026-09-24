using UnityEngine;

public class TreasureChest : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private ChestReward reward;
    [SerializeField] private bool isOpened;

    public bool IsOpened => isOpened;
    public bool CanInteract => !isOpened;

    private void Awake()
    {
        ResetToClosed();
    }

    public string GetInteractionText()
    {
        return "[ E ]  TƯƠNG TÁC";
    }

    public void Interact()
    {
        if (isOpened || spriteRenderer == null || openSprite == null) return;

        isOpened = true;
        spriteRenderer.sprite = openSprite;
        reward?.Reveal();
        Debug.Log("TreasureChest opened");
    }

    public void Configure(SpriteRenderer renderer, Sprite closed, Sprite opened)
    {
        spriteRenderer = renderer;
        closedSprite = closed;
        openSprite = opened;
        ResetToClosed();
    }

    public void ConfigureReward(ChestReward chestReward)
    {
        reward = chestReward;
        reward?.ResetReward();
    }

    private void ResetToClosed()
    {
        isOpened = false;

        if (spriteRenderer != null && closedSprite != null)
        {
            spriteRenderer.sprite = closedSprite;
        }
        reward?.ResetReward();
    }
}
