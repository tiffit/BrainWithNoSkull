#version 450
#extension GL_EXT_debug_printf : enable

layout (location = 0) in vec3 inPosition;
layout (location = 1) in uint inColor;

layout (location = 0) out vec4 fragColor;

out gl_PerVertex {
    vec4 gl_Position;
};

void main() {
    gl_Position = vec4(inPosition, 1.0);
    fragColor = unpackUnorm4x8(inColor);
}