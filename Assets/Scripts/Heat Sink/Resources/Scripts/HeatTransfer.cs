using UnityEngine;
using System.Collections;

public class HeatTransfer : MonoBehaviour {
	
	private Texture2D texture = null;
	private bool heating = false;
	
	public int radius = 15;					//circle size
	private float deltaX = 0.0f;
	private float deltaT = 0.1f;
		
	private float[] heatVals;
	private float[] heatPrecVals;	
			
	public float maxTempatature = 250.0f;	
	public float minTemparature = 0.0f;
	
	private float alpha = 0.8f;
		
	private ComputeShader cs_transfer;
	
	public GameObject flameParticle;
	
	private ColorMapping colorMapper;
	private Camera cam;
	
	// Use this for initialization
	void Start () {

		//add component's properties
		cs_transfer = Resources.Load("Scripts/CS_Transfer") as ComputeShader;
		if(flameParticle == null)
			flameParticle = Resources.Load("Prefabs/FlameParticles") as GameObject;
		
		texture = new Texture2D(256, 256, TextureFormat.ARGB32, false);
		GetComponent<Renderer>().material.mainTexture = texture;		
		

		colorMapper = GetComponent<ColorMapping>();
		
		Color32[] cols = texture.GetPixels32();
		for (int i = 0; i < cols.Length; ++i) {
			cols[i] = GetColor(10.0f);
		}
		texture.SetPixels32(cols);
		texture.Apply(false);
		
		heatVals = new float[cols.Length];
		heatPrecVals = new float[cols.Length];
		
		for (int i = 0; i < cols.Length; ++i) {
			if (i >= 0 && i < 256) {
				heatVals[i] = 250.0f;
				heatPrecVals[i] = 250.0f;
			} else {
				heatVals[i] = 10.0f;
				heatPrecVals[i] = 10.0f;
			}
		}

		cam = Camera.main;
		
		Vector3 screenPos = new Vector3(Screen.width * 0.5f , Screen.height * 0.5f, 40.0f);
		Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
		Vector3 worldOffsetPos = cam.ScreenToWorldPoint(screenPos + new Vector3(1.0f, 0.0f, 0.0f));
		deltaX = worldOffsetPos.x - worldPos.x;		
		deltaT = deltaX / 1.5f; 
		//deltaT = deltaX / (radius * 2); 
		deltaT *= 0.1f;

	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetMouseButtonDown(0)) {
			heating = true;
		}
		
