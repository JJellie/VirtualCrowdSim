using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using System.Linq;
using Cinemachine;

//Human movement script for scenario 2
public class DumbMove2 : MonoBehaviour
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
        //set center of gravity of agent
        centerOfGravity = new Vector3(0f,0f,0f);
        robot.centerOfMass = centerOfGravity;
        sleep = false;
        gameObject.GetComponent<Animator>().speed = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        //set taget of human
        currentTarget = GameObject.FindWithTag("target2").transform.position;
    }

    void FixedUpdate()
    {
        //Move human towards target
        Vector3 direction = (currentTarget - transform.position).normalized;
        MoveRobot(direction);
    }

    void MoveRobot(Vector3 direct)
    {
        transform.LookAt(currentTarget);
    }

    //If robot collides with human, continue moving
     private void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.tag == "Player")
        {
            sleep = false;
            sleepCount = 0;
            gameObject.GetComponent<Animator>().speed = 2;

        }
    }

    private void OnCollisionExit(Collision other) 
    {

    }

}