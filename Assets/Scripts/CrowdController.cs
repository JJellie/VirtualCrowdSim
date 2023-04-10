using UnityEngine;

// Class controlling spawning and respawning of the crowd
public class CrowdController : MonoBehaviour
{
  // Global instance
  public static CrowdController instance {get; private set;}
  
  // Hardcoded goals
  private Vector3[] possibleGoals = new Vector3[] {new Vector3(-40,0,3), new Vector3(0.55f,0,-11.8f), new Vector3(-2.29f,0,27.7f), new Vector3(-44.9f,0,-8.5f), new Vector3(42f,0,2.7f)};
  
  // Color of goals for visualizing
  public Color goalColor;
  
  // Awake method runs on scene load, creates a single instance and initializes data
  void Awake() {
    if (instance != null) {
      Destroy(this);
    } else {
      instance = this;
    }

    // Spawn a person on a random spawpoint with some offset and give them a random goal
    for(int i = 0; i < GlobalVars.instance.numpeople; i++) {
      // Get random spawn point
      int spawn = Random.Range(0, possibleGoals.Length);
      // Get random goal without picking the spawn point and keeping uniform distribution
      int goal = Random.Range(0, possibleGoals.Length - 1);
      goal = goal >= spawn ? goal +1 : goal;
      Vector3 offset = 10* Random.insideUnitCircle;
      offset.y = 0;
      Vector3 spawnpoint = possibleGoals[spawn] + offset;
      SocialForce.Create(spawnpoint, possibleGoals[goal]);
    }

  }

  // Function to call by a person when they have reached a goal
  // They will get a random goal and spawnpoint reassigned
  public void goalReached(SocialForce person) {
    int spawn = Random.Range(0, possibleGoals.Length);
      // Get random goal without picking the spawn point and keeping uniform distribution
    int goal = Random.Range(0, possibleGoals.Length - 1);
    goal = goal >= spawn ? goal +1 : goal;
    person.reset(possibleGoals[spawn], possibleGoals[goal]);
  }

  // Draw all the goals
  private void OnDrawGizmos() {
    // visualize goals
    Gizmos.color = goalColor;
    foreach (Vector3 goal in possibleGoals)
    {
      Gizmos.DrawCube(goal, new Vector3(1,1,1));
    }
  }
}
