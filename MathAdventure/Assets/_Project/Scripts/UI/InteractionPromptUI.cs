using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private Text promptText;

    public void Show(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    public void Configure(Text textComponent)
    {
        promptText = textComponent;
    }
}
