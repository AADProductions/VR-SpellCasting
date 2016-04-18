using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SpellCanvas : MonoBehaviour {

	public SpellCaster Caster;
	public GameObject BrushStrokePrefab;
	public Transform BrushParent;
	public Camera DrawingCamera;
	public Transform Brush;
	public Transform CanvasTr;
	public float CanvasScale = 1024f;
	public float CanvasRadius = 1f;
	public float MovementThreshold = 0.1f;
	public float BrushFadeSpeed = 10f;
	float lastTimeBrushPlaced;
	float minTimeBetweenStrokes = 0.025f;
	Vector3 brushStrokePosition;
	Vector3 positionLastFrame;
	float alphaTarget;
	SpriteRenderer brushSprite;
	Color spriteColor;

	void Start () {
		brushSprite = Brush.GetComponent <SpriteRenderer> ();
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
			brushStrokePosition.z = -50f;
			brushStrokePosition.x = Caster.CurrentUVs.x * CanvasScale;
			brushStrokePosition.y = Caster.CurrentUVs.y * CanvasScale;
			Brush.localPosition = brushStrokePosition;
		}

		float distanceFromCenter = Vector3.Distance (CanvasTr.position, Brush.position);
		float movement = Vector3.Distance (brushStrokePosition, positionLastFrame);
		if (movement >= MovementThreshold && distanceFromCenter < CanvasRadius) {
			alphaTarget = 1f;
		} else {
			alphaTarget = 0f;
		}
		spriteColor.a = Mathf.Lerp (spriteColor.a, alphaTarget, Time.deltaTime * BrushFadeSpeed);
		brushSprite.color = spriteColor;

		positionLastFrame = brushStrokePosition;
	}
}
