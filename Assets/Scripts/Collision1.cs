using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision1 : MonoBehaviour
{
  // starting frame of the collision
  public int startf { get; }

  // ending frame of the collision (determined only when collision ends)
  public int endf
  {
    get
    {
      if (endf < 0)
      {
        throw new System.Exception("End frame of collision is not determined yet.");
      }

      return endf;
    }

    set
    {
      if (value < -1 || startf > endf)
      {
        throw new System.ArgumentOutOfRangeException("end frame must be at least 0 and greater than the start frame");
      }

      this.endf = value;
    }
  }

  // duration of a collision
  public int duration
  {
    get
    {
      try
      {
        return endf - startf;
      }
      catch (System.Exception e) { throw e; }
    }
  }

  public Collision1(int start, int end = -1)
  {
    startf = start;
    endf = -1;
  }
}
