using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Pointer with opened hand.
/// </summary>
///
/// This pointer will be placed on the tip of middle finger.
///
/// Pointer that start input when all of fingers are extended.
public class PointerOpenHand: Pointer {
	public HandModel model {get ; private set; }

	protected override void UpdatePointer () {
		int extendCount = 0;
		worldPosition = model.fingers [2].GetTipPosition ();
		position = canvas.transform.InverseTransformPoint (worldPosition);

		foreach (FingerModel fmodel in model.fingers) {
			if (fmodel.GetLeapFinger ().IsExtended)
				extendCount++;
		}

		inputStrength = (extendCount - 2) / 3.0f;
	}

	public PointerOpenHand (
		Canvas canvas,
		GameObject cursorReady,
		GameObject cursorInput,
		HandModel model) : base (canvas, cursorReady, cursorInput)
	{
		this.model = model;
	}
}
