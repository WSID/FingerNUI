using UnityEngine;
using UnityEngine.UI;

using System.Text;
using System.Collections;

public class TextFeederBehaviour : MonoBehaviour {

	[HideInInspector]
	public char feeding {
		set {
			_prevFeeding = _feeding;
			_feeding = value;

			if (target != null) {
				target.ActivateInputField ();

				builder.Append (target.text);
				int indexStart = target.selectionFocusPosition;
				int indexEnd = target.selectionAnchorPosition;

				if (indexEnd != indexStart) {
					builder.Remove (indexStart, indexEnd - indexStart);
				}

				if (_feeding != '\0') {
					builder.Insert (indexStart, _feeding);
					target.text = builder.ToString ();
					target.selectionFocusPosition = indexStart;
					target.selectionAnchorPosition = indexStart + 1;
				} else {
					target.text = builder.ToString ();
					target.caretPosition = indexStart;
				}
				builder.Length = 0;
			}
		}
	}


	// Inner state
	private char _prevFeeding = '\0';
	private char _feeding = '\0';

	private StringBuilder builder;
	private InputField target;


	// Use this for initialization
	void Start () {
		builder = new StringBuilder ();
		target = GetComponent <InputField> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void FinishFeeding () {
		target.caretPosition = target.selectionAnchorPosition;
		_prevFeeding = '\0';
		_feeding = '\0';
	}

	public void AddString (string item) {
		int caret = target.selectionAnchorPosition;

		builder.Append (target.text.Substring (0, caret));
		builder.Append (item);
		builder.Append (target.text.Substring (caret));

		target.text = builder.ToString ();
		target.caretPosition = caret + item.Length;

		builder.Length = 0;
		
	}

	public void DeleteBack () {
		int caret = target.selectionAnchorPosition;
		if (caret == 0)
			return;

		builder.Append (target.text.Substring (0, caret - 1));
		builder.Append (target.text.Substring (caret));

		target.text = builder.ToString ();
		target.caretPosition = caret - 1;

		builder.Length = 0;
	}
}
