// Adapted from the EDL shader code in potree which is modified from Christian Boucheny in cloud compare:
// https://github.com/cloudcompare/trunk/tree/master/plugins/qEDL/shaders/EDL
// Author: Craig James
// Modified by: Kazys Stepanas
//  - Replaced _CameraDepthTexture with _DepthTexture to be explicitly set when calling.
//    This supports rendering the whole scene to a texture in a single pass, then extracting
//    the depth from that render into _DepthTexture. Otherwise a seperate depth pass is required.
Shader "Hidden/EDL"
{
  Properties
  {
    // modify/comment out the MainTex to turn this into a grayscale effect, showing the EDL contribution 
    _MainTex("Base (RGB)", 2D) = "white" {}
    _DepthTexture("Depth Texture", 2D) = "black" {}
    _Radius("Radius", float) = 3.0
    _ExpScale("Exp Scale", float) = 100.0
    _EdlScale("Edl Scale", float) = 1.0
  }

  SubShader
  {
    Pass
    {
      Tags{ "Queue" = "Overlay" }
      ZTest Always
      Cull Off
      ZWrite Off
      //ZWrite On

      CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"

      sampler2D _MainTex;
      sampler2D _DepthTexture;
      //uniform float4 _MainTex_TexelSize;
      float _Radius;
      float _ExpScale;
      float _EdlScale;

#define NEIGHBOUR_COUNT 8
      uniform float2 _NeighbourAddress[NEIGHBOUR_COUNT];

      ///////////////////////////////////////////////////////////////////////////
      // Support methods for EDL in fragment shader

      // this actually only returns linear depth values if LOG_BIAS is 1.0
      // lower values work out more nicely, though.
#define LOG_BIAS 0.01
      float logToLinear(float z)
      {
        return (pow((1.0 + LOG_BIAS * _ProjectionParams.z), z) - 1.0) / LOG_BIAS;
      }

      // transform linear depth to [0,1] interval with 1 beeing closest to the camera.
      float ztransform(float linearDepth)
      {
        // _ProjectionParams.y is camera near, .z is far
        return 1.0 - (linearDepth - _ProjectionParams.y) / (_ProjectionParams.z - _ProjectionParams.y);
      }

      // Look at neighbour's depth and return lower factor if neighbours are significantly different 
      float computeObscurance(float linearDepth, sampler2D depthTex, float2 uv)
      {
        float2 uvRadius = _Radius / float2(_ScreenParams.x, _ScreenParams.y);

        float sum = 0.0;

        for (int c = 0; c < NEIGHBOUR_COUNT; c++)
        {
          float2 N_rel_pos = uvRadius * _NeighbourAddress[c];
          float2 N_abs_pos = uv + N_rel_pos;

          float neighbourDepth = logToLinear(UNITY_SAMPLE_DEPTH(tex2D(depthTex, N_abs_pos)));
          // float neighbourDepth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(depthTex, N_abs_pos)));

          if (neighbourDepth != 0.0)
          {
            float Znp = ztransform(neighbourDepth) - ztransform(linearDepth);
            sum += max(0.0, Znp) / (0.05 * linearDepth);
          }
        }
        return sum;
      }

      ///////////////////////////////////////////////////////////////////////////
      // Fragment shader
      // Depth from http://williamchyr.com/2013/11/unity-shaders-depth-and-normal-textures/
      // http://beta.unity3d.com/talks/Siggraph2011_SpecialEffectsWithDepth_WithNotes.pdf
      fixed4 frag(v2f_img i) : SV_Target
      {
        float2 uv = i.uv;
        sampler2D depthTex = _DepthTexture;
        float depthValue = logToLinear(UNITY_SAMPLE_DEPTH(tex2D(depthTex, uv)));
        //float depthValue = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(depthTex, uv)));

        // The following code may be required in some instances. If the EDL image is flipped
        // then enable this code and uncomment the uniform declaration of '_MainTex_TexelSize'
        // above.
//#if UNITY_UV_STARTS_AT_TOP
//        if (_MainTex_TexelSize.y < 0)
//        {
//          uv.y = 1 - uv.y;
//        }
//#endif // UNITY_UV_STARTS_AT_TOP        

        float f = computeObscurance(depthValue, depthTex, uv);
        f = exp(-_ExpScale * _EdlScale * f);
        // For _MainTex, always use i.uv, not uv to cater for the potential flip.
        fixed4 color = tex2D(_MainTex, i.uv);

        if (color.a == 0.0 && f >= 1.0)
        {
          discard;
        }

        return fixed4(color.rgb * f, 1.0f);

        // Show depth buffer
        //return fixed4(depthValue, depthValue, depthValue, 1.0f);
      }

      ENDCG
    }
  }
}
