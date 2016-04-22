using UnityEngine;
using System.Collections;
using NewtonVR;

public class SpellCaster : MonoBehaviour {
	public bool Drawing = false;
	public float CanvasSpawnDistance = 0.5f;
	public LayerMask CanvasLayer;
	public Vector2 CurrentUVs;
	public NVRHand LeftHand;
	public NVRHand RightHand;
	public CasterMode Mode = CasterMode.Dormant;
	public Transform CanvasTr;
	public Transform CameraRig;
	public Transform FingerTipPosTr;
	public AnimationCurve ScaleUpCurve;
	public AnimationCurve ScaleDownCurve;
	RaycastHit hitInfo;
	float growStartTime;
	float shrinkStartTime;
	Collider fingerCollider;

	void OnTriggerEnter (Collider other) {
		Debug.Log ("Entered trigger: " + other.name);
		if (other.CompareTag ("Fingertip")) {
			fingerCollider = other;
			Drawing = true;
		}
	}

	void OnTriggerExit (Collider other) {
		Debug.Log ("Exited trigger: " + other.name);
		if (other.CompareTag ("Fingertip")) {
			fingerCollider = null;
			Drawing = false;
		}
	}

	public void Update () {

		switch (Mode) {
		case CasterMode.Dormant:
		default:
			Drawing = false;
			if (Time.time < shrinkStartTime + 3f) {
				CanvasTr.localScale = Vector3.one * ScaleDownCurve.Evaluate (Time.time - shrinkStartTime);
			} else {
				CanvasTr.gameObject.SetActive (false);
			}
			if (LeftHand.HoldButtonDown) {
				CanvasTr.gameObject.SetActive (true);
				CanvasTr.localScale = Vector3.one * 0.0001f;
				CanvasTr.position = CameraRig.position + CameraRig.forward * CanvasSpawnDistance;
				CanvasTr.forward = CameraRig.forward;
				growStartTime = Time.time;
				Mode = CasterMode.Drawing;
			}
			break;

		case CasterMode.Drawing:
			CanvasTr.localScale = Vector3.one * ScaleUpCurve.Evaluate (Time.time - growStartTime);
			if (Drawing) {
				FingerTipPosTr.position = fingerCollider.transform.position;
				CurrentUVs.x = FingerTipPosTr.localPosition.x;
				CurrentUVs.y = FingerTipPosTr.localPosition.y;
			}
			/*if (Physics.Raycast (
				RightHand.transform.position,
				RightHand.transform.forward,
				out hitInfo,
				10f,
				CanvasLayer,
				QueryTriggerInteraction.Ignore)) {
				CurrentUVs = hitInfo.textureCoord;
			}*/
			if (LeftHand.HoldButtonUp) {
				shrinkStartTime = Time.time;
				Mode = CasterMode.Casting;
			}
			break;

		case CasterMode.Casting:
			Drawing = false;
			//CanvasTr.gameObject.SetActive (false);
			Mode = CasterMode.Cooldown;
			break;

		case CasterMode.Cooldown:
			Drawing = false;
			Mode = CasterMode.Dormant;
			break;
		}
	}
}

public enum CasterMode {
	Dormant,//nothing going on
	Drawing,//left hand is tracking, right hand drawing
	Casting,//spell is judged / effects applied
	Cooldown//we can't start casting for [x] seconds
}
