using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TreasureChestPhase223Setup
{
    private const string ScenePath = "Assets/_Project/Scenes/Island01.unity";
    private const string TilesetPath = "Assets/_Project/Sprites/Environment/CompleteTileSet.png";
    private const string RewardSpriteName = "CompleteTileSet_99";

    [MenuItem("MathAdventure/Setup Treasure Chest Reward - Phase 2.2.3")]
    public static void Run()
    {
        Sprite rewardSprite = AssetDatabase.LoadAllAssetsAtPath(TilesetPath)
            .OfType<Sprite>()
            .FirstOrDefault(sprite => sprite.name == RewardSpriteName);
        if (rewardSprite == null)
            throw new InvalidOperationException($"Required sprite {RewardSpriteName} was not found.");

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject chestObject = FindSceneObject(scene, "TreasureChest");
        TreasureChest chest = chestObject.GetComponent<TreasureChest>();
        if (chest == null)
            throw new InvalidOperationException("TreasureChest requires the Phase 2.2.2 TreasureChest component.");

        GameObject rewardObject = GetOrCreateChild(chestObject.transform, "Reward");
        rewardObject.transform.localPosition = new Vector3(0f, 0.75f, -0.01f);
        rewardObject.transform.localRotation = Quaternion.identity;
        rewardObject.transform.localScale = Vector3.one;

        SpriteRenderer renderer = GetSingleComponent<SpriteRenderer>(rewardObject);
        renderer.sprite = rewardSprite;
        renderer.sortingLayerID = chestObject.GetComponent<SpriteRenderer>().sortingLayerID;
        renderer.sortingOrder = 6;

        BoxCollider2D trigger = GetSingleComponent<BoxCollider2D>(rewardObject);
        trigger.enabled = true;
        trigger.isTrigger = true;
        trigger.size = new Vector2(0.55f, 0.45f);
        trigger.offset = new Vector2(0f, -0.15f);

        ChestReward reward = GetSingleComponent<ChestReward>(rewardObject);
        chest.ConfigureReward(reward);

        EditorUtility.SetDirty(chest);
        EditorUtility.SetDirty(reward);
        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(trigger);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("MathAdventure Treasure Chest Reward Phase 2.2.3 setup completed successfully.");
    }

    private static GameObject GetOrCreateChild(Transform parent, string objectName)
    {
        Transform child = parent.Cast<Transform>().FirstOrDefault(item => item.name == objectName);
        if (child != null) return child.gameObject;

        var result = new GameObject(objectName);
        result.transform.SetParent(parent, false);
        return result;
    }

    private static T GetSingleComponent<T>(GameObject target) where T : Component
    {
        T[] components = target.GetComponents<T>();
        T result = components.Length == 0 ? target.AddComponent<T>() : components[0];
        for (int i = 1; i < components.Length; i++)
        {
            UnityEngine.Object.DestroyImmediate(components[i]);
        }

        return result;
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
}
