using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using System.Linq;
using Cinemachine;

public class DumbMoveScriptCopy : MonoBehaviour
{

    public float speed = 2f;

    [SerializeField]
    private Rigidbody robot;

    [SerializeField]
    public Vector3 currentTarget;
    [SerializeField]
    public Vector3 finalTarget;

    public Vector3 centerOfGravity;

    public bool sleep;
    public int sleepCount = 0;
    


    // Start is called before the first frame update
    void Start()
    {

        centerOfGravity = new Vector3(0f,0f,0f);
        robot.centerOfMass = centerOfGravity;
        sleep = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        currentTarget = GameObject.FindWithTag("target").transform.position;
    }

    void FixedUpdate()
    {

        if (sleepCount == 120)
        {
            sleep = true;
            gameObject.GetComponent<Animator>().speed = 0;
            robot.velocity = new Vector3(0,0,0);
        }

        if (!sleep)
        {
            Vector3 direction = (currentTarget - transform.position).normalized;
            MoveRobot(direction);
            sleepCount++;
        }

    }

    void MoveRobot(Vector3 direct)
    {
        transform.LookAt(currentTarget);
        robot.MovePosition(transform.position + (direct * speed * Time.deltaTime));
    }

     private void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.tag == "Player")
        {
            sleep = false;
            sleepCount = 0;
            gameObject.GetComponent<Animator>().speed = 2;

        }
    }
}
