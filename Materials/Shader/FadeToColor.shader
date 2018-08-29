Shader "Unlit/FadeToColor"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FadingColor ("Fading Color", Color) = (0,0,0,1)
		_FadingPercent ("Fading Percent", Range(0,1)) = 0.5
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float4 _FadingColor;
			float _FadingPercent;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb = lerp(col.rgb, _FadingColor.rgb, 1.0f - _FadingPercent);
				return col;
			}
			ENDCG
		}
	}
}
