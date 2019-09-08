#version 400

uniform mat4 mvp;

layout(location = 0) in vec2 vert_position;
layout(location = 1) in vec2 vert_texCoord;
layout(location = 2) in vec4 vert_color;

out vec2 frag_position;
out vec2 frag_texCoord;
out vec4 frag_color;

void main()
{
    frag_position = vert_position;
    frag_texCoord = vert_texCoord;
    frag_color = vert_color;
    gl_Position = mvp * vec4(vert_position, 0.0, 1.0);
}
