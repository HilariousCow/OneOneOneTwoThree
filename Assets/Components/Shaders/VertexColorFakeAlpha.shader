Shader "OneOneOneTwoThree/VertexFakeAlpha" {
	Properties {
		
	}
	SubShader {
		Tags {"Queue" = "Geometry" "RenderType"="Transparent" }
		
		Pass{
			
			Cull Off
			
			ZWrite On
			ZTest LEqual
			Fog { Mode Linear }
			//Blend OneMinusDstColor OneMinusSrcColor
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			
		
			float4 _OppositeOfBGColor;
			float4 _BGColor;
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
				
				o.color = v.color;
				o.normal = (mul((float3x3)UNITY_MATRIX_MVP, v.normal));
				
				o.pos = mul(_Object2World, v.vertex);
				
				
				//fun stuff with world space.
				float3 orig1 = o.pos - float3(0, -10, 10);
				float3 orig2 = o.pos - float3(0, 10, -10);
				float dist1 = (orig1.x*orig1.x+orig1.z*orig1.z)*0.01;
				float dist2 = (orig2.x*orig2.x+orig2.z*orig2.z)*0.1;
				float dist = dist1 + dist2;
				
				o.color.a = sin(dist + _Time.y*8.8);
				o.color.a *= 0.5;
				o.color.a = saturate(o.color.a);
				
				o.pos.y -= o.color.a;
			//	o.pos.x = sign(o.pos.x) *( pow( abs(o.pos.x)+o.color.a,0.2*o.color.a+.9))   ;
			//	o.pos.z = sign(o.pos.z) *( pow( abs(o.pos.z)+o.color.a,0.2*o.color.a+.9))  ;
				
			//	o.pos.y -= (o.pos.x*o.pos.x + o.pos.z * o.pos.z) *0.0125;
				
				o.pos =  mul(UNITY_MATRIX_VP, o.pos);
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				
				
				return o; 
			}

			float4 frag (v2f i) : COLOR
			{
				float4 outColor = i.color;
				outColor.a = i.color.r * i.color.a * 0.2f;
				//outColor.rgb = lerp(_BGColor,_OppositeOfBGColor.rgb, outColor.a);
				outColor.rgb = _OppositeOfBGColor.rgb;
				return outColor;
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
