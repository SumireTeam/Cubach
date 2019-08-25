#version 400

uniform sampler2D colorTexture;

in vec2 frag_position;
in vec2 frag_texCoord;
in vec4 frag_color;

layout(location = 0) out vec4 out_color;

void main()
{
	vec4 color = frag_color * texture(colorTexture, frag_texCoord);
	if (color.w < 0.1)
	{
		discard;
	}

    out_color = color;
}
