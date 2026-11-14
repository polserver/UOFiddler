/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace UoFiddler.Plugin.MultiEditor.Rendering
{
    /// <summary>
    /// Colour change of a sprite: rgb * Mul + Add, alpha * MulA.
    /// </summary>
    internal readonly record struct SpriteTint(float MulR, float MulG, float MulB, float MulA, float AddR, float AddG, float AddB)
    {
        public static readonly SpriteTint None = new SpriteTint(1f, 1f, 1f, 1f, 0f, 0f, 0f);

        public static SpriteTint Solid(Color color)
        {
            return new SpriteTint(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f, 0f, 0f, 0f);
        }

        public static SpriteTint Alpha(float alpha)
        {
            return new SpriteTint(1f, 1f, 1f, alpha, 0f, 0f, 0f);
        }
    }

    /// <summary>
    /// Draws atlas sprites as instanced quads, in the order they were added.
    /// </summary>
    internal sealed class SpriteBatch : IDisposable
    {
        private const int FloatsPerSprite = 16;

        private const string VertexSource = @"#version 330 core
layout(location = 0) in vec2 aCorner;
layout(location = 1) in vec4 aRect;
layout(location = 2) in vec4 aUv;
layout(location = 3) in float aLayer;
layout(location = 4) in vec4 aMul;
layout(location = 5) in vec3 aAdd;
uniform vec2 uViewport;
uniform vec2 uOrigin;
uniform float uZoom;
uniform float uAtlasSize;
out vec3 vUv;
out vec4 vMul;
out vec3 vAdd;
void main()
{
    vec2 world = aRect.xy + aCorner * aRect.zw;
    // Whole pixels, so art stays sharp and neighbouring tiles meet without seams.
    vec2 screen = floor((world - uOrigin) * uZoom + 0.5);
    vec2 ndc = screen / uViewport * 2.0 - 1.0;
    gl_Position = vec4(ndc.x, -ndc.y, 0.0, 1.0);
    vUv = vec3((aUv.xy + aCorner * aUv.zw) / uAtlasSize, aLayer);
    vMul = aMul;
    vAdd = aAdd;
}";

        private const string FragmentSource = @"#version 330 core
in vec3 vUv;
in vec4 vMul;
in vec3 vAdd;
uniform sampler2DArray uAtlas;
out vec4 color;
void main()
{
    vec4 c = texture(uAtlas, vUv);
    color = vec4(c.rgb * vMul.rgb + vAdd, c.a * vMul.a);
}";

        private readonly ShaderProgram _shader;
        private readonly int _vao;
        private readonly int _cornerVbo;
        private readonly int _instanceVbo;
        private float[] _data = new float[FloatsPerSprite * 2048];
        private int _count;
        private int _gpuCapacity;

        public SpriteBatch()
        {
            _shader = new ShaderProgram(VertexSource, FragmentSource);
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _cornerVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _cornerVbo);
            float[] corners = { 0f, 0f, 1f, 0f, 0f, 1f, 1f, 1f };
            GL.BufferData(BufferTarget.ArrayBuffer, corners.Length * sizeof(float), corners, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);

            _instanceVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVbo);
            const int stride = FloatsPerSprite * sizeof(float);
            Attribute(1, 4, stride, 0);
            Attribute(2, 4, stride, 4);
            Attribute(3, 1, stride, 8);
            Attribute(4, 4, stride, 9);
            Attribute(5, 3, stride, 13);
            GL.BindVertexArray(0);
        }

        public int Count => _count;

        public void Add(in AtlasSprite sprite, float x, float y, in SpriteTint tint)
        {
            Add(sprite, x, y, sprite.Width, sprite.Height, tint);
        }

        public void Add(in AtlasSprite sprite, float x, float y, float width, float height, in SpriteTint tint)
        {
            if ((_count + 1) * FloatsPerSprite > _data.Length)
            {
                Array.Resize(ref _data, _data.Length * 2);
            }

            int i = _count * FloatsPerSprite;
            _data[i] = x;
            _data[i + 1] = y;
            _data[i + 2] = width;
            _data[i + 3] = height;
            _data[i + 4] = sprite.X;
            _data[i + 5] = sprite.Y;
            _data[i + 6] = sprite.Width;
            _data[i + 7] = sprite.Height;
            _data[i + 8] = sprite.Layer;
            _data[i + 9] = tint.MulR;
            _data[i + 10] = tint.MulG;
            _data[i + 11] = tint.MulB;
            _data[i + 12] = tint.MulA;
            _data[i + 13] = tint.AddR;
            _data[i + 14] = tint.AddG;
            _data[i + 15] = tint.AddB;
            ++_count;
        }

        /// <summary>Draws what was added and empties the batch.</summary>
        public void Flush(ArtAtlas atlas, Camera2D camera, Size viewport)
        {
            if (_count == 0)
            {
                return;
            }

            _shader.Use();
            GL.Uniform2(_shader.Uniform("uViewport"), (float)viewport.Width, viewport.Height);
            GL.Uniform2(_shader.Uniform("uOrigin"), camera.OriginX, camera.OriginY);
            GL.Uniform1(_shader.Uniform("uZoom"), camera.Zoom);
            GL.Uniform1(_shader.Uniform("uAtlasSize"), (float)ArtAtlas.Size);
            GL.Uniform1(_shader.Uniform("uAtlas"), 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2DArray, atlas.Texture);

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVbo);
            int bytes = _count * FloatsPerSprite * sizeof(float);
            if (bytes > _gpuCapacity)
            {
                _gpuCapacity = _data.Length * sizeof(float);
                GL.BufferData(BufferTarget.ArrayBuffer, _gpuCapacity, IntPtr.Zero, BufferUsageHint.StreamDraw);
            }

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, bytes, _data);
            GL.DrawArraysInstanced(PrimitiveType.TriangleStrip, 0, 4, _count);
            GL.BindVertexArray(0);
            _count = 0;
        }

        public void Dispose()
        {
            _shader.Dispose();
            GL.DeleteBuffer(_cornerVbo);
            GL.DeleteBuffer(_instanceVbo);
            GL.DeleteVertexArray(_vao);
        }

        private static void Attribute(int index, int size, int stride, int offsetFloats)
        {
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(index, size, VertexAttribPointerType.Float, false, stride, offsetFloats * sizeof(float));
            GL.VertexAttribDivisor(index, 1);
        }
    }

    /// <summary>
    /// Draws one pixel wide lines in world space (grid, outlines, selection box).
    /// </summary>
    internal sealed class LineBatch : IDisposable
    {
        private const int FloatsPerVertex = 6;

        private const string VertexSource = @"#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec4 aColor;
uniform vec2 uViewport;
uniform vec2 uOrigin;
uniform float uZoom;
out vec4 vColor;
void main()
{
    vec2 screen = floor((aPos - uOrigin) * uZoom + 0.5) + 0.5;
    vec2 ndc = screen / uViewport * 2.0 - 1.0;
    gl_Position = vec4(ndc.x, -ndc.y, 0.0, 1.0);
    vColor = aColor;
}";

        private const string FragmentSource = @"#version 330 core
in vec4 vColor;
out vec4 color;
void main()
{
    color = vColor;
}";

        private readonly ShaderProgram _shader;
        private readonly int _vao;
        private readonly int _vbo;
        private float[] _data = new float[FloatsPerVertex * 1024];
        private int _vertices;
        private int _gpuCapacity;

        public LineBatch()
        {
            _shader = new ShaderProgram(VertexSource, FragmentSource);
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            const int stride = FloatsPerVertex * sizeof(float);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
            GL.BindVertexArray(0);
        }

        public void Add(float x0, float y0, float x1, float y1, Color color)
        {
            if ((_vertices + 2) * FloatsPerVertex > _data.Length)
            {
                Array.Resize(ref _data, _data.Length * 2);
            }

            Vertex(x0, y0, color);
            Vertex(x1, y1, color);
        }

        public void Rectangle(RectangleF rect, Color color)
        {
            Add(rect.Left, rect.Top, rect.Right, rect.Top, color);
            Add(rect.Right, rect.Top, rect.Right, rect.Bottom, color);
            Add(rect.Right, rect.Bottom, rect.Left, rect.Bottom, color);
            Add(rect.Left, rect.Bottom, rect.Left, rect.Top, color);
        }

        public void Flush(Camera2D camera, Size viewport)
        {
            if (_vertices == 0)
            {
                return;
            }

            _shader.Use();
            GL.Uniform2(_shader.Uniform("uViewport"), (float)viewport.Width, viewport.Height);
            GL.Uniform2(_shader.Uniform("uOrigin"), camera.OriginX, camera.OriginY);
            GL.Uniform1(_shader.Uniform("uZoom"), camera.Zoom);

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            int bytes = _vertices * FloatsPerVertex * sizeof(float);
            if (bytes > _gpuCapacity)
            {
                _gpuCapacity = _data.Length * sizeof(float);
                GL.BufferData(BufferTarget.ArrayBuffer, _gpuCapacity, IntPtr.Zero, BufferUsageHint.StreamDraw);
            }

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, bytes, _data);
            GL.DrawArrays(PrimitiveType.Lines, 0, _vertices);
            GL.BindVertexArray(0);
            _vertices = 0;
        }

        public void Dispose()
        {
            _shader.Dispose();
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
        }

        private void Vertex(float x, float y, Color color)
        {
            int i = _vertices * FloatsPerVertex;
            _data[i] = x;
            _data[i + 1] = y;
            _data[i + 2] = color.R / 255f;
            _data[i + 3] = color.G / 255f;
            _data[i + 4] = color.B / 255f;
            _data[i + 5] = color.A / 255f;
            ++_vertices;
        }
    }
}
