#ifndef PRIMITIVES_SHADER
#define PRIMITIVES_SHADER

struct VertexInput
{
  float4 vertex : POSITION;
  float4 normal : NORMAL;
  float4 colour : COLOR;
  UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct FragmentInput
{
  float4 vertex : SV_POSITION;
  float4 colour : COLOR;
};

uniform float _FlatShaded;
uniform float4 _LightDir_0;
uniform float4 _LightColour_0;
uniform float4 _LightDir_1;
uniform float4 _LightColour_1;

UNITY_INSTANCING_BUFFER_START(Props)
  UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(Props)

FragmentInput vert(VertexInput v)
{
  FragmentInput o;
  UNITY_SETUP_INSTANCE_ID(v);
  o.vertex = UnityObjectToClipPos(v.vertex);
  o.colour = v.colour * UNITY_ACCESS_INSTANCED_PROP(Props, _Color)
    // * max(float4(ShadeVertexLights(v.vertex, v.normal), 1.0f),
    //       float4(_FlatShaded, _FlatShaded, _FlatShaded, _FlatShaded))
    ;
  return o;
}

float4 frag(FragmentInput i) : COLOR
{
  return i.colour;
}

#endif // PRIMITIVES_SHADER
