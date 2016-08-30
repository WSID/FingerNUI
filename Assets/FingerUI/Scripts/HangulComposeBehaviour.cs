using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Hangul Compose Behaviour.
/// </summary>
/// 
/// Composes Hangul from characters that feed by external.
/// 
/// 0. Preface
/// 
/// To be more clear, I will call character components, like chosung, as letter-component, to
/// reduce confusion between Unity's Component (which is basic logic block)
/// 
/// 
/// 1. Constants.
/// 
/// This file holds lots of constants and lookup tables.
/// Code Folding Supported Editor is recommended.
/// 
/// 2. Character classes.
/// 
/// Same hangul characters are appeared in multiple groupes. There are 6 groups
///   a. Chosung          [0x1100 - 0x1112]: 19 characters.
///   b. Jungsung         [0x1161 - 0x1175]: 21 characters.
///   c. Jongsung         [0x11a8 - 0x11c2]: 27 characters.
///   d. letter consonant [0x3131 - 0x314E]: 30 characters.
///   e. letter vowels    [0x314f - 0x3163]: 21 characters.
///   f. letters          [0xac00 - ...]:    composed characters.
/// 
/// a-c are not used in 2-bul style input, but used for 3-bul style input.
/// d-e are not used in 3-bul style input, but used for 2-bul style input.
/// 
/// f are actual composed letters.
/// 
/// In this implementations, each letter components are converted to letters in a-c groups.
/// 
/// 
/// 3. Character Editing.
/// 
/// Characters are edited by setting property chosung, jungsung, jongsung.
/// Until someone marks its done, character editing can be continued.
/// 
/// When editing is done, it can be marked by finishChosung (), finishJungsung (), finishJongsung ().
/// Marking and feeding happen letter-component-wisely separated. Other letter-components can be 
/// edited when a letter-component is marked.
/// 
/// Editing on marked letter-component results in editing new character.
/// 
/// 4. Character Composing
/// 
/// Letters are composed by multipling indices.
/// 
/// For a letter, increasing by 1 iterates to next jongsung.
/// and increasing by 28 (including the case of no-jongsung), iterates to next jungsung.
/// and increasing by 588, iterates to next chosung.
/// 
/// By this, character can be composed by adding to character value of '가'
/// 
public class HangulComposeBehaviour : MonoBehaviour {


	#region Constants and tables.
	const char CHOSUNG_FIRST = '\u1100';
	const char CHOSUNG_LAST = '\u1112';

	const int NCHOSUNG = 19;

	const char JUNGSUNG_FIRST = '\u1161';
	const char JUNGSUNG_LAST = '\u1175';

	const int NJUNGSUNG = 21;

	const char JONGSUNG_FIRST = '\u11a8';
	const char JONGSUNG_LAST = '\u11c2';

	const int NJONGSUNG = 27;
	const int NJUNGJONG = NJUNGSUNG * (NJONGSUNG + 1);

	const char LETTER_CONSONANT_FIRST = '\u3131';
	const char LETTER_CONSONANT_LAST = '\u314E';

	const char LETTER_VOWEL_FIRST = '\u314f';
	const char LETTER_VOWEL_LAST = '\u3163';

	const char LETTER_FIRST = '\uac00';


	static char[] TABLE_LETTER_CHO = new char[] {
		'\u1100',	//ㄱ
		'\u1101',	//ㄱㄱ
		'\0',	    //ㄱㅅ
		'\u1102',	//ㄴ
		'\0',	    //ㄴㅈ
		'\0',	    //ㄴㅎ
		'\u1103',	//ㄷ
		'\u1104',	//ㄷㄷ
		'\u1105',	//ㄹ
		'\0',    	//ㄹㄱ
		'\0',	    //ㄹㅁ
		'\0',	    //ㄹㅂ
		'\0',	    //ㄹㅅ
		'\0',	    //ㄹㅌ
		'\0',	    //ㄹㅍ
		'\0',	    //ㄹㅎ
		'\u1106',	//ㅁ
		'\u1107',	//ㅂ
		'\u1108',	//ㅂㅂ
		'\0',	    //ㅂㅅ
		'\u1109',	//ㅅ
		'\u110a',	//ㅅㅅ
		'\u110b',	//ㅇ
		'\u110c',	//ㅈ
		'\u110d',	//ㅈㅈ
		'\u110e',	//ㅊ
		'\u110f',	//ㅋ
		'\u1110',	//ㅌ
		'\u1111',	//ㅍ
		'\u1112' 	//ㅎ
	};

