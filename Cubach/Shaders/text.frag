#version 400

uniform sampler2D colorTexture;

in vec2 frag_position;
in vec2 frag_texCoord;
in vec4 frag_color;

layout(location = 0) out vec4 out_color;

void main()
{
    out_color = frag_color * texture(colorTexture, frag_texCoord);
}
