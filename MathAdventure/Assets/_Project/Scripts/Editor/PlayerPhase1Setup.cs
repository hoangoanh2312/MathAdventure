using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class PlayerPhase1Setup
{
    private const string SheetPath = "Assets/_Project/Animations/PirateCat_Animation.png.png";
    private const string AnimationFolder = "Assets/_Project/Animations/Player";
    private const string ControllerPath = AnimationFolder + "/Player.controller";
    private const string ScenePath = "Assets/_Project/Scenes/Island01.unity";

    [MenuItem("MathAdventure/Setup Player - Phase 1")]
    public static void Run()
    {
        ConfigureSpritePivots();

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(SheetPath)
            .OfType<Sprite>()
            .OrderBy(GetSpriteIndex)
            .ToArray();

        if (sprites.Length != 20)
            throw new InvalidOperationException($"Expected 20 PirateCat sprites, found {sprites.Length}.");

        var clips = new Dictionary<string, AnimationClip>
        {
            ["IdleDown"] = CreateOrUpdateClip("Player_IdleDown", new[] { sprites[0] }, 6f),
            ["IdleUp"] = CreateOrUpdateClip("Player_IdleUp", new[] { sprites[8] }, 6f),
            ["IdleLeft"] = CreateOrUpdateClip("Player_IdleLeft", new[] { sprites[12] }, 6f),
            ["IdleRight"] = CreateOrUpdateClip("Player_IdleRight", new[] { sprites[16] }, 6f),
            ["WalkDown"] = CreateOrUpdateClip("Player_WalkDown", sprites[4..8], 8f),
            ["WalkUp"] = CreateOrUpdateClip("Player_WalkUp", sprites[8..12], 8f),
            ["WalkLeft"] = CreateOrUpdateClip("Player_WalkLeft", sprites[12..16], 8f),
            ["WalkRight"] = CreateOrUpdateClip("Player_WalkRight", sprites[16..20], 8f)
        };

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        ConfigureController(controller, clips);
        ConfigureScene(controller, sprites[0]);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("MathAdventure Player Phase 1 setup completed successfully.");
    }

    private static void ConfigureSpritePivots()
    {
        TextureImporter importer = AssetImporter.GetAtPath(SheetPath) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException("PirateCat sprite sheet importer was not found.");

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        SpriteRect[] spriteRects = dataProvider.GetSpriteRects();
        for (int i = 0; i < spriteRects.Length; i++)
        {
            spriteRects[i].alignment = SpriteAlignment.BottomCenter;
            spriteRects[i].pivot = new Vector2(0.5f, 0f);
        }

        dataProvider.SetSpriteRects(spriteRects);
        dataProvider.Apply();
        importer.SaveAndReimport();
    }

    private static AnimationClip CreateOrUpdateClip(string fileName, Sprite[] frames, float frameRate)
    {
        string path = $"{AnimationFolder}/{fileName}.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

        if (clip == null)
        {
            clip = new AnimationClip { name = fileName };
            AssetDatabase.CreateAsset(clip, path);
        }

        clip.frameRate = frameRate;
        var keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / frameRate,
                value = frames[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(
            clip,
            EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite"),
            keys);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = frames.Length > 1;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static void ConfigureController(AnimatorController controller, Dictionary<string, AnimationClip> clips)
    {
        if (controller.layers.Length == 0)
            controller.AddLayer("Base Layer");

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState childState in stateMachine.states)
            stateMachine.RemoveState(childState.state);

        foreach (KeyValuePair<string, AnimationClip> entry in clips)
        {
            AnimatorState state = stateMachine.AddState(entry.Key);
            state.motion = entry.Value;
            state.writeDefaultValues = true;
            if (entry.Key == "IdleDown")
                stateMachine.defaultState = state;
        }

        EditorUtility.SetDirty(controller);
    }

    private static void ConfigureScene(RuntimeAnimatorController controller, Sprite defaultSprite)
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject player = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Select(item => item.gameObject)
            .FirstOrDefault(item => item.name == "Player");

        if (player == null)
            throw new InvalidOperationException("Player GameObject was not found in Island01.");

        Animator animator = player.GetComponent<Animator>() ?? player.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
        renderer.sprite = defaultSprite;

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CapsuleCollider2D physicalCollider = player.GetComponent<CapsuleCollider2D>();
        physicalCollider.enabled = true;
        physicalCollider.isTrigger = false;
        physicalCollider.size = new Vector2(0.65f, 0.35f);
        physicalCollider.offset = new Vector2(0f, 0.18f);
        physicalCollider.direction = CapsuleDirection2D.Horizontal;

        if (player.GetComponent<PlayerMovement>() == null)
            player.AddComponent<PlayerMovement>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static int GetSpriteIndex(Sprite sprite)
    {
        string suffix = sprite.name.Split('_').Last();
        return int.TryParse(suffix, out int index) ? index : int.MaxValue;
    }
}
