using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

using System;

public class FingerUI2Behaviour : MonoBehaviour
{
	public HandController handController;

	void Start () {
		int optionInputSurfaceSet = PlayerPrefs.GetInt ("option-input-surface-set");

		if (optionInputSurfaceSet == 1) {
			OptionInputSurface option = OptionInputSurface.load_pref ("option-input-surface");

			option.ApplyTo (gameObject);
		}
	}

	public void Quit () {
		Application.Quit ();
	}

	public void ShowScene (string sceneName) {
		SceneManager.LoadScene (sceneName);
	}
}

