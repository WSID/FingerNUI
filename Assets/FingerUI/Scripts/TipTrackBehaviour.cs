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
			FOLDHAND,
			OPENHAND,
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
				case DataHand.State.FOLDHAND:
					OnFoldHandEnd ();
					break;
				case DataHand.State.OPENHAND:
					OnOpenHandEnd ();
					break;
				}

				_state = value;
				switch (_state) {
				case DataHand.State.FOLDHAND:
					OnFoldHandBegin ();
					break;
				case DataHand.State.OPENHAND:
					OnOpenHandBegin ();
					break;
				}
			}

		}

		public uint InPointerCount;

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

			this.state = State.FOLDHAND;
			this.InPointerCount = 0;
		}

		public void Destroy () {
			foreach (Pointer pointer in pointersFoldHand)
			{
				GameObject.Destroy (pointer.gameObject);
			}
			GameObject.Destroy (pointerOpenHand.gameObject);
		}


		public void OnOpenHandBegin () {
		}

		public void OnOpenHandIn () {
			if (pointerOpenHand.state == Pointer.State.INPUT)
				InPointerCount = 1;
		}

		public void OnOpenHandEnd () {
		}


		public void OnFoldHandBegin () {
			foreach (Pointer pointer in pointersFoldHand) {
				pointer.enabled = true;
			}
		}

		public void OnFoldHandIn () {
			foreach (Pointer pointer in pointersFoldHand) {
				if (pointer.state == Pointer.State.INPUT)
					InPointerCount += 1;
				
			}
		}

		public void OnFoldHandEnd () {
			foreach (Pointer pointer in pointersFoldHand) {
				pointer.enabled = false;
			}
		}

		public void OnDisabledBegin () {
			pointerOpenHand.enabled = false;
		}



		public void Update () {
			bool isPinch;

			isPinch = (pointerOpenHand.inputStrength >= 0.9);

			state = isPinch ? State.OPENHAND : State.FOLDHAND;

			InPointerCount = 0;

			switch (state) {
			case State.FOLDHAND:
				OnFoldHandIn ();
				break;
			case State.OPENHAND:
				OnOpenHandIn ();
				break;
			}
		}
	}

	public Pointer[] pointersFoldHand;
	public Pointer   pointerOpenHand;

	public UnityEvent onAnyPointerIn;
	public UnityEvent onAllPointerOut;

	// Game Objects
	private Dictionary<HandModel, DataHand>	tracked_hands = new Dictionary <HandModel, DataHand> ();


	// Internal properties
	private bool _IsAnyPointer;
	private bool IsAnyPointer {
		get {
			return _IsAnyPointer;
		}
		set {
			if ((!_IsAnyPointer) && (value)) {
				onAnyPointerIn.Invoke ();
			} else if ((_IsAnyPointer) && (!value)) {
				onAllPointerOut.Invoke ();
			}
			_IsAnyPointer = value;
		}
	}


	// Update is called once per frame
	void Update () {
		uint InPointerCount = 0;

		foreach (var pair in tracked_hands)
		{
			DataHand data_hand = pair.Value;
			data_hand.Update ();
			InPointerCount += data_hand.InPointerCount;
		}

		IsAnyPointer = (0 < InPointerCount);
	}

	void OnEnabled () {
		foreach (var pair in tracked_hands) {
			pair.Value.state = DataHand.State.FOLDHAND;
		}
	}

	void OnDisabled () {
		foreach (var pair in tracked_hands) {
			pair.Value.state = DataHand.State.DISABLED;
		}
	}

	public void addHand (HandModel model) {
		if (! tracked_hands.ContainsKey (model)) {
			DataHand data = new DataHand (this, model);

			data.state = enabled ? DataHand.State.FOLDHAND : DataHand.State.DISABLED;
			tracked_hands [model] = data;

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
