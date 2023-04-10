using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

// Data class to hold data from a collision
public class CollisionData
{

    // Ending frame of the collision (determined only when collision ends)
    private int _endf; 

    // starting frame of the collision
    [ReadOnly]
    public int startf;
    
    // Id of colliding object
    [ReadOnly]
    public int objId;
    
    // Impuls of collision
    [ReadOnly]
    public Vector3 impulse;

    // Getters and setters for ending frame
    public int endf {
        get {
            if (_endf < 0) {
                throw new System.Exception("End frame of collision is not determined yet.");
            }

            return _endf;
        }

        set {
            Debug.Log("setting end to: " + value);
            if (value < -1) {
                throw new System.ArgumentOutOfRangeException("end frame must be at least 0 and greater than the start frame");
            }

            this._endf = value;
        }
    }

    // Duration of a collision
    public int duration {
        get {
            try
            {
                return endf - startf;
            }
            catch (System.Exception e) { throw e; }
        }
    }

    // Constructor
    public CollisionData(int start, int objId, Vector3 impulse) {
        this.startf = start;
        this._endf = -1;  // setter call
        this.objId = objId;
        this.impulse = impulse;
    }
}
