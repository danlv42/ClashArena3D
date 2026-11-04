using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates the first playable prototype at runtime.
/// No imported art or scene objects are required.
/// </summary>
public sealed class GameBootstrap : MonoBehaviour
{
    private static bool created;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateGame()
    {
        if (FindObjectOfType<GameBootstrap>() != null) return;
        var root = new GameObject("PixelQuest_Game");
        root.AddComponent<GameBootstrap>();
    }

    private void Awake()
    {
        if (created)
        {
            Destroy(gameObject);
            return;
        }

        created = true;
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
        BuildLevel();
    }

    private void BuildLevel()
    {
        Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 5.2f;
        camera.transform.position = new Vector3(0f, 1.2f, -10f);
        camera.backgroundColor = new Color(0.43f, 0.72f, 0.91f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.gameObject.AddComponent<AudioListener>();
        camera.gameObject.AddComponent<CameraFollow>();

        CreatePlatform(new Vector2(40f, -3.35f), new Vector2(100f, 1.2f), new Color(0.28f, 0.62f, 0.30f));
        CreatePlatform(new Vector2(7f, -0.9f), new Vector2(4.2f, 0.55f), new Color(0.54f, 0.36f, 0.20f));
        CreatePlatform(new Vector2(17f, 0.25f), new Vector2(4.0f, 0.55f), new Color(0.54f, 0.36f, 0.20f));
        CreatePlatform(new Vector2(29f, -0.45f), new Vector2(5.0f, 0.55f), new Color(0.54f, 0.36f, 0.20f));
        CreatePlatform(new Vector2(42f, 0.35f), new Vector2(4.5f, 0.55f), new Color(0.54f, 0.36f, 0.20f));
        CreatePlatform(new Vector2(56f, -0.5f), new Vector2(5.0f, 0.55f), new Color(0.54f, 0.36f, 0.20f));
        CreatePlatform(new Vector2(68f, 0.15f), new Vector2(4.0f, 0.55f), new Color(0.54f, 0.36f, 0.20f));

        CreateBackdrop();

        // Original platforming obstacles: thorn beds to jump over and solid crates to climb around.
        CreateSpikePatch(10.5f, 2.2f);
        CreateCrate(16f, 0.8f);
        CreateSpikePatch(26.5f, 2.0f);
        CreateCrate(34f, 0.9f);
        CreateSpikePatch(46f, 2.4f);
        CreateCrate(58f, 0.8f);
        CreateSpikePatch(66f, 2.0f);
        CreateCrate(73.5f, 0.9f);

        GameObject playerObject = CreateActor("Player", new Vector2(0f, -2.1f), new Color(0.20f, 0.34f, 0.92f), true);
        PlayerController player = playerObject.AddComponent<PlayerController>();
        player.SetCamera(camera);

        float[] enemyPositions = { 7f, 12f, 18f, 23f, 30f, 36f, 43f, 49f, 56f, 63f, 70f, 76f };
        for (int i = 0; i < enemyPositions.Length; i++)
        {
            float x = enemyPositions[i];
            float y = -2.15f;
            if (i == 2) y = 0.95f;
            if (i == 5) y = 0.25f;
            if (i == 8) y = 0.4f;
            if (i == 10) y = 0.8f;

            GameObject enemy = CreateActor("Enemy_" + (i + 1), new Vector2(x, y), i % 3 == 0 ? new Color(0.88f, 0.25f, 0.24f) : new Color(0.72f, 0.30f, 0.72f), false);
            EnemyWalker walker = enemy.AddComponent<EnemyWalker>();
            walker.Initialize(x - 1.5f, x + 1.5f, i % 2 == 0 ? 1.4f : 1.9f);
        }

        CreateSign(new Vector2(2.6f, -1.8f), "10 KILLS = POWER");
    }

    private void CreateCrate(float x, float width)
    {
        GameObject crate = new GameObject("Wooden_Crate");
        crate.transform.position = new Vector3(x, -2.18f, 0f);
        SpriteRenderer renderer = crate.AddComponent<SpriteRenderer>();
        renderer.sprite = PixelArt.MakeCrateSprite();
        renderer.sortingOrder = 3;
        BoxCollider2D collider = crate.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(width, 1.05f);
        crate.transform.localScale = new Vector3(width, 1.05f, 1f);
    }

    private void CreateSpikePatch(float x, float width)
    {
        GameObject spikes = new GameObject("Thorn_Patch");
        spikes.transform.position = new Vector3(x, -2.57f, 0f);
        SpriteRenderer renderer = spikes.AddComponent<SpriteRenderer>();
        renderer.sprite = PixelArt.MakeSpikeSprite();
        renderer.sortingOrder = 4;
        spikes.transform.localScale = new Vector3(width, 0.42f, 1f);
        BoxCollider2D collider = spikes.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f);
        spikes.AddComponent<Hazard>();
    }