	static char[] TABLE_LETTER_JONG = new char[] {
		'\u11a8',	//ㄱ
		'\u11a9',	//ㄱㄱ
		'\u11aa',   //ㄱㅅ
		'\u11ab',	//ㄴ
		'\u11ac',   //ㄴㅈ
		'\u11ad',	//ㄴㅎ
		'\u11ae',	//ㄷ
		'\0',	    //ㄷㄷ
		'\u11af',	//ㄹ
		'\u11b0',	//ㄹㄱ
		'\u11b1',	//ㄹㅁ
		'\u11b2',	//ㄹㅂ
		'\u11b3',	//ㄹㅅ
		'\u11b4',	//ㄹㅌ
		'\u11b5',	//ㄹㅍ
		'\u11b6',	//ㄹㅎ
		'\u11b7',	//ㅁ
		'\u11b8',	//ㅂ
		'\0',	    //ㅂㅂ
		'\u11b9',	//ㅂㅅ
		'\u11ba',	//ㅅ
		'\u11bb',	//ㅅㅅ
		'\u11bc',	//ㅇ
		'\u11bd',	//ㅈ
		'\0',	    //ㅈㅈ
		'\u11be',	//ㅊ
		'\u11bf',	//ㅋ
		'\u11c0',	//ㅌ
		'\u11c1',	//ㅍ
		'\u11c2' 	//ㅎ
	};

	static char[] TABLE_CHO_LETTER = new char[] {
		'\u3131',   //ㄱ
		'\u3132',   //ㄲ
		'\u3134',   //ㄴ
		'\u3137',   //ㄷ
		'\u3138',   //ㄸ
		'\u3139',   //ㄹ
		'\u3141',   //ㅁ
		'\u3142',   //ㅂ
		'\u3143',   //ㅃ
		'\u3145',   //ㅅ
		'\u3146',   //ㅆ
		'\u3147',   //ㅇ
		'\u3148',   //ㅈ
		'\u3149',   //ㅉ
		'\u314a',   //ㅊ
		'\u314b',   //ㅋ
		'\u314c',   //ㅌ
		'\u314d',   //ㅍ
		'\u314e'    //ㅎ
	};

	static char[] TABLE_JONG_LETTER = new char[] {
		'\u3131',   //ㄱ
		'\u3132',   //ㄱㄱ
		'\u3133',   //ㄱㅅ
		'\u3134',   //ㄴ
		'\u3135',   //ㄴㅈ
		'\u3136',   //ㄴㅎ
		'\u3137',   //ㄷ
		'\u3139',   //ㄹ
		'\u313a',   //ㄹㄱ
		'\u313b',   //ㄹㅁ
		'\u313c',   //ㄹㅂ
		'\u313d',   //ㄹㅅ
		'\u313e',   //ㄹㅌ
		'\u313f',   //ㄹㅍ
		'\u3140',   //ㄹㅎ
		'\u3141',   //ㅁ
		'\u3142',   //ㅂ
		'\u3144',   //ㅂㅅ
		'\u3145',   //ㅅ
		'\u3146',   //ㅅㅅ
		'\u3147',   //ㅇ
		'\u3148',   //ㅈ
		'\u314a',   //ㅊ
		'\u314b',   //ㅋ
		'\u314c',   //ㅌ
		'\u314d',   //ㅍ
		'\u314e'    //ㅎ
	};

