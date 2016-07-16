using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Pointer based on grab action.
/// </summary>
///
/// This pointer will be placed on the palm.
///
/// Leap Motion offers grab strength values for each hand.
/// Pinch will be detected with this value.
public class PointerGrab: Pointer {

	protected override void UpdatePointer ()
	{
		worldPosition = hand.GetPalmPosition ();
		position = canvas.transform.InverseTransformPoint (worldPosition);
		inputStrength = hand.GetLeapHand ().GrabStrength;
	}
}
