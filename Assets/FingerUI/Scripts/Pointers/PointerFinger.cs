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
	/// The type of the finger.
	/// </summary>
	/// 
	/// This field determines which finger the pointer to be placed on.
	public Leap.Finger.FingerType fingerType;

	/// <summary>
	/// Proximity distance to get in interest.
	/// </summary>
	///
	/// this value will be affected by scaling factor of Transform of canvas.
	public float proximityDistance;

	protected override void UpdatePointer ()
	{
		FingerModel model = hand.fingers [(int) fingerType];

		if (model.GetLeapFinger ().IsExtended) {
			worldPosition = model.GetTipPosition ();
			position = canvas.transform.InverseTransformPoint (worldPosition);
			inputStrength = 1 + (position.z / proximityDistance);
		}
		else {
			inputStrength = -1;
		}
	}
}
