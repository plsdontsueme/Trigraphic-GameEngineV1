#version 330 core
out vec4 FragColor;

in vec2 texCoord;

uniform sampler2D diffuse;
uniform vec3 color;

void main()
{
    FragColor = texture(diffuse, texCoord) * vec4(color,1f);
}