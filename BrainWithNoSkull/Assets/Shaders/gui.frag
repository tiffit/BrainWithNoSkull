#version 450
#extension GL_EXT_debug_printf : enable

layout(constant_id = 1) const int TEXTURE_ARRAY_COUNT = 1;

layout (set = 0, binding = 1) uniform sampler2D texSampler[TEXTURE_ARRAY_COUNT];

layout (location = 0) in vec4 fragColor;
layout (location = 1) in vec2 fragUV;
layout (location = 2) flat in uint fragTextureIndex;
layout (location = 3) in vec3 fragNormal;

layout (location = 0) out vec4 outColor;

void main() {
    const vec3 lightDir = normalize(vec3(7, 10, 7));
    
    vec4 val = texture(texSampler[fragTextureIndex], fragUV);
    
    float diffuse = all(equal(fragNormal, vec3(0.0))) ? 1 : max(dot(fragNormal, lightDir), 0.2);
    outColor.rgb = max(diffuse * val.rgb, vec3(0));
    
    outColor.a = val.a;
    outColor *= fragColor;
    if(outColor.a == 0) discard;
}