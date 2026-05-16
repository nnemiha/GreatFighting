using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class PlayerController : MonoBehaviour {

    [SerializeField] float      m_speed = 4.0f;
    [SerializeField] float      m_jumpForce = 7.5f;

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Bancho_sensor       m_groundSensor;
    private bool                m_grounded = false;
    private bool                m_isDead = false;
    int clickStep = 0;
    float lastClickTime = 0f;
    float resetTime = 0.8f;

    // Use this for initialization
    void Start () {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = GetComponentInChildren<Bancho_sensor>();
    }
	
	// Update is called once per frame
	void Update () {
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

        else if (Input.GetKeyDown("f"))
        {
            bool isBlocking = m_animator.GetBool("isBlocking");
            m_animator.SetBool("isBlocking", true);
            
        }
        else if (Input.GetKeyUp("f"))
        {
            m_animator.SetBool("isBlocking", false);
        }

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Horizontal");

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
        m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);
        

        //Set AirSpeed in animator
        m_animator.SetFloat("Speed", Mathf.Abs(inputX));

        // -- Handle Animations --
        

        if(Input.GetMouseButtonDown(0)) {
            if (Input.GetMouseButtonDown(0))
            {
                MakeCombo();
            }
        }


        if (Input.GetKeyDown("e")) {
            if(!m_isDead)
                m_animator.SetTrigger("Die");
            m_isDead = !m_isDead;
        }
            
        //Hurt
        else if (Input.GetKeyDown("q"))
            m_animator.SetTrigger("Hurt");

        

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
    void MakeCombo()
        {
            if (Time.time - lastClickTime > resetTime)
            {
                clickStep = 0;
            }
            lastClickTime = Time.time;
            clickStep++;
            if (clickStep == 1)
            {
                m_animator.SetTrigger("Attack");
            }
            else if (clickStep == 2)
            {
                m_animator.SetTrigger("Attack2");
            }
            else if (clickStep == 3)
            {
                m_animator.SetTrigger("Attack3");
                clickStep = 0;
            }
        }
}
