using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Unity.Visualizer;
using Assets.Gamelogic.Utils;

public class BoardControl : MonoBehaviour {

	[Require] private Position.Writer PositionWriter;
	[Require] private Rotation.Writer RotationWriter; 

	public GameObject playerCamera;
	public GameObject board;
	public GameObject turbulence;
	private ParticleSystem turbulenceParticleSystem;
	private Rigidbody boardRigidbody;
	public float boardTurnSpeed = 100;
	public float maxAirBoost = 65;
	public float minAirBoost = 60;
	public float airRayDistance = 1.5f;
	public float minAirHeight = 0.2f;

	public AudioClip jumpSound;
	public AudioClip landSound;

	public Light airCapacitor;
	private float airCapacitorGauge;
	private float airTime;

	void OnEnable () {
		boardRigidbody = board.GetComponent<Rigidbody>();
		boardRigidbody.centerOfMass = new Vector3 (0, -0.15f, 0);
		turbulenceParticleSystem = turbulence.GetComponent<ParticleSystem> ();
		airTime = 0;

		Debug.Log ("Camera activate!");
		playerCamera.SetActive (true);
		// playerCamera.GetComponent<Camera> ().depth = -1;
	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		Debug.Log (playerCamera);
		// Calculate air thrust direction / ray
		Vector3 airDirection = transform.TransformDirection (Input.GetButton("Jump") ? Vector3.down : new Vector3(0, -1, -1));
		Ray airRay = new Ray (transform.position, airDirection);
		Debug.DrawLine (transform.position, transform.position + (airDirection * 2), Color.blue);

		// Air Capacitor Charge
		if (Input.GetButton ("Jump")) {
			airCapacitorGauge += Time.deltaTime * 2;
			if (airCapacitorGauge > 3.5f) airCapacitorGauge = 3.5f;
		}

		// Air Capacitor Release
		if (!Input.GetButton("Jump") && airCapacitorGauge > 0) {
			AudioSource audio = GetComponent<AudioSource> ();
			audio.PlayOneShot (jumpSound);
			boardRigidbody.AddForce (-airDirection * airCapacitorGauge * 1000 * 1000 * 0.03f);
			airCapacitorGauge = 0;
		}

		// Air Capacitor Light
		airCapacitor.intensity = airCapacitorGauge > 0 ? airCapacitorGauge * 1.5f + 0.5f : 0;

		// Main Engine (and only engine right now Kappa)
		bool onGround = false;
		float airDistance = airRayDistance * 2;
		RaycastHit hitPoint;
		if (Physics.Raycast (airRay, out hitPoint, airRayDistance)) {
			Debug.DrawLine (transform.position, hitPoint.point);
			airDistance = hitPoint.distance;
			onGround = true;
		} else {
			airTime += Time.deltaTime;
		}

		// Landing Trick Sound Effect
		if (onGround) {
			if (airTime > 1.5f) {
				AudioSource audio = GetComponent<AudioSource> ();
				audio.PlayOneShot (landSound, 2);
			}
			airTime = 0;
		}

		// Fake air collision (stronger if closer to surface)
		float airPercentage = 1;
		if (airDistance >= minAirHeight) {
			airPercentage = ((airRayDistance - minAirHeight) - (airDistance - minAirHeight)) / (airRayDistance - minAirHeight);
			airPercentage = Mathf.Min (1, Mathf.Max (0, airPercentage));
			airPercentage *= airPercentage * airPercentage;
			float airForce = (minAirBoost + Mathf.Max (0, airPercentage * (maxAirBoost - minAirBoost)));
			boardRigidbody.AddForce (
				-airDirection * airForce * 1000 * Time.deltaTime
			);
		} else {
			Vector3 velocity = boardRigidbody.velocity;
			if (velocity.y < 0)
				velocity.x *= Mathf.Min (Mathf.Max (8, Mathf.Abs(velocity.y) * 0.8f), 1);
				velocity.z *= Mathf.Min (Mathf.Max (8, Mathf.Abs(velocity.y) * 0.8f), 1);
				velocity.y *= 0.2f;
			boardRigidbody.velocity = velocity;
			boardRigidbody.AddForce (-airDirection * (minAirBoost + (maxAirBoost - minAirBoost)) * 1000 * Time.deltaTime);
		}

		/* APPLY INPUTS FOR BOARD TURNING */
		float turnSpeed = boardTurnSpeed * (onGround ? 1.5f : 1);
		board.transform.Rotate(new Vector3(
			Input.GetAxis("Vertical") * Time.deltaTime * turnSpeed,
			Input.GetAxis("Turn") * Time.deltaTime * turnSpeed,
			-Input.GetAxis("Horizontal") * Time.deltaTime * turnSpeed
		));

		UpdateTurbulenceEmission ();

		/* RESET BUTTON */
		if (Input.GetButton ("Reset")) {
			var rot = transform.rotation.eulerAngles;
			transform.rotation = UnityEngine.Quaternion.Euler (new Vector3 (0, rot.y, 0));
			boardRigidbody.velocity = Vector3.zero;
		}

		SendRigidbodyUpdate ();
	}

	void UpdateTurbulenceEmission () {
		ParticleSystem.EmissionModule emission = turbulenceParticleSystem.emission;
		emission.enabled = boardRigidbody.velocity.magnitude > 50;
	}

	void SendRigidbodyUpdate () {
		var pos = boardRigidbody.position;
		var positionUpdate = new Position.Update ()
			.SetCoords (new Coordinates(pos.x, pos.y, pos.z));
		PositionWriter.Send (positionUpdate);

		var rotationUpdate = new Rotation.Update ()
			.SetRotation (boardRigidbody.rotation.ToNativeQuaternion ());
		RotationWriter.Send (rotationUpdate);
	}

	void OnDrawGizmos () {
		Gizmos.color = Color.black;
		Gizmos.DrawSphere (this.transform.position, 0.1f);
	}
}
