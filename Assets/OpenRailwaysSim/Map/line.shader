Shader "Custom/line" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader {
	    Tags { "Queue" = "Transparent" }
        ZTest Always
        Pass {
            Color [_Color]
            ZWrite Off
        }
    }
	FallBack "Diffuse"
}
