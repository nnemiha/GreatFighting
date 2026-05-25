using UnityEngine;
using System.Collections;
using ThomasDev.HealthDamageSystem;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class EnemyController : MonoBehaviour
{
    public float speed = 3f;
    public int HP = 100;
    public int damage = 15;

    [Header("Дистанция остановки и удара")]
    public float attackDistance = 0.8f;

    private Transform player;
    private Animator anim;

    [Header("Оружие")]
    public Transform attackPos;
    public LayerMask playerMask;
    public float radius = 0.6f;
    public float recharge;
    public float startRecharge = 1.5f;

    private bool isFacingRight = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        anim = GetComponent<Animator>();
    }


    void Update()
    {
        if (player == null) return;

        recharge += Time.deltaTime;
        
        
        float distanceX = Mathf.Abs(transform.position.x - player.position.x);
        
        
        if (distanceX > attackDistance) 
        {
            Vector2 targetPosition = new Vector2(player.position.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            
            if (anim != null) anim.SetFloat("Speed", 1f);

            
            if (player.position.x > transform.position.x && !isFacingRight)
            {
                Flip();
            }
            else if (player.position.x < transform.position.x && isFacingRight)
            {
                Flip();
            }
        }
        
        else 
        {
            if (anim != null) anim.SetFloat("Speed", 0f);

            if (recharge >= startRecharge)
            {
                if (anim != null) anim.SetTrigger("Attack");
                recharge = 0;
            }
        }

        if (HP <= 0)
        {
            Destroy(gameObject);
        }
    }

    
    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        if (attackPos != null)
        {
            Gizmos.DrawWireSphere(attackPos.position, radius);
        }
    }

    
    public void OnAttack()
    {
        if (attackPos == null) return;

        
        Collider2D[] playerCollider = Physics2D.OverlapCircleAll(attackPos.position, radius, playerMask);
        
        for (int i = 0; i < playerCollider.Length; i++)
        {
            
            PlayerController playerScript = playerCollider[i].GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
                Debug.Log("Враг нанес урон игроку: " + damage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
    }
}