	static char[][] TABLE_JONG_STACK = new char [][] {
		// ㄱ 
		new char [] {
			'\u11a8',   //ㄱ
			'\0',        //ㄱㄱ
			'\0',        //ㄱㅅ
			'\0',        //ㄴ
			'\0',        //ㄴㅈ
			'\0',        //ㄴㅎ
			'\0',        //ㄷ
			'\0',        //ㄹ
			'\0',        //ㄹㄱ
			'\0',        //ㄹㅁ
			'\0',        //ㄹㅂ
			'\0',        //ㄹㅅ
			'\0',        //ㄹㅌ
			'\0',        //ㄹㅍ
			'\0',        //ㄹㅎ
			'\0',        //ㅁ
			'\0',        //ㅂ
			'\0',        //ㅂㅅ
			'\u11aa',   //ㅅ
			'\0',        //ㅅㅅ
			'\0',        //ㅇ
			'\0',        //ㅈ
			'\0',        //ㅊ
			'\0',        //ㅋ
			'\0',        //ㅌ
			'\0',        //ㅍ
			'\0',        //ㅎ
		},
		null,	// ㄱㄱ
		null,  // ㄱㅅ

		// ㄴ 
		new char [] {
			'\0',        //ㄱ
			'\0',        //ㄱㄱ
			'\0',        //ㄱㅅ
			'\0',        //ㄴ
			'\0',        //ㄴㅈ
			'\0',        //ㄴㅎ
			'\0',        //ㄷ
			'\0',        //ㄹ
			'\0',        //ㄹㄱ
			'\0',        //ㄹㅁ
			'\0',        //ㄹㅂ
			'\0',        //ㄹㅅ
			'\0',        //ㄹㅌ
			'\0',        //ㄹㅍ
			'\0',        //ㄹㅎ
			'\0',        //ㅁ
			'\0',        //ㅂ
			'\0',        //ㅂㅅ
			'\0',        //ㅅ
			'\0',        //ㅅㅅ
			'\0',        //ㅇ
			'\u11ac',   //ㅈ
			'\0',        //ㅊ
			'\0',        //ㅋ
			'\0',        //ㅌ
			'\0',        //ㅍ
			'\u11ad',   //ㅎ
		},

		null, // ㄴㅈ
		null, // ㄴㅎ
		null, // ㄷ

		// ㄹ
		new char [] {
			'\u11b0',   //ㄱ
			'\0',        //ㄱㄱ
			'\0',        //ㄱㅅ
			'\0',        //ㄴ
			'\0',        //ㄴㅈ
			'\0',        //ㄴㅎ
			'\0',        //ㄷ
			'\0',        //ㄹ
			'\0',        //ㄹㄱ
			'\0',        //ㄹㅁ
			'\0',        //ㄹㅂ
			'\0',        //ㄹㅅ
			'\0',        //ㄹㅌ
			'\0',        //ㄹㅍ
			'\0',        //ㄹㅎ
			'\u11b1',   //ㅁ
			'\u11b2',   //ㅂ
			'\0',        //ㅂㅅ
			'\u11b3',   //ㅅ
			'\0',        //ㅅㅅ
			'\0',        //ㅇ
			'\0',        //ㅈ
			'\0',        //ㅊ
			'\0',        //ㅋ
			'\u11b4',   //ㅌ
			'\u11b5',   //ㅍ
			'\u11b6'    //ㅎ
		},
		null, // ㄹㄱ
		null, // ㄹㅁ
		null, // ㄹㅂ
		null, // ㄹㅅ
		null, // ㄹㅌ
		null, // ㄹㅍ
		null, // ㄹㅎ
		null, // ㅁ

		// ㅂ
		new char [] {
			'\0',        //ㄱ
			'\0',        //ㄱㄱ
			'\0',        //ㄱㅅ
			'\0',        //ㄴ
			'\0',        //ㄴㅈ
			'\0',        //ㄴㅎ
			'\0',        //ㄷ
			'\0',        //ㄹ
			'\0',        //ㄹㄱ
			'\0',        //ㄹㅁ
			'\0',        //ㄹㅂ
			'\0',        //ㄹㅅ
			'\0',        //ㄹㅌ
			'\0',        //ㄹㅍ
			'\0',        //ㄹㅎ
			'\0',        //ㅁ
			'\0',        //ㅂ
			'\0',        //ㅂㅅ
			'\u11b9',   //ㅅ
			'\0',        //ㅅㅅ
			'\0',        //ㅇ
			'\0',        //ㅈ
			'\0',        //ㅊ
			'\0',        //ㅋ
			'\0',        //ㅌ
			'\0',        //ㅍ
			'\0',        //ㅎ
		},
		null, // ㅂㅅ

		// ㅅ
		new char [] {
			'\0',        //ㄱ
			'\0',        //ㄱㄱ
			'\0',        //ㄱㅅ
			'\0',        //ㄴ
			'\0',        //ㄴㅈ
			'\0',        //ㄴㅎ
			'\0',        //ㄷ
			'\0',        //ㄹ
			'\0',        //ㄹㄱ
			'\0',        //ㄹㅁ
			'\0',        //ㄹㅂ
			'\0',        //ㄹㅅ
			'\0',        //ㄹㅌ
			'\0',        //ㄹㅍ
			'\0',        //ㄹㅎ
			'\0',        //ㅁ
			'\0',        //ㅂ
			'\0',        //ㅂㅅ
			'\u11bb',   //ㅅ
			'\0',        //ㅅㅅ
			'\0',        //ㅇ
			'\0',        //ㅈ
			'\0',        //ㅊ
			'\0',        //ㅋ
			'\0',        //ㅌ
			'\0',        //ㅍ
			'\0',        //ㅎ
		},

		null, // ㅅㅅ
		null, // ㅇ
		null, // ㅈ
		null, // ㅊ
		null, // ㅋ
		null, // ㅌ
		null, // ㅍ
		null  // ㅎ
	};

