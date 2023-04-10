using System.Collections;
using System.IO;
using UnityEngine;

// Export collisions to CSV 
public class Exporter : MonoBehaviour
{
    // Global instance
    public static Exporter instance {get; private set;}
    
    // List of collisions
    private ArrayList collisions;

    // Get's called on scene load, creating single instance and initializing arraylist
    private void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }

        instance = this;  // create instance on first call
        collisions = new ArrayList();
    }

    // Add collision to data to memory
    public int noteCollision(int frame, Collision collision) {
        int key = collisions.Count;
        Vector3 impulse = collision.impulse;
        int id = collision.gameObject.GetInstanceID();
        collisions.Add( new CollisionData(frame, id, impulse));

        return key;
    }

    // Added final frame of collision to data
    public void endCollision(int colId, int frame) {
        Debug.Log(Time.frameCount + " ending collision " + colId + " at frame: " + frame);
        ((CollisionData) collisions[colId]).endf = frame;
    }

    // Update csv file
    public void updateFile(string filePath) {
        using (StreamWriter writer = new StreamWriter(filePath)) {
            
            // write header
            string header = "duration, impulseMagnitude, nrCollisions";
            writer.WriteLine(header);

            // evaluate every collision currently noted
            float duration = 0f;
            float largestForce = 0f;

            foreach (CollisionData col in collisions) {
                try {
                    if (col.duration > duration) { duration = col.duration; }
                }
                catch (System.Exception) {  }

                if (col.impulse.magnitude > largestForce) { largestForce = col.impulse.magnitude; }
            }

            writer.WriteLine(string.Format("{0}, {1}, {2}", duration, largestForce, collisions.Count));
        }
    }
    
    // When colliding add collision to memory
    private void OnCollisionEnter(Collision other) {
        Debug.Log("collided with: " + other.gameObject.GetInstanceID());
        noteCollision(Time.frameCount, other);
    }

    // Stop current collision when exiting 
    private void OnCollisionExit(Collision other) {
        Debug.Log("collsision ended with: " + other.gameObject.GetInstanceID());

        // find the longest collision
        for (int key = 0; key < collisions.Count; key++)
        {
            CollisionData data = (CollisionData) collisions[key];

            // determine if collision in array is with current object
            if (data.objId != other.gameObject.GetInstanceID()) {continue;}
            
            // determine if the collision in array was already ended
            try {
                if (data.endf >= 0) {continue;}
            } catch (System.Exception) {
                endCollision(key, Time.frameCount);
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {   
        // Every 1200 frame reupload the file
        if (Time.frameCount % 1200 == 0) {
            Debug.Log(Time.frameCount + " storing " + collisions.Count + " collisions in file...");
            updateFile(@"./Assets/Report/collisions.csv"); // update file
        }
    }
}