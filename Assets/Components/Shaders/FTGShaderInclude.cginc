//include for useful sprite stuff


sampler2D FTG_Background;
sampler2D FTG_Sky;//render texture
float4 FTG_BackgroundTint;

inline float4 GetBGColor(half2 screenuv)
{
    return tex2D(FTG_Background, screenuv) * FTG_BackgroundTint;
}
inline float4 GetSkyBGColor(half2 screenuv)
{
    return tex2D(FTG_Sky, screenuv) ;

}

float4 _InColorA;
float4 _InColorB;
float4 _InColorC;
float4 _OutColorA;
float4 _OutColorB;
float4 _OutColorC;