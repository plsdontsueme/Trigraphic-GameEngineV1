#version 330 core
out vec4 FragColor;

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

struct Material {
    sampler2D diffuse;
    sampler2D specular;
    float shininess;
};
uniform Material material;

struct EnvironmentLighting {
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
uniform EnvironmentLighting environmentLighting;

uniform vec3 viewPos;

void main()
{
    // ambient
    vec3 ambient = environmentLighting.ambient * vec3(texture(material.diffuse, TexCoord));

    // diffuse 
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(-environmentLighting.direction);//We still normalize the light direction since we techically dont know,
                                                    //wether it was normalized for us or not.
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = environmentLighting.diffuse * diff * vec3(texture(material.diffuse, TexCoord));

    // specular
    vec3 viewDir = normalize(viewPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = environmentLighting.specular * spec * vec3(texture(material.specular, TexCoord));

    vec3 result = ambient + diffuse + specular;
    FragColor = vec4(result, 1.0);
}