    private GameObject CreateActor(string actorName, Vector2 position, Color color, bool isPlayer)
    {
        GameObject actor = new GameObject(actorName);
        actor.transform.position = position;

        SpriteRenderer renderer = actor.AddComponent<SpriteRenderer>();
        renderer.sprite = PixelArt.MakeActorSprite(isPlayer);
        renderer.color = isPlayer ? Color.white : color;
        renderer.sortingOrder = 5;

        BoxCollider2D collider = actor.AddComponent<BoxCollider2D>();
        collider.size = isPlayer ? new Vector2(0.68f, 1.12f) : new Vector2(0.72f, 0.78f);

        Rigidbody2D body = actor.AddComponent<Rigidbody2D>();
        body.freezeRotation = true;
        body.gravityScale = 3.4f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        return actor;
    }

    private void CreatePlatform(Vector2 position, Vector2 size, Color color)
    {
        GameObject platform = new GameObject("Platform");
        platform.transform.position = position;

        SpriteRenderer renderer = platform.AddComponent<SpriteRenderer>();
        renderer.sprite = PixelArt.MakeTerrainSprite(size, color);
        renderer.color = Color.white;
        renderer.sortingOrder = 0;

        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.size = size;
        collider.sharedMaterial = new PhysicsMaterial2D("Platform_Friction") { friction = 0.8f, bounciness = 0f };
    }

    private void CreateBackdrop()
    {
        // Layered mountain valley, distant pine forest and bright pixel sky.
        for (int i = 0; i < 18; i++)
        {
            float x = i * 6.2f - 4f;
            GameObject mountain = new GameObject("Far_Mountain");
            mountain.transform.position = new Vector3(x, -1.8f + (i % 3) * 0.22f, 3.2f);
            mountain.transform.localScale = new Vector3(3.2f, 3.0f, 1f);
            SpriteRenderer mr = mountain.AddComponent<SpriteRenderer>();
            mr.sprite = PixelArt.MakeMountainSprite(i);
            mr.sortingOrder = -5;

            GameObject pine = new GameObject("Distant_Pine");
            pine.transform.position = new Vector3(x + 2.8f, -2.15f, 2.6f);
            pine.transform.localScale = new Vector3(1.1f, 2.0f, 1f);
            SpriteRenderer pr = pine.AddComponent<SpriteRenderer>();
            pr.sprite = PixelArt.MakePineSprite(i);
            pr.sortingOrder = -4;
        }

        GameObject sun = new GameObject("Pixel_Sun");
        sun.transform.position = new Vector3(9f, 3.15f, 2.4f);
        sun.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        SpriteRenderer sr = sun.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.MakeSunSprite();
        sr.sortingOrder = -3;

        for (int i = 0; i < 14; i++)
        {
            float x = i * 7.6f + 1f;
            GameObject cloud = new GameObject("Cloud");
            cloud.transform.position = new Vector3(x, 3.0f + (i % 4) * 0.65f, 3f);
            cloud.transform.localScale = new Vector3(2.0f, 1.1f, 1f);
            SpriteRenderer cr = cloud.AddComponent<SpriteRenderer>();
            cr.sprite = PixelArt.MakeCloudSprite();
            cr.sortingOrder = -2;
        }

        for (int i = 0; i < 20; i++)
        {
            float x = i * 5.2f - 1f;
            GameObject tree = new GameObject("Pixel_Tree");
            tree.transform.position = new Vector3(x, -1.95f, 0.8f);
            tree.transform.localScale = new Vector3(2.25f, 2.65f, 1f);
            SpriteRenderer tr = tree.AddComponent<SpriteRenderer>();
            tr.sprite = PixelArt.MakeTreeSprite(i);
            tr.sortingOrder = 1;
        }

        for (int i = 0; i < 48; i++)
        {
            GameObject tuft = new GameObject("Grass_Tuft");
            tuft.transform.position = new Vector3(i * 1.9f + 0.4f, -2.72f, -0.1f);
            SpriteRenderer gr = tuft.AddComponent<SpriteRenderer>();
            gr.sprite = PixelArt.MakeGrassTuftSprite(i);
            gr.sortingOrder = 2;
        }
    }

