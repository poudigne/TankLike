using UnityEngine;
using System.Collections;

public class TerrainController : MonoBehaviour {

  void OnColliderEnter2D(Collision2D other)
  {
    Debug.Log("OnColliderEnter2D");
    Destroy(gameObject);
  }
  void OnColliderExit2D(Collision2D other)
  {
    Debug.Log("OnColliderExit2D");
    Destroy(gameObject);
  }
}
