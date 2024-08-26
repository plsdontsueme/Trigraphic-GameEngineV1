#version 330 core

out vec4 FragColor;

in vec2 texCoord;

uniform vec4 color; // color.xyz is the color, color.w is the intensity


void main()
{
    vec2 lightPos = vec; // Position of the light in screen space
    float radius = 1; // Radius of the glow effect
    // Calculate distance from the light source
    float distance = length(gl_FragCoord.xy - lightPos);
    
    // Create a radial gradient based on the distance
    float gradient = smoothstep(radius, 0.0, distance);

    // Sample the texture and apply color
    vec4 baseColor = texture(diffuse, texCoord) * vec4(color.xyz, 1.0);
    
    // Combine the base color with the glow effect
    vec4 glowColor = vec4(color.xyz, color.w * gradient);
    
    // Final color with glow effect applied
    FragColor = baseColor + glowColor;
}
