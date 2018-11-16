using UnityEngine;
using System.Collections;

class ColorUtility {

	public struct HSV {

		public float	h, s, v;
	}

	public static Color	gray(float gray)
	{
		return(new Color(gray, gray, gray, 1.0f));
	}

	public static Color	HSVToRGB(HSV hsv)
	{
		int			sel;
		float		fl;
		float		m, n;
		Color		color = Color.black;
		
		float	h = Mathf.Repeat(hsv.h, 360.0f);
		float	s = hsv.s;
		float	v = hsv.v;

		sel = Mathf.FloorToInt(h/60.0f);
		
		fl = (h/60.0f) - (float)sel;
		
		if(sel%2 == 0) {
			
			fl = 1.0f - fl;
		}
		
		m = v*(1.0f - s);
		n = v*(1.0f - s*fl);
		
		switch(sel) {
			
			default:
			case 0: color.r = v;	color.g = n;	color.b = m;	break;
			case 1: color.r = n;	color.g = v;	color.b = m;	break;
			case 2: color.r = m;	color.g = v;	color.b = n;	break;
			case 3: color.r = m;	color.g = n;	color.b = v;	break;
			case 4: color.r = n;	color.g = m;	color.b = v;	break;
			case 5: color.r = v;	color.g = m;	color.b = n;	break;
		}
		
		return(color);
	}
}
