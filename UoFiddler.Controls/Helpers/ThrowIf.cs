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
                ThrowIf.Argument.IsNull(argument, argumentName);

                if (string.IsNullOrWhiteSpace(argument))
                {
                    throw new ArgumentException("String cannot be empty", argumentName);
                }
            }
        }
    }
}
