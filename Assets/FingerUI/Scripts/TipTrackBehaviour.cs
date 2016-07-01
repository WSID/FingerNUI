using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
using Leap;


/// <summary>
/// Tip track behaviour.
/// </summary>
/// 
/// Tracks finger tips and do 2 things.
/// 1. Place cursor on the surface.
/// 2. Send event to underlying UI elements.
public class TipTrackBehaviour : MonoBehaviour {

	private class DataHand
	{
		public enum State {
			UNPINCH,
			PINCH
		}

		private State _state;

		private TipTrackBehaviour behaviour;
		private HandModel model;

		private Pointer[] pointers;

		public State state {
			get {
				return _state;
			}
			set {
				if (_state == value)
					return;

				switch (_state) {
				case DataHand.State.UNPINCH:
					OnUnpinchEnd ();
					break;
				case DataHand.State.PINCH:
					OnPinchEnd ();
					break;
				}

				_state = value;
				switch (_state) {
				case DataHand.State.UNPINCH:
					OnUnpinchBegin ();
					break;
				case DataHand.State.PINCH:
					OnPinchBegin ();
					break;
				}
			}

		}

		public DataHand (TipTrackBehaviour behaviour, HandModel model)
		{
			this.behaviour = behaviour;
			this.model = model;
			this._state = State.UNPINCH;

			FingerModel[] fmodels = model.fingers;

			pointers = new Pointer[3];

			pointers[0] = new PointerOpenHand (
				behaviour.GetComponent <Canvas> (),
				behaviour.pinchCursorPrefab,
				behaviour.pinchCursorPrefabIn,
				model);

			for (int i = 1; i < 3; i ++ )
			{
				pointers[i] = new PointerFinger (
					behaviour.GetComponent <Canvas> (),
					behaviour.cursorPrefab,
					behaviour.cursorPrefabIn,
					fmodels[i],
					behaviour.proximityDistance );
			}
		}

		public void Destroy () {
			foreach (Pointer pointer in pointers)
			{
				pointer.Destroy ();
			}
		}


		public void OnPinchBegin () {
		}

		public void OnPinchIn () {
			pointers [0].Update ();
		}

		public void OnPinchEnd () {
		}


		public void OnUnpinchBegin () {
			for (int i = 1; i < 3; i++) {
				pointers [i].enabled = true;
			}
		}

		public void OnUnpinchIn () {
			for (int i = 0; i < 3; i ++) {
				pointers [i].Update ();
			}
		}

		public void OnUnpinchEnd () {
			for (int i = 1; i < 3; i++) {
				pointers [i].enabled = false;
			}
		}



		public void Update () {
			bool isPinch;

			isPinch = (pointers[0].inputStrength == 1);

			state = isPinch ? State.PINCH : State.UNPINCH;


			switch (state) {
			case State.UNPINCH:
				OnUnpinchIn ();
				break;
			case State.PINCH:
				OnPinchIn ();
				break;
			}
		}
	}
		


	public GameObject cursorPrefab;
	public GameObject cursorPrefabIn;
	public GameObject cursorPrefabAir;

	public GameObject pinchCursorPrefab;
	public GameObject pinchCursorPrefabIn;
	public GameObject pinchCursorPrefabAir;



	public float proximityDistance = 1;
	public float pinchStrengthPrepare = 0.5f;
	public float pinchStrength = 0.9f;


	// Game Objects
	private Dictionary<HandModel, DataHand>	tracked_hands;

	// Use this for initialization
	void Start () {
		tracked_hands = new Dictionary<HandModel, DataHand> ();

		if (pinchCursorPrefab == null)
			pinchCursorPrefab = cursorPrefab;

		if (pinchCursorPrefabIn == null)
			pinchCursorPrefabIn = cursorPrefabIn;
	}

	// Update is called once per frame
	void Update () {
		foreach (var pair in tracked_hands)
		{
			pair.Value.Update ();
		}
	}

	public void addHand (HandModel model) {
		if (! tracked_hands.ContainsKey (model)) {
			tracked_hands [model] = new DataHand (this, model);
		}
	}

	public void removeHand (HandModel model) {
		if (tracked_hands.ContainsKey (model)) {
			DataHand data = tracked_hands [model];

			data.Destroy ();

			tracked_hands.Remove (model);
		}
	}

}
