#version 330 core
//light shader fragment
out vec4 FragColor;

struct Material 
{
    vec4 color;
};

uniform Material material;


void main()
{
    FragColor = material.color;
}