	static char[,] TABLE_JONG_UNSTACK = new char [,] {
		{'\u11a8' ,'\0' },     //ㄱ
		{'\u11a8' ,'\u11a8' }, //ㄱㄱ
		{'\u11a8' ,'\u11ba' }, //ㄱㅅ
		{'\u11ab' ,'\0' },     //ㄴ
		{'\u11ab' ,'\u11bd' }, //ㄴㅈ
		{'\u11ab' ,'\u11c2' }, //ㄴㅎ
		{'\u11ae' ,'\0' },     //ㄷ
		{'\u11af' ,'\0' },     //ㄹ
		{'\u11af' ,'\u11a8' }, //ㄹㄱ
		{'\u11af' ,'\u11b7' }, //ㄹㅁ
		{'\u11af' ,'\u11b8' }, //ㄹㅂ
		{'\u11af' ,'\u11ba' }, //ㄹㅅ
		{'\u11af' ,'\u11c0' }, //ㄹㅌ
		{'\u11af' ,'\u11c1' }, //ㄹㅍ
		{'\u11af' ,'\u11c2' }, //ㄹㅎ
		{'\u11b7' ,'\0' },     //ㅁ
		{'\u11b8' ,'\0' },     //ㅂ
		{'\u11b8' ,'\u11ba' }, //ㅂㅅ
		{'\u11ba' ,'\0' },     //ㅅ
		{'\u11ba' ,'\u11ba' }, //ㅅㅅ
		{'\u11bc' ,'\0' },     //ㅇ
		{'\u11bd' ,'\0' },     //ㅈ
		{'\u11be' ,'\0' },     //ㅊ
		{'\u11bf' ,'\0' },     //ㅋ
		{'\u11c0' ,'\0' },     //ㅌ
		{'\u11c1' ,'\0' },     //ㅍ
		{'\u11c2' ,'\0' }      //ㅎ
	};
	#endregion




