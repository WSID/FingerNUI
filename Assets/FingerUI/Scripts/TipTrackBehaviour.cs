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
			PINCH,
			DISABLED
		}

		private State _state;

		private TipTrackBehaviour behaviour;
		private HandModel model;

		private Pointer[] pointersFoldHand;
		private Pointer pointerOpenHand;

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
			Canvas canvas = FindObjectOfType <Canvas> ();

			this.behaviour = behaviour;
			this.model = model;

			this.pointersFoldHand = new Pointer[behaviour.pointersFoldHand.Length];
			for (int i = 0; i < pointersFoldHand.Length; i++) {
				pointersFoldHand[i] = Instantiate <Pointer> (behaviour.pointersFoldHand[i]);
				pointersFoldHand[i].canvas = canvas;
				pointersFoldHand[i].hand = model;
				pointersFoldHand[i].enabled = behaviour.enabled;
			}

			this.pointerOpenHand = Instantiate <Pointer> (behaviour.pointerOpenHand);
			this.pointerOpenHand.canvas = canvas;
			this.pointerOpenHand.hand = model;
			this.pointerOpenHand.enabled = behaviour.enabled;

			this.state = State.UNPINCH;
		}

		public void Destroy () {
			foreach (Pointer pointer in pointersFoldHand)
			{
				GameObject.Destroy (pointer.gameObject);
			}
			GameObject.Destroy (pointerOpenHand.gameObject);
		}


		public void OnPinchBegin () {
		}

		public void OnPinchIn () {
		}

		public void OnPinchEnd () {
		}


		public void OnUnpinchBegin () {
			foreach (Pointer pointer in pointersFoldHand) {
				pointer.enabled = true;
			}
		}

		public void OnUnpinchIn () {
			for (int i = 0; i < 3; i ++) {
			}
		}

		public void OnUnpinchEnd () {
			foreach (Pointer pointer in pointersFoldHand) {
				pointer.enabled = false;
			}
		}

		public void OnDisabledBegin () {
			pointerOpenHand.enabled = false;
		}



		public void Update () {
			bool isPinch;

			isPinch = (pointerOpenHand.inputStrength == 1);

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
	public Pointer[] pointersFoldHand;
	public Pointer   pointerOpenHand;


	// Game Objects
	private Dictionary<HandModel, DataHand>	tracked_hands = new Dictionary <HandModel, DataHand> ();

	// Use this for initialization
	void Start () {
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

	void OnEnabled () {
		foreach (var pair in tracked_hands) {
			pair.Value.state = DataHand.State.UNPINCH;
		}
	}

	void OnDisabled () {
		foreach (var pair in tracked_hands) {
			pair.Value.state = DataHand.State.DISABLED;
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
