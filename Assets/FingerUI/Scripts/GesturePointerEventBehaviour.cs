using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class GesturePointerEventBehaviour : PointerEventBehaviour {

	public enum State {
		CLEAN,
		GESTURE,
		PINCH
	}


	private float timer = 0;
	public float time_gap = 2.0f;
	public struct StrokeVisual {
		public Transform parent;
		public Stroke stroke;
		public LineRenderer renderer;
		public bool discardZ;
		public float minPointDistance;

		public StrokeVisual (Transform parent, LineRenderer template, bool discardZ, float minPointDistance) {
			this.parent = parent;
			this.discardZ = discardZ;
			this.minPointDistance = minPointDistance;

			renderer = Instantiate (template) as LineRenderer;
			stroke = new Stroke ();

			renderer.transform.SetParent (parent, false);
		}

		public void AddPoint (Vector3 point) {
			int count = stroke.Count;
			Vector3 npoint = point;

			if (discardZ)
				npoint.z = 0;

			if ((1 <= count) && (stroke [count - 1] == point))
				return;

			if (1 <= count) {
				if (Vector3.Distance (stroke [count - 1], npoint) < minPointDistance)
					return;
			}

			stroke.Add (point);

			renderer.SetVertexCount (count + 1);
			renderer.SetPosition (count, parent.TransformPoint (npoint));
		}

		public void Clean () {
			stroke.Clear ();
			renderer.SetVertexCount (0);
		}

		public void Destroy () {
			GameObject.Destroy (renderer.gameObject);
		}
	}

	// Outer States that can be adjustmented.

	public LineRenderer strokeRendererTemplate;
	public LineRenderer pinchRendererTemplate;

	public UnityEvent onGestureBegin;
	public UnityEvent onGestureEnd;
	public ListStrokeEvent onGesture;

	public UnityEvent onPinchBegin;
	public UnityEvent onPinchEnd;
	public StrokeEvent onPinch;

	public float minPointDistance = 4;


	// Inner states
	private int nFingers;
	private Dictionary<Pointer, StrokeVisual> fingerStrokes;
	private List<StrokeVisual> strokes;
	private State state = State.CLEAN;
	private bool gestureUpdated = false;


	private bool check;

	// Use this for initialization
	void Start () {
		fingerStrokes = new Dictionary<Pointer, StrokeVisual> ();
		strokes = new List <StrokeVisual> ();

		if (pinchRendererTemplate == null)
			pinchRendererTemplate = strokeRendererTemplate;
	}
	
	// Update is called once per frame
	void Update () {
		if (gestureUpdated) {
			List<Stroke> strokeList = new List<Stroke> ();

			foreach (StrokeVisual svisual in strokes) {
				strokeList.Add (svisual.stroke);
			}

			onGesture.Invoke (strokeList);

			gestureUpdated = false;
		}
		if (Check_Time () && check) {
			onGestureEnd.Invoke ();
			gestureUpdated = false;
			check = false;
		}
	}



	public override void OnPointerBegin (Pointer pointer) {
		bool isThumb = ! (pointer is PointerFinger);

		if (isThumb) {
			if (state == State.GESTURE) {
				onGestureEnd.Invoke ();
			}
			Clean ();
			state = State.PINCH;

			StrokeVisual svisual = new StrokeVisual (transform, pinchRendererTemplate, false, minPointDistance);
			fingerStrokes [pointer] = svisual;
			strokes.Add (svisual);

			onPinchBegin.Invoke ();
		}

		else {
			if (state == State.PINCH) {
				Clean ();
			}
			state = State.GESTURE;

			StrokeVisual svisual = new StrokeVisual (transform, strokeRendererTemplate, true, minPointDistance);
			fingerStrokes [pointer] = svisual;
			strokes.Add (svisual);

			onGestureBegin.Invoke ();
		}
	}

	public override void OnPointerIn (Pointer pointer, Vector3 tipLocalPosition) {
		check = false;

		bool isThumb = ! (pointer is PointerFinger);

		if (! fingerStrokes.ContainsKey (pointer))
			return;
			
		StrokeVisual stroke = fingerStrokes [pointer];

		stroke.AddPoint (tipLocalPosition);

		if (isThumb) {
			onPinch.Invoke (stroke.stroke);
		}
		else {
			gestureUpdated = true;
		}
	}

	public override void OnPointerEnd (Pointer pointer) {
		check = true;

		bool isThumb = ! (pointer is PointerFinger);

		if (isThumb) {
			onPinchEnd.Invoke ();
		}
		timer = Time.time;
		fingerStrokes.Remove (pointer);

	}



	public void Clean () {
		foreach (StrokeVisual stroke in strokes) {
			stroke.Destroy ();
		}

		strokes = new List<StrokeVisual> ();

		state = State.CLEAN;
	}

	public bool Check_Time(){
		float current_time = Time.time;
		if ((timer + time_gap) <= current_time)
			return true;
		else
			return false;
	}
}
