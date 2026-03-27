#version 450

layout(constant_id = 1) const int TEXTURE_ARRAY_COUNT = 1;

layout(binding = 1) uniform sampler2D texSampler[TEXTURE_ARRAY_COUNT];

layout(location = 0) in vec3 fragColor;
layout(location = 1) in vec2 fragUV;

void main() {
}