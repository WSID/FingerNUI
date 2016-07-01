using UnityEngine;
using System.Collections;

/// <summary>
/// Pointer event behaviour.
/// </summary>
/// 
/// This is base class for UI Elements that receive inputs from Pointer objects.
public abstract class PointerEventBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	/// <summary>
	/// When the pointer starts input
	/// </summary>
	/// <param name="pointer">Pointer.</param>
	public virtual void OnPointerBegin (Pointer pointer) {
	}

	/// <summary>
	/// When the pointer performing input
	/// </summary>
	/// <param name="pointer">Pointer.</param>
	/// <param name="localPosition">The Local position of pointer in GameObject's space</param>
	public virtual void OnPointerIn (Pointer pointer, Vector3 localPosition) {
	}

	/// <summary>
	/// When the pointer finishes input
	/// </summary>
	/// <param name="pointer">Pointer.</param>
	public virtual void OnPointerEnd (Pointer pointer) {
	}
}