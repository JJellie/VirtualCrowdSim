using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using System.Linq;
using Cinemachine;

public class PhysCollisionScript : MonoBehaviour
{

    public float speed = 2f;

    [SerializeField]
    private Rigidbody robot;

    [SerializeField]
    public Vector3 currentTarget;
    [SerializeField]
    public Vector3 finalTarget;

    public Vector3 centerOfGravity;
    public Vector3 movementDirection;
    public float currRotation;

    public UnityEngine.AI.NavMeshPath pathToFinal;
    public Vector3[] corners;

    [SerializeField]
    CinemachineVirtualCamera mainCamera;
    [SerializeField]
    CinemachineVirtualCamera collisionCamera1;

    public float numberOfRays;
    public float angle;
 

    // Start is called before the first frame update
    void Start()
    {
        //Set cinema camera to the main camera
        CameraSwitcher.register(mainCamera);
        CameraSwitcher.register(collisionCamera1);
        CameraSwitcher.switchCamera(mainCamera);


        //Initialize variables such as center of gravity of robot
        robot = gameObject.GetComponent<Rigidbody>();
        centerOfGravity = new Vector3(0f,0f,0f);
        robot.centerOfMass = centerOfGravity;
        finalTarget = GameObject.FindWithTag("taget").transform.position;
        pathToFinal = new UnityEngine.AI.NavMeshPath();

        numberOfRays = 6f;
        angle = 60;
    }

    void Update()
    {
        //Get current rotation of the robot
        currRotation = gameObject.transform.eulerAngles.y;
        //Set final destination of robot
        finalTarget = GameObject.FindWithTag("taget").transform.position;

    }

    // Update is called once per frame
    void FixedUpdate()
    {        
        //Set target and move robot towards target
        var direction = targetSelection();
        MoveRobot(direction);
        transform.LookAt(currentTarget);
        
        //Once robot has reached target freeze simulation
        if (Vector3.Distance(finalTarget,transform.position) <= 0.2f)
        {
            Time.timeScale = 0;
        }
        
    }

