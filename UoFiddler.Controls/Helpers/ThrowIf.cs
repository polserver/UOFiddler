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

namespace UoFiddler.Controls.Helpers
{
    public static class ThrowIf
    {
        public static class Argument
        {
            public static void IsNull(object argument, string argumentName)
            {
                if (argument == null)
                {
                    throw new ArgumentNullException(argumentName);
                }
            }

            public static void IsNullOrEmptyString(string argument, string argumentName)
            {
                IsNull(argument, argumentName);

                if (string.IsNullOrWhiteSpace(argument))
                {
                    throw new ArgumentException("String cannot be empty", argumentName);
                }
            }
        }
    }
}