	#region Events
	/// <summary>
	/// Emitted on updating
	/// </summary>
	/// This is used for 
	public CharEvent onUpdate;
	public StringEvent onUpdateString;

	public StringEvent onUpdateChosung;
	public StringEvent onUpdateJungsung;
	public StringEvent onUpdateJongsung;

	public CharEvent onEmit;
	public StringEvent onEmitString;

	public UnityEvent onDeleteBack;
	#endregion

	// Inner state.
	private char _chosung;
	private char _jungsung;

	private char _jongsung;
	private char _jongsung1;
	private char _jongsung2;

	[HideInInspector]
	public char chosung {
		get {
			return _chosung;
		}
		set {
			if (value == '\0')
				return;

			if (chosungDone) {
				Emit ();
			}

			_chosung = ToChosung (value);
			onUpdateChosung.Invoke (_chosung.ToString ());
			UpdateLetter ();
		}
	}

	[HideInInspector]
	public char jungsung {
		get {
			return _jungsung;
		}
		set{
			if (value == '\0')
				return;
			
			if (jungsungDone) {
				Emit ();
			}

			_jungsung = ToJungsung (value);
			onUpdateJungsung.Invoke (_jungsung.ToString ());
			UpdateLetter ();
		}
	}

	[HideInInspector]
	public char jongsung {
		get {
			return _jongsung;
		}
		set {
			if (value == '\0')
				return;
			
			if (jongsungDone) {
				Emit ();
			}

			_jongsung = ToJongsung (value);
			UnstackJongsung (
				_jongsung,
				out _jongsung1,
				out _jongsung2);
			
			onUpdateJongsung.Invoke (_jongsung.ToString () );
			UpdateLetter ();
		}
	}

	public char jongsung1 {
		get {
			return _jongsung1;
		}
		set {
			if (value == '\0')
				return;

			if (jongsungDone1) {
				Emit ();
			}

			_jongsung1 = ToJongsung (value);
			_jongsung = StackJongsung (_jongsung1, _jongsung2);

			onUpdateJongsung.Invoke (_jongsung.ToString ());
			UpdateLetter ();
		}
	}

	public char jongsung2 {
		get {
			return _jongsung2;
		}
		set {
			if (value == '\0')
				return;

			if (jongsungDone2) {
				Emit ();
			}

			_jongsung2 = ToJongsung (value);
			_jongsung = StackJongsung (_jongsung1, _jongsung2);

			onUpdateJongsung.Invoke (_jongsung.ToString ());
			UpdateLetter ();
		}
	}

	public bool chosungDone { get; private set; }
	public bool jungsungDone { get; private set; }
	public bool jongsungDone { get; private set; }

	public bool jongsungDone1;
	public bool jongsungDone2;

