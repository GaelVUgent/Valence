Shader "Unlit/Heatmap_Color"
{
    Properties
    {
		_HeatMapTime ("HeatMapTime", Range(0.0, 120.0)) = 0.1
		[HideInInspector] _BottomColor ("Bottom Color", Color) = (1.0, 0.0, 0.0, 1.0)
		[HideInInspector] _MiddleColor ("Middle Color", Color) = (0.0, 1.0, 0.0, 1.0)
		[HideInInspector] _TopColor ("Top Color", Color) = (0.0, 0.0, 1.0, 1.0)
		[HideInInspector] _Duration ("Duration", float) = 60.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

			float _HeatMapTime;
			fixed4 _BottomColor;
			fixed4 _MiddleColor;
			fixed4 _TopColor;
			float _Duration;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//fixed4 c = lerp(_MiddleColor, _TopColor, clamp(_HeatMapPercentage-0.5, 0, 1)*2) * step(0.5, _HeatMapPercentage);
				//c += lerp(_BottomColor, _MiddleColor, clamp(_HeatMapPercentage, 0, 5)*2) * step(_HeatMapPercentage, 0.5);
				fixed4 c = lerp(_MiddleColor, _TopColor, clamp(_HeatMapTime-_Duration, 0, _Duration)/_Duration) * step(_Duration, _HeatMapTime);
				c += lerp(_BottomColor, _MiddleColor, clamp(_HeatMapTime, 0, _Duration)/_Duration) * step(_HeatMapTime, _Duration);

				// apply fog
                UNITY_APPLY_FOG(i.fogCoord, c);

                return c;//lerp(half4(1.0,0.0,0.0,1.0), half4(0.0,1.0,0.0,1.0), _HeatMapPercentage);
            }
            ENDCG
        }
    }
}
