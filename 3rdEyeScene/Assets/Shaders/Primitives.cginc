#ifndef PRIMITIVES_SHADER
#define PRIMITIVES_SHADER

struct VertexInput
{
  float4 vertex : POSITION;
  float4 normal : NORMAL;
  UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct FragmentInput
{
  float4 vertex : SV_POSITION;
  float4 colour : COLOR;
};

uniform float _FlatShaded;

UNITY_INSTANCING_BUFFER_START(Props)
  UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(Props)

FragmentInput vert(VertexInput v)
{
  FragmentInput o;
  UNITY_SETUP_INSTANCE_ID(v);
  o.vertex = UnityObjectToClipPos(v.vertex);
  o.colour = UNITY_ACCESS_INSTANCED_PROP(Props, _Color) *
    max(float4(ShadeVertexLights(v.vertex, v.normal), 1.0f),
        float4(_FlatShaded, _FlatShaded, _FlatShaded, _FlatShaded));
  o.colour.w = 0.25f;
  return o;
}

float4 frag(FragmentInput i) : COLOR
{
  return i.colour;
}

#endif // PRIMITIVES_SHADER
