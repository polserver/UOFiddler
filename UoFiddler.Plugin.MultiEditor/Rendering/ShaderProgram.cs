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

namespace UoFiddler.Plugin.MultiEditor.Rendering
{
    internal sealed class ShaderProgram : IDisposable
    {
        public int Handle { get; }

        public ShaderProgram(string vertexSource, string fragmentSource)
        {
            int vertex = Compile(ShaderType.VertexShader, vertexSource);
            int fragment = Compile(ShaderType.FragmentShader, fragmentSource);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertex);
            GL.AttachShader(Handle, fragment);
            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int linked);

            GL.DetachShader(Handle, vertex);
            GL.DetachShader(Handle, fragment);
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            if (linked == 0)
            {
                string log = GL.GetProgramInfoLog(Handle);
                GL.DeleteProgram(Handle);
                throw new InvalidOperationException($"Shader link failed: {log}");
            }
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int Uniform(string name)
        {
            return GL.GetUniformLocation(Handle, name);
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }

        private static int Compile(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int compiled);
            if (compiled != 0)
            {
                return shader;
            }

            string log = GL.GetShaderInfoLog(shader);
            GL.DeleteShader(shader);
            throw new InvalidOperationException($"{type} compile failed: {log}");
        }
    }
}
