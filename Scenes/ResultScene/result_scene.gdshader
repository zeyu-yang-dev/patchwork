shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;
uniform float blur_strength = 1.2;
uniform vec4 tint_color = vec4(0.92, 0.72, 0.36, 0.18);

void fragment() {
    vec2 uv = SCREEN_UV;
    vec2 px = SCREEN_PIXEL_SIZE * blur_strength;

    vec4 col = texture(screen_texture, uv) * 0.40;
    col += texture(screen_texture, uv + vec2(px.x, 0.0)) * 0.15;
    col += texture(screen_texture, uv - vec2(px.x, 0.0)) * 0.15;
    col += texture(screen_texture, uv + vec2(0.0, px.y)) * 0.15;
    col += texture(screen_texture, uv - vec2(0.0, px.y)) * 0.15;

    COLOR = mix(col, tint_color, tint_color.a);
}