﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DIV2Tools.MethodExtensions
{
    public static class ByteMethodExtensions
    {
        /// <summary>
        /// Check if a bit is set or not.
        /// </summary>
        /// <param name="value">This <see cref="byte"/> instance.</param>
        /// <param name="bit">Bit to check (0 to 7).</param>
        /// <returns>Returns <see cref="true"/> if the bit is set.</returns>
        public static bool IsBitSet(this byte value, int bit)
        {
            return (value & (1 << bit)) != 0;
        }

        /// <summary>
        /// Sets or clear a bit.
        /// </summary>
        /// <param name="value">This <see cref="byte"/> instance.</param>
        /// <param name="bit">Bit to set (0 to 7).</param>
        /// <param name="set"><see cref="bool"/> value that sets or clear the bit.</param>
        /// <returns>Returns a new <see cref="byte"/> value with the bit changed.</returns>
        public static byte SetBit(this byte value, int bit, bool set)
        {
            if (!bit.IsClamped(0, 7)) throw new ArgumentOutOfRangeException(nameof(bit), "The bit must be a value between 0 and 7.");
            return set ? (byte)SetBit(value, bit) : (byte)ClearBit(value, bit);
        }

        static int SetBit(int value, int bit)
        {
            return value |= 1 << bit;
        }

        static int ClearBit(int value, int bit)
        {
            return value & ~(1 << bit);
        }

        /// <summary>
        /// Read all <see cref="char"/>s, using ASCII encoding, from a <see cref="byte"/> array until get <see cref="null"/> char termination.
        /// </summary>
        /// <param name="buffer">This <see cref="byte"/> array instance.</param>
        /// <returns>Returns the <see cref="string"/> with all chars readed using ASCII encoding.</returns>
        public static string GetNullTerminatedASCIIString(this byte[] buffer)
        {
            int len = 0;
            do { len++; } while (len < buffer.Length && buffer[len] != 0);
            return Encoding.ASCII.GetString(buffer, 0, len);
        }

        /// <summary>
        /// Print in a string all values from the <see cref="byte"/> array.
        /// </summary>
        /// <param name="array">This <see cref="byte"/> array instance.</param>
        /// <returns>Returns a <see cref="string"/> with format "{ 0, 1, 2, ... }".</returns>
        public static string Print(this byte[] array)
        {
            var sb = new StringBuilder();
            sb.Append("{ ");
            foreach (var item in array)
            {
                sb.Append($"{item} ");
            }
            sb.Append('}');

            return sb.ToString();
        }

        /// <summary>
        /// Performs a <see cref="byte"/> to <see cref="byte"/> comparison of this array with another.
        /// </summary>
        /// <param name="array">This <see cref="byte"/> array instance.</param>
        /// <param name="other">Other <see cref="byte"/> array to compare.</param>
        /// <returns>Returns <see cref="true"/> if two arrays has the same values and length.</returns>
        public static bool Compare(this byte[] array, byte[] other)
        {
            if (array.Length != other.Length) return false;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != other[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Converts this <see cref="byte"/> array to <see cref="uint"/> value.
        /// </summary>
        /// <param name="array">This <see cref="byte"/> array instance.</param>
        /// <returns>Returns a <see cref="uint"/> value.</returns>
        public static uint ToUInt32(this byte[] array)
        {
            return BitConverter.ToUInt32(array);
        }

        /// <summary>
        /// Converts this <see cref="byte"/> array to <see cref="string"/> value.
        /// </summary>
        /// <param name="array">This <see cref="byte"/> array instance.</param>
        /// <returns>Returns a <see cref="string"/> value.</returns>
        public static string ToASCIIString(this byte[] array)
        {
            return BitConverter.ToString(array);
        }
    }
}