		if (heating && Input.GetMouseButton(0)) {
			RaycastHit ray = new RaycastHit();
			
			if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out ray)) {
				return;
			}
			
			Renderer renderer = ray.collider.GetComponent<Renderer>();
			
			if (renderer == null || renderer.material == null || renderer.material.mainTexture == null) {
				return;
			}
				    
			Vector2 pixelUV = ray.textureCoord;
			pixelUV.x *= texture.width;
			pixelUV.y *= texture.height;
								
			for (int i = -radius; i <= radius; ++i) {
				for (int j = -radius; j <= radius; ++j) {
					int x = (int)pixelUV.x + j;
					int y = (int)pixelUV.y + i;
					if (i*i + j*j <= radius * radius) {						
						if( y * texture.width + x < heatVals.Length){
							heatVals[y * texture.width + x] = maxTempatature;
							heatPrecVals[y * texture.width + x] = maxTempatature	;
						}
					}
				}
			}
			
			Color32[] cols = texture.GetPixels32();
			for (int hi = 0; hi < heatVals.Length; ++hi) {
				cols[hi] = GetColor(heatVals[hi]);
			}
			texture.SetPixels32(cols);
						
			texture.Apply(false);
			
			Vector3 screenPos = Input.mousePosition + Vector3.forward * 10.0f;
			Vector3 targetPos = cam.ScreenToWorldPoint(screenPos);
			GameObject flame = (GameObject)Instantiate(flameParticle, targetPos, Quaternion.identity);
			flame.transform.parent = this.transform;
		}
		
		if (Input.GetMouseButtonUp(0)) {
			heating = false;
		}
		
		/*if (!heating)*/ {
			ComputeBuffer prevBuf = new ComputeBuffer(256 * 256, sizeof(float));
			ComputeBuffer curBuf = new ComputeBuffer(256 * 256, sizeof(float));
			
			prevBuf.SetData(heatPrecVals);
			curBuf.SetData(heatVals);
			
			HeatStep(prevBuf, curBuf, 0);
			HeatStep(curBuf, prevBuf, 0);
			
			prevBuf.GetData(heatPrecVals);
			curBuf.GetData(heatVals);
						
			prevBuf.Release();
			curBuf.Release();
			
			Color32[] cols = texture.GetPixels32();
			for (int hi = 0; hi < heatVals.Length; ++hi) {
				cols[hi] = GetColor(heatVals[hi]);
			}
			texture.SetPixels32(cols);
						
			texture.Apply(false);						
		}
	}
	
	Color32 GetColor(float t) {
		Color color = new Color(0,0,0);
		color = colorMapper.getColor(minTemparature, maxTempatature, t);

		byte R = (byte)(color.r * 255);
		byte G = (byte)(color.g * 255);
		byte B = (byte)(color.b * 255);
		
		return new Color32(R, G, B, 255);
	}
	
	Color32 GetColorRainbow(float t) {
		t = Mathf.Clamp(t, minTemparature, maxTempatature);
		
		float invMaxH = 1.0f / (maxTempatature - minTemparature);
		float zRel = (t - minTemparature) * invMaxH;
		
		float cR = 0, cG = 0, cB = 0;

		if (t == 200) 
		{
			cB = 1.0f;
		}
		
	    if (0 <= zRel && zRel < 0.2f)
	    {
	        cB = 1.0f;
	        cG = zRel * 5.0f;
	    }
	    else if (0.2f <= zRel && zRel < 0.4f)
	    {
	        cG = 1.0f;
	        cB = 1.0f - (zRel - 0.2f) * 5.0f;
	    }
	    else if (0.4f <= zRel && zRel < 0.6f)
	    {
	        cG = 1.0f;
	        cR = (zRel - 0.4f) * 5.0f;
	    }
	    else if (0.6f <= zRel && zRel < 0.8f)
	    {
	        cR = 1.0f;
	        cG = 1.0f - (zRel - 0.6f) * 5.0f;
	    }
	    else
	    {
	        cR = 1.0f;
	        cG = (zRel - 0.8f) * 5.0f;
	        cB = cG;
	    }
		
		byte R = (byte)(cR * 255);
		byte G = (byte)(cG * 255);
		byte B = (byte)(cB * 255);
		
		return new Color32(R, G, B, 255); 
	}
	
	void HeatStep(ComputeBuffer heatPrev, ComputeBuffer heatVals, int kernelNum) {
		
		if (!SystemInfo.supportsComputeShaders)
			return;
		
		cs_transfer.SetVector("g_params", new Vector4(deltaX, deltaT, alpha, 1.0f));		
		cs_transfer.SetBuffer(kernelNum, "Prev", heatPrev);
		cs_transfer.SetBuffer(kernelNum, "Result", heatVals);
		cs_transfer.Dispatch(kernelNum, 256, 256, 1);
		/*
		for (int y = 1; y < texture.height - 1; ++y) {
			for (int x = 1; x < texture.width - 1; ++x) {
				float origVal = heatPrev[y * texture.width + x];
				float vVal = -2.0f * origVal;
				
				float uxx = vVal + heatPrev[y * texture.width + x - 1] + heatPrev[y * texture.width + x + 1];
				float uyy = vVal + heatPrev[(y - 1) * texture.width + x] + heatPrev[(y + 1) * texture.width + x];
				
				float temp = 1.0f / deltaX;
				temp *= temp;
				
				float val = origVal + deltaT * alpha * (uxx + uyy) * temp;
				heatVals[y * texture.width + x] = val;
			}
		}
		*/
	}	
	
	/*
	void HeatStep(float[] heatPrev, float[] heatVals) {
		
		for (int y = 1; y < texture.height - 1; ++y) {
			for (int x = 1; x < texture.width - 1; ++x) {
				float origVal = heatPrev[y * texture.width + x];
				float vVal = -2.0f * origVal;
				
				float uxx = vVal + heatPrev[y * texture.width + x - 1] + heatPrev[y * texture.width + x + 1];
				float uyy = vVal + heatPrev[(y - 1) * texture.width + x] + heatPrev[(y + 1) * texture.width + x];
				
				float temp = 1.0f / deltaX;
				temp *= temp;
				
				float val = origVal + deltaT * alpha * (uxx + uyy) * temp;
				heatVals[y * texture.width + x] = val;
			}
		}
	}
	*/
}
