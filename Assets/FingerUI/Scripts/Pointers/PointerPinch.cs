using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Pointer based on pinch action.
/// </summary>
///
/// This pointer will be placed on the tip of thumb.
///
/// Leap Motion offers pinch strength values for each hand.
/// Pinch will be detected with this value.
public class PointerPinch: Pointer {

	public float pinchStrength { get; set; }


	protected override void UpdatePointer ()
	{
		FingerModel thumbModel = hand.fingers [0];
		worldPosition = thumbModel.GetTipPosition ();
		position = canvas.transform.InverseTransformPoint (worldPosition);
		inputStrength = hand.GetLeapHand ().PinchStrength;
	}
}
