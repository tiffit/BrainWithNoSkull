#version 450
#extension GL_EXT_debug_printf : enable

layout( push_constant ) uniform TransformPushConstant {
    mat4 model;
} transformConstant;

layout(set = 0, binding = 0) uniform UniformBufferObject {
    mat4 model;
    mat4 view;
    mat4 proj;
} ubo;

layout (location = 0) in vec3 inPosition;
layout (location = 1) in vec3 inColor;
layout (location = 2) in vec2 inUV;
layout (location = 3) in vec3 inNormal;
layout (location = 4) in uint inTextureIndex;

layout (location = 0) out vec3 fragColor;
layout (location = 1) out vec2 fragUV;
layout (location = 2) out vec4 fragViewPos;
layout (location = 3) out vec4 fragPos;
layout (location = 4) out vec3 fragNormal;
layout (location = 5) flat out uint fragTextureIndex;

out gl_PerVertex {
    vec4 gl_Position;
};

void main() {
    fragTextureIndex = inTextureIndex;
    vec4 pos = transformConstant.model * vec4(inPosition, 1.0);
    fragPos = pos;
    fragViewPos = (ubo.view * pos);
    gl_Position = ubo.proj * ubo.view * ubo.model * pos;
    fragColor = inColor;
    fragUV = inUV;
    fragNormal = inNormal;
}