using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WordUIBehaviour : MonoBehaviour {

	private string _word;

	public string word {
		get {
			return _word;
		}

		set {
			this._word = value;
			wordText.text = word;
		}
	}
	public Text wordText;

	[HideInInspector]
	public TextFeederBehaviour feederBehaviour;

	// Use this for initialization
	void Start () {
		BtnPointerEventBehaviour btnBehaviour = GetComponent <BtnPointerEventBehaviour> ();

		if (btnBehaviour != null)
			btnBehaviour.onPushed.AddListener (feed);
	}

	// Update is called once per frame
	void Update () {

	}

	public void feed () {
		if (feederBehaviour != null) {
			feederBehaviour.text = word;
		}
	}
}
