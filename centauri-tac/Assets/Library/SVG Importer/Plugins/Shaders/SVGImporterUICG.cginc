float2 SVG_GRADIENT_ANTIALIASING_WIDTH;

sampler2D _GradientColor;
sampler2D _GradientShape;
float4 _Params;

bool _UseClipRect;
float4 _ClipRect;
			
bool _UseAlphaClip;

struct vertex_input
{
    float4 vertex : POSITION;	
    float2 texcoord0 : TEXCOORD0;
    float2 texcoord1 : TEXCOORD1;
    float3 normal : NORMAL;
    half4 color : COLOR;
};

struct vertex_output
{
    float4 vertex : POSITION;			    
    float4 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 localPosition : TEXCOORD3;
    half4 color : COLOR;
};

vertex_output vertexGradients(vertex_input v)
{
    vertex_output o;
    o.localPosition = v.vertex;
    o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
    o.uv0.xy = v.texcoord0;
    o.color = v.color;
	
	// half pixel
	float2 texelOffset = float2(0.5 / _Params.x, 0.5 / _Params.y);
	float imageIndex = v.texcoord1.x * _Params.z;
	
	// Horizontal Start
    o.uv0.z = saturate((fmod(imageIndex, _Params.x) / _Params.x) + texelOffset.x);
    // Horizontal Width
    o.uv1 = saturate((1.0 - abs(float4(0.0, 1.0, 2.0, 3.0) - v.texcoord1.y)) * (_Params.z / _Params.x - texelOffset.x * 2.0));
    // Vertical Start
    o.uv0.w = saturate((floor((imageIndex / _Params.x) * _Params.w) / _Params.y) + texelOffset.y);  
    
    return o;
}

vertex_output vertexGradientsAntialiased(vertex_input v)
{
    vertex_output o;
    o.localPosition = v.vertex;
    // Antialiasing	
	// Perspective Camera
    if(UNITY_MATRIX_P[3][3] == 0)
	{
		float4 vertex = v.vertex;
		float objSpaceLength = length(ObjSpaceViewDir(vertex));
		vertex.x += v.normal.x * objSpaceLength * SVG_GRADIENT_ANTIALIASING_WIDTH.x;
		vertex.y += v.normal.y * objSpaceLength * SVG_GRADIENT_ANTIALIASING_WIDTH.y;
		o.vertex = mul(UNITY_MATRIX_MVP, vertex);
	// Orthographic Camera
	} else {
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.vertex.x += v.normal.x * SVG_GRADIENT_ANTIALIASING_WIDTH.x;
		o.vertex.y += v.normal.y * SVG_GRADIENT_ANTIALIASING_WIDTH.y;
	} 
	
	
    o.uv0.xy = v.texcoord0;
    o.color = v.color;
	
	// half pixel
	float2 texelOffset = float2(0.5 / _Params.x, 0.5 / _Params.y);
	float imageIndex = v.texcoord1.x * _Params.z;
	
	// Horizontal Start
    o.uv0.z = saturate((fmod(imageIndex, _Params.x) / _Params.x) + texelOffset.x);
    // Horizontal Width
    o.uv1 = saturate((1.0 - abs(float4(0.0, 1.0, 2.0, 3.0) - v.texcoord1.y)) * (_Params.z / _Params.x - texelOffset.x * 2.0));
    // Vertical Start
    o.uv0.w = saturate((floor((imageIndex / _Params.x) * _Params.w) / _Params.y) + texelOffset.y);    
    
    return o;
}

half4 fragmentGradientsOpaque(vertex_output i) : COLOR
{
	float gradient = dot(tex2D(_GradientShape, i.uv0), i.uv1) ;
	float2 gradientColorUV = float2(i.uv0.z + gradient, i.uv0.w);
	return float4(tex2D(_GradientColor, gradientColorUV).rgb * i.color.rgb, 1.0);
}

half4 fragmentGradientsAlphaBlended(vertex_output i) : COLOR
{
	float gradient = dot(tex2D(_GradientShape, i.uv0), i.uv1) ;
	float2 gradientColorUV = float2(i.uv0.z + gradient, i.uv0.w);
	half4 output = tex2D(_GradientColor, gradientColorUV) * i.color;
	
	if (_UseClipRect)
		output *= UnityGet2DClipping(i.localPosition.xy, _ClipRect);
	
	if (_UseAlphaClip)
		clip (output.a - 0.001);

	return output;
}