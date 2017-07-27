using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public GameObject target;
	private Vector3 lastPosition;

	void Start () {
		lastPosition = transform.position;
	}

	// Update is called once per frame
	void LateUpdate () {
		Rigidbody targetRigidbody = target.GetComponent<Rigidbody> ();
		Vector3 velocity = targetRigidbody.velocity;
		Vector3 cameraPosition = target.transform.position - (velocity.normalized * (3 + (0.5f * velocity.magnitude / 50)));
		cameraPosition.y = Mathf.Max (cameraPosition.y, target.transform.position.y + 1);
		transform.position = cameraPosition;
		lastPosition = transform.position;
		transform.LookAt(target.transform);
		Camera cam = GetComponent<Camera>();
		cam.fieldOfView = 60 + 20 * (velocity.magnitude / 50);
	}
}
