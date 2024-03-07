using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [SerializeField] private float movementSpeed = 3.0f;
    [SerializeField] private float jumpForce = 14.0f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private GameObject LoadingScreen;

    private enum MovementState
    {
        Idle,
        Walking,
        Attacking,
        Falling,
        Jumping,
    }

    private enum HDirection
    {
        Left,
        Right,
    }

    private Vector2 movement;
    private Animator animator;
    private Rigidbody2D rigidBody;
    private BoxCollider2D collider2D;
    private SpriteRenderer spriteRenderer;
    private HDirection currentHDirection = HDirection.Right;
    private bool isAttacking = false;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        bool grounding = collider2D.IsTouchingLayers(LayerMask.GetMask("Ground"));
        movement = new Vector2(Input.GetAxis("Horizontal"), 0).normalized;

        if (GameManager.Instance.PlayerStatus.Health <= 0) return;

        if (GameManager.Instance.GameStatus.IsPaused) return;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            DoAttack();
        }

        if (Input.GetButtonDown("Jump") && grounding)
        {
            GameManager.Instance.PlaySFX("Jump");
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
        }

        if (movement.x != 0)
        {
            bool flipped = movement.x < 0;

            //transform.rotation = Quaternion.Euler(0, flipped ? 180 : 0, 0);
            spriteRenderer.flipX = flipped;

            // check if animator is playing attack animation
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                return;
            }


            // check left/right pressed
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    currentHDirection = HDirection.Left;
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    currentHDirection = HDirection.Right;
                }
            }
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.IsFinished) return;

        if (GameManager.Instance.PlayerStatus.Health <= 0) return;


        if (movement.x != 0)
        {
            var x = movement.x * Time.deltaTime * movementSpeed;
            
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                return;
            }

            this.transform.Translate(new Vector3(x, 0), Space.World);
        }

        spriteRenderer.flipX = currentHDirection == HDirection.Left;
    }

    private void UpdateAnimation()
    {
        MovementState state;
        bool grounding = collider2D.IsTouchingLayers(LayerMask.GetMask("Ground"));
        bool inStairs = collider2D.IsTouchingLayers(LayerMask.GetMask("Stairs"));

        bool isPressingLeftOrRight = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);

        if (movement.x != 0 && grounding || (isPressingLeftOrRight && inStairs))
        {
            state = MovementState.Walking;
        }
        else
        {
            state = MovementState.Idle;
        }

        if (rigidBody.velocity.y > 0.05f)
        {
            state = MovementState.Jumping;
        }
        else if (rigidBody.velocity.y < -.01f && !inStairs)
        {
            state = MovementState.Falling;
        }

        if (state == MovementState.Walking)
        {
            GameManager.Instance.PlaySFX("Walking_grass", true, 2f);
        }

        animator.SetInteger("State", (int)state);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Enemy"))
        {
            if (currentHDirection == HDirection.Left && gameObject.transform.position.x > transform.position.x)
            {
                rigidBody.AddForce(new Vector2(-5, 0), ForceMode2D.Impulse);
            }
            else if (currentHDirection == HDirection.Left && gameObject.transform.position.x < transform.position.x)
            {
                rigidBody.AddForce(new Vector2(5, 0), ForceMode2D.Impulse);
            }
            else if (currentHDirection == HDirection.Right && gameObject.transform.position.x > transform.position.x)
            {
                rigidBody.AddForce(new Vector2(-5, 0), ForceMode2D.Impulse);
            }
            else if (currentHDirection == HDirection.Right && gameObject.transform.position.x < transform.position.x)
            {
                rigidBody.AddForce(new Vector2(5, 0), ForceMode2D.Impulse);
            }

            GameManager.Instance.PlaySFX("Player_take_damage");

            // tint the player red for a short time with fade effect
            StartCoroutine(TintPlayerRed());
            HealthSystem.Instance.TakeDamage(10);

            if (GameManager.Instance.PlayerStatus.Health <= 0)
            {
                animator.SetTrigger("Die");
                StartCoroutine(GoNextLevel());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance.IsFinished) return;
        if (GameManager.Instance.PlayerStatus.Health <= 0) return;

        if (collision.gameObject.CompareTag("NextLevelPoint"))
        {
            StartCoroutine(GoNextLevel());
        }

        if (GameManager.Instance.GameStatus.Level == 1)
        {
            onHitEnemyLevel1(collision);
        }
    }

    private void onHitEnemyLevel1(Collision2D collision)
    {
        if (GameManager.Instance.PlayerStatus.Health <= 0) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            //SkullEnemy enemy = collision.gameObject.GetComponent<SkullEnemy>();
            GameObject gameObject = collision.gameObject;
            SkullEnemy skullEnemy = gameObject.GetComponent<SkullEnemy>();

            if (skullEnemy.IsDead || isAttacking)
            {
                return;
            }

            if (currentHDirection == HDirection.Left && gameObject.transform.position.x > transform.position.x)
            {
                rigidBody.AddForce(new Vector2(-5, 0), ForceMode2D.Impulse);
            }
            else if (currentHDirection == HDirection.Left && gameObject.transform.position.x < transform.position.x)
            {
                rigidBody.AddForce(new Vector2(5, 0), ForceMode2D.Impulse);
            }
            else if (currentHDirection == HDirection.Right && gameObject.transform.position.x > transform.position.x)
            {
                rigidBody.AddForce(new Vector2(-5, 0), ForceMode2D.Impulse);
            }
            else if (currentHDirection == HDirection.Right && gameObject.transform.position.x < transform.position.x)
            {
                rigidBody.AddForce(new Vector2(5, 0), ForceMode2D.Impulse);
            }

            GameManager.Instance.PlaySFX("Player_take_damage");

            // tint the player red for a short time with fade effect
            StartCoroutine(TintPlayerRed());
            HealthSystem.Instance.TakeDamage(10);

            if (GameManager.Instance.PlayerStatus.Health <= 0)
            {
                animator.SetTrigger("Die");
                GameManager.Instance.GameOver();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    public void Hurt(float damage)
    {
        if (currentHDirection == HDirection.Left && gameObject.transform.position.x > transform.position.x)
        {
            rigidBody.AddForce(new Vector2(-5, 0), ForceMode2D.Impulse);
        }
        else if (currentHDirection == HDirection.Left && gameObject.transform.position.x < transform.position.x)
        {
            rigidBody.AddForce(new Vector2(5, 0), ForceMode2D.Impulse);
        }
        else if (currentHDirection == HDirection.Right && gameObject.transform.position.x > transform.position.x)
        {
            rigidBody.AddForce(new Vector2(-5, 0), ForceMode2D.Impulse);
        }
        else if (currentHDirection == HDirection.Right && gameObject.transform.position.x < transform.position.x)
        {
            rigidBody.AddForce(new Vector2(5, 0), ForceMode2D.Impulse);
        }

        GameManager.Instance.PlaySFX("Player_take_damage");

        // tint the player red for a short time with fade effect
        StartCoroutine(TintPlayerRed());
        HealthSystem.Instance.TakeDamage(damage);

        if (GameManager.Instance.PlayerStatus.Health <= 0)
        {
            animator.SetTrigger("Die");
            GameManager.Instance.GameOver();
        }
    }

    private void DoAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        isAttacking = true;
        GameManager.Instance.PlaySFX("Attack");
        animator.SetTrigger("Attack1");

        foreach (Collider2D enemy in hitEnemies)
        {
            // get gameobject
            GameObject gameObject = enemy.gameObject;
            SkullEnemy skullEnemy = gameObject.GetComponent<SkullEnemy>();
            Enemy2 enemy2 = gameObject.GetComponent<Enemy2>();

            if (skullEnemy != null)
            {
                skullEnemy.TakeDamage(50.0f, this);
            }

            if (enemy2 != null)
            {
                enemy2.TakeDamage(50.0f, this);
            }
        }

        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    // tint the player red for a short time with fade effect
    private IEnumerator TintPlayerRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    private IEnumerator GoNextLevel()
    {
        GameManager.Instance.NextLevel();
        yield return new WaitForSeconds(2f);
    }
}
