#version 450
#extension GL_EXT_debug_printf : enable

layout(constant_id = 0) const int SHADOW_MAP_CASCADE_COUNT = 4;

layout( push_constant ) uniform TransformPushConstant {
    uint cascadeIndex;
} transformConstant;

struct UniformBufferObject {
    mat4 model;
    mat4 view;
    mat4 proj;
};

layout(set = 0, binding = 2) uniform CascadeUbos {
    UniformBufferObject cascadeUbos[SHADOW_MAP_CASCADE_COUNT];
} ubos;

layout (location = 0) in vec3 inPosition;
layout (location = 1) in uint inColor;
layout (location = 2) in vec2 inUV;
layout (location = 3) in vec3 inNormal;
layout (location = 4) in uint inTextureIndex;

layout (location = 0) out vec4 fragColor;
layout (location = 1) out vec2 fragUV;

out gl_PerVertex {
    vec4 gl_Position;
};

void main() {
    UniformBufferObject ubo = ubos.cascadeUbos[transformConstant.cascadeIndex];
    gl_Position = ubo.proj * ubo.view * ubo.model * vec4(inPosition, 1.0);
    fragColor = unpackUnorm4x8(inColor);
    fragUV = inUV;
}