#version 400

uniform sampler2D font_texture;

in vec4 frag_color;
in vec2 frag_texCoord;

layout(location = 0) out vec4 out_color;

void main()
{
    out_color = frag_color * texture(font_texture, frag_texCoord);
}
