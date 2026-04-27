/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// Static logger façade for sites that cannot accept ILogger via constructor injection
    /// (static helpers, designer-built UserControls, plugins instantiated via Activator).
    /// Constructor-injectable types should take ILogger&lt;T&gt; directly instead of using this.
    /// </summary>
    public static class AppLog
    {
        private static ILoggerFactory _factory = NullLoggerFactory.Instance;

        public static void Initialize(ILoggerFactory factory)
        {
            _factory = factory ?? NullLoggerFactory.Instance;
        }

        public static ILogger<T> For<T>() => _factory.CreateLogger<T>();

        public static ILogger For(Type type) => _factory.CreateLogger(type.FullName);
    }
}