    //Method to handle selecting targets, either final or local
    private Vector3 targetSelection()
    {
        //Get path to the final target via NavMesh
        UnityEngine.AI.NavMesh.CalculatePath(gameObject.transform.position, finalTarget, UnityEngine.AI.NavMesh.AllAreas, pathToFinal);
        corners = pathToFinal.corners;

        //Initialize arrays that hold front,left and right obstacles
        List<GameObject> obstaclesFront = new List<GameObject>();
        List<GameObject> obstaclesRight = new List<GameObject>();
        List<GameObject> obstaclesLeft = new List<GameObject>();
        Vector3 direction = new Vector3(0,0,0);


        if (pathToFinal != null)
        {
            //Create a line that shows the path the robot intends to take to reach final destination
            for (int i = 0; i < pathToFinal.corners.Length - 1; i++)
                Debug.DrawLine(pathToFinal.corners[i], pathToFinal.corners[i + 1], Color.red);


            //Set the current target to a corner of the path or the final goal, if there are no corners
            if(pathToFinal.corners.Length < 2 || corners == null)
            {
                currentTarget = finalTarget;
            }
            else
            {
                currentTarget = corners[1];
            }
            
            //Set arrays and variables, based on detected obstacles from the raycasts
            var holder1 =  detectObstacleScan("front");
            obstaclesFront = holder1.Item1;

            var holder2 = detectObstacleScan("right");
            obstaclesRight = holder2.Item1;
            List<Vector3> freeRayRight = holder2.Item2;

            var holder3 = detectObstacleScan("left");
            obstaclesLeft = holder3.Item1;
            List<Vector3> freeRayLeft = holder3.Item3;

            bool frontObstacle = (obstaclesFront != null && obstaclesFront.Count != 0);
            bool rightObstacle = (obstaclesRight != null && obstaclesRight.Count != 0);
            bool leftObstacle = (obstaclesLeft != null && obstaclesLeft.Count != 0);

            if (frontObstacle)
            {
                //Switch case to decide movement option, if the front of the robot is blocked
                switch (( rightObstacle, leftObstacle))
                {
                    case (true, false):
                    direction = freeRayLeft.ElementAt(0);

                    break;

                    case (false, true):
                    direction = freeRayRight.ElementAt(0);

                    break;

                    case (false, false):
                    direction = freeRayLeft.ElementAt(0);

                    break;
                
                    case (true, true):
                    //Bump
                    float minDist = 1000;
                    Vector3 chosenTarg = new Vector3(0,0,0);

                    //If all directions are blocked, follow closest human with same direction as robot
                    foreach(GameObject obstacle in obstaclesFront)
                    {
                        if (obstacle.CompareTag("human"))
                        {
                            if (obstacle.transform.eulerAngles.y >= currRotation - 10 && obstacle.transform.eulerAngles.y <= currRotation + 10) 
                            {
                                float dist = Vector3.Distance(gameObject.transform.position, obstacle.transform.position);
                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    chosenTarg = obstacle.transform.position; 
                                }
                            }
                        }
                    }
                    

                    if (chosenTarg != new Vector3(0,0,0) )
                    {
                        direction = (chosenTarg - transform.position).normalized;
                    }

                    break;
                }
            }
            //If no obstacles in front, continue following path as normal
            else
            {
                direction = (currentTarget - transform.position).normalized;  
            }
        }  
        return direction;
    }


    //Method to move the robot agent
    void MoveRobot(Vector3 direct)
    {
        if (transform.position != currentTarget)
        {
            transform.LookAt(direct);
            robot.MovePosition(transform.position + (direct * speed * Time.deltaTime));
        }
    }


    //When colliding, switch camera to see collision better
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
        }
    }

    //Revert to main camera after collision has finished
    private void OnCollisionExit(Collision other) 
    {
        if (other.gameObject.tag == "human")
        {
            if (CameraSwitcher.isActiveCamera(collisionCamera1))
            {
                CameraSwitcher.switchCamera(mainCamera);
            }
        }
    }

    //Helper method that unfreees or frezes the simulation during camera change
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

    //Method to detect obstacles around the agent using raycasts
    public (List<GameObject>, List<Vector3>, List<Vector3>) detectObstacleScan(string mode)
    {
        bool obstacleBool = false;
        var rotation = this.transform.rotation;
        Quaternion rotationMod;
        Vector3 direction = new Vector3(0f,0f,0f);
        List<GameObject> obstacles = new List<GameObject>();

        //List to contain directions on the left and right, where there are no obstacles
        List<Vector3> freeRayLeft;
        List<Vector3> freeRayRight;
        
        freeRayLeft = new List<Vector3>();
        freeRayRight = new List<Vector3 >();

        //Creating the set of rays
        for (int i = 0; i < numberOfRays; i++)
        {
            //Set the angle of the raycasts depending on the mode parameter
            switch(mode)
            {
                //Front
                case "front":
                rotationMod = Quaternion.AngleAxis((i/ (float)numberOfRays - 1) * angle - angle/4, this.transform.up);
                direction = rotation * rotationMod * Vector3.forward * 6;   

                break;

                //Right
                case "right":
                rotationMod = Quaternion.AngleAxis((i/ (float)numberOfRays - 1) * angle + 4*angle/3, this.transform.up);
                direction = rotation * rotationMod * Vector3.forward * 6;  
                freeRayRight.Add(direction/6);

                break;

                //Left
                case "left":
                rotationMod = Quaternion.AngleAxis((i/ (float)numberOfRays - 1) * angle/2  + angle/4 , this.transform.up);
                direction = rotation * rotationMod * Vector3.forward * 6;    
                freeRayLeft.Add(direction/6);
                break;
            }         

            var ray = new Ray(this.transform.position, direction);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 5.0f))
            {
                if (hitInfo.collider != null)
                {   
                    //If a obstacle is hit, add it to the list
                    if(!obstacles.Contains(hitInfo.collider.gameObject))
                    {
                        obstacles.Add(hitInfo.collider.gameObject);
                    }

                    //If a human is discovered using the rays, get their rotation
                    if (hitInfo.collider.gameObject.tag == "human")
                    {
                        float humRotation = hitInfo.collider.gameObject.transform.eulerAngles.y;
                        if( (Mathf.Abs(currRotation) + 270)% 360 >= Mathf.Abs(humRotation) && (Mathf.Abs(currRotation) + 90) % 360 <= Mathf.Abs(humRotation))   
                        {
                            hitInfo.collider.gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().carving = true;
                        }
                        //Remove rays that have hit obstacles, to get the direcitons where no obstacles were detected
                        switch (mode)
                        {
                            //Right
                            case "right":
                            freeRayRight.Remove(direction);

                            break;

                            //Left
                            case "left":
                            freeRayLeft.Remove(direction);         

                            break;
                        }     
                    }
                    obstacleBool = true;
                }
            } 
        }  

        return (obstacles, freeRayRight, freeRayLeft);
    }


}
