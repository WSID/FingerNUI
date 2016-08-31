using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AlgoLib;

public class TextFeederBehaviour : MonoBehaviour {

	public Trie<string> words = new Trie<string> ();

	// Renamed and retyped from TrieEntry<string> result;
	// To express multiple results.
	private IEnumerable<TrieEntry<string>> results;
	public string[] recommend = new string[4];
	int index;

	[HideInInspector]
	public string feeding {
		set {
			_prevFeeding = _feeding;
			_feeding = value ?? "";

			if (target != null) {
				if (! searching) {
					searching = true;
					wordStart = target.caretPosition;
					wordEnd = wordStart;
				}

				target.ActivateInputField ();

				builder.Append (target.text);
				int indexStart = target.selectionFocusPosition;
				int indexEnd = target.selectionAnchorPosition;

				if (indexEnd != indexStart) {
					builder.Remove (indexStart, indexEnd - indexStart);
				}

				if (true) {
					builder.Insert (indexStart, _feeding);
					target.text = builder.ToString ();
					target.selectionFocusPosition = indexStart;
					target.selectionAnchorPosition = indexStart + _feeding.Length;
				} else {
					target.text = builder.ToString ();
					target.caretPosition = indexStart;
				}
				builder.Length = 0;
				if (searching) {

					wordEnd = wordEnd + _feeding.Length - _prevFeeding.Length;
					
					index = 0;
					int nindex = 0;

					if (wordStart < wordEnd) {
						string prefix = target.text.Substring (wordStart, wordEnd - wordStart);
						
						// If text has some bite of string, get from trie with string as prefix.
						if (prefix != null && prefix != "") {
							foreach (var result in words.GetByPrefix (prefix)) {
								if (index > 3)
									break;
								recommend [index++] = result.Key;
							}
						}

						// If user did not input any text, get all from trie.
						else {
							foreach (var result in words) {
								if (index > 3)
									break;
								recommend [index++] = result.Key;
							}
						}


						// Put them on the UI
						for (nindex = 0; nindex < index; nindex++) {
							wordUi [nindex].word = recommend [nindex];
							wordUi [nindex].gameObject.SetActive (true);

						}

						for (; nindex < 4; nindex++) {
							wordUi [nindex].gameObject.SetActive (false);
						}
					}
				}
			}
		}
	}

	public bool searching {
		get { return _searching; }
		set {
			if (! value) {
				foreach (WordUIBehaviour ui in wordUi)
					ui.gameObject.SetActive (false);
			}

			_searching = value;
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
				searching = false;
				wordStart = 0;
				wordEnd = 0;

				FinishFeeding ();
				target.text = value;
				target.caretPosition = value.Length;
			}
		}
	}

	// Inner state
	private string _prevFeeding = "";
	private string _feeding = "";

	private StringBuilder builder;
	private InputField target;

	private bool _searching;

	private int wordStart;
	private int wordEnd;


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
		_prevFeeding = "";
		_feeding = "";

		foreach (var Ui in wordUi)
			Ui.gameObject.SetActive (false);

		onFeedFinished.Invoke ();
	}

	public void AddString (string item) {
		FinishFeeding ();

		int caret = target.caretPosition;
		builder.Append (target.text.Substring (0, caret));
		builder.Append (item);
		builder.Append (target.text.Substring (caret));

		if (searching) {
			int nletter = item.Count (char.IsLetterOrDigit);

			// If item has non letter parts (space, etc,...)
			if (nletter != item.Length) {
				if (wordStart < caret) {
					string substr = target.text.Substring (wordStart, caret + nletter - wordStart);

					if (!words.ContainsKey (substr))
						words.Add (substr, null);
				}
					
				searching = false;
			}

			// If item just contains letters, just extend word range.
			else {
				wordEnd += nletter;
			}
		}

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

		if (searching)
			wordEnd--;

		if ((target.caretPosition < wordStart) || (wordEnd <= wordStart))
			searching = false;

		builder.Length = 0;
	}

	public void MoveLeft () {
		FinishFeeding ();

		int position = target.caretPosition;

		target.caretPosition = (position != 0) ? (position - 1) : 0;

		if (target.caretPosition < wordStart)
			searching = false;
	}

	public void MoveRight () {
		FinishFeeding ();

		int position = target.caretPosition;
		int length = target.text.Length;

		target.caretPosition = (length != position) ? (position + 1) : length;

		if (wordEnd < target.caretPosition)
			searching = false;
	}


	public void SetWord (string word) {
		if (searching) {
			builder.Length = 0;

			builder.Append (target.text.Substring (0, wordStart));
			builder.Append (word);
			builder.Append (target.text.Substring (wordEnd, target.text.Length - wordEnd));

			target.text = builder.ToString ();
			target.caretPosition = wordStart + word.Length;
			searching = false;
		}
	}
}
