﻿using System;
using System.Security.Cryptography;
using System.Text;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Objects;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

namespace Trinity.Framework.Helpers
{
    public static class HashGenerator
    {
        /*
         * Reference implementation: http://msdn.microsoft.com/en-us/library/s02tk69a.aspx
         */

        public static string GenerateItemHash(TrinityItem item)
        {
            return GenerateItemHash(item.Position, item.ActorSnoId, item.InternalName, Core.Player.WorldSnoId, item.ItemQualityLevel, item.ItemLevel);
        }

        /// <summary>
        /// Generates an MD5 Hash given the dropped item parameters
        /// </summary>
        /// <param name="position">The Vector3 position of hte item</param>
        /// <param name="actorSNO">The ActorSnoId of the item</param>
        /// <param name="name">The Name of the item</param>
        /// <param name="worldID">The current World ID</param>
        /// <param name="itemQuality">The ItemQuality of the item</param>
        /// <param name="itemLevel">The Item Level</param>
        public static string GenerateItemHash(Vector3 position, int actorSNO, string name, int worldID, ItemQuality itemQuality, int itemLevel)
        {
            using (MD5 md5 = MD5.Create())
            {
                string itemHashBase = String.Format("{0}{1}{2}{3}{4}{5}", position, actorSNO, name, worldID, itemQuality, itemLevel);
                string itemHash = GetMd5Hash(md5, itemHashBase);
                return itemHash;
            }
        }

        private static UInt64 CalculateKnuthHash(string read)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }

        public static string GenerateObjecthash(int actorSnoId, Vector3 position, string internalName, TrinityObjectType type)
        {
            //using (MD5 md5 = MD5.Create())
            //{
            //string objHashBase = actorSnoId + internalName + position + type + TrinityPlugin.CurrentWorldDynamicId;
            //string objHash = CalculateKnuthHash(objHashBase).ToString(); //GetMd5Hash(md5, objHashBase);
            //return objHash;
            //}

            return CalculateKnuthHash(actorSnoId + internalName + position + type).ToString();
        }

        /// <summary>
        /// This is a "psuedo" hash, and used just to compare objects which might have a shifting RActorGUID
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GenerateWorldObjectHash(DiaObject obj)
        {
            return GenerateWorldObjectHash(obj.ActorSnoId, obj.Position, obj.GetType().ToString(), obj.WorldId);
        }

        /// <summary>
        /// This is a "psuedo" hash, and used just to compare objects which might have a shifting RActorGUID
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GenerateWorldObjectHash(int actorSNO, Vector3 position, string type, int dynanmicWorldId)
        {
            return String.Format("{0}{1}{2}{3}", actorSNO, position, type, dynanmicWorldId);
        }

        /// <summary>
        /// Gets an MD5 hash given a string input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetGenericHash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                return GetMd5Hash(md5, input);
            }
        }

        internal static string GetMd5Hash(MD5 md5Hash, string input)
        {
            using (new PerformanceLogger("GetMd5Hash"))
            {
                byte[] data = md5Hash.ComputeHash(Encoding.Default.GetBytes(input));
                return BitConverter.ToString(data);
            }
        }

        // Verify a hash against a string.
        private static bool VerifySha1Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}