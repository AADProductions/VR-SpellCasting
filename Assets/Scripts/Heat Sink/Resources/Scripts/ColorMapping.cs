using UnityEngine;
using System.Collections;

public class ColorMapping : MonoBehaviour {
	
	public enum FunctionOption {
		Rainbow,
		HeatedBody, 
		BlueYellow, 
		Gray,
		GreenRed,
		isoGreenRed,
		Diverging
	}
	
	private delegate void FunctionDelegate (Texture2D tex);
	private static FunctionDelegate[] functionDelegates = {
		Rainbow,
		HeatedBody, 
		BlueYellow, 
		Gray,
		GreenRed,
		isoGreenRed,
		Diverging
	};

	public FunctionOption function;	
	private int prevFunction;
	public static float Lvalstat;
	public float Lval = 65f;
	public bool useDefinedPosition = true;
	public int positionLeft = 69;
	public int positionTop  = 9;
	public int dispMapWidth, dispMapHeight;
	public static int textureWidth  = 250;//250;//Hue range (red to blue only, entire range is 360)
	public static int textureHeight = 20;
	public bool flip = false;
	//public bool verticalBar = false;
  
   // the solid texture which everything is compared against
	public Texture2D colorPicker, colorPickerFlip;
	
	// the picker being displayed
	private Texture2D displayPicker;
	
	// the color that has been chosen
	//public Color setColor;
	//private Color lastSetColor;
	
	//public int textureStepsize = 4;
	
	//private float saturationSlider = 0.0F;
	private Texture2D saturationTexture;
	
	//private Texture2D styleTexture;
	
	private bool showPicker = true;	
	
	
	void Awake() {
		
		Lvalstat = Lval;
		
	  if (!useDefinedPosition) {
	    positionLeft = (Screen.width / 2) - (textureWidth / 2);
	    positionTop  = (Screen.height / 2) - (textureHeight / 2);
	  }
	  		
		dispMapWidth=textureWidth;
		dispMapHeight=textureHeight;
		
	  // if a default color picker texture hasn't been assigned, make one dynamically
	  if (!colorPicker) {
			colorPicker = new Texture2D(dispMapWidth, dispMapHeight, TextureFormat.ARGB32, false);
			colorPickerFlip = new Texture2D(dispMapWidth, dispMapHeight, TextureFormat.ARGB32, false);
		}
		
		FunctionDelegate f = functionDelegates[(int)function];
		prevFunction = (int)function;
		
	  f (colorPicker);//HeatedBody BlueYellow Gray
		  flipColorMap(colorPickerFlip);
		
		displayPicker = colorPicker;
		
	  // small color picker box texture
	  //styleTexture = new Texture2D(1, 1);
	  //styleTexture.SetPixel(0, 0, setColor);
	}
 
	void Update () {
		if((int)function != prevFunction) {
			
			FunctionDelegate f = functionDelegates[(int)function];
			f (colorPicker);//HeatedBody BlueYellow Gray
			flipColorMap(colorPickerFlip);
			displayPicker = colorPicker;
			
			prevFunction = (int)function;
		}
		
		//colorPicker.
		if(dispMapWidth!=textureWidth || dispMapHeight!=textureHeight) {
			colorPicker.Resize(dispMapWidth, dispMapHeight);
			
			textureWidth = dispMapWidth;
	    	textureHeight = dispMapHeight;
			FunctionDelegate f = functionDelegates[(int)function];
			f (colorPicker);
			
			displayPicker = colorPicker;
		}
		
		if(flip) {			
			displayPicker = colorPickerFlip;
		}
		else {
			displayPicker = colorPicker;
		}
	}
	
	public void flipColorMap (Texture2D tex) {
		for (int i = 0; i < textureWidth; i++) {
			Color col = colorPicker.GetPixel(textureWidth-1-i, 0);
			
			for (int j = 0; j < textureHeight; j++) {			
				tex.SetPixel(i, j, new Color(col.r, col.g, col.b));
			}
		}
		tex.Apply();
	}
	
	public Color getColor (float min, float max, float val) {
		float newVal;

		newVal = Mathf.InverseLerp(min, max, val);//0~1

		Color col = displayPicker.GetPixel(Mathf.Clamp(Mathf.RoundToInt(newVal*(float)textureWidth), 0, textureWidth-1), 0);
		//Debug.Log(col);
		return col;
	}
	
