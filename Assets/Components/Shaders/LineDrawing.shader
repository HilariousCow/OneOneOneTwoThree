Shader "LineDrawer/LineBasic" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_LineWidth("LineWidth", Range (0,2)) = 1
    
	}
	SubShader {
		Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
		
		Pass{
			
			Cull Back
			
			ZWrite Off
			ZTest LEqual
			Offset -10,-10
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			#include "LineDrawing.cginc"
			
			
			float4 _Color;
			float _LineWidth;
			
			
			struct appdata {
				float4 vertex	: POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float2 texcoord: TEXCOORD0;
			};
 
			struct v2f {
				float4 pos		: SV_POSITION;
				float3 normal	: NORMAL;
				float2 uv		: TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				
				//o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				v.tangent.w *= _LineWidth;
				o.pos = RibbonPosMitre(v.vertex, v.normal, v.tangent);
				
				//since i know that this is on a sphere, the normal is just the normalized position.
				//could optimize above
				
				
				//o.normal = normalize(mul((float3x3)_Object2World, v.vertex));
				float3 localNormal = cross((v.normal + v.tangent.xyz), v.normal);
				localNormal = cross(localNormal,  v.normal);
				o.normal = (mul((float3x3)UNITY_MATRIX_MVP, localNormal));
				//o.pos.xyz += o.normal*_LineWidth;
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o; 
			}

			float4 frag (v2f i) : COLOR
			{
				float4 outColor = _Color;
				outColor.a *= 1.0f-abs((i.uv.x - 0.5f)*2);
				return outColor;
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
