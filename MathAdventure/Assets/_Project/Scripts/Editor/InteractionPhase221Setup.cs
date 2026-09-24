using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class InteractionPhase221Setup
{
    private const string ScenePath = "Assets/_Project/Scenes/Island01.unity";
    private const float DetectorRadius = 1f;

    [MenuItem("MathAdventure/Setup Interaction - Phase 2.2.1")]
    public static void Run()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject player = FindSceneObject(scene, "Player");
        GameObject treasureChest = FindSceneObject(scene, "TreasureChest");
        GameObject uiRoot = FindSceneObject(scene, "UI");

        InteractionPromptUI promptUI = ConfigurePrompt(uiRoot.transform);
        ConfigurePlayerInteraction(player, promptUI);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("MathAdventure Interaction Phase 2.2.1 setup completed successfully.");
    }

    private static InteractionPromptUI ConfigurePrompt(Transform uiRoot)
    {
        GameObject canvasObject = GetOrCreateUIChild(uiRoot, "Canvas");
        Canvas canvas = GetOrAddComponent<Canvas>(canvasObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = GetOrAddComponent<CanvasScaler>(canvasObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        GetOrAddComponent<GraphicRaycaster>(canvasObject);

        GameObject promptObject = GetOrCreateUIChild(canvasObject.transform, "InteractionPrompt");
        RectTransform promptRect = promptObject.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0f);
        promptRect.anchorMax = new Vector2(0.5f, 0f);
        promptRect.pivot = new Vector2(0.5f, 0f);
        promptRect.anchoredPosition = new Vector2(0f, 110f);
        promptRect.sizeDelta = new Vector2(370f, 70f);
        promptRect.localScale = Vector3.one;

        Image background = GetOrAddComponent<Image>(promptObject);
        background.color = new Color(0f, 0f, 0f, 0.93f);
        background.raycastTarget = false;

        GameObject labelObject = GetOrCreateUIChild(promptObject.transform, "Label");
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(16f, 7f);
        labelRect.offsetMax = new Vector2(-16f, -7f);
        labelRect.localScale = Vector3.one;

        Text label = GetOrAddComponent<Text>(labelObject);
        label.text = "[ E ]  TƯƠNG TÁC";
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 36;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(1f, 1f, 1f, 1f);
        label.raycastTarget = false;

        InteractionPromptUI promptUI = GetOrAddComponent<InteractionPromptUI>(promptObject);
        promptUI.Configure(label);
        promptUI.Hide();

        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(scaler);
        EditorUtility.SetDirty(background);
        EditorUtility.SetDirty(label);
        EditorUtility.SetDirty(promptUI);
        return promptUI;
    }

    private static void ConfigurePlayerInteraction(GameObject player, InteractionPromptUI promptUI)
    {
        PlayerInteraction interaction = GetOrAddComponent<PlayerInteraction>(player);
        interaction.Configure(promptUI);

        GameObject detector = GetOrCreateChild(player.transform, "InteractionDetector");
        detector.transform.localPosition = Vector3.zero;
        detector.transform.localRotation = Quaternion.identity;
        detector.transform.localScale = Vector3.one;

        CircleCollider2D trigger = GetOrAddComponent<CircleCollider2D>(detector);
        trigger.isTrigger = true;
        trigger.offset = Vector2.zero;
        trigger.radius = DetectorRadius;

        EditorUtility.SetDirty(interaction);
        EditorUtility.SetDirty(trigger);
    }

    private static GameObject FindSceneObject(Scene scene, string objectName)
    {
        GameObject result = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Select(item => item.gameObject)
            .FirstOrDefault(item => item.name == objectName);

        return result != null
            ? result
            : throw new InvalidOperationException($"{objectName} was not found in Island01.");
    }

    private static GameObject GetOrCreateChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null) return child.gameObject;

        var result = new GameObject(childName);
        result.transform.SetParent(parent, false);
        return result;
    }

    private static GameObject GetOrCreateUIChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null)
        {
            if (child.GetComponent<RectTransform>() == null)
                throw new InvalidOperationException($"Existing UI object {childName} does not use RectTransform.");

            return child.gameObject;
        }

        var result = new GameObject(childName, typeof(RectTransform));
        result.transform.SetParent(parent, false);
        return result;
    }

    private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        return component != null ? component : gameObject.AddComponent<T>();
    }
}
