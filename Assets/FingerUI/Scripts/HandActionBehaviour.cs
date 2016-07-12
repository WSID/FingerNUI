using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Leap;

/// <summary>
/// Performs some action on Hand Action/Gesture
/// </summary>
public class HandActionBehaviour : MonoBehaviour
{

	private class DataHand
	{

		private HandActionBehaviour behaviour;
		private HandModel model;

		private float sweepTime = -1;
		private Vector3 palmVelocityPrev;

		public DataHand (HandActionBehaviour behaviour, HandModel model)
		{
			this.behaviour = behaviour;
			this.model = model;
		}

		public void Update () {

			float currentTime = Time.time;
			Vector3 palmVelocity = model.GetLeapHand ().PalmVelocity.ToUnity ();

			if (behaviour.sweepInterval < (currentTime - sweepTime)) {

				if ((-behaviour.sweepSpeed < palmVelocityPrev.x) && (palmVelocity.x < -behaviour.sweepSpeed)) {
					behaviour.onSweepLeft.Invoke ();
					sweepTime = currentTime;
				}
				else if ((behaviour.sweepSpeed < palmVelocity.x) && (palmVelocityPrev.x < behaviour.sweepSpeed)) {
					behaviour.onSweepRight.Invoke ();
					sweepTime = currentTime;
				}
				if ((-behaviour.sweepSpeed < palmVelocityPrev.y) && (palmVelocity.y < -behaviour.sweepSpeed)) {
					behaviour.onSweepDown.Invoke ();
					sweepTime = currentTime;
				}
				else if ((behaviour.sweepSpeed < palmVelocity.y) && (palmVelocityPrev.y < behaviour.sweepSpeed)) {
					behaviour.onSweepUp.Invoke ();
					sweepTime = currentTime;
				}
			}
			palmVelocityPrev = palmVelocity;

		}
	}

	/// <summary>
	/// Speed for sweep action.
	/// </summary>
	public float sweepSpeed = 300;

	/// <summary>
	/// Minimal interval between sweep actions.
	/// </summary>
	public float sweepInterval = 1.0f;

	public float rotateSpeed = 1;
	public float rotateInterval = 1.0f;

	/// <summary>
	/// When hand sweeps to left
	/// </summary>
	public UnityEvent onSweepLeft;

	/// <summary>
	/// When hand sweeps to right
	/// </summary>
	public UnityEvent onSweepRight;

	/// <summary>
	/// When hand sweeps to up
	/// </summary>
	public UnityEvent onSweepUp;

	/// <summary>
	/// When hand sweeps to down
	/// </summary>
	public UnityEvent onSweepDown;



	public UnityEvent onRotateLeft;
	public UnityEvent onRotateRight;

	private Dictionary<HandModel, DataHand>	tracked_hands;

	// Use this for initialization
	void Start ()
	{
		tracked_hands = new Dictionary<HandModel, DataHand> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		foreach (var pair in tracked_hands) {
			pair.Value.Update ();
		}
	}

	// Finalize things.
	void Stop ()
	{
		tracked_hands = null;
	}

	public void addHand (HandModel model) {
		if (! tracked_hands.ContainsKey (model)) {
			tracked_hands [model] = new DataHand (this, model);
		}
	}

	public void removeHand (HandModel model) {
		if (tracked_hands.ContainsKey (model)) {
			tracked_hands.Remove (model);
		}
	}
}

