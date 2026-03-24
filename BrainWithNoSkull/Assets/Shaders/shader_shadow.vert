#version 450
#extension GL_EXT_debug_printf : enable

layout( push_constant ) uniform TransformPushConstant {
    mat4 model;
    uint cascadeIndex;
} transformConstant;

struct UniformBufferObject {
    mat4 model;
    mat4 view;
    mat4 proj;
};

layout(set = 0, binding = 2) uniform CascadeUbos {
    UniformBufferObject cascadeUbos[4];
} ubos;

layout (location = 0) in vec3 inPosition;
layout (location = 1) in vec3 inColor;
layout (location = 2) in vec2 inUV;

layout (location = 0) out vec3 fragColor;
layout (location = 1) out vec2 fragUV;

out gl_PerVertex {
    vec4 gl_Position;
};

void main() {
    UniformBufferObject ubo = ubos.cascadeUbos[transformConstant.cascadeIndex];
    gl_Position = ubo.proj * ubo.view * ubo.model * transformConstant.model * vec4(inPosition, 1.0);
    fragColor = inColor;
    fragUV = inUV;
}