	private static void Diverging (Texture2D tex) {
		Vector3 green = new Vector3(59f/255f, 76f/255f, 192f/255f);
		Vector3 red = new Vector3(180f/255f, 4f/255f, 38f/255f);

		// Set tex
		float C = 0F;
		float diff = 1f / textureWidth;
		
		for (int i = 0; i < textureWidth; i++) {
			for (int j = 0; j < textureHeight; j++) {
				
				Vector3 tmp = interpolateColor(green, red, C);
			
	        	tex.SetPixel(i, j, new Color(tmp.x, tmp.y, tmp.z));
			}
	  	  	C += diff;
		}
		tex.Apply();	
		
	}
	
	private static void isoGreenRed (Texture2D tex) {
		
		Vector3 green = new Vector3(0f, 1f, 0f);//(0f, 0.7f, 1f);blue
		Vector3 red = new Vector3(1f, 0f, 0f);//(1f, 0.3f, 0f);red
		
		// convert to XYZ and to Lab
		Vector3 greenxyz = rgb2XYZ(green);
		Vector3 greenlab = xyz2LAB(greenxyz);
		
		Vector3 redxyz = rgb2XYZ(red);
		Vector3 redlab = xyz2LAB(redxyz);
		
//		Debug.Log(greenxyz + " " + greenlab);
//		Debug.Log(redxyz + " " + redlab);
		
		// Lerp
		Vector3 lerplab = new Vector3(Lvalstat, 0f, 0f);
		Vector3 lerpxyz, lerprgb;
		
		// Set tex
		float C = 0F;
		float diff = 1f / textureWidth;
		
		for (int i = 0; i < textureWidth; i++) {
			for (int j = 0; j < textureHeight; j++) {
				
				Vector2 tmp = Vector2.Lerp(new Vector2(greenlab.y, greenlab.z), new Vector2(redlab.y, redlab.z), C);
				lerplab.y = tmp.x;
				lerplab.z = tmp.y;
				
				lerpxyz = lab2XYZ(lerplab);
				lerprgb = xyz2RGB(lerpxyz);
				
	        	tex.SetPixel(i, j, new Color(lerprgb.x, lerprgb.y, lerprgb.z));
			}
	  	  	C += diff;
		}
		tex.Apply();
	}
	
	private static void GreenRed (Texture2D tex) {
		// CMYK Green(1,0,1,0), Red(0,1,1,0)
		
		float C = 0F;
		float diff = 1f / textureWidth;
		
		for (int i = 0; i < textureWidth; i++) {
			for (int j = 0; j < textureHeight; j++) {
	        	tex.SetPixel(i, j, new Color(C, 1-C, 0));
			}
	  	  	C += diff;
		}
		tex.Apply();
	}
	
	private static void Rainbow (Texture2D tex) {
		ColorHSV hsvColor;
		for (int i = 0; i < textureWidth; i++) {
			for (int j = 0; j < textureHeight; j++) {
				hsvColor = new ColorHSV((float)i, 1.0f, 1.0f);
				tex.SetPixel(i, j, hsvColor.ToColor());
			}
		}
		tex.Apply();
	}

	//Black-red to Yellow
	private static void HeatedBody (Texture2D tex) {
		// CMYK Black-red(0,1,1,1), Yellow(0,0,1,0)
		
		float K = 1.0F;
		int halfWidth = Mathf.FloorToInt(textureWidth/2f);
		float diff = -1f / halfWidth;
		
		for (int i = 0; i < halfWidth; i++) {
			for (int j = 0; j < textureHeight; j++) {
	        	tex.SetPixel(i, j, new Color(1-K, 0f, 0f));
				tex.SetPixel(i+halfWidth, j, new Color(1, 1-K, 0f));
			}
	  	  	K += diff;
		}
		tex.Apply();
	}
	
	private static void BlueYellow (Texture2D tex) {
		// CMYK Blue(1,1,0,0), Yellow(0,0,1,0)
		
		float Y = 1.0F;
		float diff = -1f / textureWidth;
		
		for (int i = 0; i < textureWidth; i++) {
			for (int j = 0; j < textureHeight; j++) {
	        	tex.SetPixel(i, j, new Color(1-Y, 1-Y, Y));
			}
	  	  	Y += diff;
		}
		tex.Apply();		
	}
	
	private static void Gray (Texture2D tex) {
		float v = 0.0F;
		float diff = 1f / textureWidth;
		
		for (int i = 0; i < textureWidth; i++) {
			for (int j = 0; j < textureHeight; j++) {
	        	tex.SetPixel(i, j, new Color(v, v, v));	        	
			}
	  	  	v += diff;
		}
		tex.Apply();
	}
	
