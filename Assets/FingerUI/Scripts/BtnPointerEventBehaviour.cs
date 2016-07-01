using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class BtnPointerEventBehaviour : PointerEventBehaviour {

	public Image image;
	public Color pushColor;
	public float pushColorDuration;

	public UnityEvent onPushed;


	private uint nfingers = 0;
	private Color nonpushColor;

	// Use this for initialization
	void Start () {
		if (image == null)
			image = GetComponent<Image> ();

		if (image != null) {
			nonpushColor = image.color;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override void OnPointerBegin (Pointer pointer) {
		if (! (pointer is PointerFinger))
			return;

		nfingers++;
		if ((image != null) && (nfingers == 1)) {
			image.CrossFadeColor (pushColor, pushColorDuration, false, true);
		}
	}

	public override void OnPointerIn (Pointer pointer, Vector3 tipLocalPosition) {
	}

	public override void OnPointerEnd (Pointer pointer) {
		if (! (pointer is PointerFinger))
			return;
		
		nfingers--;

		if ((image != null) && (nfingers == 0)) {
			image.CrossFadeColor (nonpushColor, pushColorDuration, false, true);
		}

		if (nfingers == 0)
			onPushed.Invoke ();
	}
}
