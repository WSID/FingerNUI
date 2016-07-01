using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Notifies when hand is added or removed.
/// </summary>
public class HandTrackBehaviour : MonoBehaviour
{
	private Dictionary<int, HandModel>	tracked_hands;
	private List<int> tracked_hands_remove;

	/// <summary>
	/// The hand controller.
	/// </summary>
	/// 
	/// A Hand controller.
	/// 
	/// If null, this will be set first HandController in the scene.
	public HandController handController;


	/// <summary>
	/// When Hand is added.
	/// </summary>
	public HandModelEvent onHandAdded;

	/// <summary>
	/// When hand is removed.
	/// </summary>
	public HandModelEvent onHandRemoved;

	// Use this for initialization
	void Start ()
	{
		tracked_hands = new Dictionary<int, HandModel> ();
		tracked_hands_remove = new List<int> ();

		if (handController == null)
			handController = GameObject.FindObjectOfType <HandController> ();

		if (handController == null)
			throw new MissingComponentException (
				"HandTrackBehaviour requires at least one HandController component on the scene.");
	}
	
	// Update is called once per frame
	void Update ()
	{
		HandModel[] hmodels = handController.GetAllGraphicsHands ();

		if (hmodels != null) {

			// Add hand models that is being tracked.
			foreach (HandModel hmodel in hmodels) {
				if (!tracked_hands.ContainsKey (hmodel.GetInstanceID ())) {
					tracked_hands [hmodel.GetInstanceID ()] = hmodel;

					onHandAdded.Invoke (hmodel);
				}
			}


			// Look for hand models that is not tracked.
			foreach (var pair in tracked_hands) {
				bool toRemove = true;

				foreach (HandModel hmodel in hmodels) {
					if (pair.Key == hmodel.GetInstanceID ()) {
						toRemove = false;
						break;
					}
				}
				if (toRemove)
					tracked_hands_remove.Add (pair.Key);
			}

			tracked_hands_remove.ForEach (RemoveHand);
			tracked_hands_remove.Clear ();
		} else {
			tracked_hands_remove.AddRange (tracked_hands.Keys);
			tracked_hands_remove.ForEach (RemoveHand);
			tracked_hands_remove.Clear ();
		}
	}

	private void RemoveHand ( int id ) {
		if (tracked_hands.ContainsKey (id)) {

			onHandRemoved.Invoke (tracked_hands [id]);
			tracked_hands.Remove (id);

		}
	}
}