	static Vector3 interpolateColor (Vector3 rgb1, Vector3 rgb2, float interp) {
		Vector3 xyz1 = rgb2XYZ(rgb1);
		Vector3 lab1 = xyz2LAB(xyz1);
		
		Vector3 xyz2 = rgb2XYZ(rgb2);
		Vector3 lab2 = xyz2LAB(xyz2);
		
		Vector3 msh1 = lab2Msh(lab1);
		Vector3 msh2 = lab2Msh(lab2);
		
		Vector3 mshid = new Vector3();
		
		// If points saturated and distinct, place white in middle
		if(msh1.y > 0.05f && msh2.y > 0.05f && Mathf.DeltaAngle(msh1.z, msh2.z)>(Mathf.PI/3f)) {
			mshid.x = Mathf.Max(msh1.x, msh2.x, 88f);
			
			if (interp < 0.5f) {
				msh2.x = mshid.x; msh2.y = 0; msh2.z = 0;
				interp *= 2f;
			}
			else {
				msh1.x = mshid.x; msh1.y = 0; msh1.z = 0;
				interp = 2f * interp - 1f;
			}
		}
		
		// Adjust Hue of unsaturated colors
		if(msh1.y < 0.05f && msh2.y > 0.05f)
			msh1.z = adjustHue (msh2, msh1.x);
		else if (msh2.y < 0.05f && msh1.y > 0.05f)
			msh2.z = adjustHue (msh1, msh2.x);
		
		//Linear interpolation on adjusted control points
		mshid = (1f-interp)*msh1 + interp*msh2;
		
		Vector3 labid = msh2Lab(mshid);
		Vector3 xyzid = lab2XYZ(labid);
		Vector3 rgbid = xyz2RGB(xyzid);
		
		return rgbid;
	}
	
	
	static float adjustHue (Vector3 msh, float mu) {
		if (msh.x >= mu)
			return msh.z;
		else {
			float hSpin = msh.y * Mathf.Sqrt(mu*mu - msh.x*msh.x) / (msh.x * Mathf.Sin(msh.y));
			
			if (msh.z > -Mathf.PI/3f)
				return (msh.z + hSpin);
			else
				return (msh.z - hSpin);
		}
	}
	
	static Vector3 lab2Msh (Vector3 lab) {
		Vector3 msh = new Vector3();
		
		msh.x = Mathf.Sqrt(lab.x * lab.x + lab.y * lab.y + lab.z * lab.z);
		msh.y = Mathf.Acos(lab.x/msh.x);
		msh.z = Mathf.Atan(lab.z/lab.y);
		
		return msh;
	}
	
	static Vector3 msh2Lab (Vector3 msh) {
		Vector3 lab = new Vector3();
		
		lab.x = msh.x * Mathf.Cos(msh.y);
		lab.y = msh.x * Mathf.Sin(msh.y) * Mathf.Cos(msh.z);
		lab.z = msh.x * Mathf.Sin(msh.y) * Mathf.Sin(msh.z);
		
		return lab;
	}
	
	static Vector3 rgb2XYZ (Vector3 rgb) {
		
		Vector3 xyz = new Vector3();
		
		if(rgb.x > 0.04045f)
			xyz.x = Mathf.Pow((rgb.x + 0.055f)/1.055f, 2.4f);
		else xyz.x /= 12.92f;
		
		if(rgb.y > 0.04045f)
			xyz.y = Mathf.Pow((rgb.y + 0.055f)/1.055f, 2.4f);
		else xyz.y /= 12.92f;
		
		if(rgb.z > 0.04045f)
			xyz.z = Mathf.Pow((rgb.z + 0.055f)/1.055f, 2.4f);
		else xyz.z /= 12.92f;
		
		xyz *= 100f;
		
		Vector3 tmp = new Vector3();
		tmp.x = Vector3.Dot(xyz, new Vector3(0.4124f, 0.3576f, 0.1805f));
		
		tmp.y = Vector3.Dot(xyz, new Vector3(0.2126f, 0.7152f, 0.0722f));
		
		tmp.z = Vector3.Dot(xyz, new Vector3(0.0193f, 0.1192f, 0.9505f));
		xyz = tmp;
			
		return xyz;
	}
	
