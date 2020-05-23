#version 400

uniform mat4 projection;

layout(location = 0) in vec2 vert_position;
layout(location = 1) in vec2 vert_texCoord;
layout(location = 2) in vec4 vert_color;

out vec4 frag_color;
out vec2 frag_texCoord;

void main()
{
    gl_Position = projection * vec4(vert_position, 0.0, 1.0);
    frag_color = vert_color;
    frag_texCoord = vert_texCoord;
}
