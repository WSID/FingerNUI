using UnityEngine;
using System.Collections;

public class LoadOptionInputSurface : MonoBehaviour {

	// Use this for initialization
	void Start () {
		int optionInputSurfaceSet = PlayerPrefs.GetInt ("option-input-surface-set");

		if (optionInputSurfaceSet == 1) {
			OptionInputSurface option = OptionInputSurface.load_pref ("option-input-surface");

			option.ApplyTo (gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
