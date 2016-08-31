using UnityEngine;
using System.Collections;

public class GuideDisplayBehaviour : MonoBehaviour {
	private GameObject _displayObject;


	public GameObject displayObject {
		get {
			return _displayObject;
		}

		set {

			if (_displayObject != null)
				_displayObject.SetActive (false);

			if (value.transform.parent != transform) {
				Debug.LogWarningFormat ("{0} is not child of {1}", value, this);
				_displayObject = null;
				return;
			}
			_displayObject = value;

			if (_displayObject != null)
				_displayObject.SetActive (true);
		}
			
	}

	public GameObject initialDisplayObject;

	// Use this for initialization
	void Start () {
		foreach (Transform child in transform) {
			child.gameObject.SetActive (false);
		}

		displayObject = initialDisplayObject;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
