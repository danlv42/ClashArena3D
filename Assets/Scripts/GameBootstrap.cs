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

    private GameObject CreateActor(string actorName, Vector2 position, Color color, bool isPlayer)
    {
        GameObject actor = new GameObject(actorName);
        actor.transform.position = position;

        SpriteRenderer renderer = actor.AddComponent<SpriteRenderer>();
        renderer.sprite = PixelArt.MakeActorSprite(isPlayer);
        renderer.color = color;
        renderer.sortingOrder = 5;

        BoxCollider2D collider = actor.AddComponent<BoxCollider2D>();
        collider.size = isPlayer ? new Vector2(0.72f, 0.95f) : new Vector2(0.72f, 0.78f);

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
        platform.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = platform.AddComponent<SpriteRenderer>();
        renderer.sprite = PixelArt.WhiteSprite;
        renderer.color = color;
        renderer.sortingOrder = 0;

        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        collider.sharedMaterial = new PhysicsMaterial2D("Platform_Friction") { friction = 0.8f, bounciness = 0f };
    }

    private void CreateBackdrop()
    {
        for (int i = 0; i < 14; i++)
        {
            float x = i * 7.5f - 3f;
            GameObject hill = new GameObject("Distant_Hill");
            hill.transform.position = new Vector3(x, -2.65f, 2f);
            hill.transform.localScale = new Vector3(8f, 2.4f, 1f);
            SpriteRenderer renderer = hill.AddComponent<SpriteRenderer>();
            renderer.sprite = PixelArt.WhiteSprite;
            renderer.color = new Color(0.34f, 0.66f, 0.58f, 0.55f);
            renderer.sortingOrder = -2;
        }

        for (int i = 0; i < 12; i++)
        {
            float x = i * 8f + 1f;
            GameObject cloud = new GameObject("Cloud");
            cloud.transform.position = new Vector3(x, 3.2f + (i % 3) * 0.55f, 3f);
            cloud.transform.localScale = new Vector3(1.8f, 0.55f, 1f);
            SpriteRenderer renderer = cloud.AddComponent<SpriteRenderer>();
            renderer.sprite = PixelArt.WhiteSprite;
            renderer.color = new Color(1f, 1f, 1f, 0.72f);
            renderer.sortingOrder = -1;
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

    public static Sprite MakeActorSprite(bool player)
    {
        const int width = 16;
        const int height = 20;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color outline = new Color(0.12f, 0.10f, 0.16f);
        Color skin = new Color(1f, 0.72f, 0.48f);
        Color cloth = player ? new Color(0.22f, 0.42f, 0.98f) : new Color(0.82f, 0.24f, 0.28f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool body = x >= 3 && x <= 12 && y >= 2 && y <= 13;
                bool head = x >= 4 && x <= 11 && y >= 13 && y <= 18;
                bool feet = y <= 2 && ((x >= 2 && x <= 6) || (x >= 9 && x <= 13));
                Color pixel = clear;

                if (body || head || feet)
                {
                    bool edge = (body && (x == 3 || x == 12 || y == 2 || y == 13)) ||
                                (head && (x == 4 || x == 11 || y == 13 || y == 18)) ||
                                (feet && y == 0);
                    pixel = edge ? outline : (head ? skin : (feet ? outline : cloth));
                }

                if (head && y >= 15 && (x == 6 || x == 10)) pixel = outline;
                if (player && y == 18 && x >= 3 && x <= 12) pixel = new Color(0.12f, 0.16f, 0.42f);
                if (!player && y == 17 && x >= 3 && x <= 12) pixel = new Color(0.24f, 0.12f, 0.24f);

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.35f), 16f);
    }
}

