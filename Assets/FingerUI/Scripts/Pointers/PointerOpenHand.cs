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
	public float proximityDistance;

	protected override void UpdatePointer () {
		int extendCount = 0;
		Vector3 tipWorldPosition;
		Vector3 tipPosition;

		tipWorldPosition = hand.fingers [2].GetTipPosition ();
		tipPosition = canvas.transform.InverseTransformPoint (tipWorldPosition);

		worldPosition = hand.GetPalmPosition ();
		position = canvas.transform.InverseTransformPoint (worldPosition);

		foreach (FingerModel fmodel in hand.fingers) {
			if (fmodel.GetLeapFinger ().IsExtended)
				extendCount++;
		}

		inputStrength = (1 + (tipPosition.z / proximityDistance)) * (extendCount - 2) / 3.0f;
	}
}
