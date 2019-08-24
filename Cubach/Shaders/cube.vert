#version 400

uniform mat4 mvp;

layout(location = 0) in vec3 vert_position;
layout(location = 1) in vec3 vert_normal;
layout(location = 2) in vec2 vert_texCoord;

out vec3 frag_position;
out vec3 frag_normal;
out vec2 frag_texCoord;

void main()
{
    frag_position = vert_position;
    frag_normal = vert_normal;
    frag_texCoord = vert_texCoord;
    gl_Position = mvp * vec4(vert_position, 1.0);
}
