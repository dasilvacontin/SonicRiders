using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Unity.Visualizer;
using Improbable.Core;
using Assets.Gamelogic.Utils;

public class RigidbodyReceiver : MonoBehaviour {

	[Require] private Position.Reader PositionReader;
	[Require] private Rotation.Reader RotationReader;

	private Rigidbody rigidbody;

	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
		rigidbody.position = PositionReader.Data.coords.ToUnityVector ();
		rigidbody.rotation = RotationReader.Data.rotation.ToUnityQuaternion ();
	}
	
	void Update () {
		// No point in applying one's updates
		if (PositionReader.HasAuthority)
			return;

		rigidbody.position = PositionReader.Data.coords.ToUnityVector ();
		rigidbody.rotation = RotationReader.Data.rotation.ToUnityQuaternion ();
	}
}
