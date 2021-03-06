﻿Shader "OneOneOneTwoThree/FontShader" {
	Properties {
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_MainTexBias ("Mip Bias",  Range (-1,1)) = 0
	}
	SubShader {
		Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
		
		Pass{
			
			Cull Back
			
			ZWrite Off
			ZTest On
			
			//Blend OneMinusDstColor OneMinusSrcColor //negative color
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			
		
			sampler2D _MainTex;
			float4 _BGColor;
			float4 _OppositeOfBGColor;
			
			struct appdata {
				float4 vertex	: POSITION;
				float4 color : COLOR;
				//float3 normal : NORMAL;
				
				float2 texcoord: TEXCOORD0;
			};
 
			struct v2f {
				float4 pos		: SV_POSITION;
				float4 color : COLOR;
				//float3 normal	: NORMAL;
				float2 uv		: TEXCOORD0;
			};

			float _MainTexBias;
			v2f vert (appdata v)
			{
				v2f o;
				
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			//	o.normal = (mul((float3x3)UNITY_MATRIX_MVP, v.normal));
				o.color = v.color;// * _Color;
				o.color.rgb = _OppositeOfBGColor.rgb;
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o; 
			}

			float4 frag (v2f i) : COLOR
			{
				float4 biasCoord = half4(i.uv.x,i.uv.y,0.0,_MainTexBias);
				float alpha =tex2Dbias(_MainTex, biasCoord);
				float4 outColor = i.color;
				outColor.a *= alpha;
				return outColor;
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
