using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVars : MonoBehaviour
{
  // Public variable for assigning a prefab
  public Object prefab;

  // Arraylist to keep track of all walls
  public ArrayList socialForceWalls = new ArrayList();

  // Global single instance
  public static GlobalVars instance {get; private set;}
  
  // Initialize all hyperparameters for the crowd, settible in editor
  public int numpeople;
  public float deltat = 2f;
  public float relaxation = 0.5f;
  public float repelspeed = 2.1f;
  public float repeldistance = 0.3f;
  public float influencedist = 2f;
  public float targetRadius = 2.0f;

  // Called on scene load, creates single instance of this class
  void Awake() {
    if (instance != null) {
      Destroy(this);
    } else {
      instance = this;
    }
  }

  // On start loop through all game objects and check which ones are walls and record those
  private void Start() {
    GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

    int nrNotWalls = 0;
    int nrWalls = 0;

    foreach (GameObject obj in allObjects)
    {
      if (obj.layer != LayerMask.NameToLayer("SocialForceEnviroment")) {
        nrNotWalls++;
        continue;
      }
      
      socialForceWalls.Add(obj);
      nrWalls++;
    }

    Debug.Log("Found " + nrNotWalls + "not walls and " + nrWalls + " walls!");
  }

  // Box-muller transformation to get a variable from a random guassian distribution
  public float randn(float mean = 0, float std = 1) {
    float rand1 = 1.0f - Random.value;
    float rand2 = 1.0f - Random.value;
    float randomstdn = Mathf.Sqrt(-2 * Mathf.Log(rand1)) * Mathf.Sin(2 * Mathf.PI * rand2);
    return mean + randomstdn * std;
  }

}
