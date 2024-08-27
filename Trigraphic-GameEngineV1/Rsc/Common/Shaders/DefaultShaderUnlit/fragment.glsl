#version 330 core

out vec4 FragColor;

in vec2 texCoord;

struct Material {
    vec4 color;
    sampler2D diffuse;
};
uniform Material material;

void main()
{
    FragColor = texture(material.diffuse, texCoord) * material.color;
}
