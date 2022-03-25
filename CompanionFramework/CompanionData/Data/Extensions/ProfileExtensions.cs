using System;
using System.Collections.Generic;
using System.Text;

namespace Companion.Data.Extensions
{
    public static class ProfileExtensions
    {
        /// <summary>
        /// Count the number of characteristics in this profile list.
        /// </summary>
        /// <param name="profiles">Profile list</param>
        /// <returns>Total characteristics count</returns>
        public static int GetCharacteristicCount(this List<Profile> profiles)
        {
            int count = 0;
            foreach (Profile profile in profiles)
            {
                count += profile.characteristics.Count;
            }
            return count;
        }
    }
}
