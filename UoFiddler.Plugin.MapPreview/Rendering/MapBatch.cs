/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using OpenTK.Graphics.OpenGL4;
using UoFiddler.Rendering;

namespace UoFiddler.Plugin.MapPreview.Rendering
{
    /// <summary>
    /// The whole scene as one vertex buffer of quads, already in draw order. Each quad has four
    /// explicit corners, so stretched land and upright sprites share one shader and one pass.
    /// </summary>
    internal sealed class MapBatch : IDisposable
    {
        /// <summary>x, y, u, v, layer, light, hue, partial, alpha.</summary>
        public const int FloatsPerVertex = 9;

        public const int FloatsPerQuad = FloatsPerVertex * 4;

        private const string VertexSource = @"#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec3 aUv;
layout(location = 2) in vec4 aExtra;

uniform vec2 uOrigin;
uniform float uZoom;
uniform vec2 uViewport;
uniform float uAtlasSize;

out vec3 vUv;
out vec4 vExtra;

void main()
{
    // Whole pixels, so art stays crisp. Shared corners snap alike, so land does not crack.
    vec2 screen = floor((aPos - uOrigin) * uZoom + 0.5);
    gl_Position = vec4(screen.x / uViewport.x * 2.0 - 1.0, 1.0 - screen.y / uViewport.y * 2.0, 0.0, 1.0);
    vUv = vec3(aUv.xy / uAtlasSize, aUv.z);
    vExtra = aExtra;
}";

        // Art is ARGB1555 widened to 8 bits, so red * 31 is the gray level a hue row is indexed by.
        // A partial hue recolours only gray pixels.
        private const string FragmentSource = @"#version 330 core
in vec3 vUv;
in vec4 vExtra;

uniform sampler2DArray uAtlas;
uniform sampler2D uHues;

out vec4 fragColor;

void main()
{
    // Art alpha is on or off. Filtering softens it at sprite edges when zoomed out, which would let
    // the background show through as seams between land tiles, so it is made hard again.
    vec4 color = texture(uAtlas, vUv);
    if (color.a < 0.5)
    {
        discard;
    }

    color.rgb /= color.a;
    color.a = 1.0;

    float hue = vExtra.y;
    if (hue >= 0.0)
    {
        bool gray = abs(color.r - color.g) < 0.02 && abs(color.g - color.b) < 0.02;
        if (vExtra.z < 0.5 || gray)
        {
            int level = int(color.r * 31.0 + 0.5);
            color.rgb = texelFetch(uHues, ivec2(level, int(hue)), 0).rgb;
        }
    }

    fragColor = vec4(color.rgb * vExtra.x, color.a * vExtra.w);
}";

        private readonly ShaderProgram _shader;
        private readonly int _vao;
        private readonly int _vbo;
        private readonly int _ebo;
        private int _indexedQuads;
        private int _quads;

        public MapBatch()
        {
            _shader = new ShaderProgram(VertexSource, FragmentSource);
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

            const int stride = FloatsPerVertex * sizeof(float);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));

            GL.BindVertexArray(0);
        }

        /// <summary>Replaces the scene. <paramref name="vertices"/> holds <paramref name="quads"/> quads.</summary>
        public void Upload(float[] vertices, int quads)
        {
            _quads = quads;

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quads * FloatsPerQuad * sizeof(float), vertices,
                BufferUsageHint.StaticDraw);

            if (quads > _indexedQuads)
            {
                // Corners are top-left, top-right, bottom-left, bottom-right (for land: top, right,
                // left, bottom), so both triangles fold along the right-left diagonal.
                uint[] indices = new uint[quads * 6];
                for (uint q = 0, i = 0; q < quads; q++)
                {
                    uint v = q * 4;
                    indices[i++] = v;
                    indices[i++] = v + 1;
                    indices[i++] = v + 2;
                    indices[i++] = v + 2;
                    indices[i++] = v + 1;
                    indices[i++] = v + 3;
                }

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices,
                    BufferUsageHint.StaticDraw);
                _indexedQuads = quads;
            }

            GL.BindVertexArray(0);
        }

        /// <summary>Draws the scene with world point (originX, originY) at the top left of the viewport.</summary>
        public void Draw(ArtAtlas atlas, HueTexture hues, float originX, float originY, float zoom, int viewportWidth,
            int viewportHeight)
        {
            if (_quads == 0)
            {
                return;
            }

            _shader.Use();
            GL.Uniform2(_shader.Uniform("uOrigin"), originX, originY);
            GL.Uniform1(_shader.Uniform("uZoom"), zoom);
            GL.Uniform2(_shader.Uniform("uViewport"), (float)viewportWidth, viewportHeight);
            GL.Uniform1(_shader.Uniform("uAtlasSize"), (float)ArtAtlas.Size);
            GL.Uniform1(_shader.Uniform("uAtlas"), 0);
            GL.Uniform1(_shader.Uniform("uHues"), 1);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2DArray, atlas.Texture);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, hues.Texture);
            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _quads * 6, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteVertexArray(_vao);
            _shader.Dispose();
        }
    }
}
