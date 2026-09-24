using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TreasureChestPhase222Setup
{
    private const string ScenePath = "Assets/_Project/Scenes/Island01.unity";
    private const string TilesetPath = "Assets/_Project/Sprites/Environment/CompleteTileSet.png";
    private const int ClosedSpriteIndex = 96;
    private const int OpenSpriteIndex = 97;

    [MenuItem("MathAdventure/Setup Treasure Chest - Phase 2.2.2")]
    public static void Run()
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(TilesetPath)
            .OfType<Sprite>()
            .ToArray();

        Sprite openSprite = FindSprite(sprites, OpenSpriteIndex);
        Sprite closedSprite = FindSprite(sprites, ClosedSpriteIndex);

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject chestObject = FindSceneObject(scene, "TreasureChest");
        GameObject playerObject = FindSceneObject(scene, "Player");

        ConfigurePlayerPhysics(playerObject);

        SpriteRenderer renderer = chestObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
            throw new InvalidOperationException("TreasureChest requires its existing SpriteRenderer.");

        BoxCollider2D physicalCollider = chestObject.GetComponent<BoxCollider2D>();
        if (physicalCollider == null)
            throw new InvalidOperationException("TreasureChest requires its existing BoxCollider2D.");

        physicalCollider.enabled = true;
        physicalCollider.isTrigger = false;
        physicalCollider.size = new Vector2(0.9f, 0.65f);
        physicalCollider.offset = new Vector2(0f, -0.1f);

        // TestInteractable was retired for Phase 2.2.2. Remove its now-missing
        // serialized component before attaching the real chest behaviour.
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(chestObject);

        TreasureChest[] chestComponents = chestObject.GetComponents<TreasureChest>();
        TreasureChest chest = chestComponents.Length == 0
            ? chestObject.AddComponent<TreasureChest>()
            : chestComponents[0];

        for (int i = 1; i < chestComponents.Length; i++)
        {
            UnityEngine.Object.DestroyImmediate(chestComponents[i]);
        }

        chest.Configure(renderer, closedSprite, openSprite);
        EditorUtility.SetDirty(chest);
        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(physicalCollider);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("MathAdventure Treasure Chest Phase 2.2.2 setup completed successfully.");
    }

    private static void ConfigurePlayerPhysics(GameObject playerObject)
    {
        Rigidbody2D body = playerObject.GetComponent<Rigidbody2D>();
        CapsuleCollider2D physicalCollider = playerObject.GetComponent<CapsuleCollider2D>();
        if (body == null || physicalCollider == null)
            throw new InvalidOperationException("Player requires its existing Rigidbody2D and CapsuleCollider2D.");

        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        physicalCollider.enabled = true;
        physicalCollider.isTrigger = false;
        physicalCollider.size = new Vector2(0.65f, 0.35f);
        physicalCollider.offset = new Vector2(0f, 0.18f);
        physicalCollider.direction = CapsuleDirection2D.Horizontal;

        EditorUtility.SetDirty(body);
        EditorUtility.SetDirty(physicalCollider);
    }

    private static Sprite FindSprite(Sprite[] sprites, int index)
    {
        string spriteName = $"CompleteTileSet_{index}";
        Sprite sprite = sprites.FirstOrDefault(item => item.name == spriteName);
        return sprite != null
            ? sprite
            : throw new InvalidOperationException($"Required sprite {spriteName} was not found.");
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
