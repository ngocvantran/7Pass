/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Text;

namespace KeePassLib.Utility
{
    /// <summary>
    /// Contains static buffer manipulation and string conversion routines.
    /// </summary>
    public static class MemUtil
    {
        /// <summary>
        /// Convert a hexadecimal string to a byte array. The input string must be
        /// even (i.e. its length is a multiple of 2).
        /// </summary>
        /// <param name="strHexString">String containing hexadecimal characters.</param>
        /// <returns>Returns a byte array. Returns <c>null</c> if the string parameter
        /// was <c>null</c> or is an uneven string (i.e. if its length isn't a
        /// multiple of 2).</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="strHexString" />
        /// is <c>null</c>.</exception>
        public static byte[] HexStringToByteArray(string strHexString)
        {
            if (strHexString == null)
                throw new ArgumentNullException("strHexString");

            var nStrLen = strHexString.Length;
            if ((nStrLen & 1) != 0)
                return null; // Only even strings supported

            var pb = new byte[nStrLen / 2];

            for (var i = 0; i < nStrLen; ++i)
            {
                var ch = strHexString[i];
                if ((ch == ' ') || (ch == '\t') ||
                    (ch == '\r') || (ch == '\n'))
                {
                    continue;
                }

                byte bt;
                if ((ch >= '0') && (ch <= '9'))
                    bt = (byte)(ch - '0');
                else if ((ch >= 'a') && (ch <= 'f'))
                    bt = (byte)(ch - 'a' + 10);
                else if ((ch >= 'A') && (ch <= 'F'))
                    bt = (byte)(ch - 'A' + 10);
                else bt = 0;

                bt <<= 4;
                ++i;

                ch = strHexString[i];
                if ((ch >= '0') && (ch <= '9'))
                    bt += (byte)(ch - '0');
                else if ((ch >= 'a') && (ch <= 'f'))
                    bt += (byte)(ch - 'a' + 10);
                else if ((ch >= 'A') && (ch <= 'F'))
                    bt += (byte)(ch - 'A' + 10);

                pb[i / 2] = bt;
            }

            return pb;
        }
    }
}