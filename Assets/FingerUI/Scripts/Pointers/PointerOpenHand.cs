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

	protected override void UpdatePointer () {
		int extendCount = 0;
		worldPosition = hand.fingers [2].GetTipPosition ();
		position = canvas.transform.InverseTransformPoint (worldPosition);

		foreach (FingerModel fmodel in hand.fingers) {
			if (fmodel.GetLeapFinger ().IsExtended)
				extendCount++;
		}

		inputStrength = (extendCount - 2) / 3.0f;
	}
}
