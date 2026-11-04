using UnityEngine;

public sealed class EnemyWalker : MonoBehaviour
{
    public int maxHealth = 3;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private PlayerController player;
    private float left;
    private float right;
    private float speed;
    private int direction = -1;
    private int health;
    private bool defeated;
    private float hitFlashUntil;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = maxHealth;
    }

    public void Initialize(float patrolLeft, float patrolRight, float walkSpeed)
    {
        left = patrolLeft;
        right = patrolRight;
        speed = walkSpeed;
        direction = -1;
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
        spriteRenderer.flipX = direction > 0;
    }

    private void Update()
    {
        if (defeated) return;

        int pose = (Mathf.FloorToInt(Time.time * 8f) % 2 == 0) ? 1 : 2;
        spriteRenderer.sprite = PixelArt.MakeActorSprite(false, pose);
        spriteRenderer.color = Time.time < hitFlashUntil ? new Color(1f, 0.55f, 0.55f) : Color.white;
    }

    public void TakeDamage(int damage)
    {
        if (defeated || damage <= 0) return;

        health = Mathf.Max(0, health - damage);
        hitFlashUntil = Time.time + 0.14f;

        if (health <= 0)
            Defeat();
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

    private void OnGUI()
    {
        if (defeated || Camera.main == null) return;

        Vector3 screen = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.9f);
        if (screen.z < 0f) return;

        float width = 34f;
        float height = 5f;
        Rect background = new Rect(screen.x - width * 0.5f, Screen.height - screen.y, width, height);
        GUI.color = new Color(0.12f, 0.08f, 0.10f, 0.9f);
        GUI.DrawTexture(background, Texture2D.whiteTexture);
        GUI.color = new Color(0.9f, 0.18f, 0.2f);
        GUI.DrawTexture(new Rect(background.x + 1f, background.y + 1f, (width - 2f) * health / maxHealth, height - 2f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (defeated) return;

        PlayerController touchingPlayer = collision.gameObject.GetComponent<PlayerController>();
        if (touchingPlayer != null)
            touchingPlayer.TryStomp(this);
    }
}