	public char letter { get; private set; }






	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}



	public void FinishChosung () {
		chosungDone = (_chosung != '\0');
	}

	public void FinishJungsung () {
		jungsungDone = (_jungsung != '\0');
	}

	public void FinishJongsung () {
		jongsungDone1 = (_jongsung1 != '\0');
		jongsungDone2 = (_jongsung2 != '\0');
		jongsungDone = jongsungDone1 && jongsungDone2;
	}

	public void FinishJongsung1 () {
		jongsungDone1 = (_jongsung1 != '\0');
		jongsungDone = jongsungDone1 && jongsungDone2;
	}

	public void FinishJongsung2 () {
		jongsungDone2 = (_jongsung2 != '\0');
		jongsungDone = jongsungDone1 && jongsungDone2;
	}





	public void FeedConsonant (char input) {
		if (input == '\0')
			return;
		
		if (chosungDone && jungsungDone && (!jongsungDone)) {
			if (jongsungDone1)
				jongsung2 = input;
			else
				jongsung1 = input;
		}
		else {
			if (chosungDone)
				Emit ();
			chosung = input;
		}
	}

	public void FeedConsonantDone () {
		if (chosungDone && jungsungDone && (!jongsungDone)) {
			if (jongsungDone1)
				FinishJongsung2 ();
			else
				FinishJongsung1 ();
		}
		else {
			FinishChosung ();
		}
	}

	public void FeedVowel (char input) {
		if (input == '\0')
			return;
		
		if (jongsungDone1) {
			char nchosung;

			if (jongsungDone2) {
				nchosung = _jongsung2;
				_jongsung2 = '\0';
				_jongsung = _jongsung1;
			}
			else {
				nchosung = _jongsung1;
				_jongsung1 = '\0';
				_jongsung = '\0';
			}

			UpdateLetter ();
			Emit ();

			Debug.Log (nchosung);

			chosung = ToConsonant (nchosung);
			chosungDone = true;
			jungsung = input;
		} else {
			if (jungsungDone)
				Emit ();
			jungsung = input;
		}
	}

	public void FeedVowelDone () {
		FinishJungsung ();
	}

	public void DeleteOne () {
		if (_chosung != '\0') {
			if (_jungsung != '\0') {
				if (_jongsung1 != '\0') {
					if (_jongsung2 != '\0') {
						_jongsung2 = '\0';
						_jongsung = _jongsung1;
						jongsungDone = false;
						jongsungDone2 = false;
					}
					else {
						_jongsung1 = '\0';
						_jongsung = '\0';
						jongsungDone = false;
						jongsungDone1 = false;
					}
					onUpdateJongsung.Invoke (_jongsung.ToString ());
				}
				else {
					_jungsung = '\0';
					jungsungDone = false;
					onUpdateJungsung.Invoke (_jungsung.ToString ());
				}
			}
			else {
				_chosung = '\0';
				chosungDone = false;
				onUpdateChosung.Invoke (_chosung.ToString ());
			}
			UpdateLetter ();
		}
		else if (_jungsung != '\0') {
			if (_jongsung1 != '\0') {
				if (_jongsung2 != '\0') {
					_jongsung2 = '\0';
					_jongsung = _jongsung1;
					jongsungDone = false;
					jongsungDone2 = false;
				}
				else {
					_jongsung1 = '\0';
					_jongsung = '\0';
					jongsungDone = false;
					jongsungDone1 = false;
				}
			}
			else {
				_jungsung = '\0';
				jungsungDone = false;
			}
			UpdateLetter ();
		}
		else if (_jongsung != '\0') {
			_jongsung = '\0';
			jongsungDone = false;
			UpdateLetter ();
		}
		else {
			onDeleteBack.Invoke ();
		}
	}


	public void DropLetter () {
		chosungDone = false;
		jungsungDone = false;
		jongsungDone = false;
		jongsungDone1 = false;
		jongsungDone2 = false;

		_chosung = '\0';
		_jungsung = '\0';
		_jongsung = '\0';
		_jongsung1 = '\0';
		_jongsung2 = '\0';

		letter = '\0';

		onUpdate.Invoke ('\0');
		onUpdateString.Invoke ("");
		onUpdateChosung.Invoke (chosung.ToString ());
		onUpdateJungsung.Invoke (jungsung.ToString ());
		onUpdateJongsung.Invoke (jongsung.ToString ());
	}

	public void UpdateLetter () {
		letter = Compose (_chosung, _jungsung, _jongsung);
		onUpdate.Invoke (letter);
		onUpdateString.Invoke ((letter != '\0') ? letter.ToString () : "");
	}

	public void Emit () {
		if (letter != '\0') {
			onEmit.Invoke (letter);
			onEmitString.Invoke (new string (new char[] { letter }));
		}

		DropLetter ();
	}


	private static bool IsBetween (char c, char a, char b) {
		return (a <= c) && (c <= b);
	}

	private static char ToChosung (char letterChosung) {
		if (IsBetween (letterChosung, LETTER_CONSONANT_FIRST, LETTER_CONSONANT_LAST)) {
			int index = (int)(letterChosung - LETTER_CONSONANT_FIRST);
			return TABLE_LETTER_CHO [index];
		}
		else if (IsBetween (letterChosung, CHOSUNG_FIRST, CHOSUNG_LAST)) {
			return letterChosung;
		}
		return '\0';
	}

	private static char ToJungsung (char letterJungsung) {
		if (IsBetween (letterJungsung, LETTER_VOWEL_FIRST, LETTER_VOWEL_LAST)) {
			char index = (char)(letterJungsung - LETTER_VOWEL_FIRST);
			return (char)(JUNGSUNG_FIRST + index);
		}
		else if (IsBetween (letterJungsung, JUNGSUNG_FIRST, JUNGSUNG_LAST)) {
			return letterJungsung;
		}
		return '\0';
	}

	private static char ToJongsung(char letterJongsung) {
		if (IsBetween (letterJongsung, LETTER_CONSONANT_FIRST, LETTER_CONSONANT_LAST)) {
			int index = (int)(letterJongsung - LETTER_CONSONANT_FIRST);

			return TABLE_LETTER_JONG [index];
		} else if (IsBetween (letterJongsung, JONGSUNG_FIRST, JONGSUNG_LAST)) {
			return letterJongsung;
		}
		return '\0';
	}

	private static char ToConsonant (char chojong) {
		int index;

		if (IsBetween (chojong, LETTER_CONSONANT_FIRST, LETTER_CONSONANT_LAST)) {
			return chojong;
		}
		else if (IsBetween (chojong, CHOSUNG_FIRST, CHOSUNG_LAST)) {
			index = (int)(chojong - CHOSUNG_FIRST);
			return TABLE_CHO_LETTER [index];
		}
		else if (IsBetween (chojong, JONGSUNG_FIRST, JONGSUNG_LAST)) {
			index = (int)(chojong - JONGSUNG_FIRST);
			return TABLE_JONG_LETTER [index];
		}
		else {
			return '\0';
		}	
	}

	private static  char StackJongsung (char a, char b) {
		if (b == '\0')
			return a;

		int index_a = (int)(a - JONGSUNG_FIRST);
		int index_b = (int)(b - JONGSUNG_FIRST);

		char[] table_a = TABLE_JONG_STACK [index_a];

		if (table_a == null)
			return '\0';

		return table_a [index_b];
	}

	private static bool UnstackJongsung (char jongsung, out char a, out char b) {
		int index = (int)(jongsung - JONGSUNG_FIRST);

		a = TABLE_JONG_UNSTACK [index, 0];
		b = TABLE_JONG_UNSTACK [index, 1];

		return (b != '\0');
	}




	private static char Compose (char chosung, char jungsung, char jongsung) {
		char njongsung = ToJongsung (jongsung);
		int ichosung;
		int ijungsung;
		int ijongsung;

		ichosung = (chosung == '\0') ? -1 : (int)(chosung - CHOSUNG_FIRST);
		ijungsung = (jungsung == '\0') ? -1 : (int)(jungsung - JUNGSUNG_FIRST);
		ijongsung = (jongsung == '\0') ? -1 : (int)(njongsung - JONGSUNG_FIRST);

		// Check cases that skips compositions.
		if (ichosung == -1) {
			if (ijungsung == -1) {
				if (ijongsung == -1) {
					return '\0';
				} else {
					return TABLE_JONG_LETTER [ijongsung]; // Letter version of jongsung.
				}
			} else {
				return (char) (LETTER_VOWEL_FIRST + ijungsung); // Letter version of jungsung.
			}
		} else {
			if (ijungsung == -1) {
				return TABLE_CHO_LETTER [ichosung]; // Letter version of chosung.
			}
		}

		// Compose letter.
		int index =
			(ichosung * NJUNGJONG) +
			(ijungsung * (NJONGSUNG + 1)) +
			(ijongsung) + 1;

		return (char) (LETTER_FIRST + index);
	}
}
