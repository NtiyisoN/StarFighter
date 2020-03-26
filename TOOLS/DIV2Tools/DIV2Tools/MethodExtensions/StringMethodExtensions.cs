﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DIV2Tools.MethodExtensions
{
    public static class StringMethodExtensions
    {
        #region Methods & Functions
        /// <summary>
        /// Get fixed-length <see cref="string"/>.
        /// </summary>
        /// <param name="text">this <see cref="string"/> instance.</param>
        /// <param name="length">The <see cref="string"/> length.</param>
        /// <returns>Returns a new <see cref="string"/> with the new length, filled with spaces if the source <see cref="string"/> length is less than new length or get the substring of the source <see cref="string"/> if length is major of new length./returns>
        public static string GetFixedLengthString(this string text, int length)
        {
            return text.Length > length ? text.Substring(0, length) : text.PadRight(length);
        }

        /// <summary>
        /// Convert to <see cref="byte"/> array using ASCII encoding.
        /// </summary>
        /// <param name="text">This <see cref="string"/> instance.</param>
        /// <returns>Returns the <see cref="byte"/> array with the <see cref="string"/> data encoding as ASCII.</returns>
        public static byte[] GetASCIIBytes(this string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        /// <summary>
        /// Convert to <see cref="byte"/> array using ASCII encoding.
        /// </summary>
        /// <param name="text">This <see cref="char"/> array instance.</param>
        /// <returns>Returns the <see cref="byte"/> array with the <see cref="char"/> array data encoding as ASCII.</returns>
        public static byte[] GetASCIIBytes(this char[] text)
        {
            return Encoding.ASCII.GetBytes(text);
        } 

        /// <summary>
        /// Removed any invalid character for filenames.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string RemoveInvalidFilenameCharacters(this string filename)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '\b');
            }
            return filename;
        }

        /// <summary>
        /// Get the filename from a path <see cref="string"/>.
        /// </summary>
        /// <param name="path">This <see cref="string"/> instance.</param>
        /// <returns>Returns the filename.</returns>
        public static string GetFilenameFromPath(this string path)
        {
            return Path.GetFileName(path);
        }
        #endregion
    }
}
