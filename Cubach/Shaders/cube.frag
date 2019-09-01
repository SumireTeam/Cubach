#version 400

uniform sampler2D colorTexture;

in vec3 frag_position;
in vec3 frag_normal;
in vec2 frag_texCoord;

layout(location = 0) out vec4 out_color;

void main()
{
    vec3 light = normalize(vec3(0.2, 0.4, 0.8));
    vec3 ambient = vec3(0.2);
    vec3 diffuse = mix(vec3(0.0, 0.0, 0.1), vec3(0.5, 0.5, 0.45), (dot(frag_normal, light) + 1) / 2);
    out_color = vec4(ambient + diffuse, 1.0) * texture(colorTexture, frag_texCoord);
}
