using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardControl : MonoBehaviour {

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

	void Start () {
		boardRigidbody = board.GetComponent<Rigidbody>();
		boardRigidbody.centerOfMass = new Vector3 (0, -0.15f, 0);
		turbulenceParticleSystem = turbulence.GetComponent<ParticleSystem> ();
		airTime = 0;
	}

	// Update is called once per frame
	void FixedUpdate ()
	{

		// Air Capacitor
		if (Input.GetButton ("Jump")) {
			airCapacitorGauge += Time.deltaTime * 2;
		}

		Vector3 airDirection = transform.TransformDirection (Input.GetButton("Jump") ? Vector3.down : new Vector3(0, -1, -1));
		Debug.DrawLine (transform.position, transform.position + (airDirection * 2), Color.blue);

		// Air Capacitor release
		if (!Input.GetButton("Jump") && airCapacitorGauge > 0) {
			AudioSource audio = GetComponent<AudioSource> ();
			audio.PlayOneShot (jumpSound);
			boardRigidbody.AddForce (-airDirection * airCapacitorGauge * 1000 * 1000 * 0.03f);
			airCapacitorGauge = 0;
		}

		// Air Capacitor Light
		if (airCapacitorGauge > 3.5f)
			airCapacitorGauge = 3.5f;
		airCapacitor.intensity = airCapacitorGauge > 0 ? airCapacitorGauge * 1.5f + 0.5f : 0;

		// Main Engine
		Ray airRay = new Ray (transform.position, airDirection);

		float airDistance = airRayDistance * 2;
		bool onGround = false;
		RaycastHit hitPoint;
		if (Physics.Raycast (airRay, out hitPoint, airRayDistance)) {
			Debug.DrawLine (transform.position, hitPoint.point);
			airDistance = hitPoint.distance;
			onGround = true;
		} else {
			airTime += Time.deltaTime;
		}

		float turnSpeed = boardTurnSpeed;
		if (onGround) {
			if (airTime > 1.5f) {
				AudioSource audio = GetComponent<AudioSource> ();
				audio.PlayOneShot (landSound, 2);
			}
			airTime = 0;
		} else {
			turnSpeed *= 1.5f;
		}

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

		board.transform.Rotate(new Vector3(
			Input.GetAxis("Vertical") * Time.deltaTime * turnSpeed,
			Input.GetAxis("Turn") * Time.deltaTime * turnSpeed,
			-Input.GetAxis("Horizontal") * Time.deltaTime * turnSpeed
		));

		ParticleSystem.EmissionModule emission = turbulenceParticleSystem.emission;
		if (boardRigidbody.velocity.magnitude > 50) {
			emission.enabled = true;
		} else {
			emission.enabled = false;
		}

		if (Input.GetButton ("Reset")) {
			var rot = transform.rotation.eulerAngles;
			transform.rotation = Quaternion.Euler (new Vector3 (0, rot.y, 0));
			boardRigidbody.velocity = Vector3.zero;
		}
	}

	void onTriggerEnter (Collider other) {
		Debug.Log (other);
	}

	void OnDrawGizmos () {
		Gizmos.color = Color.black;
		Gizmos.DrawSphere (this.transform.position, 0.1f);
	}
}
