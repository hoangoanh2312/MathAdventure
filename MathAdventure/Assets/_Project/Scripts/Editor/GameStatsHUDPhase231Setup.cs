using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameStatsHUDPhase231Setup
{
    const string ScenePath="Assets/_Project/Scenes/Island01.unity", HeartPath="Assets/_Project/Sprites/UI/heart.png", StarPath="Assets/_Project/Sprites/UI/star.png", KeyPath="Assets/_Project/Sprites/UI/key.png", GoldPath="Assets/_Project/Sprites/Items/GoldCoin.png";

    [MenuItem("MathAdventure/Setup Game Stats HUD - Phase 2.3.1")]
    public static void Run()
    {
        ConfigureImporter(HeartPath); ConfigureImporter(StarPath); ConfigureImporter(KeyPath);
        Sprite heart=LoadSprite(HeartPath), star=LoadSprite(StarPath), key=LoadSprite(KeyPath);
        Sprite gold=AssetDatabase.LoadAllAssetsAtPath(GoldPath).OfType<Sprite>().FirstOrDefault(s=>s.name=="GoldCoin_0");
        if(gold==null) throw new InvalidOperationException("GoldCoin_0 was not found.");

        Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        GameStats stats=Single<GameStats>(Child(Root(scene,"Managers").transform,"GameStats",false));
        GameObject canvasObject=Find(scene,"Canvas"); ConfigureCanvas(canvasObject);
        GameObject hud=Child(canvasObject.transform,"HUD",true); ConfigureHud(hud); RemoveLegacy(hud.transform);
        Text lives=Group(hud.transform,"LivesGroup",heart,16f,"3",118f);
        Text score=Group(hud.transform,"ScoreGroup",star,184f,"0",118f);
        Text goldText=Group(hud.transform,"GoldGroup",gold,352f,"0",118f);
        Text keyText=Group(hud.transform,"KeyGroup",key,520f,"0/1",150f);
        Single<GameHUD>(hud).Configure(stats,lives,score,goldText,keyText);
        ChestReward reward=Find(scene,"Reward").GetComponent<ChestReward>();
        if(reward==null) throw new InvalidOperationException("TreasureChest/Reward requires ChestReward.");
        reward.Configure(stats);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        Debug.Log("MathAdventure Game Stats HUD Phase 2.3.1 setup completed successfully.");
    }

    static void ConfigureImporter(string path)
    {
        TextureImporter importer=AssetImporter.GetAtPath(path) as TextureImporter;
        if(importer==null) throw new InvalidOperationException(path+" was not found.");
        importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
        importer.mipmapEnabled=false; importer.alphaIsTransparency=true; importer.filterMode=FilterMode.Point;
        importer.textureCompression=TextureImporterCompression.Uncompressed; importer.npotScale=TextureImporterNPOTScale.None;
        importer.SaveAndReimport();
    }

    static Sprite LoadSprite(string path)=>AssetDatabase.LoadAssetAtPath<Sprite>(path)??throw new InvalidOperationException(path+" did not import as a Sprite.");
    static void ConfigureCanvas(GameObject target)
    {
        Single<Canvas>(target).renderMode=RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler=Single<CanvasScaler>(target); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1920,1080); scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight=.5f;
        Single<GraphicRaycaster>(target);
    }
    static void ConfigureHud(GameObject target)
    {
        RectTransform r=target.GetComponent<RectTransform>(); r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);
        r.anchoredPosition=new Vector2(16,-16); r.sizeDelta=new Vector2(704,76); r.localScale=Vector3.one;
        Image image=Single<Image>(target); image.color=new Color(.05f,.06f,.09f,.78f); image.raycastTarget=false;
    }
    static Text Group(Transform hud,string name,Sprite sprite,float x,string value,float width)
    {
        GameObject group=Child(hud,name,true); RectTransform g=group.GetComponent<RectTransform>();
        g.anchorMin=g.anchorMax=g.pivot=new Vector2(0,.5f); g.anchoredPosition=new Vector2(x,0); g.sizeDelta=new Vector2(width,60); g.localScale=Vector3.one;
        GameObject iconObject=Child(group.transform,"Icon",true); RectTransform ir=iconObject.GetComponent<RectTransform>();
        ir.anchorMin=ir.anchorMax=ir.pivot=new Vector2(0,.5f); ir.anchoredPosition=Vector2.zero; ir.sizeDelta=new Vector2(40,40); ir.localScale=Vector3.one;
        Image icon=Single<Image>(iconObject); icon.sprite=sprite; icon.preserveAspect=true; icon.raycastTarget=false; icon.color=Color.white;
        GameObject textObject=Child(group.transform,"ValueText",true); RectTransform tr=textObject.GetComponent<RectTransform>();
        tr.anchorMin=tr.anchorMax=tr.pivot=new Vector2(0,.5f); tr.anchoredPosition=new Vector2(50,0); tr.sizeDelta=new Vector2(width-50,48); tr.localScale=Vector3.one;
        Text text=Single<Text>(textObject); text.text=value; text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize=30;
        text.fontStyle=FontStyle.Bold; text.alignment=TextAnchor.MiddleLeft; text.color=Color.white; text.raycastTarget=false; return text;
    }
    static void RemoveLegacy(Transform hud)
    {
        string[] names={"LivesText","ScoreText","GoldText","KeyText"};
        foreach(Transform child in hud.Cast<Transform>().Where(c=>names.Contains(c.name)).ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
    }
    static GameObject Root(Scene scene,string name)=>scene.GetRootGameObjects().FirstOrDefault(o=>o.name==name)??new GameObject(name);
    static GameObject Child(Transform parent,string name,bool ui){Transform found=parent.Cast<Transform>().FirstOrDefault(c=>c.name==name);if(found!=null)return found.gameObject;GameObject result=ui?new GameObject(name,typeof(RectTransform)):new GameObject(name);result.transform.SetParent(parent,false);return result;}
    static T Single<T>(GameObject target)where T:Component{T[] all=target.GetComponents<T>();T result=all.Length==0?target.AddComponent<T>():all[0];for(int i=1;i<all.Length;i++)UnityEngine.Object.DestroyImmediate(all[i]);return result;}
    static GameObject Find(Scene scene,string name){GameObject result=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).Select(t=>t.gameObject).FirstOrDefault(o=>o.name==name);return result??throw new InvalidOperationException(name+" was not found in Island01.");}
}
