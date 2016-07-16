using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Pointer based on finger tip.
/// </summary>
///
/// This pointer is plain type of pointer, which will be placed at the finger tip.
///
/// Gets in interest when finger tip approaches the canvas.
/// Gets in input when finger tip get through the canvas.
///
/// If the finger is folded, this will not be visible.
public class PointerFinger: Pointer {

	/// <summary>
	/// Finger model.
	/// </summary>
	/// <value>The finger model.</value>
	public FingerModel model { get; private set; }

	/// <summary>
	/// Proximity distance to get in interest.
	/// </summary>
	///
	/// this value will be affected by scaling factor of Transform of canvas.
	///
	/// <value>The proximity distance.</value>
	public float proximityDistance { get; set; }

	protected override void UpdatePointer ()
	{
		if (model.GetLeapFinger ().IsExtended) {
			worldPosition = model.GetTipPosition ();
			position = canvas.transform.InverseTransformPoint (worldPosition);
			inputStrength = 1 + (position.z / proximityDistance);
		}
		else {
			inputStrength = -1;
		}
	}



	public PointerFinger (
		Canvas canvas,
		GameObject cursorReady,
		GameObject cursorInput,
		FingerModel model,
		float proximityDistance = 1.0f) : base (canvas, cursorReady, cursorInput)
	{
		this.model = model;
		this.proximityDistance = proximityDistance;
	}
}
