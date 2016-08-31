using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class ToggleBtnPointerEventBehaviour : BtnPointerEventBehaviour
{

	public bool switchOn;

	public BoolEvent onToggled;

	public UnityEvent onSwitchOn;
	public UnityEvent onSwitchOff;

	public override void Start () {
		base.Start ();
		onPushed.AddListener (() => {
			switchOn = ! switchOn;

			onToggled.Invoke (switchOn);
			((switchOn) ? onSwitchOn : onSwitchOff).Invoke ();
		});
	}
}
