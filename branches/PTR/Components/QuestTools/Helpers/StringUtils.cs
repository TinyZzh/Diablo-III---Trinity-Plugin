using System;
using System.Linq;
using Zeta.Common;

namespace QuestTools.Helpers
{
    public static class StringUtils
    {
        /// <summary>
        /// Returns x="123" y="456" z="789"
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static string GetProfilePosition(Vector3 pos)
        {
            return String.Format("x=\"{0:0}\" y=\"{1:0}\" z=\"{2:0}\" ", pos.X, pos.Y, pos.Z);
        }
        /// <summary>
        /// Returns: "123, 456, 789"
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static string GetSimplePosition(Vector3 pos)
        {
            return String.Format("{0:0}, {1:0}, {2:0}", pos.X, pos.Y, pos.Z);
        }
        /// <summary>
        /// Returns a concatenated ToString of given a set of arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string SpacedConcat(params object[] args)
        {
            return args.Aggregate("", (current, o) => current + (o + ", "));
        }
        
        /// <summary>
        /// Returns x="123" y="456" z="789"
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static string GetProfileCoordinates(Vector3 position)
        {
            return String.Format("x=\"{0:0}\" y=\"{1:0}\" z=\"{2:0}\"", position.X, position.Y, position.Z);
        }

        public static string TrimStart(this string target, string trimChars)
        {
            return target.TrimStart(trimChars.ToCharArray());
        }

        public static string TrimEnd(this string target, string trimChars)
        {
            return target.TrimEnd(trimChars.ToCharArray());
        }
    }
}
