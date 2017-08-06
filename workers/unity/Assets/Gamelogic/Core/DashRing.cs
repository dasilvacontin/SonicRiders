using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashRing : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter (Collider other) {
		if (other.gameObject.CompareTag ("Player")) {
			other.transform.position = this.transform.position;
			other.transform.rotation = this.transform.rotation;
			other.GetComponent<Rigidbody> ().velocity = this.transform.forward.normalized * 60;
			GetComponent<AudioSource> ().Play ();
		}
	}

	void OnTriggerStay (Collider other) {
		if (other.gameObject.CompareTag ("Player")) {
			other.GetComponent<Rigidbody> ().velocity = this.transform.forward.normalized * 60;
		}
	}

	void OnDrawGizmos () {
		Gizmos.color = Color.white;
		Gizmos.DrawLine (transform.position, transform.position + transform.forward.normalized * 60);
	}
}
