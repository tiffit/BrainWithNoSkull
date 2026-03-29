#version 450
#extension GL_EXT_debug_printf : enable

layout(constant_id = 1) const int TEXTURE_ARRAY_COUNT = 1;

layout (set = 0, binding = 1) uniform sampler2D texSampler[TEXTURE_ARRAY_COUNT];

layout (location = 0) in vec3 fragColor;
layout (location = 1) in vec2 fragUV;
layout (location = 2) flat in uint fragTextureIndex;

layout (location = 0) out vec4 outColor;

void main() {
    vec4 val = texture(texSampler[fragTextureIndex], fragUV);
    if(val.a == 0) discard;
    outColor = val;
}