using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using System.Linq;
using Cinemachine;

//Robot movement srcipt for first scenario to test forces and intended behaviour
public class PushScript : MonoBehaviour
{

    public float speed = 2f;

    [SerializeField]
    private Rigidbody robot;

    [SerializeField]
    public Vector3 currentTarget;
    [SerializeField]
    public Vector3 finalTarget;

    public Vector3 centerOfGravity;

    [SerializeField]
    CinemachineVirtualCamera mainCamera;
    [SerializeField]
    CinemachineVirtualCamera collisionCamera1;

    public int collisions;
    public bool stop;
    public int go;


    // Start is called before the first frame update
    void Start()
    {
        //set camera to main camera
        CameraSwitcher.register(mainCamera);
        CameraSwitcher.register(collisionCamera1);
        CameraSwitcher.switchCamera(mainCamera);

        robot = gameObject.GetComponent<Rigidbody>();
        centerOfGravity = new Vector3(0f,0f,0f);
        robot.centerOfMass = centerOfGravity;

    }

    void FixedUpdate()
    {
        //Move towards target or change target, after done
        if (collisions >= 12)
        {
            transform.LookAt(currentTarget);
            currentTarget = GameObject.FindWithTag("target2").transform.position;
        }
        else
        {
            currentTarget = GameObject.FindWithTag("taget").transform.position;
        }

        //Move robot after finised waiting
        if(go == 30)
        {
            stop = false;
            go = 0;
        }

        //Move robot if not stopped, otherwise increment waiting counter
        if (!stop)
        {
        Vector3 direction = (currentTarget - transform.position).normalized;
        MoveRobot(direction);
        }
        else
        {
            go++;
        }

    }

    //Method to move the robot
    void MoveRobot(Vector3 direct)
    {
        //Slow down robot when about to bump
        if (Vector3.Distance(currentTarget, transform.position) <= 1.5f)
        {
            speed = 0.5f;
        }
        else
        {
            speed= 2f;
        }
        robot.MovePosition(transform.position + (direct * speed * Time.deltaTime));
    }


    //Change camera when detecting collision
     private void OnCollisionEnter(Collision other) 
    {
        bool flag = true;
        if (other.gameObject.tag == "taget")
        {
            if (CameraSwitcher.isActiveCamera(mainCamera))
            {

                Invoke("pauser", 0.1f);
                pauser();
                CameraSwitcher.switchCamera(collisionCamera1);
            }
                collisions++;
        }
    }

    private void OnCollisionExit(Collision other) 
    {
        if (other.gameObject.tag == "taget")
        {
            stop = true;
            robot.velocity = new Vector3(0,0,0);

        }
    }


    private void pauser()
    {
        if (Time.timeScale == 1)
        {
            Time.timeScale = 0.1f;
        }
        else
        {
            Time.timeScale = 1;  
        }
    }
}