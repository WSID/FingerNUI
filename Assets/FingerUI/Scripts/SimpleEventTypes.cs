using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;


[Serializable]
public class HandModelEvent: UnityEvent<HandModel> {}

/// <summary>
/// A Simple Character Event
/// </summary>
[Serializable]
public class CharEvent: UnityEvent<char> {}

[Serializable]
public class StringEvent: UnityEvent<string> {}

[Serializable]
public class StrokeEvent: UnityEvent<Stroke> {}

[Serializable]
public class ListStrokeEvent: UnityEvent<List<Stroke>> {}