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
	private FingerModel thumbModel;

	public HandModel model { get; private set; }
	public float pinchStrength { get; set; }


	protected override void UpdatePointer ()
	{
		worldPosition = thumbModel.GetTipPosition ();
		position = canvas.transform.InverseTransformPoint (worldPosition);
		inputStrength = model.GetLeapHand ().PinchStrength;
	}


	public PointerPinch (
		Canvas canvas,
		GameObject cursorReady,
		GameObject cursorInput,
		HandModel model,
		float pinchStrength = 0.95f) : base (canvas, cursorReady, cursorInput)
	{
		this.model = model;
		this.pinchStrength = pinchStrength;

		thumbModel = model.fingers [0];
	}
}
