using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2 : MonoBehaviour
{
    private Animator m_animator;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask playerLayer;

    private float health = 150.0f;
    private bool takingDamage = false;
    public bool IsDead { get { return health <= 0; } }
    private bool isAttacking = false;
    private SpriteRenderer spriteRenderer;

    public bool IsAttacking
    {
        get { return isAttacking; }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(Attack());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    IEnumerator Attack()
    {
        while(true)
        {
            if (health <= 0)
            {
                break;
            }

            m_animator.SetTrigger("Attack");
            yield return new WaitForSeconds(0.3f);
            DoAttack();
            yield return new WaitForSeconds(3);
        }
    }

    private IEnumerator Delay(float time = 0.5f, Action callback = null)
    {
        yield return new WaitForSeconds(time);
        if (callback != null)
        {
            callback();
        }
    }

    private IEnumerator TintEnemy()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        spriteRenderer.color = Color.white;
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
            m_animator.SetTrigger("Die");
            GetComponent<PolygonCollider2D>().enabled = false;
            GetComponent<Rigidbody2D>().gravityScale = 0;
            rigidBody2D.velocity = Vector2.zero;
        }

        StartCoroutine(TintEnemy());

        takingDamage = false;
    }

    private void DoAttack()
    {
        Collider2D[] target = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        foreach (Collider2D player in target)
        {
            GameObject gameObject = player.gameObject;
            PlayerController playerController = gameObject.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.Hurt(10f);
            }
        }

        isAttacking = true;
        StartCoroutine(Delay(0.5f, () => {
            isAttacking = false;
        }));
    }
}
