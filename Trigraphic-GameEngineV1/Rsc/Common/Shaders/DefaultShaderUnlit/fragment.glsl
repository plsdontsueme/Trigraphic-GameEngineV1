#version 330 core
out vec4 FragColor;

in vec2 texCoord;

uniform sampler2D diffuse;
uniform vec4 color;

void main()
{
    FragColor = mix(vec4(1f), texture(diffuse, texCoord) * vec4(color.xyz,1f), color.w);
}