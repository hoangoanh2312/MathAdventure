public interface IInteractable
{
    bool CanInteract { get; }
    string GetInteractionText();
    void Interact();
}
