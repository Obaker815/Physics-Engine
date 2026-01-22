#version 330 core

in vec3 vNormal;
in vec3 vWorldPos;
in vec2 vTexCoord;

out vec4 FragColor;

uniform sampler2D uTexture;

uniform vec3 uLightDir;
uniform vec3 uLightColor;
uniform float uAmbient;

void main()
{
    vec3 temp = uLightColor * uAmbient;
    vec3 normal = normalize(vNormal);
    vec3 lightDir = normalize(uLightDir);

    float diff = dot(normal, lightDir) * 0.5 + 0.5;

    vec3 color = texture(uTexture, vTexCoord).rgb;// * diff;
    // color = color * (1 - uAmbient) + vec3(uAmbient);
    // color = color * uLightColor;


    FragColor = vec4(color, 1.0);
}
