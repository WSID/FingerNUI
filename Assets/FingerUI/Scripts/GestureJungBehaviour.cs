using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// A Behaviour that recognizes jung sung.
/// </summary>
/// 
/// This behaviour inputs jung sung from List of Vector3.
/// 
/// 1. Input style
///  
/// This follows similar fashion of mo-a key, which inputs jung sung by dragging
/// to a direction.
/// 
/// For more flexible development, it will hold a 5x5 lookup table.
/// 
/// 2. Strong sound
/// 
/// For strong sound input (or more precisely, first row/column and last row/
/// column), drag finger to a direction, and return its origin and drag again.
/// 
/// If finger was moved in the opposite direction that it was moved before, this
/// will be reseted, and checked for this direction.
public class GestureJungBehaviour : MonoBehaviour {

	/// <summary>
	/// The lookup table.
	/// </summary>
	/// 
	/// 1, 5th row/column will be used for strong sound.
	char[,] lookupTable = new char [7,9] {
		{'\0', '\0', '\0', '\0', 'ㅛ', '\0', '\0', '\0', '\0'},
		{'\0', '\0', 'ㅢ', 'ㅢ', 'ㅚ', '\0', '\0', '\0', '\0'},
		{'\0', '\0', 'ㅢ', 'ㅣ', 'ㅗ', 'ㅘ', 'ㅙ', '\0', '\0'},
		{'ㅖ', 'ㅕ', 'ㅔ', 'ㅓ', '\0', 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ'},
		{'\0', '\0', 'ㅞ', 'ㅝ', 'ㅜ', 'ㅡ', 'ㅢ', '\0', '\0'},
		{'\0', '\0', '\0', '\0', 'ㅟ', 'ㅢ', 'ㅢ', '\0', '\0'},
		{'\0', '\0', '\0', '\0', 'ㅠ', '\0', '\0', '\0', '\0'}
	};



	// Public adjustments

	/// <summary>
	/// Point Count Threshole
	/// </summary>
	/// Minimum point count to start recognization.
	public int thresholePointCount;

	/// <summary>
	/// Origin Area size.
	/// </summary>
	/// 
	/// User can move finger outside of origin area to input character.
	public Vector3 originVolumeSize;




	// Events

	/// <summary>
	/// Emitted When Jung sung is updated.
	/// </summary>
	/// char: Recognized character
	public CharEvent onUpdate;

	public UnityEvent<char> someEvent;




	// Public status

	/// <summary>
	/// The recognized jungsung.
	/// </summary>
	[HideInInspector]
	public char recognized;




	/// <summary>
	/// Recognized jungsung.
	/// </summary>
	/// <param name="points">Points as List</param>
	public void Recognize (List<Vector3> points) {
		if (points.Count < thresholePointCount)
			return;

		Vector3 origin = points [0];

		// Precalculate the position of border.
		float left = origin.x - originVolumeSize.x * 0.5f;
		float right = origin.x + originVolumeSize.x * 0.5f;
		float bot = origin.y - originVolumeSize.y * 0.5f;
		float top = origin.y + originVolumeSize.y * 0.5f;

		float front = origin.z - originVolumeSize.z * 0.5f;
		float back = origin.z + originVolumeSize.z * 0.5f;

		int nx = 0;
		int ny = 0;
		bool tripz = false;

		// Check how many times that finger get out of borders.
		//
		// Stores previous point locations for each iteration, and
		// if point gets out of border, set count variables.
		Vector3 prevPoint = origin;
		foreach (Vector3 point in points) {
			if (((left < prevPoint.x) && (point.x < left)) || ((left > prevPoint.x) && (left < point.x))) {
				if (nx < -4)
					nx = -4;
				else
					nx--;
			} else if (((prevPoint.x < right) && (right < point.x)) || ((prevPoint.x > right) && (right > point.x))) {
				if (nx > 4)
					nx = 4;
				else
					nx++;
			}

			if (((bot < prevPoint.y) && (point.y < bot)) || ((prevPoint.y < bot) && (point.y > bot))) {
				if (ny > 3)
					ny = 3;
				else
					ny++;
			} else if (((prevPoint.y < top) && (top < point.y)) || ((prevPoint.y > top) && (point.y < top))) {
				if (ny < -3)
					ny = -3;
				else
					ny--;
			}
			prevPoint = point;
		}

		recognized = lookupTable[(3 + ny),(4 + nx)];


		onUpdate.Invoke (recognized);
	}
}
