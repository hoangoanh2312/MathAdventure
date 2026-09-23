using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class Island01Phase21Setup
{
    private const int WaterTileIndex = 11;
    private static readonly int[] SandTileIndices = { 27, 75, 81 };
    private const int ChestSpriteIndex = 97;
    private const int BarrelSpriteIndex = 98;
    private const int RedBarrelSpriteIndex = 99;
    private const string ScenePath = "Assets/_Project/Scenes/Island01.unity";
    private const string TilesetPath = "Assets/_Project/Sprites/Environment/CompleteTileSet.png";
    private const string TileFolder = "Assets/_Project/Tiles/Environment";

    [MenuItem("MathAdventure/Setup Island01 - Phase 2.1")]
    public static void Run()
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(TilesetPath)
            .OfType<Sprite>()
            .OrderBy(GetSpriteIndex)
            .ToArray();

        if (sprites.Length != 100)
            throw new InvalidOperationException($"Expected 100 CompleteTileSet sprites, found {sprites.Length}.");

        EnsureFolder("Assets/_Project/Tiles");
        EnsureFolder(TileFolder);
        Tile[] tiles = CreateOrUpdateTiles(sprites);

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject player = FindSceneObject(scene, "Player");
        GameObject environment = FindSceneObject(scene, "Environment");
        GameObject mainCameraObject = FindSceneObject(scene, "Main Camera");

        RemoveOldTestSprite(scene);
        ConfigureEnvironment(environment, tiles, sprites);
        ConfigurePlayer(player);
        ConfigureCamera(mainCameraObject, player.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("MathAdventure Island01 Phase 2.1 setup completed successfully.");
    }

    private static Tile[] CreateOrUpdateTiles(Sprite[] sprites)
    {
        var tiles = new Tile[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            string path = $"{TileFolder}/CompleteTileSet_{i}.asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }

            tile.sprite = sprites[i];
            tile.color = Color.white;
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            tiles[i] = tile;
        }

        return tiles;
    }

    private static void ConfigureEnvironment(GameObject environment, Tile[] tiles, Sprite[] sprites)
    {
        environment.transform.position = Vector3.zero;
        environment.transform.rotation = Quaternion.identity;
        environment.transform.localScale = Vector3.one;

        Grid grid = environment.GetComponent<Grid>();
        if (grid == null) grid = environment.AddComponent<Grid>();
        grid.cellSize = Vector3.one;
        grid.cellGap = Vector3.zero;
        grid.cellLayout = GridLayout.CellLayout.Rectangle;
        grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;

        GameObject water = GetOrCreateChild(environment.transform, "Water");
        ConfigureWaterBackground(water, tiles[WaterTileIndex].sprite);

        GameObject ground = GetOrCreateChild(environment.transform, "Ground");
        Tilemap groundTilemap = GetOrCreateTilemap(ground, -10);

        HashSet<Vector2Int> landCells = BuildPlayableIsland(groundTilemap, tiles);

        GameObject obstacles = GetOrCreateChild(environment.transform, "Obstacles");
        GameObject decorations = GetOrCreateChild(environment.transform, "Decorations");
        GameObject mapBounds = GetOrCreateChild(environment.transform, "MapBounds");
        GameObject spawnPoints = GetOrCreateChild(environment.transform, "SpawnPoints");

        obstacles.transform.localPosition = Vector3.zero;
        decorations.transform.localPosition = Vector3.zero;
        ConfigureObstacles(obstacles.transform, sprites);
        ConfigureIslandBoundary(mapBounds, landCells);

        GameObject playerSpawn = GetOrCreateChild(spawnPoints.transform, "PlayerSpawn");
        playerSpawn.transform.localPosition = Vector3.zero;
        playerSpawn.transform.localRotation = Quaternion.identity;
        playerSpawn.transform.localScale = Vector3.one;
    }

    private static Tilemap GetOrCreateTilemap(GameObject gameObject, int sortingOrder)
    {
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;
        gameObject.transform.localScale = Vector3.one;

        Tilemap tilemap = gameObject.GetComponent<Tilemap>();
        if (tilemap == null) tilemap = gameObject.AddComponent<Tilemap>();
        TilemapRenderer renderer = gameObject.GetComponent<TilemapRenderer>();
        if (renderer == null) renderer = gameObject.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;
        tilemap.ClearAllTiles();
        return tilemap;
    }

    private static void ConfigureWaterBackground(GameObject water, Sprite waterSprite)
    {
        water.transform.localPosition = Vector3.zero;
        water.transform.localRotation = Quaternion.identity;
        water.transform.localScale = Vector3.one;

        RemoveComponentIfPresent<TilemapCollider2D>(water);
        RemoveComponentIfPresent<TilemapRenderer>(water);
        RemoveComponentIfPresent<Tilemap>(water);

        SpriteRenderer renderer = water.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = water.AddComponent<SpriteRenderer>();
        renderer.sprite = waterSprite;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.tileMode = SpriteTileMode.Continuous;
        renderer.size = new Vector2(40f, 28f);
        renderer.sortingOrder = -20;
        renderer.color = Color.white;
    }

    private static HashSet<Vector2Int> BuildPlayableIsland(Tilemap ground, Tile[] tiles)
    {
        // Row widths and offsets form one connected, asymmetric island silhouette.
        int[] rowWidths = { 5, 9, 13, 16, 18, 18, 18, 17, 16, 14, 10, 6 };
        int[] rowOffsets = { 0, -1, 0, -1, 0, 0, 0, 1, 0, 0, -1, 0 };

        var landCells = new HashSet<Vector2Int>();
        for (int row = 0; row < rowWidths.Length; row++)
        {
            int y = row - 6;
            int width = rowWidths[row];
            int startX = -width / 2 + rowOffsets[row];

            for (int column = 0; column < width; column++)
            {
                int x = startX + column;
                int variation = Mathf.Abs(x * 3 + y * 5) % SandTileIndices.Length;
                ground.SetTile(new Vector3Int(x, y, 0), tiles[SandTileIndices[variation]]);
                landCells.Add(new Vector2Int(x, y));
            }
        }

        return landCells;
    }

    private static void ConfigureObstacles(Transform obstacles, Sprite[] sprites)
    {
        obstacles.localPosition = Vector3.zero;
        obstacles.localRotation = Quaternion.identity;
        obstacles.localScale = Vector3.one;

        ConfigureStaticObstacle(
            obstacles,
            "TreasureChest",
            sprites[ChestSpriteIndex],
            new Vector3(-5.5f, 2.8f, 0f),
            new Vector2(0.8f, 0.65f));
        ConfigureStaticObstacle(
            obstacles,
            "WoodenBarrel",
            sprites[BarrelSpriteIndex],
            new Vector3(5.7f, -2.7f, 0f),
            new Vector2(0.75f, 0.75f));
        ConfigureStaticObstacle(
            obstacles,
            "RedBarrel",
            sprites[RedBarrelSpriteIndex],
            new Vector3(6.2f, 1.7f, 0f),
            new Vector2(0.75f, 0.75f));
    }

    private static void ConfigureStaticObstacle(
        Transform parent,
        string objectName,
        Sprite sprite,
        Vector3 localPosition,
        Vector2 colliderSize)
    {
        GameObject obstacle = GetOrCreateChild(parent, objectName);
        obstacle.transform.localPosition = localPosition;
        obstacle.transform.localRotation = Quaternion.identity;
        obstacle.transform.localScale = Vector3.one;

        SpriteRenderer renderer = obstacle.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = obstacle.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 5;
        renderer.color = Color.white;

        BoxCollider2D collider = obstacle.GetComponent<BoxCollider2D>();
        if (collider == null) collider = obstacle.AddComponent<BoxCollider2D>();
        collider.isTrigger = false;
        collider.offset = Vector2.zero;
        collider.size = colliderSize;
    }

    private static void ConfigureIslandBoundary(GameObject mapBounds, HashSet<Vector2Int> landCells)
    {
        Transform parent = mapBounds.transform;
        parent.localPosition = Vector3.zero;
        parent.localRotation = Quaternion.identity;
        parent.localScale = Vector3.one;

        foreach (string wallName in new[] { "Top", "Bottom", "Left", "Right" })
        {
            Transform wall = parent.Find(wallName);
            if (wall != null) UnityEngine.Object.DestroyImmediate(wall.gameObject);
        }

        EdgeCollider2D boundary = mapBounds.GetComponent<EdgeCollider2D>();
        if (boundary == null) boundary = mapBounds.AddComponent<EdgeCollider2D>();

        List<Vector2> points = BuildBoundaryFromGround(landCells);
        boundary.points = points.ToArray();
        boundary.edgeRadius = 0.05f;
        boundary.isTrigger = false;
    }

    private static List<Vector2> BuildBoundaryFromGround(HashSet<Vector2Int> landCells)
    {
        var edges = new Dictionary<Vector2Int, Vector2Int>();

        foreach (Vector2Int cell in landCells)
        {
            int x = cell.x;
            int y = cell.y;

            if (!landCells.Contains(new Vector2Int(x, y - 1)))
                edges[new Vector2Int(x, y)] = new Vector2Int(x + 1, y);
            if (!landCells.Contains(new Vector2Int(x + 1, y)))
                edges[new Vector2Int(x + 1, y)] = new Vector2Int(x + 1, y + 1);
            if (!landCells.Contains(new Vector2Int(x, y + 1)))
                edges[new Vector2Int(x + 1, y + 1)] = new Vector2Int(x, y + 1);
            if (!landCells.Contains(new Vector2Int(x - 1, y)))
                edges[new Vector2Int(x, y + 1)] = new Vector2Int(x, y);
        }

        if (edges.Count == 0)
            throw new InvalidOperationException("Ground has no boundary edges.");

        Vector2Int start = edges.Keys.OrderBy(point => point.y).ThenBy(point => point.x).First();
        var rawLoop = new List<Vector2Int> { start };
        Vector2Int current = start;

        do
        {
            if (!edges.TryGetValue(current, out Vector2Int next))
                throw new InvalidOperationException($"Ground boundary is open at {current}.");

            rawLoop.Add(next);
            current = next;
        }
        while (current != start && rawLoop.Count <= edges.Count + 1);

        if (current != start)
            throw new InvalidOperationException("Ground boundary did not form a closed loop.");

        var simplified = new List<Vector2>();
        for (int i = 0; i < rawLoop.Count - 1; i++)
        {
            Vector2Int previous = rawLoop[(i - 1 + rawLoop.Count - 1) % (rawLoop.Count - 1)];
            Vector2Int point = rawLoop[i];
            Vector2Int next = rawLoop[(i + 1) % (rawLoop.Count - 1)];
            Vector2Int incoming = point - previous;
            Vector2Int outgoing = next - point;

            if (incoming != outgoing)
                simplified.Add(point);
        }

        simplified.Add(simplified[0]);
        return simplified;
    }

    private static void RemoveComponentIfPresent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component != null) UnityEngine.Object.DestroyImmediate(component);
    }

    private static void ConfigurePlayer(GameObject player)
    {
        // Only place the existing Phase 1 Player at the island spawn.
        player.transform.position = new Vector3(0f, 0f, 0.005f);

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private static void ConfigureCamera(GameObject cameraObject, Transform player)
    {
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.5f;

        cameraObject.transform.position = player.position + new Vector3(0f, 0f, -10.005f);
        cameraObject.transform.rotation = Quaternion.identity;

        CameraFollow follow = cameraObject.GetComponent<CameraFollow>();
        if (follow == null) follow = cameraObject.AddComponent<CameraFollow>();
        follow.Configure(
            player,
            0.2f,
            new Vector3(0f, 0f, -10.005f),
            new Vector2(-8f, -4.5f),
            new Vector2(8f, 4.5f));
        EditorUtility.SetDirty(follow);
    }

    private static void RemoveOldTestSprite(Scene scene)
    {
        GameObject testSprite = scene.GetRootGameObjects()
            .FirstOrDefault(item => item.name == "PirateCat_Animation.png_0");

        if (testSprite != null &&
            testSprite.GetComponents<Component>().All(component =>
                component is Transform || component is SpriteRenderer))
        {
            UnityEngine.Object.DestroyImmediate(testSprite);
        }
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

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        int separator = path.LastIndexOf('/');
        string parent = path[..separator];
        string name = path[(separator + 1)..];
        AssetDatabase.CreateFolder(parent, name);
    }

    private static int GetSpriteIndex(Sprite sprite)
    {
        string suffix = sprite.name.Split('_').Last();
        return int.TryParse(suffix, out int index) ? index : int.MaxValue;
    }
}
