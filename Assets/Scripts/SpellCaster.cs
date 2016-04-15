using UnityEngine;
using System.Collections;
using NewtonVR;

public class SpellCaster : MonoBehaviour {
	public bool Drawing = false;
	public LayerMask CanvasLayer;
	public Vector2 CurrentUVs;
	public NVRHand LeftHand;
	public NVRHand RightHand;
	public CasterMode Mode = CasterMode.Dormant;
	public Transform CanvasTr;
	public Transform CameraRig;
	Vector3 canvasDirection;
	RaycastHit hitInfo;

	public void Update () {

		CanvasTr.position = CameraRig.position;

		switch (Mode) {
		case CasterMode.Dormant:
		default:
			Drawing = false;
			CanvasTr.gameObject.SetActive (false);
			if (LeftHand.HoldButtonDown) {
				CanvasTr.gameObject.SetActive (true);
				Mode = CasterMode.Drawing;
			}
			break;

		case CasterMode.Drawing:
			Drawing = false;
			canvasDirection = (LeftHand.transform.position - CameraRig.position).normalized;
			CanvasTr.forward = canvasDirection;
			if (Physics.Raycast (
				RightHand.transform.position,
				RightHand.transform.forward,
				out hitInfo,
				10f,
				CanvasLayer,
				QueryTriggerInteraction.Ignore)) {
				CurrentUVs = hitInfo.textureCoord;

				if (RightHand.HoldButtonPressed) {				
					Drawing = true;	
				}
			}
			if (LeftHand.HoldButtonUp) {
				Mode = CasterMode.Casting;
			}
			break;

		case CasterMode.Casting:
			Drawing = false;
			CanvasTr.gameObject.SetActive (false);
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
