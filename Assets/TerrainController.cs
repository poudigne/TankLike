using UnityEngine;
using System.Collections;

public class TerrainController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnTriggerExit2D (Collider2D other){
		Debug.Log ("KABOOOM ! (OnTriggerEnter)");
		Destroy(gameObject);
	}
	void OnCollisionEnter2D(Collision2D other){
		Debug.Log(" LOL  : " + other.gameObject.name );
	}
	void OnCollisionExit2D(Collision2D other){
		Debug.Log(" POIL");
	}
}
