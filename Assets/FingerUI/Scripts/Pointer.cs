using UnityEngine;
using System.Collections;
using System;
using Leap;

/// <summary>
/// Represents a pointer on the canvas.
/// </summary>
public abstract class Pointer {

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
	private bool _enabled;


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
	/// Gets or sets the enabled.
	/// </summary>
	/// 
	/// When enabled, pointer can perform input.
	/// When disabled, it does not.
	/// 
	/// <value>Enabled or disabled.</value>
	public bool enabled {
		get {
			return _enabled;
		}
		set {
			_enabled = value;

			if (!value)
				state = State.OUT;
		}
	}

	/// <summary>
	/// Gets the canvas object.
	/// </summary>
	/// <value>The canvas.</value>
	public Canvas canvas { get; private set; }

	/// <summary>
	/// Gets the cursor when pointer is in interest.
	/// </summary>
	/// <value>The Ready cursor.</value>
	public GameObject cursorReady { get; private set; }

	/// <summary>
	/// Gets the cursor when pointer is in input.
	/// </summary>
	/// <value>The Input Cursor.</value>
	public GameObject cursorInput { get; private set; }

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

	/// <summary>
	/// Update pointer information and cursors.
	/// </summary>
	/// 
	/// This updates pointer information, and updates cursor, performs input with this information.
	/// 
	/// This is called from TipTrackBehaviour
	public void Update () {
		if (enabled) {
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

	/// <summary>
	/// Destroy associated GameObjects with this pointer.
	/// </summary>
	/// 
	/// If implementation introduces additional GameObjects, they should be destroyed
	/// in this function, too.
	public virtual void Destroy () {
		enabled = false;
		GameObject.Destroy (this.cursorInput);
		GameObject.Destroy (this.cursorReady);
	}


	public Pointer (Canvas canvas, GameObject cursorReady, GameObject cursorInput) {
		if (canvas == null)
			throw new ArgumentNullException (
				"canvas", 
				"Pointer needs a canvas.");

		_state = State.OUT;
		
		inputPeb = null;
		this.canvas = canvas;
		this.enabled = true;

		this.cursorReady = GameObject.Instantiate (cursorReady);
		this.cursorInput = GameObject.Instantiate (cursorInput);

		this.cursorReady.tag = "cursor";
		this.cursorInput.tag = "cursor";

		this.cursorReady.SetActive (false);
		this.cursorInput.SetActive (false);

		this.cursorReady.transform.SetParent (canvas.transform, false);
		this.cursorInput.transform.SetParent (canvas.transform, false);
	}
}


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
	/// Finger model.
	/// </summary>
	/// <value>The finger model.</value>
	public FingerModel model { get; private set; }

	/// <summary>
	/// Proximity distance to get in interest.
	/// </summary>
	/// 
	/// this value will be affected by scaling factor of Transform of canvas.
	/// 
	/// <value>The proximity distance.</value>
	public float proximityDistance { get; set; }

	protected override void UpdatePointer ()
	{
		if (model.GetLeapFinger ().IsExtended) {
			worldPosition = model.GetTipPosition ();
			position = canvas.transform.InverseTransformPoint (worldPosition);
			inputStrength = 1 + (position.z / proximityDistance);
		}
		else {
			inputStrength = -1;
		}
	}



	public PointerFinger (
		Canvas canvas,
		GameObject cursorReady,
		GameObject cursorInput,
		FingerModel model,
		float proximityDistance = 1.0f) : base (canvas, cursorReady, cursorInput)
	{
		this.model = model;
		this.proximityDistance = proximityDistance;
	}
}

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