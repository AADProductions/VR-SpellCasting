using UnityEngine;
using System.Collections;

public class SpellCanvas : MonoBehaviour {

	public SpellCaster Caster;
	public GameObject BrushStrokePrefab;
	public Transform BrushParent;
	public Transform BrushPos;
	public float CanvasScale = 1024f;
	float lastTimeBrushPlaced;
	float minTimeBetweenStrokes = 0.025f;
	Vector3 brushStrokePosition;

	void Update () {

		if (Caster.Mode == CasterMode.Drawing) {
			BrushPos.position = Caster.CurrentUVs * CanvasScale;
		}

		if (Caster.Drawing) {
			if (Time.time > lastTimeBrushPlaced + minTimeBetweenStrokes) {
				lastTimeBrushPlaced = Time.time;
				GameObject newBrushStroke = GameObject.Instantiate (BrushStrokePrefab) as GameObject;
				newBrushStroke.transform.parent = BrushParent;
				brushStrokePosition.z = -50f;
				brushStrokePosition.x = Caster.CurrentUVs.x * CanvasScale;
				brushStrokePosition.y = Caster.CurrentUVs.y * CanvasScale;
				newBrushStroke.transform.localPosition = brushStrokePosition;
			}
		}
	}
}
