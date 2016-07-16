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
	public HandModel model { get; private set; }


	protected override void UpdatePointer ()
	{
		worldPosition = model.GetPalmPosition ();
		position = canvas.transform.InverseTransformPoint (worldPosition);
		inputStrength = model.GetLeapHand ().GrabStrength;
	}


	public PointerGrab (
		Canvas canvas,
		GameObject cursorReady,
		GameObject cursorInput,
		HandModel model) : base (canvas, cursorReady, cursorInput)
	{
		this.model = model;
	}
}
