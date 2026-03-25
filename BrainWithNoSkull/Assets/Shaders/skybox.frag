#version 450
#extension GL_EXT_debug_printf : enable

layout (location = 0) in vec3 inWorldPos;
layout (location = 0) out vec4 outColor;

void main() {
    vec3 color = vec3(0.5, 0.8, 0.9);
    vec3 sunPos = vec3(0, 1, 0);
    vec3 sunColor = vec3(1, 0.95, 0.1);
    float sunDist = dot(normalize(sunPos), normalize(inWorldPos));
    const float sunSize = 0.005;
    sunDist *= sunSize + 1;
    sunDist -= (1 - sunSize);
    sunDist *= (1.0f/sunSize);
    sunDist = clamp(sunDist, 0, 1);
    color = mix(color, sunColor, sunDist);
    outColor = vec4(color, 1);
}