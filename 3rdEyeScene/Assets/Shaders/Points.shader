Shader "Tes/Points"
{
  Properties
  {
    _Color("Main Colour", Color) = (1, 1, 1, 1)
    _Tint("Tint", Color) = (1, 1, 1, 1)
    _PointSize("Point Size", Range(1, 64)) = 2
    _PointHighlighting("Point Highlighting", Range(0, 1)) = 1
    // Are we building for a left handed coordinate system?
    _LeftHanded("Left Handed", Range(0, 1)) = 1
  }

  CGINCLUDE
  #pragma multi_compile __ WITH_COLOURS_UINT WITH_COLOURS_V4 WITH_COLOURS_RANGE_X WITH_COLOURS_RANGE_Y WITH_COLOURS_RANGE_Z
  #pragma multi_compile __ WITH_NORMALS
  #include "UnityCG.cginc"

  // **************************************************************
  // Data structures                        *
  // **************************************************************
  struct GeometryInput
  {
    float4 pos : POSITION;
    #ifdef WITH_NORMALS
    float3 normal : NORMAL;
    #endif // WITH_NORMALS
    float4 colour : COLOR;
  };

  struct FragmentInput
  {
    float4 pos : POSITION;
    float2 tex0 : TEXCOORD0;
    float4 colour : COLOR;
  };

  // User properties.
  uniform float _PointSize;
  uniform float4 _Color;
  uniform float4 _Tint;
  uniform int _PointHighlighting;
  uniform int _LeftHanded;
  uniform float _ColourRangeMin;
  uniform float _ColourRangeMax;

  StructuredBuffer<float3> _Vertices;
  #ifdef WITH_NORMALS
  StructuredBuffer<float3> _Normals;
  #endif // WITH_NORMALS
  #ifdef WITH_COLOURS_UINT
  StructuredBuffer<uint> _Colours;
  #endif // WITH_COLOURS_UINT
  #ifdef WITH_COLOURS_V4
  StructuredBuffer<float4> _Colours;
  #endif // WITH_COLOURS_V4

  // **************************************************************
  // Shader Programs                        *
  // **************************************************************
  float4 colourRangeRainbow(float value)
  {
    // Using HSV colour with S and V = 1, adjusting H from 0 255 based on value.
    const float colourRange = _ColourRangeMin;
    const float h = max(0.0f, min((value - _ColourRangeMin) / (colourRange != 0 ? colourRange : 1.0f), 1.0f));
    const float s = 1.0f;
    const float v = 1.0f;

    const float hSector = h / 60.0f; // sector 0 to 5
    const int sectorIndex = int(min(max(0.0f, floor(hSector)), 5.0f));
    const float f = hSector - float(sectorIndex);
    const float p = v * (1 - s);
    const float q = v * (1 - s * f);
    const float t = v * (1 - s * (1 - f));

    static const int vindex[] = { 0, 1, 1, 2, 2, 0 };
    static const int pindex[] = { 2, 2, 0, 0, 1, 1 };
    static const int qindex[] = { 3, 0, 3, 1, 3, 2 };
    static const int tindex[] = { 1, 3, 2, 3, 0, 3 };

    float4 rgb;
    rgb[vindex[sectorIndex]] = v;
    rgb[pindex[sectorIndex]] = p;
    rgb[qindex[sectorIndex]] = q;
    rgb[tindex[sectorIndex]] = t;

    // Handle achromatic here by testing s inline.
    rgb[0] = (s != 0) ? rgb[0] : v;
    rgb[1] = (s != 0) ? rgb[1] : v;
    rgb[2] = (s != 0) ? rgb[2] : v;

    rgb[3] = 1.0f;

    return rgb;
  }

  // Vertex Shader ------------------------------------------------
  GeometryInput vert(uint vid : SV_VertexID)
  {
    GeometryInput o;
    o.pos = mul(unity_ObjectToWorld, _Vertices[vid]);
    #ifdef WITH_NORMALS
    o.normal = mul(UNITY_MATRIX_MV, _Normals[vid]);
    #endif // WITH_NORMALS

    o.colour =
    #if defined(WITH_COLOURS_UINT)
      float4((float)((_Colours[vid] >> 24) & 255) / 255.0f,
                  (float)((_Colours[vid] >> 16) & 255) / 255.0f,
                  (float)((_Colours[vid] >> 8) & 255) / 255.0f,
                  (float)(_Colours[vid] & 255) / 255.0f)
    #elif defined(WITH_COLOURS_V4)
      _Colours[vid]
    #elif defined(WITH_COLOURS_RANGE_X)
      colourRangeRainbow(_Vertices[vid].x)
    #elif defined(WITH_COLOURS_RANGE_Y)
      colourRangeRainbow(_Vertices[vid].y)
    #elif defined(WITH_COLOURS_RANGE_Z)
      colourRangeRainbow(_Vertices[vid].z)
    #else
      float4(1, 1, 1, 1)
    #endif // WITH_COLOURS
    ;

    return o;
  }

  // Geometry Shader -----------------------------------------------------
  [maxvertexcount(4)]
  void geom(point GeometryInput p[1], inout TriangleStream<FragmentInput> triStream)
  {
    // Minimum point size. Greater than one due to potential floating point error.
    // Will generally equate to not overflowing into adjacent pixels anyway.
    const float MinScale = 1.5f;
    FragmentInput fin;

    // _ScreenParams:
    // x is the current render target width in pixels
    // y is the current render target height in pixels
    // z is 1.0 + 1.0/width
    // w is 1.0 + 1.0/height.
    const float4 ppos = mul(UNITY_MATRIX_VP, float4(p[0].pos.xyz, 1));
    const float depth = ppos.w;
    const float size = 0.5f * max(0.5f + _PointSize * (1 + depth), MinScale * depth) * (_ScreenParams.w - 1.0f);
    const float3 right = mul(UNITY_MATRIX_VP, UNITY_MATRIX_V[0].xyz * size) * (_LeftHanded ? -1.0f : 1.0f);
    const float3 up = mul(UNITY_MATRIX_VP, UNITY_MATRIX_V[1].xyz * size);

    fin.pos = ppos - float4((right + up), 0);
    fin.colour = p[0].colour;
    fin.tex0 = float2(0, 0);
    triStream.Append(fin);

    fin.pos = ppos + float4((right - up), 0);
    fin.tex0 = float2(1, 0);
    triStream.Append(fin);

    fin.pos = ppos - float4((right - up), 0);
    fin.tex0 = float2(0, 1);
    triStream.Append(fin);

    fin.pos = ppos + float4((right + up), 0);
    fin.tex0 = float2(1, 1);
    triStream.Append(fin);
  }

  // Fragment Shader -----------------------------------------------
  float4 frag(FragmentInput input) : COLOR
  {
    const float2 uvoffset = input.tex0 - float2(0.5f, 0.5f);
    const float uvdist2 = uvoffset.x * uvoffset.x + uvoffset.y * uvoffset.y;
    // Drop points outside the point size radius.
    // Use 0.25 because point radius has ended up being 0.5 (from uvoffset)
    // and we need radius squared (0.25)
    if (uvdist2 > 0.25f || input.colour.a == 0)
    {
      discard;
    }
    // Colour scale is set by 1 - pow(pixel radius, 2) / pow(max radius, 2);
    // The max radius is 0.5, which yields a division by 0.25, which is the same
    // as a multiplication by 4. uvdist2 is already pow(pixel radius, 2)
    const float scale = (_PointHighlighting) ? 1.0f - uvdist2 * 4.0f : 1.0f;
    return scale * _Color * _Tint * input.colour;
  }

  ENDCG

  SubShader
  {
    Pass
    {
      //Tags {"RenderType" = "Opaque" "LightMode" = "ShadowCaster" }
      Tags{ "Queue" = "Opaque" "RenderType" = "Opaque" }
      LOD 200
      // The coordinate system may vary, so we must rendering without culling.
      Cull Off

      CGPROGRAM
      #pragma target 4.0
      #pragma vertex vert
      #pragma fragment frag
      #pragma geometry geom
      ENDCG
    }
  }
}
