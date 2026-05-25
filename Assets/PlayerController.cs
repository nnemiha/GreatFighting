using UnityEngine;
using System.Collections;
using ThomasDev.HealthDamageSystem;
using System.Diagnostics;
using Debug = UnityEngine.Debug;



public class PlayerController : MonoBehaviour {
    [Header("Movement")]
    [SerializeField] float      m_speed = 4.0f;
    [SerializeField] float      m_jumpForce = 7.5f;
    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private BoxCollider2D       m_boxCollider;
    private Bancho_sensor       m_groundSensor;
    private Health              m_health;
    private bool                m_grounded = false;
    private bool                m_isDead = false;
    private bool                m_isFacingRight = true;
    int clickStep = 0;
    float lastClickTime = 0f;
    float resetTime = 0.8f;
    [Header("Оружие")]
    public Transform m_attackPos;
    public LayerMask m_enemy;
    public float m_radius = 0.5f;
    public int m_damage = 25;
    private float m_recharge;
    public float m_startRecharge = 0.4f;
    


    // Use this for initialization
    void Start () {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = GetComponentInChildren<Bancho_sensor>();
        m_health = GetComponent<Health>();
        m_boxCollider = GetComponent<BoxCollider2D>();
        m_recharge = 0f;

        if (m_health != null)
        {
            m_health.OnDamaged.AddListener(OnCharacterDamaged);
            m_health.OnDeath.AddListener(OnCharacterDeath); 
        }
    }
	
	// Update is called once per frame
	void Update () {
        float inputX = Input.GetAxis("Horizontal");
        if (m_isDead) return;
        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State()) {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if(m_grounded && !m_groundSensor.State()) {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        if (Input.GetKeyDown("f"))
        {
            bool isBlocking = m_animator.GetBool("isBlocking");
            m_animator.SetBool("isBlocking", true);
            
        }
        else if (Input.GetKeyUp("f"))
        {
            m_animator.SetBool("isBlocking", false);
        }

        if (m_recharge >= m_startRecharge)
        {
            if (Input.GetMouseButtonDown(0))
            {
                AutoFlipToNearestEnemy();

                m_animator.SetTrigger("Attack");
                m_recharge = 0;
            }
            
        }
        else
        {
            m_recharge += Time.deltaTime;
        }

        // -- Handle input and movement --
        

        float currentSpeed = m_speed;
        if (m_animator.GetBool("isBlocking"))
        {
            currentSpeed = m_speed * 0.3f;
        }

        // Swap direction of sprite depending on walk direction
        //if (inputX > 0)
        //    transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        //else if (inputX < 0)
        //    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        // Move
        m_body2d.linearVelocity = new Vector2(inputX * currentSpeed, m_body2d.linearVelocity.y);
        

        //Set AirSpeed in animator
        m_animator.SetFloat("Speed", Mathf.Abs(inputX));

        // -- Handle Animations --
        

        // if(Input.GetMouseButtonDown(0)) {
            // if (Input.GetMouseButtonDown(0))
            // {
                // MakeCombo();
            // }
        // }


        if (Input.GetKeyDown("e")) {
            if(!m_isDead) OnCharacterDeath();
        }
            
        //Hurt
        else if (Input.GetKeyDown("q"))
        {
            m_animator.SetTrigger("Hurt");
            if (m_health != null)
            {
                m_health.TakeDamage(10f);
            }
        }
            

        

        //Change between idle and combat idle
        
            

        //Jump
        else if (Input.GetKeyDown("space") && m_grounded) {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }

    }
    // void MakeCombo()
        // {
            // if (Time.time - lastClickTime > resetTime)
            // {
                // clickStep = 0;
            // }
            // lastClickTime = Time.time;
            // clickStep++;
            // if (clickStep == 1)
            // {
                // m_animator.SetTrigger("Attack");
            // }
            // else if (clickStep == 2)
            // {
                // m_animator.SetTrigger("Attack2");
            // }
            // else if (clickStep == 3)
            // {
                // m_animator.SetTrigger("Attack3");
                // clickStep = 0;
            // }
        // }
    void AutoFlipToNearestEnemy()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 3.0f, m_enemy);
        
        if (hitEnemies.Length > 0)
        {
            Transform nearestEnemy = hitEnemies[0].transform;

            if (nearestEnemy.position.x > transform.position.x && !m_isFacingRight)
            {
                FlipPlayer();
            }
            else if (nearestEnemy.position.x < transform.position.x && m_isFacingRight)
            {
                FlipPlayer();
            }
        }
    }
    void OnCharacterDamaged(float currentHealth, float maxHealth)
    {
        if (m_animator.GetBool("isBlocking"))
        {
            m_animator.SetTrigger("Hurt");
        }
    }
    public void OnAttack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(m_attackPos.position, m_radius, m_enemy);
        for (int i = 0; i < enemies.Length; i++)
        {
            
            //enemies[i].GetComponent<Enemy>().TakeDamage(m_damage);
            Health enemyHealth = enemies[i].GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage((float)m_damage);
                Debug.Log("Попадание по: " + enemies[i].name);
            }

        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(m_attackPos.position, m_radius);
    }
    void OnCharacterDeath()
    {
        if (!m_isDead)
        {
            m_isDead = true;
            //m_boxCollider.size = new Vector2(m_boxCollider.size.x, 0.2f);
            m_boxCollider.offset += new Vector2(m_boxCollider.offset.x, 0.30f);
            m_animator.SetTrigger("Die");
            m_body2d.linearVelocity = Vector2.zero;
            m_body2d.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        }
    }
    public void TakeDamage (int damage)
    {
        if (m_health != null)
        {
            m_health.TakeDamage((float)damage);
        }
    }

    void FlipPlayer()
    {
        m_isFacingRight = !m_isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
}
