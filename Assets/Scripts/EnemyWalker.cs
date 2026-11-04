using UnityEngine;

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
