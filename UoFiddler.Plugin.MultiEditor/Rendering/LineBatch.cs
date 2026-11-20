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
using UoFiddler.Rendering;

namespace UoFiddler.Plugin.MultiEditor.Rendering
{
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
