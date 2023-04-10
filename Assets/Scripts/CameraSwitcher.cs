using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;

// Cinematic camera control
public class CameraSwitcher : MonoBehaviour
{
  // List of all cameras in the scene
  static List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>();

  // Current camera viewing from
  public static CinemachineVirtualCamera activeCamera = null;

  // Function that changes camera by changing priority 
  public static void switchCamera(CinemachineVirtualCamera camera)
  {
    camera.Priority = 10;
    activeCamera = camera;

    foreach (CinemachineVirtualCamera c in cameras)
    {
      if (c != camera)
      {
        c.Priority = 0;
      }
    }
  }

  // Add new camera
  public static void register(CinemachineVirtualCamera camera)
  {
    cameras.Add(camera);
  }

  // Remove camera
  public static void unregister(CinemachineVirtualCamera camera)
  {
    cameras.Remove(camera);
  }

  // Check if camera is the active one
  public static bool isActiveCamera(CinemachineVirtualCamera camera)
  {
    return camera == activeCamera;
  }

}
