#version 450
#extension GL_EXT_debug_printf : enable

layout( push_constant ) uniform LightDataStruct {
    vec4 lightData;
} lightData;

layout (location = 0) in vec3 inWorldPos;
layout (location = 0) out vec4 outColor;

void main() {
    vec3 sunPos = normalize(vec3(-lightData.lightData));
    vec3 skyColor = vec3(0.5, 0.8, 0.9);
    float sunHorizonDist = dot(sunPos, vec3(0, 1, 0));
    float darken = (clamp(sunHorizonDist, -0.2, 0.1) + 0.2)*(1/0.3);
    
    vec3 nightTimeColor = vec3(2.0/255, 2.0/255, 4.0/255);
    
    skyColor = mix(skyColor, nightTimeColor, 1 - darken);
    
    vec3 sunColor = vec3(1, 0.95, 0.1);
    float sunDist = dot(sunPos, normalize(inWorldPos));
    const float sunSize = 0.005;
    sunDist *= sunSize + 1;
    sunDist -= (1 - sunSize);
    sunDist *= (1.0f/sunSize);
    sunDist = clamp(sunDist, 0, 1);
    vec3 color = mix(skyColor, sunColor, sunDist);
    outColor = vec4(color, 1);
}