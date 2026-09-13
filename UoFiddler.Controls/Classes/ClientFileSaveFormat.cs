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

using System.Collections.Generic;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// What a save writes for the file types the client ships in either container.
    /// </summary>
    public enum ClientFileSaveFormat
    {
        /// <summary>Whatever the loaded client keeps that type in. Asked per type, not per client.</summary>
        FollowSource,

        /// <summary>Always the mul and idx pair, even on a uop client.</summary>
        Mul,

        /// <summary>Always the uop, even on a mul client.</summary>
        Uop,

        /// <summary>Choose at save time.</summary>
        Ask
    }

    public static class ClientFileSaveFormats
    {
        public static IReadOnlyList<ClientFileSaveFormat> All { get; } = new[]
        {
            ClientFileSaveFormat.FollowSource,
            ClientFileSaveFormat.Mul,
            ClientFileSaveFormat.Uop,
            ClientFileSaveFormat.Ask
        };

        /// <summary>Position of a format in <see cref="All"/>, for driving a combo box.</summary>
        public static int IndexOf(ClientFileSaveFormat format)
        {
            for (int i = 0; i < All.Count; ++i)
            {
                if (All[i] == format)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// Which entry of a "the same format as this client / .mul / .uop" list the option opens on.
        /// Ask opens on the source format: a form carrying its own format control asks by being there.
        /// </summary>
        public static int DefaultIndex(ClientFileSaveFormat format) => format switch
        {
            ClientFileSaveFormat.Mul => 1,
            ClientFileSaveFormat.Uop => 2,
            _ => 0
        };

        public static string DisplayName(ClientFileSaveFormat format) => format switch
        {
            ClientFileSaveFormat.FollowSource => "The same format as this client",
            ClientFileSaveFormat.Mul => "Always .mul and .idx",
            ClientFileSaveFormat.Uop => "Always .uop",
            ClientFileSaveFormat.Ask => "Ask every time I save",
            _ => format.ToString()
        };
    }
}