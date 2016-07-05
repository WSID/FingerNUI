using UnityEngine;
using System;

public struct OptionInputSurface
{
	public static OptionInputSurface optionResult = null;

	public Vector3 position;
	public Vector3 normal;
	public Vector3 up;

	public OptionInputSurface (Vector3 position, Vector3 normal, Vector3 up = Vector3.up) {
		this.position = position;
		this.normal = normal;
		this.up = up;
	}

	public void ApplyTo (Transform transform) {
		transform.position = position;
		transform.rotation = Quaternion.LookRotation (normal, up);
	}

	public void ApplyTo (GameObject gameObject) {
		ApplyTo (gameObject.transform);
	}
}