    private void CreateSign(Vector2 position, string message)
    {
        GameObject sign = new GameObject("Tutorial_Sign");
        sign.transform.position = position;
        TextMesh text = sign.AddComponent<TextMesh>();
        text.text = message;
        text.fontSize = 48;
        text.characterSize = 0.075f;
        text.anchor = TextAnchor.MiddleCenter;
        text.color = new Color(0.12f, 0.12f, 0.16f);
        text.alignment = TextAlignment.Center;
        sign.GetComponent<MeshRenderer>().sortingOrder = 10;
    }
}

public static class PixelArt
{
    private static Sprite whiteSprite;

    public static Sprite WhiteSprite
    {
        get
        {
            if (whiteSprite == null)
            {
                Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                texture.SetPixel(0, 0, Color.white);
                texture.filterMode = FilterMode.Point;
                texture.Apply();
                whiteSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }
            return whiteSprite;
        }
    }

    private static readonly Dictionary<int, Sprite> actorFrames = new Dictionary<int, Sprite>();

    public static Sprite MakeActorSprite(bool player)
    {
        return MakeActorSprite(player, 0);
    }

    private static Sprite MakeDancerSprite(int pose)
    {
        int key = 1000 + pose;
        Sprite cached;
        if (actorFrames.TryGetValue(key, out cached) && cached != null) return cached;

        const int width = 24;
        const int height = 26;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color outline = new Color(0.07f, 0.06f, 0.10f);
        Color suit = new Color(0.95f, 0.94f, 0.88f);
        Color shade = new Color(0.62f, 0.68f, 0.76f);
        Color skin = new Color(0.72f, 0.43f, 0.30f);
        Color hair = new Color(0.08f, 0.06f, 0.06f);
        Color black = new Color(0.07f, 0.07f, 0.10f);
        int legA = pose == 1 ? -3 : (pose == 2 ? 3 : (pose == 3 ? -2 : 0));
        int legB = -legA;
        int armRaise = pose == 3 || pose == 4 ? 3 : 0;

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            Color px = clear;

            // Oversized expressive head, curls, and compact torso.
            bool head = x >= 5 && x <= 18 && y >= 14 && y <= 22;
            bool curls = y >= 16 && y <= 22 &&
                ((x >= 3 && x <= 5) || (x >= 18 && x <= 20) ||
                 (y >= 20 && y <= 22 && x >= 6 && x <= 17));
            bool torso = x >= 8 && x <= 15 && y >= 7 && y <= 14;
            bool leftArm = x >= 4 && x <= 8 && y >= 9 + armRaise && y <= 14 + armRaise;
            bool rightArm = x >= 15 && x <= 20 && y >= 9 && y <= 14 + (pose == 3 ? 2 : 0);
            bool leftLeg = x >= 8 + legA && x <= 10 + legA && y >= 2 && y <= 7;
            bool rightLeg = x >= 13 + legB && x <= 15 + legB && y >= 2 && y <= 7;

            if (curls) px = hair;
            if (head) px = (x == 5 || x == 18 || y == 14 || y == 22) ? outline : skin;
            if (torso) px = (x == 8 || x == 15 || y == 7) ? outline : ((x == 11 || x == 12) ? shade : suit);
            if (leftArm || rightArm) px = (x == 4 || x == 20) ? outline : suit;
            if (leftLeg || rightLeg) px = (x == 8 + legA || x == 15 + legB) ? shade : suit;
            if (y == 1 && ((x >= 6 + legA && x <= 11 + legA) || (x >= 12 + legB && x <= 17 + legB))) px = black;
            if (y == 0 && ((x >= 5 + legA && x <= 11 + legA) || (x >= 12 + legB && x <= 18 + legB))) px = outline;

            // Fedora: broad brim, crown and dark band.
            if (y == 23 && x >= 3 && x <= 20) px = outline;
            if (y == 24 && x >= 6 && x <= 17) px = suit;
            if (y == 25 && x >= 8 && x <= 15) px = suit;
            if (y == 24 && x >= 8 && x <= 15) px = black;

            // Face, tie and signature white glove.
            if (x == 9 && y == 18) px = outline;
            if (x == 14 && y == 18) px = outline;
            if (x >= 11 && x <= 12 && y >= 10 && y <= 13) px = black;
            if (pose == 3 && x >= 20 && y >= 12 && y <= 15) px = new Color(1f, 0.86f, 0.3f);
            if (pose == 5 && x == 20 && y == 19) px = new Color(1f, 0.35f, 0.25f);

            texture.SetPixel(x, y, px);
        }

