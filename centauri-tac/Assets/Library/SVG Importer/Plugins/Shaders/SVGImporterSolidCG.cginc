float2 SVG_SOLID_ANTIALIASING_WIDTH;

struct vertex_input
{
    float4 vertex : POSITION;			    
    half4 color : COLOR;			    
};

struct vertex_input_normal
{
    float4 vertex : POSITION;			    
    float3 normal : NORMAL;
    half4 color : COLOR;    
};

struct vertex_output
{
    float4 vertex : SV_POSITION;			    
    half4 color : COLOR;			    
};	

vertex_output vertexColor(vertex_input v)
{
    vertex_output o;
    o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);    
    o.color = v.color;
    return o;
}

vertex_output vertexColorAntialiased(vertex_input_normal v)
{
    vertex_output o;
	// Antialiasing	
	// Perspective Camera
	if(UNITY_MATRIX_P[3][3] == 0)
	{
		float4 vertex = v.vertex;
		float objSpaceLength = length(ObjSpaceViewDir(vertex));
		vertex.x += v.normal.x * objSpaceLength * SVG_SOLID_ANTIALIASING_WIDTH.x;
		vertex.y += v.normal.y * objSpaceLength * SVG_SOLID_ANTIALIASING_WIDTH.y;
		o.vertex = mul(UNITY_MATRIX_MVP, vertex);
	// Orthographic Camera
	} else {
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.vertex.x += v.normal.x * SVG_SOLID_ANTIALIASING_WIDTH.x;
		o.vertex.y += v.normal.y * SVG_SOLID_ANTIALIASING_WIDTH.y;
	} 
	
    o.color = v.color;
    return o;
}

half4 fragmentColor(vertex_output i) : COLOR
{
	return i.color;
}