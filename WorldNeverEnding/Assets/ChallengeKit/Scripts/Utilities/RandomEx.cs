using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace ChallengeKit
{
    public static class RandomEx
    {
        public static T Range<T>(T[] Array)
        {
            int index = UnityEngine.Random.Range(0, Array.Length);
            return Array[index];
        }

        public static T Range<T>(List<T> List)
        {
            int index = UnityEngine.Random.Range(0, List.Count);
            return List[index];
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do
                    provider.GetBytes(box);
                while (!( box[0] < n * ( Byte.MaxValue / n ) ));
                int k = ( box[0] % n );
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

}
