using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using System.Linq;
using Cinemachine;

public class MainRoboMovement : MonoBehaviour
{

  public UnityEngine.AI.NavMeshAgent mainAgent;
  public Camera camera;
  public NavMeshSurface surface;
  public GameObject[] discoveredObjects;
  public Vector3 finalTarget;
  public float currRotation;


  [SerializeField]
  private LineRenderer lineRenderer;
  
  [SerializeField]
  public CinemachineVirtualCamera mainCamera;
  [SerializeField]
  public CinemachineVirtualCamera collisionCamera1;

  public float targetVelocity = 10.0f;
  public int numberOfRays = 17;
  public float angle = 90;
  public bool removeEnemy = false;



  
  // Start is called before the first frame update
  void Start()
  {
    //Set camera to main Camera
    CameraSwitcher.register(mainCamera);
    CameraSwitcher.register(collisionCamera1);
    CameraSwitcher.switchCamera(mainCamera);

    lineRenderer.startWidth = 0.15f;
    lineRenderer.endWidth = 0.15f;
    lineRenderer.positionCount = 0;
    finalTarget = GameObject.FindWithTag("target").transform.position;
  }
  

  // Update is called once per frame
  void Update()
  {
    //Set target of agent
    finalTarget = GameObject.FindWithTag("target").transform.position;
    currRotation = gameObject.transform.eulerAngles.y;
    setDestination(detectObstacles());

  }

  //Method to set the current destination of the agent
  private void setDestination(Vector3 target)
  { 
    if(target != null)
    {
      //Get the path
      UnityEngine.AI.NavMeshPath pathToFinal = mainAgent.path;   
      //Draws the path the agent will take
      for (int i = 0; i < pathToFinal.corners.Length - 1; i++)
        Debug.DrawLine(pathToFinal.corners[i], pathToFinal.corners[i + 1], Color.red);

      //Sets destination of NavMeshAgent and starts moving towards destination
      mainAgent.SetDestination(target);
    }
  }
  
  //Method that detects obstacles 
  private Vector3 detectObstacles()
  {
      bool obstacleBool = false;

      //Set obstacle detection bool
      obstacleBool = rayScanner();

      if (obstacleBool)
      {
        GameObject[] humanEnc;
        List<GameObject> distHum = new List<GameObject>();
        GameObject finalT = null;

        humanEnc = GameObject.FindGameObjectsWithTag("discoveredHuman");

        //Get rotation of every discovered human
        foreach(GameObject human in humanEnc)
        {
            float humRotation = human.transform.eulerAngles.y;

                human.GetComponent<UnityEngine.AI.NavMeshObstacle>().carving = true;
        }
      }

    return finalTarget;
  }

  
  //Method that retursn a boolean based on wheter a obstacle has been detected
  private bool rayScanner()
  {
    //Create rays in front of agent to detect obstacles
    Vector3 up = new Vector3(0f,1f,5f);
    var vectorFront = Vector3.forward;
    return (drawVectorLine(numberOfRays, vectorFront, true));
  } 

  //Method to use raycast to detect if obstacles are in front and update hte navMesh
  // Takes an integer as the number of rays to be created, a vector3 giving the direction and a bool for the mode
  //mode = true discoveres, while mode = false, undiscoveres
  private bool drawVectorLine(int numberOfRays, Vector3 vector, bool mode)
  {
    bool enemyDetect = false;
    //Start creating the rays
    for (int i = 0; i < numberOfRays; i++)
    {
      var rotation = this.transform.rotation;
      var rotationMod = Quaternion.AngleAxis((i/ (float)numberOfRays - 1) * 2*angle + angle, this.transform.up);
      var direction = rotation * rotationMod * vector * 6;
      if (!mode)
      {
          direction = rotation * rotationMod * vector * 8;
      }
      Vector3 source = this.transform.position;
      source.y = source.y + 1;
      var ray = new Ray(source, direction);
      RaycastHit hitInfo;


      if (Physics.Raycast(ray, out hitInfo, 5.0f))
      {
        if (hitInfo.collider != null)
        {
          //If mode is true, discover humans and obstacles and set carving to true, updating navMesh
          if(mode)
          {
              if(hitInfo.collider.gameObject.tag == "undiscoveredHuman")
              {
                  hitInfo.collider.gameObject.tag = "discoveredHuman";
                  enemyDetect = true;
                  hitInfo.collider.gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().carving = true;

              }

              if(hitInfo.collider.gameObject.tag == "undiscoveredObstacle")
              {
                  hitInfo.collider.gameObject.tag = "discoveredObstacle";

              }
          }
        //If mode is false, undiscover humans and obstacles and set carving to false, updating navMesh
          else if (!mode)
          {
              if(hitInfo.collider.gameObject.tag == "discoveredHuman")
              {
                  hitInfo.collider.gameObject.tag = "undiscoveredHuman";
                  hitInfo.collider.gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().carving = false;

              }

              if(hitInfo.collider.gameObject.tag == "discoveredHuman")
              {
                  hitInfo.collider.gameObject.tag = "undiscoveredObstacle";
                  hitInfo.collider.gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().carving = false;

              }
          }

        } 
      }
    } 

    if (enemyDetect)
    {
        return true;
    }
    else
    {
        return false;
    }    

  }

  //Swap to collision camera on collision
  private void OnCollisionEnter(Collision other) 
  {
    bool flag = true;
    if (other.gameObject.tag == "human")
    {
        if (CameraSwitcher.isActiveCamera(mainCamera))
        {
            Invoke("pauser", 0.1f);
            pauser();
            CameraSwitcher.switchCamera(collisionCamera1);
        }
    }
  }

  //Swap back camera when collision finishes
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

  //Helper Method to freeze simulation when changing cameras
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

