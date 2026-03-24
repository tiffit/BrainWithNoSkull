#version 450
#extension GL_EXT_debug_printf : enable

#define SHADOW_MAP_CASCADE_COUNT 4

layout (set = 0, binding = 1) uniform sampler2D texSampler;
layout (set = 0, binding = 3) uniform sampler2DArray shadowSampler;

struct UniformBufferObject {
    mat4 model;
    mat4 view;
    mat4 proj;
};

layout (set = 0, binding = 2) uniform CascadeUbos {
    UniformBufferObject cascadeUbos[4];
} ubos;

layout (set = 0, binding = 4) uniform ShadowMapData {
    vec4 cascadeSplits;
    vec3 lightDir;
} shadowMapData;

layout (location = 0) in vec3 fragColor;
layout (location = 1) in vec2 fragUV;
layout (location = 2) in vec4 inViewPos;
layout (location = 3) in vec4 inPos;

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
    float bias = 0.1;

    if ( shadowCoord.z > -1.0 && shadowCoord.z < 1.0 ) {
        float dist = texture(shadowSampler, vec3(shadowCoord.st + offset, cascadeIndex)).r;
        if (shadowCoord.w > 0 && dist < shadowCoord.z - bias) {
            shadow = 0.3;
        }
    }
    return shadow;

}

void main() {
    vec4 color = texture(texSampler, fragUV) * vec4(fragColor, 1);

    uint cascadeIndex = 0;
    for (uint i = 0; i < SHADOW_MAP_CASCADE_COUNT - 1; i++) {
        if (inViewPos.z < shadowMapData.cascadeSplits[i]) {
            cascadeIndex = i + 1;
        }
    }

    UniformBufferObject ubo = ubos.cascadeUbos[cascadeIndex];
    vec4 shadowCoord = (biasMat * ubo.proj * ubo.view) * inPos;

    float shadow = textureProj(shadowCoord / shadowCoord.w, vec2(0.0), cascadeIndex);

    vec3 normal = vec3(0, 1, 0);
    vec3 lightDir = normalize(-shadowMapData.lightDir);

    float diffuse = max(dot(normal, lightDir), 0.3);
    vec3 lightColor = vec3(1.0, 0.0, 0.0);
    outColor.rgb = max(lightColor * (diffuse * color.rgb), vec3(0));
    outColor.rgb *= shadow;
    outColor.a = color.a;

//    switch(cascadeIndex) {
//        case 0 :
//            outColor.rgb *= vec3(1.0f, 0.25f, 0.25f);
//            break;
//        case 1 :
//            outColor.rgb *= vec3(0.25f, 1.0f, 0.25f);
//            break;
//        case 2 :
//            outColor.rgb *= vec3(0.25f, 0.25f, 1.0f);
//            break;
//        case 3 :
//            outColor.rgb *= vec3(1.0f, 1.0f, 0.25f);
//            break;
//    }
}