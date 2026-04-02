#version 450
#extension GL_EXT_debug_printf : enable

layout(constant_id = 0) const int SHADOW_MAP_CASCADE_COUNT = 4;
layout(constant_id = 1) const int TEXTURE_ARRAY_COUNT = 1;

layout (set = 0, binding = 1) uniform sampler2D texSampler[TEXTURE_ARRAY_COUNT];
layout (set = 0, binding = 3) uniform sampler2DArray shadowSampler;

struct UniformBufferObject {
    mat4 model;
    mat4 view;
    mat4 proj;
};

layout (set = 0, binding = 2) uniform CascadeUbos {
    UniformBufferObject cascadeUbos[SHADOW_MAP_CASCADE_COUNT];
} ubos;

layout (set = 0, binding = 4) uniform ShadowMapData {
    vec4 cascadeSplits;
    vec3 lightDir;
} shadowMapData;

layout (location = 0) in vec4 fragColor;
layout (location = 1) in vec2 fragUV;
layout (location = 2) in vec4 inViewPos;
layout (location = 3) in vec4 inPos;
layout (location = 4) in vec3 inNormal;
layout (location = 5) flat in uint inTextureIndex;

layout (location = 0) out vec4 outColor;

const mat4 biasMat = mat4(
0.5, 0.0, 0.0, 0.0,
0.0, 0.5, 0.0, 0.0,
0.0, 0.0, 1.0, 0.0,
0.5, 0.5, 0.0, 1.0
);

float textureProj(vec4 shadowCoord, vec2 offset, uint cascadeIndex)
{
    float shadow = 1.0;
    float bias = 0.001f;

    if ( shadowCoord.z > -1.0 && shadowCoord.z < 1.0 ) {
        float dist = texture(shadowSampler, vec3(shadowCoord.st + offset, cascadeIndex)).r;
        if (shadowCoord.w > 0 && dist < shadowCoord.z - bias) {
            shadow = 0.3;
        }
    }
    return shadow;
}

float filterPCF(vec4 sc, uint cascadeIndex)
{
    ivec2 texDim = textureSize(shadowSampler, 0).xy;
    float scale = 0.75;
    float dx = scale * 1.0 / float(texDim.x);
    float dy = scale * 1.0 / float(texDim.y);

    float shadowFactor = 0.0;
    int count = 0;
    int range = 1;

    for (int x = -range; x <= range; x++) {
        for (int y = -range; y <= range; y++) {
            shadowFactor += textureProj(sc, vec2(dx*x, dy*y), cascadeIndex);
            count++;
        }
    }
    return shadowFactor / count;
}

void main() {
    vec4 color = texture(texSampler[inTextureIndex], fragUV) * fragColor;

    uint cascadeIndex = 0;
    for (uint i = 0; i < SHADOW_MAP_CASCADE_COUNT - 1; i++) {
        if (inViewPos.z < shadowMapData.cascadeSplits[i]) {
            cascadeIndex = i + 1;
        }
    }

    UniformBufferObject ubo = ubos.cascadeUbos[cascadeIndex];

    vec3 normal = normalize(inNormal);
    vec3 lightDir = normalize(shadowMapData.lightDir);
    float normalLightDot = dot(normal, lightDir);
    
    float biasSlopeScale = 1.0 - abs(normalLightDot);
    vec4 shadowWorldPos = inPos + vec4(normal*-0.5f*biasSlopeScale, 0.0);
    
    vec4 shadowCoord = (biasMat * ubo.proj * ubo.view) * shadowWorldPos;
    
    float shadow = filterPCF(shadowCoord / shadowCoord.w, cascadeIndex);

    float darken = (clamp(dot(lightDir, vec3(0, -1, 0)), -0.1, 0.1) + 0.1)*5;
    shadow = mix(shadow, 0.1, 1 - darken);
    
    float diffuse = max(normalLightDot, 0.3);
    
    vec3 lightColor = vec3(1);
    outColor.rgb = max(lightColor * (diffuse * color.rgb), vec3(0));
    outColor.rgb *= shadow;
    
    const float fogStart = 112.0f;
    const float fogEnd = 128.0f;
    const vec3 fogColor = vec3(0.5, 0.8, 0.9);
    float fogRange = fogEnd - fogStart;
    float fogDist = fogEnd - abs(inViewPos.z);
    float fog = clamp(fogDist/fogRange, 0, 1);
    
    outColor.rgb = mix(fogColor, outColor.rgb, fog);
    
    outColor.a = color.a;
    
}