public sealed class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public int killsToCharge = 10;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private CameraFollow cameraFollow;
    private int facing = 1;
    private int kills;
    private int health = 3;
    private float attackReadyAt;
    private float invulnerableUntil;
    private bool grounded;
        private string statusMessage = "Defeat 10 enemies to charge your special!";
    private float statusUntil;

    public int Kills { get { return kills; } }
    public int Health { get { return health; } }
    public int ChargeNeeded { get { return killsToCharge; } }
    public bool AbilityReady { get { return kills >= killsToCharge; } }
    public string StatusMessage { get { return Time.time < statusUntil ? statusMessage : ""; } }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetCamera(Camera camera)
    {
        cameraFollow = camera.GetComponent<CameraFollow>();
        cameraFollow.SetTarget(transform);
    }

    private void Update()
    {
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;

        body.velocity = new Vector2(horizontal * moveSpeed, body.velocity.y);

        if (horizontal != 0f)
        {
            facing = horizontal > 0f ? 1 : -1;
            spriteRenderer.flipX = facing < 0;
        }

        grounded = Mathf.Abs(body.velocity.y) < 0.08f && Physics2D.Raycast(transform.position, Vector2.down, 0.8f, LayerMask.GetMask("Default"));
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && grounded)
        {
            body.velocity = new Vector2(body.velocity.x, jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Z))
            Attack();

        if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.X))
            UseAbility();

        if (transform.position.y < -12f)
            Respawn();
    }

    private void Attack()
    {
        if (Time.time < attackReadyAt) return;
        attackReadyAt = Time.time + 0.28f;

        Vector2 center = (Vector2)transform.position + new Vector2(facing * 0.88f, 0.05f);
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, 1.15f);
        bool hitSomething = false;

        foreach (Collider2D hit in hits)
        {
            EnemyWalker enemy = hit.GetComponent<EnemyWalker>();
            if (enemy != null)
            {
                enemy.Defeat();
                hitSomething = true;
            }
        }

        ShowStatus(hitSomething ? "Nice hit!" : "Swing!");
    }

    private void UseAbility()
    {
        if (!AbilityReady)
        {
            ShowStatus("Need " + (killsToCharge - kills) + " more kills!");
            return;
        }

        kills -= killsToCharge;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 4.5f);
        int defeated = 0;
        foreach (Collider2D hit in hits)
        {
            EnemyWalker enemy = hit.GetComponent<EnemyWalker>();
            if (enemy != null)
            {
                enemy.Defeat();
                defeated++;
            }
        }

        body.velocity = new Vector2(body.velocity.x, Mathf.Max(body.velocity.y, 3f));
        ShowStatus("POWER BURST! " + defeated + " enemies hit");
    }

    public void RegisterKill()
    {
        kills++;
        if (kills >= killsToCharge)
            ShowStatus("SPECIAL READY! Press K or X");
        else
            ShowStatus("Enemy defeated: " + kills + "/" + killsToCharge);
    }

    public void TryStomp(EnemyWalker enemy)
    {
        if (body.velocity.y < -0.5f && transform.position.y > enemy.transform.position.y + 0.15f)
        {
            enemy.Defeat();
            body.velocity = new Vector2(body.velocity.x, jumpForce * 0.72f);
        }
        else if (Time.time > invulnerableUntil)
        {
            TakeHit();
        }
    }

    private void TakeHit()
    {
        health--;
        invulnerableUntil = Time.time + 1.1f;
        body.velocity = new Vector2(-facing * 5f, 5.5f);
        ShowStatus("Ouch! HP: " + health);

        if (health <= 0)
            Respawn();
    }

    private void Respawn()
    {
        health = 3;
        transform.position = new Vector3(0f, -1.8f, 0f);
        body.velocity = Vector2.zero;
        invulnerableUntil = Time.time + 1.5f;
        ShowStatus("Back in the game!");
    }

    private void ShowStatus(string message)
    {
        statusMessage = message;
        statusUntil = Time.time + 1.5f;
    }

    private void OnGUI()
    {
        GUIStyle title = new GUIStyle(GUI.skin.label);
        title.fontSize = 22;
        title.fontStyle = FontStyle.Bold;
        title.normal.textColor = Color.white;

        GUIStyle text = new GUIStyle(GUI.skin.label);
        text.fontSize = 17;
        text.normal.textColor = Color.white;

        GUIStyle panel = new GUIStyle(GUI.skin.box);
        panel.normal.background = MakePanelTexture();

        GUI.Box(new Rect(14, 14, 310, 112), GUIContent.none, panel);
        GUI.Label(new Rect(28, 20, 280, 30), "PIXEL QUEST  •  WORLD 1", title);
        GUI.Label(new Rect(28, 52, 280, 24), "HP: " + new string('♥', health), text);
        GUI.Label(new Rect(28, 77, 280, 24), "KILLS: " + kills + " / " + killsToCharge, text);

        Rect bar = new Rect(28, 101, 270, 12);
        GUI.color = new Color(0.12f, 0.12f, 0.16f);
        GUI.DrawTexture(bar, Texture2D.whiteTexture);
        GUI.color = new Color(1f, 0.72f, 0.18f);
        GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01((float)kills / killsToCharge), bar.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        string abilityLabel = AbilityReady ? "SPECIAL: READY  [K]" : "SPECIAL: " + kills + "/" + killsToCharge + "  [K]";
        GUI.Label(new Rect(18, 136, 300, 26), abilityLabel, title);
        GUI.Label(new Rect(18, Screen.height - 34, 620, 24), "MOVE: A/D or arrows    J/Z: ATTACK    SPACE: JUMP    K/X: SPECIAL", text);

        if (!string.IsNullOrEmpty(StatusMessage))
        {
            GUIStyle status = new GUIStyle(GUI.skin.label);
            status.fontSize = 22;
            status.fontStyle = FontStyle.Bold;
            status.alignment = TextAnchor.MiddleCenter;
            status.normal.textColor = new Color(1f, 0.93f, 0.55f);
            GUI.Label(new Rect(Screen.width * 0.25f, 40, Screen.width * 0.5f, 40), StatusMessage, status);
        }
    }

    private Texture2D panelTexture;
    private Texture2D MakePanelTexture()
    {
        if (panelTexture == null)
        {
            panelTexture = new Texture2D(1, 1);
            panelTexture.SetPixel(0, 0, new Color(0.08f, 0.10f, 0.16f, 0.82f));
            panelTexture.Apply();
        }
        return panelTexture;
    }
}

public sealed class EnemyWalker : MonoBehaviour
{
    private Rigidbody2D body;
    private PlayerController player;
    private float left;
    private float right;
    private float speed;
    private int direction = -1;
    private bool defeated;

    public void Initialize(float patrolLeft, float patrolRight, float walkSpeed)
    {
        left = patrolLeft;
        right = patrolRight;
        speed = walkSpeed;
        direction = -1;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    private void FixedUpdate()
    {
        if (defeated) return;

        if (transform.position.x <= left) direction = 1;
        if (transform.position.x >= right) direction = -1;

        body.velocity = new Vector2(direction * speed, body.velocity.y);
        GetComponent<SpriteRenderer>().flipX = direction > 0;
    }

    public void Defeat()
    {
        if (defeated) return;
        defeated = true;

        if (player != null)
            player.RegisterKill();

        transform.localScale = new Vector3(0.2f, 0.2f, 1f);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (defeated) return;

        PlayerController touchingPlayer = collision.gameObject.GetComponent<PlayerController>();
        if (touchingPlayer != null)
            touchingPlayer.TryStomp(this);
    }
}

public sealed class CameraFollow : MonoBehaviour
{
    private Transform target;

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float desiredX = Mathf.Max(0f, target.position.x + 2.0f);
        Vector3 desired = new Vector3(desiredX, 1.0f, -10f);
        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * 5f);
    }
}
