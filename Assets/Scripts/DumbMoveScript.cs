using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using System.Linq;
using Cinemachine;

//Human movement script for scenario 1
public class DumbMoveScript : MonoBehaviour
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
        //Set center of gravity of human
        centerOfGravity = new Vector3(0f, 0f, 0f);
        robot.centerOfMass = centerOfGravity;
        sleep = false;
        //Set mvoement speed of human
        gameObject.GetComponent<Animator>().speed = 1;   
    }

    // Update is called once per frame
    void Update()
    {
        //Set target of human
        currentTarget = GameObject.FindWithTag("target").transform.position;
    }

    void FixedUpdate()
    {
        //If sleep counter is 120, stop human
        if (sleepCount == 120)
        {
            sleep = true;
            gameObject.GetComponent<Animator>().speed = 0;
            robot.velocity = new Vector3(0, 0, 0);
        }
        
        //If not asleep, move towards target
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
    }

    //If robot collides with human, wake human up
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
