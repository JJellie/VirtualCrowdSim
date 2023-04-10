using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using System.Linq;
using Cinemachine;

//Robot movement srcipt for second scenario to test forces and intended behaviour
public class PushScript2 : MonoBehaviour
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
        //Set the camera to the main camera
        CameraSwitcher.register(mainCamera);
        CameraSwitcher.register(collisionCamera1);
        CameraSwitcher.switchCamera(mainCamera);

        //Set center of mass for the agent
        robot = gameObject.GetComponent<Rigidbody>();
        centerOfGravity = new Vector3(0f,0f,0f);
        robot.centerOfMass = centerOfGravity;
        
    }

    void FixedUpdate()
    {
        var crosser = GameObject.FindWithTag("human");
        var distancer = Vector3.Distance(crosser.transform.position, transform.position);

        currentTarget = GameObject.FindWithTag("taget").transform.position;

        Vector3 direction = (currentTarget - transform.position).normalized;

        if (distancer <= 1.5f )
        {
            MoveRobot(new Vector3(0,0,0));

        }
        else
        {
            MoveRobot(direction);
        }

    }

    void MoveRobot(Vector3 direct)
    {

        robot.MovePosition(transform.position + (direct * speed * Time.deltaTime));
    }


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


    //Helper method to pause the game, when switching cameras
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
