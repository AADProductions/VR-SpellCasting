using UnityEngine;
using System.Collections;

public class GlyphAnalyzer : MonoBehaviour {

	public struct result {
		public float data;
	}

	public float Average;
	public bool Analyzing;
	public ComputeShader CompareShader;
	public ComputeShader ReduceShader;
	public RenderTexture CompareResult;
	public RenderTexture ReduceResultPass1;
	public RenderTexture ReduceResultPass2;
	public Texture2D GlyphTemplate;
	public Texture2D GlyphAttempt;

	public IEnumerator CompareGlyph () {

		Analyzing = true;

		CompareResult = new RenderTexture(GlyphAttempt.width, GlyphAttempt.height, 0, RenderTextureFormat.RFloat);
		CompareResult.enableRandomWrite = true;
		CompareResult.antiAliasing = 2;
		CompareResult.filterMode = FilterMode.Trilinear;
		CompareResult.Create();

		CompareShader.SetTexture (0, "Attempt", GlyphAttempt);
		CompareShader.SetTexture (0, "Template", GlyphTemplate);
		CompareShader.SetTexture (0, "Result", CompareResult);
		CompareShader.Dispatch (0, CompareResult.width / 8, CompareResult.width / 8, 1);

		yield return null;

		ReduceResultPass1 = new RenderTexture (16, 16, 0, RenderTextureFormat.RFloat);
		ReduceResultPass1.enableRandomWrite = true;
		ReduceResultPass1.antiAliasing = 1;
		ReduceResultPass1.filterMode = FilterMode.Point;
		ReduceResultPass1.Create();

		ReduceShader.SetTexture (0, "InputTexture", CompareResult);
		ReduceShader.SetTexture (0, "OutputTexture", ReduceResultPass1);
		ReduceShader.Dispatch (0, 32, 32, 1);

		yield return null;

		ReduceResultPass2 = new RenderTexture (1, 1, 0, RenderTextureFormat.RFloat);
		ReduceResultPass2.enableRandomWrite = true;
		ReduceResultPass2.antiAliasing = 1;
		ReduceResultPass2.filterMode = FilterMode.Point;
		ReduceResultPass2.Create();

		ReduceShader.SetTexture (0, "InputTexture", ReduceResultPass1);
		ReduceShader.SetTexture (0, "OutputTexture", ReduceResultPass2);
		ReduceShader.Dispatch (0, 32, 32, 1);

		yield return null;

		Analyzing = false;
	}

	void OnGUI()
	{
		int s = 256;
		if (ReduceResultPass1 != null) {
			GUI.DrawTexture (new Rect (0, 0, s, s), CompareResult);
			GUI.DrawTexture (new Rect (s, 0, s, s), ReduceResultPass1);
			GUI.DrawTexture (new Rect (s * 2, 0, s, s), ReduceResultPass2);
		}
	}

	void OnDestroy () {
		if (CompareResult != null) {
			CompareResult.Release ();
		}
		if (ReduceResultPass1 != null) {
			ReduceResultPass1.Release ();
		}
		if (ReduceResultPass2 != null) {
			ReduceResultPass2.Release ();
		}
	}
}
