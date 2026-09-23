using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    public string GetInteractionText()
    {
        return "[ E ]  TƯƠNG TÁC";
    }

    public void Interact()
    {
        Debug.Log("Interacted with TreasureChest");
    }
}
