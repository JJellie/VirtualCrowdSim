using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Collision detecting script for the limbs of the dummies
public class COLLISIONlIMBCopy : MonoBehaviour
{
     private void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.tag == "Player")
        {
            GameObject mainBody = GameObject.FindWithTag("taget");
            mainBody.GetComponent<DumbMoveScript>().sleep = false;
            mainBody.GetComponent<DumbMoveScript>().sleepCount = 0;
            mainBody.GetComponent<Animator>().speed = 2;

        }
    }
}
