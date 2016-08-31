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

		private float actionTime;
		private float rotateTime;
		private Vector3 palmVelocityPrev;

		private Vector3 handDirPrev;
		private Vector3 handNorPrev;
		private float rotSpdPrev;

		private UnityEvent actionPrev;

		public DataHand (HandActionBehaviour behaviour, HandModel model)
		{
			this.behaviour = behaviour;
			this.model = model;

			ResetPrevs ();
		}

		public void ResetPrevs () {
			handDirPrev = model.GetPalmDirection ();
			handNorPrev = model.GetPalmNormal ();
			rotSpdPrev = 0;

			actionTime = -1;
			rotateTime = -1;
		}

		public void Update () {
			float deltaTimeInv = 1.0f / Time.deltaTime;
			float currentTime = Time.time;

			UnityEvent action = null;

			Vector3 palmVelocity = model.GetLeapHand ().PalmVelocity.ToUnity ();
			Vector3 palmAccel = (palmVelocity - palmVelocityPrev) * deltaTimeInv;

			// Rotational.
			Vector3 handDir = model.GetPalmDirection ();
			Vector3 handNor = model.GetPalmNormal ();

			Quaternion handDirRot = Quaternion.FromToRotation (handDirPrev, handDir);
			Vector3 handNorPrevRot = handDirRot * handNorPrev;

			Vector3 rotCross = Vector3.Cross (handNor, handNorPrevRot);
			bool rotRight = (0 < Vector3.Dot (rotCross, handDir));
			float rotAmount = Mathf.Asin (rotCross.magnitude);
			rotAmount = (rotRight) ? rotAmount : -rotAmount;

			float rotSpd = rotAmount * deltaTimeInv;
			float rotAcc = rotSpd * deltaTimeInv;

			if (behaviour.actionInterval < (currentTime - actionTime)) {

				if ((palmVelocity.x < -behaviour.sweepSpeed) &&
				    (palmAccel.x < -behaviour.sweepAccel))
				{
					action = behaviour.onSweepLeft;
				}
				else if ((behaviour.sweepSpeed < palmVelocity.x) &&
				         (behaviour.sweepAccel < palmAccel.x))
				{
					action = behaviour.onSweepRight;
				}
				else if ((palmVelocity.y < -behaviour.sweepSpeed) &&
				         (palmAccel.y < -behaviour.sweepAccel))
				{
					action = behaviour.onSweepDown;
				}
				else if ((behaviour.sweepSpeed < palmVelocity.y) &&
				         (behaviour.sweepAccel < palmAccel.y))
				{
					action = behaviour.onSweepUp;
				}
				else if (palmVelocity.magnitude < behaviour.rotateMaxVelocity)
				{
					if ((rotSpd < -behaviour.rotateSpeed) &&
					    (rotAcc < -behaviour.rotateAccel))
					{
						action = behaviour.onRotateLeft;
					}
					else if ((behaviour.rotateSpeed < rotSpd) &&
					         (behaviour.rotateAccel < rotAcc))
					{
						action = behaviour.onRotateRight;
					}
				}
			}

			palmVelocityPrev = palmVelocity;

			handDirPrev = handDir;
			handNorPrev = handNor;
			rotSpdPrev = rotSpd;

			if ((action != null) &&
				((action == actionPrev) ||
					(behaviour.oppositeInterval < (currentTime - actionTime))))
			{
				action.Invoke ();
				actionPrev = action;
				actionTime = currentTime;
			}
		}
	}

	/// <summary>
	/// Speed for sweep action.
	/// </summary>
	public float sweepSpeed = 300;

	public float sweepAccel = 1000;

	public float rotateSpeed = 20;
	public float rotateAccel = 100;

	/// <summary>
	/// Minimum palm velocity for rotation action.
	/// </summary>
	public float rotateMaxVelocity = 200;

	/// <summary>
	/// Minimal interval between same actions.
	/// </summary>
	public float actionInterval = 1.0f;

	/// <summary>
	/// Minimal interval between actions.
	/// </summary>
	public float oppositeInterval = 2.5f;

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

	void OnDisabled ()
	{
		foreach (var pair in tracked_hands) {
			pair.Value.ResetPrevs ();
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

