Shader "Custom/line" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader {
	    Tags { "Queue" = "Geometry+1" }
        ZTest Always
        Pass {
            Color [_Color]
            ZWrite Off
        }
    }
	FallBack "Diffuse"
}
