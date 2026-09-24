using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private InteractionPromptUI promptUI;

    private readonly Dictionary<MonoBehaviour, int> nearbyInteractables = new();
    private MonoBehaviour currentBehaviour;
    private IInteractable currentInteractable;

    private void Update()
    {
        SelectNearestInteractable();

        Keyboard keyboard = Keyboard.current;
        if (currentInteractable != null &&
            currentInteractable.CanInteract &&
            keyboard != null &&
            keyboard.eKey.wasPressedThisFrame)
        {
            currentInteractable.Interact();
            SelectNearestInteractable();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        MonoBehaviour behaviour = FindInteractableBehaviour(other.transform);
        if (behaviour == null) return;

        nearbyInteractables.TryGetValue(behaviour, out int overlapCount);
        nearbyInteractables[behaviour] = overlapCount + 1;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        MonoBehaviour behaviour = FindInteractableBehaviour(other.transform);
        if (behaviour == null || !nearbyInteractables.TryGetValue(behaviour, out int overlapCount)) return;

        if (overlapCount <= 1)
        {
            nearbyInteractables.Remove(behaviour);
        }
        else
        {
            nearbyInteractables[behaviour] = overlapCount - 1;
        }
    }

    private void SelectNearestInteractable()
    {
        MonoBehaviour nearestBehaviour = null;
        IInteractable nearestInteractable = null;
        float nearestSqrDistance = float.PositiveInfinity;
        List<MonoBehaviour> invalidEntries = null;

        foreach (MonoBehaviour behaviour in nearbyInteractables.Keys)
        {
            if (behaviour == null || !behaviour.isActiveAndEnabled || behaviour is not IInteractable interactable)
            {
                invalidEntries ??= new List<MonoBehaviour>();
                invalidEntries.Add(behaviour);
                continue;
            }

            if (!interactable.CanInteract) continue;

            float sqrDistance = (behaviour.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestBehaviour = behaviour;
                nearestInteractable = interactable;
            }
        }

        if (invalidEntries != null)
        {
            foreach (MonoBehaviour invalidEntry in invalidEntries)
            {
                nearbyInteractables.Remove(invalidEntry);
            }
        }

        if (nearestBehaviour == currentBehaviour) return;

        currentBehaviour = nearestBehaviour;
        currentInteractable = nearestInteractable;

        if (currentInteractable == null)
        {
            promptUI?.Hide();
        }
        else
        {
            promptUI?.Show(currentInteractable.GetInteractionText());
        }
    }

    private static MonoBehaviour FindInteractableBehaviour(Transform source)
    {
        Transform current = source;
        while (current != null)
        {
            foreach (MonoBehaviour behaviour in current.GetComponents<MonoBehaviour>())
            {
                if (behaviour is IInteractable)
                {
                    return behaviour;
                }
            }

            current = current.parent;
        }

        return null;
    }

    private void OnDisable()
    {
        nearbyInteractables.Clear();
        currentBehaviour = null;
        currentInteractable = null;
        promptUI?.Hide();
    }

    public void Configure(InteractionPromptUI interactionPromptUI)
    {
        promptUI = interactionPromptUI;
    }
}
