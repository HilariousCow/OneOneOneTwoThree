Shader "OneOneOneTwoThree/Sky" {
	Properties {
		_MainTex ("Sprite Texture", 2D) = "white" {}
	}
	SubShader {
		Tags {"Queue" = "Geometry-2" "RenderType"="Opaque" }
		
		Pass{
			
			Cull Back
			
			ZWrite On
			ZTest LEqual
			
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			
		
			sampler2D _MainTex;
			
			struct appdata {
				float4 vertex	: POSITION;
				float4 color : COLOR;
				float3 normal : NORMAL;
				
				float2 texcoord: TEXCOORD0;
			};
 
			struct v2f {
				float4 pos		: SV_POSITION;
				float4 color : COLOR;
				float3 normal	: NORMAL;
				float2 uv		: TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = (mul((float3x3)UNITY_MATRIX_MVP, v.normal));
				o.color = v.color;
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o; 
			}

			float4 frag (v2f i) : COLOR
			{
				float4 outColor = tex2D(_MainTex, i.uv) * i.color;
				
				return outColor;
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
