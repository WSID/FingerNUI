using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using AlgoLib;

public class TextFeederBehaviour : MonoBehaviour {

	public Trie<string> words = new Trie<string> ();
	bool searching = false;

	// Renamed and retyped from TrieEntry<string> result;
	// To express multiple results.
	private IEnumerable<TrieEntry<string>> results;
	public string[] recommend = new string[4];
	int index;

	[HideInInspector]
	public char feeding {
		set {
			_prevFeeding = _feeding;
			_feeding = value;
			searching = true;
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
				if (searching) {
					if (!words.ContainsKey (target.text))
						words.Add (target.text, null);
					else {
						index = 0;
						int nindex = 0;

						if (target.text != null) {
							results = words.GetByPrefix (target.text);

							// Iterate over results instead of whole trie.
							foreach (var result in results) {
								if (index > 3)
									break;
								recommend [index++] = result.Key;
							}

							// Put them on the UI
							for (nindex = 0; nindex < index; nindex++) {
								wordUi [nindex].word = recommend [nindex];
								wordUi [nindex].gameObject.SetActive (true);

							}
						}
						for (; nindex < 4; nindex++) {
							wordUi [nindex].gameObject.SetActive (false);
						}
					}
				}
			}
		}
	}

	public WordUIBehaviour[] wordUi;
	public UnityEvent onFeedFinished;

	public string text {
		get {
			return (target != null) ? target.text : null;
		}
		set {
			if (target != null) {
				FinishFeeding ();
				target.text = value;
				target.caretPosition = value.Length;
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

		foreach (var Ui in wordUi) {
			Ui.gameObject.SetActive (false);
			Ui.feederBehaviour = this;
		}
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void FinishFeeding () {
		target.caretPosition = target.selectionAnchorPosition;
		searching = false;
		_prevFeeding = '\0';
		_feeding = '\0';

		foreach (var Ui in wordUi)
			Ui.gameObject.SetActive (false);

		onFeedFinished.Invoke ();
	}

	public void AddString (string item) {
		int caret = target.selectionAnchorPosition;
		searching = false;
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
		searching = false;
		builder.Append (target.text.Substring (0, caret - 1));
		builder.Append (target.text.Substring (caret));

		target.text = builder.ToString ();
		target.caretPosition = caret - 1;

		builder.Length = 0;
	}

	public void MoveLeft () {
		int position = target.caretPosition;
		searching = false;
		FinishFeeding ();

		target.caretPosition = (position != 0) ? (position - 1) : 0;
	}

	public void MoveRight () {
		int position = target.caretPosition;
		int length = target.text.Length;
		searching = false;
		FinishFeeding ();
		target.caretPosition = (length != position) ? (position + 1) : length;
	}
}
