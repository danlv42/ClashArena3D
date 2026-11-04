using UnityEngine;

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
    private int health = 5;
    private float attackReadyAt;
    private float attackAnimationUntil;
    private float invulnerableUntil;
    private float hurtAnimationUntil;
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

        grounded = Mathf.Abs(body.velocity.y) < 0.08f &&
                   Physics2D.Raycast(transform.position, Vector2.down, 0.8f, LayerMask.GetMask("Default"));

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && grounded)
            body.velocity = new Vector2(body.velocity.x, jumpForce);

        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Z))
            Attack();

        if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.X))
            UseAbility();

        UpdateAnimation(horizontal);

        if (transform.position.y < -12f)
            Respawn();
    }

    private void UpdateAnimation(float horizontal)
    {
        int pose;
        if (Time.time < attackAnimationUntil) pose = 3;
        else if (Time.time < hurtAnimationUntil) pose = 5;
        else if (!grounded) pose = 4;
        else if (Mathf.Abs(horizontal) > 0.1f) pose = (Mathf.FloorToInt(Time.time * 10f) % 2 == 0) ? 1 : 2;
        else pose = 0;

        spriteRenderer.sprite = PixelArt.MakeActorSprite(true, pose);
        spriteRenderer.color = (Time.time < invulnerableUntil && Mathf.FloorToInt(Time.time * 18f) % 2 == 0)
            ? new Color(1f, 1f, 1f, 0.45f) : Color.white;
    }

    private void Attack()
    {
        if (Time.time < attackReadyAt) return;
        attackReadyAt = Time.time + 0.38f;
        attackAnimationUntil = Time.time + 0.24f;

        Vector2 center = (Vector2)transform.position + new Vector2(facing * 0.88f, 0.05f);
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, 1.15f);
        bool hitSomething = false;

        foreach (Collider2D hit in hits)
        {
            EnemyWalker enemy = hit.GetComponent<EnemyWalker>();
            if (enemy != null)
            {
                enemy.TakeDamage(1);
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
        int damaged = 0;
        foreach (Collider2D hit in hits)
        {
            EnemyWalker enemy = hit.GetComponent<EnemyWalker>();
            if (enemy != null)
            {
                enemy.TakeDamage(3);
                damaged++;
            }
        }

        body.velocity = new Vector2(body.velocity.x, Mathf.Max(body.velocity.y, 3f));
        attackAnimationUntil = Time.time + 0.3f;
        ShowStatus("POWER BURST! " + damaged + " enemies hit");
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
            enemy.TakeDamage(1);
            body.velocity = new Vector2(body.velocity.x, jumpForce * 0.72f);
        }
        else if (Time.time > invulnerableUntil)
        {
            TakeHit();
        }
    }

    public void HitHazard()
    {
        if (Time.time > invulnerableUntil)
            TakeHit();
    }

    private void TakeHit()
    {
        health--;
        invulnerableUntil = Time.time + 1.1f;
        hurtAnimationUntil = Time.time + 0.25f;
        body.velocity = new Vector2(-facing * 5f, 5.5f);
        ShowStatus("Ouch! HP: " + health);

        if (health <= 0)
            Respawn();
    }

    private void Respawn()
    {
        health = 5;
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