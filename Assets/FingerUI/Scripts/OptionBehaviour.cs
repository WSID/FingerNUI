using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class OptionBehaviour : MonoBehaviour {
	public static Dictionary<string, Object> optionResults = null;

	public const string KEY_OPTION_INPUT_SURFACE = "input-surface";

	// Use this for initialization
	void Start () {
		if (optionResults == null) {
			optionResults = new Dictionary <string, Object> ();
			Debug.Log ("Option cleared!");
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public void showOptionInputSurface () {
		optionResults.Remove (KEY_OPTION_INPUT_SURFACE);

		SceneManager.LoadScene ("OptionInputSurface");
	}
}
