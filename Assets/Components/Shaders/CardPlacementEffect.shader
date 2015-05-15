Shader "OneOneOneTwoThree/CardPlacementEffect" {
		Properties {
		_LineWidth("LineWidth", Range (0,200)) = 15
		
		_InnerEdge("InnerEdge", float) =0
		_OuterEdge("OuterEdge", float) =0
		
	}
	SubShader {
		Tags {"Queue" = "Transparent" "RenderType"="Opaque" }
		
		Pass{
			
			Cull front
			
			ZWrite On
			ZTest LEqual
			ColorMask 0
			Offset -10,-10
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			float _LineWidth;
			float _InnerEdge;
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
				o.normal   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
			o.normal = normalize(o.normal);
				//this is the mask
			
			
				float2 pixelSize = (1/_ScreenParams.xy) * _LineWidth * _InnerEdge;
				o.pos.xy += (o.normal.xy * pixelSize)* o.pos.z;// * o.pos.z;
				//o.pos.z+= pixelSize;
				
				
				o.color = v.color;
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o; 
			}

			float4 frag (v2f i) : COLOR
			{
				float4 outColor  = i.color;
				outColor.a = 0;
				return outColor;
			}
			
			ENDCG
		}
		Pass{
			
			Cull front
			
			ZWrite On
			ZTest LEqual
			//Fog Disable
			Blend SrcAlpha OneMinusSrcAlpha
			//Blend OneMinusDstColor OneMinusSrcColor //negative color
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			float4 _BGColor;
			float4 _OppositeOfBGColor;
			float _LineWidth;
			float _OuterEdge;
			float _InnerEdge;
			
			struct appdata {
				float4 vertex	: POSITION;
				float4 color : COLOR;
				float3 normal : NORMAL;
				
				float2 texcoord: TEXCOORD0;
			};
 
			struct v2f {
				float4 pos		: SV_POSITION;
				float4 extraPos		: TANGENT;
				float4 color : COLOR;
				float3 normal	: NORMAL;
				float2 uv		: TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
				//float2 offset = TransformViewToProjection(o.normal.xy);
			 
				//this is the outline 
						
				float2 pixelSize = (1/_ScreenParams.xy) * _LineWidth * _OuterEdge;
				o.pos.xy += (o.normal.xy * pixelSize)* o.pos.z;// * o.pos.z;
				//o.pos.z+= pixelSize;
				
				//o.color = lerp(_OppositeOfBGColor, float4(0,0,0,1), _OuterEdge);
				o.color = _OppositeOfBGColor;
				//o.color.a = (1-abs(_OuterEdge-_InnerEdge)) * 10;
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				o.extraPos = o.pos;
				return o; 
			}

			float4 frag (v2f i) : COLOR
			{
				float aspect = _ScreenParams.x/ _ScreenParams.y;
				float hatch = (i.extraPos.x*aspect+i.extraPos.y)/i.extraPos.w;
				hatch = (frac(hatch*25) * 2) -1;//repeat -1..+1
				
				hatch = abs(hatch);
				hatch -= 0.5f;
				hatch *=100;
				hatch += 0.5f;
				float lineDelta = _OuterEdge - _InnerEdge;
				hatch += (lineDelta) * 75;// * lineDelta * 20;
				//frac();
				i.color.a = 1;//saturate(frac(length(i.extraPos.xz/i.extraPos.w) * 5) - 0.5 * 2);
				i.color.a = hatch;
				return i.color;//i.color;//*10;
			}
			
			ENDCG
		}
	}
	FallBack "Diffuse"
}