        texture.Apply();
        Sprite frame = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.12f), 20f);
        actorFrames[key] = frame;
        return frame;
    }

    public static Sprite MakeCrateSprite()
    {
        const int w = 16, h = 16;
        Texture2D t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point;
        Color wood = new Color(0.48f, 0.25f, 0.10f);
        Color light = new Color(0.72f, 0.43f, 0.19f);
        Color dark = new Color(0.18f, 0.10f, 0.06f);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            Color px = (x == 0 || y == 0 || x == 15 || y == 15) ? dark : wood;
            if (x == y || x == 15 - y) px = light;
            if (x == 7 || x == 8 || y == 7 || y == 8) px = dark;
            if ((x == 2 || x == 13) && (y == 2 || y == 13)) px = light;
            t.SetPixel(x, y, px);
        }
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }

    public static Sprite MakeSpikeSprite()
    {
        const int w = 24, h = 10;
        Texture2D t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color thorn = new Color(0.16f, 0.28f, 0.12f);
        Color tip = new Color(0.82f, 0.88f, 0.58f);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            Color px = clear;
            if (y <= 1) px = thorn;
            for (int spike = 0; spike < 4; spike++)
            {
                int center = 3 + spike * 6;
                int height = 5 + (spike % 2) * 2;
                int distance = Mathf.Abs(x - center);
                if (distance <= 2 && y >= 2 && y <= height - distance) px = y == height - distance ? tip : thorn;
            }
            t.SetPixel(x, y, px);
        }
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }

    public static Sprite MakeTerrainSprite(Vector2 worldSize, Color baseColor)
    {
        int width = Mathf.Clamp(Mathf.CeilToInt(worldSize.x * 16f), 16, 2048);
        int height = Mathf.Clamp(Mathf.CeilToInt(worldSize.y * 16f), 8, 64);
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color grass = new Color(0.18f, 0.58f, 0.20f);
        Color brightGrass = new Color(0.38f, 0.78f, 0.25f);
        Color soil = baseColor;
        Color darkSoil = new Color(baseColor.r * 0.58f, baseColor.g * 0.55f, baseColor.b * 0.5f);
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            Color px = y >= height - 4 ? grass : soil;
            if (y == height - 1 && (x % 7 < 5)) px = brightGrass;
            if (y < height - 4 && ((x * 17 + y * 31) % 29 == 0)) px = darkSoil;
            if (y < height - 4 && ((x * 11 + y * 7) % 47 == 0)) px = new Color(0.72f, 0.48f, 0.28f);
            if (y == height - 5 && x % 13 < 2) px = darkSoil;
            texture.SetPixel(x, y, px);
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 16f);
    }

    public static Sprite MakeSunSprite()
    {
        const int w = 24, h = 24;
        Texture2D t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point;
        Color clear = new Color(0,0,0,0), gold = new Color(1f,0.72f,0.12f), light = new Color(1f,0.88f,0.36f);
        for (int y=0;y<h;y++) for (int x=0;x<w;x++)
        {
            int dx=x-12, dy=y-12;
            bool ray=((x==11||x==12)&&(y<4||y>19))||((y==11||y==12)&&(x<4||x>19))||(Mathf.Abs(dx)==Mathf.Abs(dy)&&Mathf.Abs(dx)>=8);
            Color px=dx*dx+dy*dy<=64?(dx*dx+dy*dy>42?light:gold):(ray?gold:clear);
            t.SetPixel(x,y,px);
        }
        t.Apply();
        return Sprite.Create(t,new Rect(0,0,w,h),new Vector2(0.5f,0.5f),16f);
    }

    public static Sprite MakePineSprite(int seed)
    {
        const int w=24,h=44;
        Texture2D t=new Texture2D(w,h,TextureFormat.RGBA32,false);
        t.filterMode=FilterMode.Point;
        Color clear=new Color(0,0,0,0), trunk=new Color(0.28f,0.16f,0.10f), dark=new Color(0.08f,0.25f,0.24f), green=new Color(0.12f,0.42f,0.36f);
        for(int y=0;y<h;y++) for(int x=0;x<w;x++)
        {
            Color px=clear;
            if(x>=10&&x<=13&&y<9) px=trunk;
            int half=Mathf.Clamp((y-5)/2,1,10);
            if(y>=5&&y<40&&Mathf.Abs(x-12)<=half) px=((x+y+seed)%5==0)?green:dark;
            if(y>=13&&y<16&&x>=7&&x<=17) px=green;
            t.SetPixel(x,y,px);
        }
        t.Apply();
        return Sprite.Create(t,new Rect(0,0,w,h),new Vector2(0.5f,0f),16f);
    }

    public static Sprite MakeGrassTuftSprite(int seed)
    {
        const int w=12,h=8;
        Texture2D t=new Texture2D(w,h,TextureFormat.RGBA32,false);
        t.filterMode=FilterMode.Point;
        Color clear=new Color(0,0,0,0), green=new Color(0.14f,0.48f,0.12f), bright=new Color(0.40f,0.78f,0.18f);
        for(int y=0;y<h;y++) for(int x=0;x<w;x++)
        {
            bool blade=(x==2&&y<=4)||(x==3&&y<=6)||(x==5&&y<=5)||(x==7&&y<=7)||(x==9&&y<=4);
            t.SetPixel(x,y,blade?(((x+y+seed)%3==0)?bright:green):clear);
        }
        t.Apply();
        return Sprite.Create(t,new Rect(0,0,w,h),new Vector2(0.5f,0f),16f);
    }

    public static Sprite MakeMountainSprite(int seed)
    {
        const int w = 48, h = 32;
        Texture2D t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point;
        Color clear = new Color(0,0,0,0);
        Color mountain = new Color(0.35f,0.60f,0.61f,0.8f);
        Color snow = new Color(0.72f,0.82f,0.78f,0.85f);
        int peak = 22 + seed % 8;
        for (int y=0;y<h;y++)
        for (int x=0;x<w;x++)
        {
            int ridge = peak - Mathf.Abs(x-24)/2;
            Color px = y < ridge ? clear : mountain;
            if (y >= ridge && y < ridge + 3 && (x < 24 ? x > 19-(ridge-y) : x < 29+(ridge-y))) px = snow;
            t.SetPixel(x,y,px);
        }
        t.Apply();
        return Sprite.Create(t,new Rect(0,0,w,h),new Vector2(0.5f,0.5f),16f);
    }

    public static Sprite MakeCloudSprite()
    {
        const int w=32,h=12;
        Texture2D t=new Texture2D(w,h,TextureFormat.RGBA32,false);
        t.filterMode=FilterMode.Point;
        Color clear=new Color(0,0,0,0), cloud=new Color(0.94f,0.97f,1f,0.9f);
        for(int y=0;y<h;y++) for(int x=0;x<w;x++)
        {
            bool body=(y>=2&&y<=7&&x>=3&&x<=28)||(y>=5&&y<=10&&x>=8&&x<=22);
            t.SetPixel(x,y,body?cloud:clear);
        }
        t.Apply();
        return Sprite.Create(t,new Rect(0,0,w,h),new Vector2(0.5f,0.5f),16f);
    }

    public static Sprite MakeTreeSprite(int seed)
    {
        const int w=24,h=40;
        Texture2D t=new Texture2D(w,h,TextureFormat.RGBA32,false);
        t.filterMode=FilterMode.Point;
        Color clear=new Color(0,0,0,0), trunk=new Color(0.34f,0.18f,0.09f), leaf=new Color(0.08f,0.40f,0.16f), light=new Color(0.22f,0.62f,0.20f);
        for(int y=0;y<h;y++) for(int x=0;x<w;x++)
        {
            Color px=clear;
            if(x>=10&&x<=13&&y<25) px=trunk;
            int dx=x-12, dy=y-28;
            int radius=8+(seed%3);
            if(dx*dx+dy*dy<radius*radius) px=((x*7+y*3+seed)%5==0)?light:leaf;
            if(y==39&&x>=8&&x<=15) px=light;
            t.SetPixel(x,y,px);
        }
        t.Apply();
        return Sprite.Create(t,new Rect(0,0,w,h),new Vector2(0.5f,0f),16f);
    }

    // Pose: 0 idle, 1/2 run cycle, 3 attack, 4 jump, 5 hurt.
    public static Sprite MakeActorSprite(bool player, int pose)
    {
        if (player) return MakeDancerSprite(pose);
        int key = (player ? 100 : 0) + pose;
        Sprite cached;
        if (actorFrames.TryGetValue(key, out cached) && cached != null) return cached;

        const int width = 16;
        const int height = 20;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color outline = new Color(0.10f, 0.08f, 0.14f);
        Color skin = new Color(1f, 0.72f, 0.48f);
        Color cloth = player ? new Color(0.22f, 0.42f, 0.98f) : new Color(0.82f, 0.24f, 0.28f);
        Color darkCloth = player ? new Color(0.10f, 0.20f, 0.52f) : new Color(0.35f, 0.10f, 0.20f);

        int bodyBottom = pose == 4 ? 5 : 4;
        int legShiftA = pose == 1 ? -2 : (pose == 2 ? 2 : 0);
        int legShiftB = -legShiftA;
        int armY = pose == 3 ? 10 : (pose == 4 ? 12 : 8);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = clear;
                bool head = x >= 4 && x <= 11 && y >= 14 && y <= 18;
                bool body = x >= 4 && x <= 11 && y >= bodyBottom && y <= 13;
                bool leftLeg = x >= 4 + legShiftA && x <= 6 + legShiftA && y >= 1 && y <= bodyBottom;
                bool rightLeg = x >= 9 + legShiftB && x <= 11 + legShiftB && y >= 1 && y <= bodyBottom;
                bool leftArm = x >= 1 && x <= 4 && y >= armY - 2 && y <= armY;
                bool rightArm = pose == 3 ? (x >= 11 && x <= 15 && y >= armY && y <= armY + 2)
                                           : (x >= 11 && x <= 14 && y >= armY - 2 && y <= armY);

                if (head) pixel = (x == 4 || x == 11 || y == 18) ? outline : skin;
                if (body) pixel = (x == 4 || x == 11 || y == bodyBottom) ? outline : cloth;
                if (leftLeg || rightLeg)
                {
                    bool foot = y <= 2;
                    pixel = foot ? outline : darkCloth;
                }
                if (leftArm || rightArm) pixel = (x == 1 || x == 14 || y == armY - 2) ? outline : cloth;
                if (head && y >= 15 && (x == 7 || x == 10)) pixel = outline;

                if (pose == 3 && x == 15 && y == armY + 2) pixel = new Color(1f, 0.9f, 0.35f);
                if (pose == 5 && (x == 2 || x == 13) && y == 16) pixel = new Color(1f, 0.35f, 0.25f);
                if (player && y == 18 && x >= 4 && x <= 11) pixel = darkCloth;

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        Sprite frame = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.35f), 16f);
        actorFrames[key] = frame;
        return frame;
    }
}
