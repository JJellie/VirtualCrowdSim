using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;

// Script that controls social force behaviour
public class SocialForce : MonoBehaviour
{
  // The point in space we want to navigate towards
  public Vector3 goal;

  // Force pointing towards the goals
  public Vector3 goalForce;

  // Closest point on the closest wall
  public Vector3 wallPoint;


  // Distance to closest wall
  public float wallDistance;

  // Object to store the path generated on the navmesh
  private NavMeshPath path;

  // Current navmeshcorner we are targeting
  private int corneridx = 0;

  // Rigidbody of this object
  private Rigidbody movement;

  // Acceleration returned by socialforces
  private Vector3 acceleration;

  // Force visualization
  public Vector3 wallForce; 

  // Current velocity
  private Vector3 velocity;

  // AgentID for gpu
  private int gpuID;

  // Desired speed of this agent
  private float desiredspeed;

  // public to monitor via properties panel
  public int pathTries;  

  // Check if a path is created
  public bool madePath;

  // Runs before first tick
  void Start()
  {
    // Get rigidbody
    movement = GetComponent<Rigidbody>();

    // Get Path and closest wall
    path = new NavMeshPath();
    wallPoint = findClosestWallPoint();

    // Set desired speed as a normally distributed random variable with a mean of 1.34 and a std deviation of 0.26
    desiredspeed = GlobalVars.instance.randn(1.34f, 0.26f);
   
    // Try to make a path
    pathTries = 0;
    madePath = false;
    
    // While path is not made, retry until a amax
    while(!madePath && pathTries < 10) {
      NavMesh.CalculatePath(transform.position, goal, NavMesh.AllAreas, path);
      madePath = (path.status == NavMeshPathStatus.PathComplete);
      pathTries += 1;
    }

    // If impossible to make a path respawn this person
    if (pathTries >= 10) {
      CrowdController.instance.goalReached(this);
    }

    // If path is not complete debug, this should not happen
    if (path.status != NavMeshPathStatus.PathComplete) {
      Debug.Log(goal);
    }

    // register agent with GPU dispatcher to obtain acceleration force from GPU in callback and get back an ID
    gpuID = GPUDispatcher.instance.registerAgent( (NetForce f) => {
      // Callback: Try to add acceleration to velocity and cap it at the desiredspeed
      try {
        movement.velocity += f.acceleration;
      } catch (System.Exception e) {
        // On error log for debugging
        Debug.Log(string.Format("dipatched faulty agent data:\n pos: {0}\nvel: {1}\ntarget: {2}\ndesiredspd: {3}\nwall: {4}", 
        this.transform.position, 
        this.movement.velocity, 
        path.corners[corneridx], 
        this.desiredspeed,
        this.wallPoint));
        Debug.Log(string.Format("returned data:\n\tacceleration: {0}\n\twall: {1}", f.acceleration, f.wallForce));
        throw e;
      }
      
      if (movement.velocity.sqrMagnitude > desiredspeed*desiredspeed) {
        movement.velocity = movement.velocity.normalized * (1.3f * desiredspeed);
      }
      // Set Gizmo variables
      goalForce = f.goalForce;
      acceleration = f.acceleration;
      wallForce = f.wallForce;
      wallDistance = f.wallDistance;
    });
  }

  // Update on physics frame
  void FixedUpdate()
  {
    // If we are close enough to a corner, move towards the next one
    if ((path.corners[corneridx] - transform.position).magnitude < GlobalVars.instance.targetRadius) {
      corneridx += 1;
    }
    // If we are at the end, call goalreached of the controller to signal arriving
    if (corneridx >= path.corners.Length) {
        CrowdController.instance.goalReached(this);
    }

    // Fill forceagent data and set model look direction
    Vector3 pos = transform.position;
    Vector3 tpos = path.corners[corneridx];
    Quaternion motiondir = Quaternion.LookRotation(tpos-pos);
    motiondir.x = 0;
    motiondir.z = 0;
    transform.rotation = motiondir;
    pos.y = 0;
    tpos.y = 0;
    wallPoint = findClosestWallPoint();
    ForceAgentData currentData = new ForceAgentData(pos, tpos, movement.velocity, desiredspeed, 0, wallPoint);

    // Call dispatch
    GPUDispatcher.instance.dispatch(currentData, gpuID);

  }

  // Create function to make new instances on crowdcontroller initialization
  public static void Create(Vector3 position, Vector3 goal) {
    GameObject newObject = Instantiate(GlobalVars.instance.prefab, position, Quaternion.identity) as GameObject;
    newObject.GetComponent<SocialForce>().goal = goal;
  }

  // Function to get closes wallpoint
  public Vector3 findClosestWallPoint() {
    Vector3 closest = new Vector3(10000, 10000, 10000);
    foreach (GameObject wall in GlobalVars.instance.socialForceWalls) {
      if (!wall) {
        throw new System.Exception("Can't find ATLAS/Walls object!");
      }

      var wallCollider = wall.GetComponent<Collider>();

      if (!wallCollider) {
        throw new System.Exception("No Valid Collider found on existing wall!");
      }

      Vector3 p = wallCollider.ClosestPoint(this.transform.position);
      if ((closest - transform.position).magnitude > (p - transform.position).magnitude) {
        closest = p;
      }
    }

    return closest;
  }

  // Function to reset this to a new position and goal with a new path
  public void reset(Vector3 position, Vector3 newgoal) {
    transform.position = position;
    goal = newgoal;
    wallPoint = findClosestWallPoint();
    NavMesh.CalculatePath(transform.position, goal, NavMesh.AllAreas, path);
    corneridx = 0;
    if ((path.corners[corneridx] - transform.position).magnitude < 5) {
      corneridx += 1;
    }
  }

  // Draw forces for debugging
  void OnDrawGizmosSelected() {
    Gizmos.color = Color.red;
    if (wallForce.magnitude > 1) {
      Gizmos.DrawSphere(wallPoint,0.1f);
      Gizmos.DrawLine(transform.position + new Vector3(0,1,0), transform.position + wallForce);
    } else {
      Gizmos.DrawWireSphere(wallPoint, 0.5f + wallForce.magnitude);
    }
    
    Gizmos.color = new Color(1.0f, 0.7f, 0.5f);
    Vector3 repulsiveForces = acceleration - wallForce - goalForce;
    Gizmos.DrawLine(transform.position + new Vector3(0,1,0), transform.position + repulsiveForces);

    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position + new Vector3(0,1,0), transform.position + goalForce);

    Gizmos.color = Color.blue;
    Gizmos.DrawLine(transform.position + new Vector3(0, 1, 0), transform.position + acceleration);

    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere(path.corners[corneridx], GlobalVars.instance.targetRadius);
  }
}
