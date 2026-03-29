#version 450
#extension GL_EXT_debug_printf : enable

layout (set = 0, binding = 1) uniform sampler2D fontSampler;

layout (location = 0) in vec3 fragColor;
layout (location = 1) in vec2 fragUV;

layout (location = 0) out vec4 outColor;

void main() {
    float val = texture(fontSampler, fragUV).r;
    if(val == 0) discard;
    outColor = vec4(val) * vec4(fragColor, 1);
}