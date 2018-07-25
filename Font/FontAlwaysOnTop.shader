Shader "Custom/FontAlwaysOnTop"
{
	Properties{
		_MainTex("Font Texture", 2D) = "white" {}
		_TextColor("Text Color", Color) = (1,1,1,1)
	}

	SubShader{
		Lighting Off
		cull off
		Zwrite off
		ZTest Always
		Fog{ Mode Off }
		Tags{ "Queue" = "Transparent" }
		Pass{
			Blend SrcAlpha OneMinusSrcAlpha
			Color[_TextColor]
			SetTexture[_MainTex]{
				combine primary, texture * primary
			}
		}
	}
}