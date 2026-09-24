using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GoldCoinRewardPhase224Setup
{
    private const string ScenePath = "Assets/_Project/Scenes/Island01.unity";
    private const string CoinPath = "Assets/_Project/Sprites/Items/GoldCoin.png";
    private const string AnimationFolder = "Assets/_Project/Animations/Items";
    private const string ClipPath = AnimationFolder + "/GoldCoinSpin.anim";
    private const string ControllerPath = AnimationFolder + "/GoldCoin.controller";

    [MenuItem("MathAdventure/Setup Gold Coin Reward - Phase 2.2.4")]
    public static void Run()
    {
        Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(CoinPath).OfType<Sprite>()
            .Where(sprite => sprite.name.StartsWith("GoldCoin_", StringComparison.Ordinal))
            .OrderBy(sprite => int.Parse(sprite.name.Substring("GoldCoin_".Length)))
            .ToArray();
        if (frames.Length != 8 || frames.Select((sprite, index) => sprite.name == $"GoldCoin_{index}").Any(valid => !valid))
            throw new InvalidOperationException("GoldCoin.png must contain exactly GoldCoin_0 through GoldCoin_7.");

        EnsureFolder("Assets/_Project/Animations", "Items");
        AnimationClip clip = CreateOrUpdateClip(frames);
        AnimatorController controller = CreateOrUpdateController(clip);

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject chestObject = FindSceneObject(scene, "TreasureChest");
        Transform rewardTransform = chestObject.transform.Cast<Transform>().FirstOrDefault(child => child.name == "Reward");
        if (rewardTransform == null) throw new InvalidOperationException("Existing TreasureChest/Reward was not found.");

        GameObject rewardObject = rewardTransform.gameObject;
        rewardTransform.localPosition = new Vector3(0f, 0.8f, -0.01f);
        rewardTransform.localRotation = Quaternion.identity;
        rewardTransform.localScale = Vector3.one * 0.1f;

        SpriteRenderer renderer = GetSingle<SpriteRenderer>(rewardObject);
        renderer.sprite = frames[0];
        renderer.sortingLayerID = chestObject.GetComponent<SpriteRenderer>().sortingLayerID;
        renderer.sortingOrder = 6;

        Animator animator = GetSingle<Animator>(rewardObject);
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        BoxCollider2D trigger = GetSingle<BoxCollider2D>(rewardObject);
        trigger.enabled = true;
        trigger.isTrigger = true;
        trigger.size = new Vector2(5.5f, 5.5f);
        trigger.offset = Vector2.zero;

        ChestReward reward = GetSingle<ChestReward>(rewardObject);
        GetSingle<CollectibleBob>(rewardObject);
        chestObject.GetComponent<TreasureChest>().ConfigureReward(reward);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("MathAdventure Gold Coin Reward Phase 2.2.4 setup completed successfully.");
    }

    private static AnimationClip CreateOrUpdateClip(Sprite[] frames)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
        if (clip == null) { clip = new AnimationClip { name = "GoldCoinSpin" }; AssetDatabase.CreateAsset(clip, ClipPath); }
        clip.frameRate = 10f;
        ObjectReferenceKeyframe[] keys = frames.Select((sprite, index) => new ObjectReferenceKeyframe { time = index / 10f, value = sprite }).ToArray();
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite"), keys);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static AnimatorController CreateOrUpdateController(AnimationClip clip)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState child in machine.states) machine.RemoveState(child.state);
        AnimatorState state = machine.AddState("GoldCoinSpin"); state.motion = clip; machine.defaultState = state;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static void EnsureFolder(string parent, string child) { if (!AssetDatabase.IsValidFolder(parent + "/" + child)) AssetDatabase.CreateFolder(parent, child); }
    private static T GetSingle<T>(GameObject target) where T : Component { T[] all = target.GetComponents<T>(); T result = all.Length == 0 ? target.AddComponent<T>() : all[0]; for (int i = 1; i < all.Length; i++) UnityEngine.Object.DestroyImmediate(all[i]); return result; }
    private static GameObject FindSceneObject(Scene scene, string name) { GameObject result = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).Select(item => item.gameObject).FirstOrDefault(item => item.name == name); return result != null ? result : throw new InvalidOperationException(name + " was not found in Island01."); }
}