	static Vector3 xyz2LAB (Vector3 xyz) {
		Vector3 lab = new Vector3();
		
		lab.x = xyz.x / 95.047f; //reference white value
		lab.y = xyz.y / 100f;
		lab.z = xyz.z / 108.883f;
		
		if(lab.x > 0.008856f)
			lab.x = Mathf.Pow(lab.x, 1f/3f);
		else lab.x = (7.787f * lab.x)+(16f/116f);
		
		if(lab.y > 0.008856f)
			lab.y = Mathf.Pow(lab.y, 1f/3f);
		else lab.y = (7.787f * lab.y)+(16f/116f);
		
		if(lab.z > 0.008856f)
			lab.z = Mathf.Pow(lab.z, 1f/3f);
		else lab.z = (7.787f * lab.z)+(16f/116f);
		
		Vector3 tmp = new Vector3();
		tmp.x = (116f * lab.y) - 16f;
		tmp.y = 500f * (lab.x - lab.y);
		tmp.z = 200f * (lab.y - lab.z);
		
		lab = tmp;
		
		return lab;
	}
	
	static Vector3 lab2XYZ (Vector3 lab) {
		Vector3 xyz = new Vector3();
		
		xyz.y = (lab.x + 16f)/116f;
		xyz.x = lab.y/500f + xyz.y;
		xyz.z = xyz.y - lab.z/200f;
		
		if(xyz.x > 0.008856f)
			xyz.x = Mathf.Pow(xyz.x, 3f);
		else xyz.x = (xyz.x - 16f/116f)/7.787f;
		
		if(xyz.y > 0.008856f)
			xyz.y = Mathf.Pow(xyz.y, 3f);
		else xyz.y = (xyz.y - 16f/116f)/7.787f;
		
		if(xyz.z > 0.008856f)
			xyz.z = Mathf.Pow(xyz.z, 3f);
		else xyz.z = (xyz.z - 16f/116f)/7.787f;
		
		xyz.x = xyz.x * 95.047f; //reference white value
		xyz.y = xyz.y * 100f;
		xyz.z = xyz.z * 108.883f;
		
		return xyz;
	}
	
	static Vector3 xyz2RGB (Vector3 xyz) {
		Vector3 rgb = new Vector3();
		
		Vector3 tmp = new Vector3(xyz.x / 100f, xyz.y / 100f, xyz.z / 100f);
		
		rgb.x = Vector3.Dot(tmp, new Vector3(3.2406f, -1.5372f, -0.4986f));
		rgb.y = Vector3.Dot(tmp, new Vector3(-0.9689f, 1.8758f, 0.0415f));
		rgb.z = Vector3.Dot(tmp, new Vector3(0.0557f, -0.2040f, 1.0570f));
		
		if(rgb.x > 0.0031308f)
			rgb.x = 1.055f * Mathf.Pow(rgb.x, 1f/2.4f) - 0.055f;
		else rgb.x *= 12.92f;
		
		if(rgb.y > 0.0031308f)
			rgb.y = 1.055f * Mathf.Pow(rgb.y, 1f/2.4f) - 0.055f;
		else rgb.y *= 12.92f;
		
		if(rgb.z > 0.0031308f)
			rgb.z = 1.055f * Mathf.Pow(rgb.z, 1f/2.4f) - 0.055f;
		else rgb.z *= 12.92f;
		
		return rgb;
	}
	
	void OnGUI(){
	  	if (!showPicker) return;
 
	  	GUI.Box(new Rect(positionLeft - 3, positionTop - 3, textureWidth + 3, textureHeight + 3), "");
		GUI.Box (new Rect(positionLeft, positionTop, textureWidth, textureHeight), displayPicker);
	  
		//color palette
/*		if (GUI.RepeatButton(new Rect(positionLeft, positionTop, textureWidth, textureHeight), displayPicker)) {
	  		int a = (int)Input.mousePosition.x;
	  		int b = Screen.height - (int)Input.mousePosition.y;
			
	  		setColor = displayPicker.GetPixel(a - positionLeft, -(b - positionTop));
	  		lastSetColor = setColor;
		}
		
		saturationSlider = GUI.HorizontalSlider(new Rect(positionLeft, positionTop + textureHeight + 23, textureWidth, 10), saturationSlider, 1, -1);
  		setColor = lastSetColor + new Color(saturationSlider, saturationSlider, saturationSlider);
		GUI.Box(new Rect(positionLeft, positionTop + textureHeight, textureWidth, 20), saturationTexture);
		
		if (GUI.Button(new Rect(positionLeft + textureWidth - 60, positionTop + textureHeight + 26, 60, 25), "Apply")) {
			setColor = styleTexture.GetPixel(0, 0);
 
	  		// hide picker
	  		showPicker = false;
		}
		
		// Selected color 
		GUIStyle style = new GUIStyle();
		styleTexture.SetPixel(0, 0, setColor);
		styleTexture.Apply();
		
		style.normal.background = styleTexture;
		GUI.Box(new Rect(positionLeft + textureWidth + 10, positionTop + textureHeight + 10, 30, 30), new GUIContent(""), style);
*/			
	}
}
