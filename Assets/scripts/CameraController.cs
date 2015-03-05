using System.Linq.Expressions;
using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
  private Vector2 cameraTankOffset = new Vector2(0, 1.1f);
	
  public void RepositionCamera(Vector2 position, bool useOffset = true)
  {
    Camera.main.transform.position = position + (useOffset ? cameraTankOffset : new Vector2(0, 0));
  }
}
