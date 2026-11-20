/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using OpenTK.GLControl;
using OpenTK.Windowing.Common;

namespace UoFiddler.Rendering
{
    /// <summary>
    /// A WinForms surface with an OpenGL 3.3 core, forward-compatible context: the context every
    /// shader in this library is written for.
    /// </summary>
    public abstract class GlHostControl : GLControl
    {
        protected GlHostControl()
            : base(new GLControlSettings
            {
                API = ContextAPI.OpenGL,
                APIVersion = new Version(3, 3),
                Profile = ContextProfile.Core,
                Flags = ContextFlags.ForwardCompatible
            })
        {
        }
    }
}
