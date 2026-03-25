#version 450
#extension GL_EXT_debug_printf : enable

layout(set = 0, binding = 0) uniform UniformBufferObject {
    mat4 model;
    mat4 view;
    mat4 proj;
} ubo;

layout (location = 0) in vec3 inPosition;

layout (location = 0) out vec3 fragWorldPos;

out gl_PerVertex {
    vec4 gl_Position;
};

void main() {
    vec4 pos = vec4(inPosition, 1);
    mat4 centeredView = mat4(mat3(ubo.view));
    fragWorldPos = (inverse(ubo.proj * centeredView) * pos).xyz;
    gl_Position = pos;
}