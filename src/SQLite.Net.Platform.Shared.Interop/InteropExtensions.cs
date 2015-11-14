using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

// Inspired by Robert Simpson

/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace SQLite.Net.Platform.Shared.Interop
{
    public static class InteropExtensions
    {
        /// <summary>
        /// Converts a string to a UTF-8 encoded byte array sized to include a null-terminating character.
        /// </summary>
        /// <param name="sourceText">The string to convert to UTF-8</param>
        /// <returns>A byte array containing the converted string plus an extra 0 terminating byte at the end of the array.</returns>
        public static byte[] ToUTF8Bytes(this string sourceText)
        {
            var _utf8 = Encoding.UTF8;

            Byte[] byteArray;
            int nlen = _utf8.GetByteCount(sourceText) + 1;

            byteArray = new byte[nlen];
            nlen = _utf8.GetBytes(sourceText, 0, sourceText.Length, byteArray, 0);
            byteArray[nlen] = 0;

            return byteArray;
        }

        /// <summary>
        /// Converts a UTF-8 encoded IntPtr of the specified length into a .NET string
        /// </summary>
        /// <param name="nativestring">The pointer to the memory where the UTF-8 string is encoded</param>
        /// <param name="nativestringlen">The number of bytes to decode</param>
        /// <returns>A string containing the translated character(s)</returns>
        public static string ToUT8String(this IntPtr nativestring, int nativestringlen = -1)
        {
            var _utf8 = Encoding.UTF8;

            if (nativestringlen == 0 || nativestring == IntPtr.Zero) return "";
            if (nativestringlen == -1)
            {
                do
                {
                    nativestringlen++;
                } while (Marshal.ReadByte(nativestring, nativestringlen) != 0);
            }

            byte[] byteArray = new byte[nativestringlen];

            Marshal.Copy(nativestring, byteArray, 0, nativestringlen);

            return _utf8.GetString(byteArray, 0, nativestringlen);
        }
    }
}
