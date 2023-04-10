using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Struct for computebuffer containing data of a single agent.
public struct ForceAgentData
{
  public Vector3 position;
  public Vector3 target;
  public Vector3 velocity;
  public float desiredSpeed;
  public int group;
  public Vector3 wallPoint;
  public ForceAgentData(Vector3 pos, Vector3 targ, Vector3 vel, float speed, int grp, Vector3 wall)
  {
    position = pos;
    velocity = vel;
    target = targ;
    desiredSpeed = speed;
    group = grp;
    wallPoint = wall;
  }
}

// Struct for computed forces 
public struct NetForce
{
  public Vector3 acceleration;
  public Vector3 wallForce;
  public float wallDistance;
  public Vector3 goalForce;
}

// Delegate for callback
public delegate void GPUCallback(NetForce force);

public class GPUDispatcher : MonoBehaviour
{
  // Size for agentobject
  int totalAgentSize = sizeof(float) * 3 * 4 + sizeof(int) + sizeof(float);

  // The shader that computes agent behaviour
  public ComputeShader socialForces;

  // The number of people that are supposed to spawn
  private int numPeople;

  // Single global instance
  public static GPUDispatcher instance { get; private set; }

  //Dictionary mapping agentId to a callback
  private Dictionary<int, GPUCallback> callbacks;

  // List of all agents data
  private ForceAgentData[] agentData;

  // List of all results
  private NetForce[] agentResult;

  // Number of registerd people
  private int registered = -1;

  // Number of people ready for dispatch
  private int dispatched = 0;

  // Create singleton instance and initialize variables
  void Awake()
  {
    if (instance != null)
    {
      Destroy(gameObject);
    }
    else
    {
      instance = this;
      numPeople = GlobalVars.instance.numpeople;
      callbacks = new Dictionary<int, GPUCallback>();
      agentData = new ForceAgentData[numPeople];
      agentResult = new NetForce[numPeople];

      // Initialize compute shader with hyperparameters
      socialForces.SetInt("numpeeps", numPeople);
      socialForces.SetFloat("deltat", GlobalVars.instance.deltat);
      socialForces.SetFloat("relaxation", GlobalVars.instance.relaxation);
      socialForces.SetFloat("influencedist", GlobalVars.instance.influencedist);
      socialForces.SetFloat("repelspeed", GlobalVars.instance.repelspeed);
      socialForces.SetFloat("repeldistance", GlobalVars.instance.repeldistance);
    }
  }

  // Register agent as a participant in social forces,
  // Adds callback to hashmap and returns an ID for further interaction
  public int registerAgent(GPUCallback call)
  {
    registered += 1;
    callbacks[registered] = call;
    return registered;
  }

  // Called by agent with data for current frame to dispatch, includes agents identifier
  public void dispatch(ForceAgentData currentagent, int agentId)
  {
    // Replace old data with current one and indicate an extra agent is ready to dispatch
    agentData[agentId] = currentagent;
    dispatched += 1;

    // If number of waiting agents equals total number of agents run the calculation
    if (dispatched == numPeople)
    {
      dispatched = 0;
      RunComputeShader();
    }
  }

  // Function that runs the compute shader with current data
  void RunComputeShader()
  {
    // Create new buffers and set their data and add to shader
    ComputeBuffer gpubufin = new ComputeBuffer(numPeople, totalAgentSize);
    ComputeBuffer gpubufout = new ComputeBuffer(numPeople, sizeof(float) * 3 * 3 + sizeof(float));  // add field size here
    gpubufin.SetData(agentData);
    gpubufout.SetData(agentResult);
    socialForces.SetBuffer(0, "input", gpubufin);
    socialForces.SetBuffer(0, "output", gpubufout);

    // Run shader with a number of workgroups that will satisfy the amount of people
    socialForces.Dispatch(socialForces.FindKernel("CSMain"), (numPeople / (int)10) + 1, 1, 1);

    // When the GPU returns back a result put it in agentResult and release the buffers
    gpubufout.GetData(agentResult);
    gpubufin.Release();
    gpubufout.Release();

    // Return data to agents by means of a callback function
    for (int i = 0; i < agentResult.Length; i++)
    {
      callbacks[i].Invoke(agentResult[i]);
    }
  }
}
