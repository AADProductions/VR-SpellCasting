using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpellCanvas : MonoBehaviour {

	public SpellCaster Caster;
	public GameObject BrushStrokePrefab;
	public Transform BrushParent;
	public Camera DrawingCamera;
	public Transform Brush;
	public Transform CanvasTr;
	public float CanvasScale = 1024f;
	public float CanvasRadius = 1f;
	public float StrokeRadius = 0.05f;
	public float CanvasFadePoint = 0.25f;
	public float MovementThreshold = 0.1f;
	public float BrushFadeSpeed = 10f;
	public float MaxPointDistance = 0.002f;
	public Camera TubeRendererCamera;
	public Material PaintMaterial;
	float lastTimeBrushPlaced;
	float minTimeBetweenStrokes = 0.025f;
	Vector3 brushStrokePosition;
	Vector3 positionLastFrame;
	SpriteRenderer brushSprite;
	float strokeOpacityTarget;
	float strokeOpacity;
	float lastStrokeOpacity;
	TubeRenderer currentMark;
	Vector3 lastBrushStrokePos;
	List <Vector3> currentPositions = new List<Vector3> ();
	List <float> currentOpacities = new List<float> ();

	void Start () {
		brushSprite = Brush.GetComponent <SpriteRenderer> ();
		strokeOpacity = 0f;
	}

	void Update () {

		if (Caster.Mode == CasterMode.Cooldown) {
			DrawingCamera.clearFlags = CameraClearFlags.SolidColor;
			DrawingCamera.backgroundColor = Color.clear;
			return;
		} else {
			DrawingCamera.clearFlags = CameraClearFlags.Nothing;
		}

		if (DrawingCamera.targetTexture == null) {
			DrawingCamera.targetTexture = Resources.Load ("SpellCanvasRT") as RenderTexture;
		}

		if (Caster.Drawing) {
			/*float movement = Vector3.Distance (brushStrokePosition, positionLastFrame);
			if (movement >= MovementThreshold) {
				strokeOpacityTarget = 1f;
			} else {
				strokeOpacityTarget = 0f;
			}
			strokeOpacity = Mathf.Lerp (strokeOpacity, strokeOpacityTarget, Time.deltaTime * BrushFadeSpeed);*/
			strokeOpacity = 1f;

			float distanceFromCenter = Vector3.Distance (CanvasTr.position, BrushParent.TransformPoint (brushStrokePosition));
			if (distanceFromCenter > CanvasRadius) {
				float fadeAmount = 1f - Mathf.Clamp (distanceFromCenter - CanvasRadius, 0, CanvasFadePoint) / CanvasFadePoint;
				strokeOpacity *= fadeAmount;
			}

			brushStrokePosition.z = -50f;
			brushStrokePosition.x = Caster.CurrentUVs.x * CanvasScale;
			brushStrokePosition.y = Caster.CurrentUVs.y * CanvasScale;
			Brush.localPosition = brushStrokePosition;

			if (currentMark == null) {
				currentMark = new GameObject ("BrushStroke").AddComponent <TubeRenderer> ();
				currentMark.gameObject.layer = gameObject.layer;
				currentMark.transform.parent = BrushParent;
				currentMark.MainCamera = TubeRendererCamera;
				currentMark.material = new Material (PaintMaterial);
				currentMark.material.color = Color.white;
				currentPositions.Clear ();
			}

			Debug.Log (strokeOpacity);

			if (currentPositions.Count == 0 || Vector3.Distance (lastBrushStrokePos, brushStrokePosition) > MaxPointDistance) {
				lastBrushStrokePos = brushStrokePosition;
				lastStrokeOpacity = strokeOpacity;
				currentOpacities.Add (lastStrokeOpacity);
				currentPositions.Add (lastBrushStrokePos);
				if (currentPositions.Count > 1) {
					currentMark.vertices = new TubeRenderer.TubeVertex [currentPositions.Count];
					for (int i = 0; i < currentPositions.Count; i++) {
						currentMark.vertices [i] = new TubeRenderer.TubeVertex (
							BrushParent.TransformPoint (currentPositions [i]),
							StrokeRadius,
							Color.Lerp (Color.black, Color.white, currentOpacities [i]));
					}
				}
			}
		} else {
			currentMark = null;
		}

		positionLastFrame = brushStrokePosition;
	}
}
