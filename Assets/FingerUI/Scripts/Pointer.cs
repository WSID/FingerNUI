using UnityEngine;
using System.Collections;
using System;
using Leap;

/// <summary>
/// Represents a pointer on the canvas.
/// </summary>
public abstract class Pointer : MonoBehaviour{

	private Canvas _canvas;

	/// <summary>
	/// State of Input
	/// </summary>
	public static int pointer_In;
	private enum State
	{
		/// <summary>
		/// Pointer is out of interest.
		/// </summary>
		/// Cursor will not be visible
		OUT,

		/// <summary>
		/// Pointer is in interest.
		/// </summary>
		/// Cursor will be visible, but not performing input
		READY,

		/// <summary>
		/// Pointer is performing input
		/// </summary>
		/// Cursor will be changed to indicate the pointer is performing input.
		INPUT
	}

	/// <summary>
	/// PointerEventBehaviour that this pointer is handled.
	/// </summary>
	private PointerEventBehaviour inputPeb;
	private State _state;


	/// <summary>
	/// The state of input
	/// </summary>
	/// <value>The state.</value>
	private State state {
		get {
			return _state;
		}
		set {
			if (_state != value) {
				if (_state == State.READY)
					OnReadyEnd ();
				else if (_state == State.INPUT)
					OnInputEnd ();

				_state = value;

				if (_state == State.READY)
					OnReadyBegin ();
				else if (_state == State.INPUT)
					OnInputBegin ();
			}
		}
	}

	/// <summary>
	/// HandModel for this pointer.
	/// </summary>
	/// <value>The hand model</value>
	[HideInInspector]
	public virtual HandModel hand { get; set; }

	/// <summary>
	/// Gets the canvas object.
	/// </summary>
	/// <value>The canvas.</value>
	public Canvas canvas {
		get {
			return _canvas;
		}
		set {
			_canvas = value;

			if (_canvas != null) {
				if (cursorInput != null) {
					cursorInput.transform.SetParent (value.transform, false);
				}

				if (cursorReady != null) {
					cursorReady.transform.SetParent (value.transform, false);
				}
			} else {
				if (cursorInput != null) {
					cursorInput.transform.SetParent (transform);
					cursorInput.SetActive (false);
				}

				if (cursorReady != null) {
					cursorReady.transform.SetParent (transform);
					cursorReady.SetActive (false);
				}
				
			}
		}
	}

	/// <summary>
	/// Whether cursors are should be duplicated.
	/// </summary>
	/// 
	/// Sometimes pointer owns cursor. but most of times,
	/// it would share cursor objects.
	/// 
	/// If this is true, this will duplicate cursor for it.
	public bool duplicateCursor;

	/// <summary>
	/// Gets the cursor when pointer is in interest.
	/// </summary>
	/// <value>The Ready cursor.</value>
	public GameObject cursorReady;

	/// <summary>
	/// Gets the cursor when pointer is in input.
	/// </summary>
	/// <value>The Input Cursor.</value>
	public GameObject cursorInput;

	/// <summary>
	/// the input strength that updated in UpdatePointer().
	/// </summary>
	/// Implementation should set this in UpdatePointer() function.
	///
	/// If input depth is less than 0, pointer won't visible.
	/// If input depth is between 0 ~ 1, pointer will be visible, and let
	/// user to know where would input will begin.
	/// 
	/// If input depth is greater than 1, actual input will take place.
	///
	/// <value>The input depth.</value>
	public float inputStrength { get; protected set; }

	/// <summary>
	/// World position of pointer that updated in UpdatePointer().
	/// </summary>
	/// Implementation should set this in UpdatePointer() function.
	///
	/// Setting this does not set position property.
	/// These two should be setted by individual.
	/// 
	/// <value>The position in world space.</value>
	public Vector3 worldPosition { get; protected set; }

	/// <summary>
	/// Canvas position of pointer that updated in UpdatePointer()
	/// </summary>
	/// Implementation should set this in UpdatePointer() function.
	/// 
	/// This position will be used to determine where the input will have effect.
	/// 
	/// <value>The position.</value>
	public Vector3 position { get; protected set; }


