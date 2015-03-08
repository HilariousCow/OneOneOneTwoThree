Shader "OneOneOneTwoThree/PreviewTokens" {
	Properties {
		
	}
	SubShader {
		Tags {"Queue" = "Geometry+1" "RenderType"="Opaque" }
		
		//depth mask first
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
				float4 outColor = i.color;
				outColor.a = 0;
				return outColor;
			}
			
			ENDCG
		}
		Pass{
			
			Cull Front
			
			ZWrite On
			ZTest LEqual
			//Fog Disable
			Blend OneMinusDstColor OneMinusSrcColor 
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			
		
			
			
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
				o.pos.xyz = o.pos.xyz + o.normal * 0.1f;
				o.color = v.color;
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o; 
			}

			float4 frag (v2f i) : COLOR
			{
				float4 outColor  = float4(1,1,1,1);
				
				return outColor;
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
