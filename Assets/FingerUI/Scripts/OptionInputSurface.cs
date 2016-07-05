using UnityEngine;
using System;

public class OptionInputSurface
{
	public Vector3 position;
	public Quaternion rotation;

	public OptionInputSurface (Vector3 position, Vector3 normal, Vector3 up) :
	this (position, Quaternion.LookRotation (normal, up)) {
	}

	public OptionInputSurface (Vector3 position, Quaternion rotation) {
		this.position = position;
		this.rotation = rotation;
	}

	public void ApplyTo (Transform transform) {
		transform.position = position;
		transform.rotation = rotation;
	}

	public void ApplyTo (GameObject gameObject) {
		ApplyTo (gameObject.transform);
	}


	public void store_pref (string prefix) {
		store_pref (prefix + "-position", position);
		store_pref (prefix + "-rotation", rotation);
	}


	public static OptionInputSurface load_pref (string prefix) {
		return new OptionInputSurface (
			load_pref_v3 (prefix + "-position"),
			load_pref_q (prefix + "-rotation"));
	}

	private static Vector3 load_pref_v3 (string prefix) {
		Vector3 result = new Vector3 ();

		result.x = PlayerPrefs.GetFloat (prefix + "_x");
		result.y = PlayerPrefs.GetFloat (prefix + "_y");
		result.z = PlayerPrefs.GetFloat (prefix + "_z");

		return result;
	}

	private static Quaternion load_pref_q (string prefix) {
		Quaternion result = new Quaternion ();

		result.x = PlayerPrefs.GetFloat (prefix + "_x");
		result.y = PlayerPrefs.GetFloat (prefix + "_y");
		result.z = PlayerPrefs.GetFloat (prefix + "_z");
		result.w = PlayerPrefs.GetFloat (prefix + "_w");

		return result;
	}

	private static void store_pref (string prefix, Vector3 vector) {
		PlayerPrefs.SetFloat (prefix + "_x", vector.x);
		PlayerPrefs.SetFloat (prefix + "_y", vector.y);
		PlayerPrefs.SetFloat (prefix + "_z", vector.z);
	}

	private static void store_pref (string prefix, Quaternion q) {
		PlayerPrefs.SetFloat (prefix + "_x", q.x);
		PlayerPrefs.SetFloat (prefix + "_y", q.y);
		PlayerPrefs.SetFloat (prefix + "_z", q.z);
		PlayerPrefs.SetFloat (prefix + "_w", q.w);
	}
}

