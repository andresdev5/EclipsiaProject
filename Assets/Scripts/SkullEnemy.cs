using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum SkullEnemyState
{
    Idle,
    Walking,
    Attacking,
    Die
}

enum SkullHorizontalDirection
{
    Left,
    Right
}

public class SkullEnemy : MonoBehaviour
{
    private SkullEnemyState State = SkullEnemyState.Idle;
    [SerializeField] private float movementSpeed = 3.0f;
    [SerializeField] private float maxHorizontalDistance = 3.0f;
    private float StartPositionX = 0f;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private SkullHorizontalDirection currentHDirection = SkullHorizontalDirection.Right;
    private Vector3 originalScale;
    private float health = 150.0f;
    private bool takingDamage = false;
    public bool IsDead { get { return health <= 0; } }

    // Start is called before the first frame update
    void Start()
    {
        StartPositionX = transform.position.x;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        originalScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);

        State = SkullEnemyState.Walking;
        animator.Play("walk");
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GameStatus.IsPaused) return;

        if (State == SkullEnemyState.Walking && !takingDamage)
        {
            if (transform.position.x < StartPositionX - maxHorizontalDistance)
            {
                currentHDirection = SkullHorizontalDirection.Right;
                transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            }
            else if (transform.position.x > StartPositionX)
            {
                currentHDirection = SkullHorizontalDirection.Left;
                transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
            }

            Vector2 direction = currentHDirection == SkullHorizontalDirection.Right ? Vector2.right : Vector2.left;
            transform.Translate(direction * movementSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(float damage, PlayerController player)
    {
        takingDamage = true;
        health -= damage;

        Rigidbody2D rigidBody2D = GetComponent<Rigidbody2D>();

        // Knockback
        if (player.transform.position.x < transform.position.x)
        {
            rigidBody2D.AddForce(new Vector2(-5, 0), ForceMode2D.Impulse);
            transform.Translate(Vector2.right * 0.5f);
        }
        else
        {
            rigidBody2D.AddForce(new Vector2(-5, 0), ForceMode2D.Impulse);
            transform.Translate(Vector2.left * 0.5f);
        }

        GameManager.Instance.PlaySFX("Skull_take_damage");

        if (health <= 0)
        {
            State = SkullEnemyState.Die;
            animator.Play("death");
            GetComponent<PolygonCollider2D>().enabled = false;
            GetComponent<Rigidbody2D>().gravityScale = 0;
            rigidBody2D.velocity = Vector2.zero;
        }

        StartCoroutine(TintEnemy());

        takingDamage = false;
    }

    private IEnumerator TintEnemy()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        spriteRenderer.color = Color.white;
    }

    private IEnumerator DestroyEnemy()
    {
        yield return new WaitForSeconds(2.0f);
        Destroy(gameObject);
    }
}
