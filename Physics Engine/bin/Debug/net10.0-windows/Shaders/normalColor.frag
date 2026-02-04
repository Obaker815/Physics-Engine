#version 330 core
in vec3 vNormal;
in vec3 vWorldPos;
in vec2 vTexCoord;

out vec4 FragColor;

void main()
{
    FragColor = vec4(vNormal, 0.0);
}
