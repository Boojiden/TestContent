sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2;
sampler uImage3;
float3 uColor;
float uOpacity;
float3 uSecondaryColor;
float uTime;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uImageOffset;
float uIntensity;
float uProgress;
float2 uDirection;
float uSaturation;
float4 uSourceRect;
float2 uZoom;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;

float dist(float a, float b, float c, float d)
{
    return sqrt((a - c) * (a - c) + (b - d) * (b - d));
}
float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
	float scale = 6;
    float2 nonuv = uv * uImageSize1;
    float2 uva = (nonuv + 3. * sin(uTime * float2(.42, .51) + nonuv / 270. * 6.).xy) / uImageSize1;
    float2 uvb = (nonuv + 3. * sin(uTime * float2(.462, .391) + nonuv / 280. * 6.).yx) / uImageSize1;
    float2 centered = (nonuv * 4 - uImageSize1.xy) / uImageSize1.y;
    
    uva *= scale;
    uvb *= scale;
    uva += float2(0.4, 0.45) * uTime * 1. / scale;
    uvb += float2(0.4, 0.45) * uTime * 1. / scale;
    float4 col = max(tex2D(uImage1, uva), tex2D(uImage1, uvb));
    return col * 1. - (.5 * length(centered));
    //return tex2D(uImage1, nonuv / uImageSize1);

}

technique Technique1
{
    pass SkyPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}