	/// <summary>
	/// Update pointer information (inputStrength, position,...).
	/// </summary>
	/// 
	/// On each frame, this function is called to update pointer position.
	/// This is due to the tracking model of leap motion device, which
	/// also updates on every frame.
	protected abstract void UpdatePointer ();


	void Start () {
		if (duplicateCursor) {
			if (cursorReady != null) {
				cursorReady = Instantiate (cursorReady);
				cursorReady.transform.SetParent (canvas.transform, false);
			}

			if (cursorInput != null) {
				cursorInput = Instantiate (cursorInput);
				cursorInput.transform.SetParent (canvas.transform, false);
			}
		}
	}

	void OnDisable () {
		state = State.OUT;
	}

	void OnDestroy () {
		GameObject.Destroy (this.cursorInput);
		GameObject.Destroy (this.cursorReady);
	}

	/// <summary>
	/// Update pointer information and cursors.
	/// </summary>
	/// 
	/// This updates pointer information, and updates cursor, performs input with this information.
	/// 
	/// This is called from TipTrackBehaviour
	public void Update () {
		UpdatePointer ();

		if (inputStrength <= 0)
			state = State.OUT;
		else if (inputStrength < 1)
			state = State.READY;
		else
			state = State.INPUT;


		if (state == State.READY)
			OnReadyIn ();
		else if (state == State.INPUT)
			OnInputIn ();
	}


	#region READY State functions
	private void OnReadyBegin () {
		cursorReady.SetActive (true);
	}

	private void OnReadyIn () {
		cursorReady.transform.localPosition = new Vector3 (position.x, position.y, 0);
		cursorReady.GetComponent <CanvasRenderer> ().SetAlpha (inputStrength);
	}

	private void OnReadyEnd () {
		cursorReady.SetActive (false);
	}
	#endregion


	#region INPUT State functions
	private void OnInputBegin () {
		cursorInput.SetActive (true);
		pointer_In++;
		if(pointer_In > 0){
			GameObject.FindObjectOfType <Camera> ().backgroundColor = Color.yellow;			
		}
		
		RectTransform enterTransform = canvas.transform as RectTransform;
		RectTransform enterTransformNext;

		do {
			enterTransformNext = null;

			foreach (Transform child in enterTransform) {
				RectTransform childRect = child as RectTransform;

				if (childRect != null) {
					if (childRect.tag == "cursor")
						break;

					if (RectTransformContains (childRect)) {
						enterTransform = childRect;
						enterTransformNext = childRect;
						break;
					}
				}
			}

		} while (enterTransformNext != null);

		inputPeb = enterTransform.GetComponentInParent <PointerEventBehaviour> ();

		if (inputPeb != null) {
			inputPeb.OnPointerBegin (this);
		}	
	}
	
	private void OnInputIn () {
		cursorInput.transform.localPosition = new Vector3 (position.x, position.y, 0);

		// Call OnFingerIn Function.
		if (inputPeb != null) {
			Vector3 localPosition = inputPeb.transform.InverseTransformPoint (worldPosition);
			inputPeb.OnPointerIn (this, localPosition);
		}
	}

	private void OnInputEnd () {
		cursorInput.SetActive (false);
		if (inputPeb != null)
			inputPeb.OnPointerEnd (this);
		inputPeb = null;
		pointer_In--;
		
		if(pointer_In<=0)
			GameObject.FindObjectOfType <Camera> ().backgroundColor = Color.blue;
	}

	#endregion


	/// <summary>
	/// Checks whether pointer is in the RectTransform.
	/// </summary>
	/// 
	/// <returns>Whether the pointer is in the RectTransform</returns>
	/// <param name="trans">RectTransform to check</param>
	private bool RectTransformContains (RectTransform trans) {
		Vector3 positionCurrent = trans.InverseTransformPoint (worldPosition);

		return trans.rect.Contains (positionCurrent);
	}

}
