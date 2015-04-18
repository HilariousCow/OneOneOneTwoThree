#pragma glsl_no_auto_normalization

float4 RibbonPosWorld(float4 vertPos, float4 localTangent)
{
		float4 worldPos = mul(_Object2World, vertPos);
		float3 worldTangent =  mul((float3x3)_Object2World, localTangent.xyz);
		float3 delta = normalize(worldPos - _WorldSpaceCameraPos);
		
		float3 crossProd = cross( delta, worldTangent);//Note that normal is not normalized - its length is used as "width"
		worldPos.xyz -= crossProd*localTangent.w ;//use trail w for sign.
		
		return worldPos;
}

//expects a raw world position - i.e. no world to object. Might not actually be cheaper. hmm
float4 RibbonPos(float4 vertPos, float4 localTangent)
{
		float4 worldPos = mul(_Object2World, vertPos);
		float3 worldTangent =  mul((float3x3)_Object2World, localTangent.xyz);
		float3 delta = normalize(worldPos - _WorldSpaceCameraPos);
		
		float3 crossProd = cross( delta, worldTangent);//Note that normal is not normalized - its length is used as "width"
		worldPos.xyz -= crossProd*localTangent.w ;//use trail w for sign.
		
		return mul(UNITY_MATRIX_VP, worldPos);
}

//segment tangents constructed from normals?
//toPrevSegment = normalized direction to previous segment
//toNextAndSide.xyz = normalized direction to next segment
//toNextAndSide.w = width and offset desired
//only possible if the pragma above works?
float4 RibbonPosMitre(float4 vertPos, float3 fromPrevious, float4 toNextAndSide)
{
		float4 worldPos = mul(_Object2World, vertPos);
		float3 toNext = toNextAndSide.xyz;
		float3 worldToNext =  mul((float3x3)_Object2World, toNext);
		float3 worldFromPrev =  mul((float3x3)_Object2World, fromPrevious);
		float3 worldTangent = normalize(worldToNext + worldFromPrev);
		float zeroPoint = mul((float3x3)_Object2World, float3(0,0,0));
		float3 _CameraToWorld = -UNITY_MATRIX_V[2].xyz;
		float3 eyeDelta = _CameraToWorld ;//normalize(worldPos - _WorldSpaceCameraPos);
		
		float3 currentNormal = cross(worldToNext, eyeDelta);
		float3 mitreDirection = cross(worldTangent, eyeDelta);
		mitreDirection = mitreDirection - dot(eyeDelta,mitreDirection) * eyeDelta;//flatten mitre direction to eye
		mitreDirection = normalize(mitreDirection);
		currentNormal = currentNormal - dot(eyeDelta,currentNormal) * eyeDelta;
		currentNormal = normalize(currentNormal);
		
		float side = toNextAndSide.w;
		//float thickness = side/dot(currentNormal,mitreDirection) / worldPos.w;
		float thickness = side/ worldPos.w;
		worldPos.xyz += mitreDirection * thickness;
		
		return mul(UNITY_MATRIX_VP, worldPos);

} 

