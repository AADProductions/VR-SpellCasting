using UnityEngine;
using System.Collections;

public class GlyphAnalyzer : MonoBehaviour {

	public Texture2D GlyphTemplate;
	public Texture2D GlyphAttempt;
	public Color result;
	public float combinedResult;
	Color32 [] templateColors;
	Color32 [] attemptColors;
	Color32 [] blendedResult;

	public void Start () {
		StartCoroutine (CheckResult ());

		templateColors = GlyphTemplate.GetPixels32 ();
		attemptColors = GlyphAttempt.GetPixels32 ();
		blendedResult = new Color32 [templateColors.Length];
	}

	public IEnumerator CheckResult ( ) {

		yield return new WaitForSeconds (3f);

		Color32 template = Color.black;
		Color32 attempt = Color.black;
		Color32 difference = Color.black;

		for (int i = 0; i < templateColors.Length; i++) {
			template = templateColors [i];
			attempt = attemptColors [i];
			difference.r = (byte)Mathf.Abs ((int)template.r - (int)attempt.r);
			difference.g = (byte)Mathf.Abs ((int)template.g - (int)attempt.g);
			difference.b = (byte)Mathf.Abs ((int)template.b - (int)attempt.b);
			blendedResult [i] = difference;
		}

		int r = 0;
		int g = 0;
		int b = 0;
		for (int i = 0; i < blendedResult.Length; i++) {
			r += blendedResult [i].r;
			g += blendedResult [i].g;
			b += blendedResult [i].b;
		}
		r /= blendedResult.Length;
		g /= blendedResult.Length;
		b /= blendedResult.Length;
		result.r = (float)r / 255;
		result.g = (float)g / 255;
		result.b = (float)b / 255;
		combinedResult = ((float)result.r / 255 + (float)result.g / 255 + (float)result.b / 255) / 3;

		yield break;
	}
}
