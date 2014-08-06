#region Copyright notice and license

// Protocol Buffers - Google's data interchange format
// Copyright 2008 Google Inc.  All rights reserved.
// http://github.com/jskeet/dotnet-protobufs/
// Original C++/Java/Python code:
// http://code.google.com/p/protobuf/
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//     * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
//     * Neither the name of Google Inc. nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using Google.ProtocolBuffers.Descriptors;
using System.Globalization;
using System;

namespace Google.ProtocolBuffers.Plugins
{
    public enum CSharpType
    {
        NotSupported = 0,
        DateTime = 1,
        DateTimeOffset = 2,
        Decimal = 3,
        Guid = 4,
    }

    public static partial class CSharpTypes
    {
        public static CSharpType GetCSharpType(string name)
        {
            switch (name)
            {
                case "CSharp.DateTime":
                    return CSharpType.DateTime;
                case "CSharp.DateTimeOffset":
                    return CSharpType.DateTimeOffset;
                case "CSharp.Decimal":
                    return CSharpType.Decimal;
                case "CSharp.Guid":
                    return CSharpType.Guid;
            }
            return CSharpType.NotSupported;
        }

        public static bool ToString(CSharpType type, object value, out string text)
        {
            switch (type)
            {
                case CSharpType.DateTime:
                    text = ((DateTime)value).ToString("O");
                    return true;
                case CSharpType.DateTimeOffset:
                    text = ((DateTimeOffset)value).ToString("O");
                    return true;
                case CSharpType.Decimal:
                    text = ((Decimal)value).ToString(CultureInfo.InvariantCulture);
                    return true;
                case CSharpType.Guid:
                    text = ((Guid)value).ToString("N");
                    return true;
            }
            text = "";
            return false;
        }

        public static bool ToString(object value, out string text)
        {
            if (value is DateTime)
            {
                return ToString(CSharpType.DateTime, value, out text);
            }
            if (value is DateTimeOffset)
            {
                return ToString(CSharpType.DateTimeOffset, value, out text);
            }
            if (value is decimal)
            {
                return ToString(CSharpType.Decimal, value, out text);
            }
            if (value is Guid)
            {
                return ToString(CSharpType.Guid, value, out text);
            }
            text = "";
            return false;
        }

        public static bool TryParse(CSharpType type, string text, out object value)
        {
            value = null;
            if (text == null) return false;
            var culture = CultureInfo.InvariantCulture;
            switch (type)
            {
                case CSharpType.DateTime:
                    DateTime date;
                    if (DateTime.TryParse(text, culture, DateTimeStyles.AssumeUniversal, out date))
                    {
                        value = date;
                        return true;
                    }
                    break;
                case CSharpType.DateTimeOffset:
                    DateTimeOffset dateTimeOffset;
                    if (DateTimeOffset.TryParse(text, culture, DateTimeStyles.AssumeUniversal, out dateTimeOffset))
                    {
                        value = dateTimeOffset;
                        return true;
                    }
                    break;
                case CSharpType.Decimal:
                    decimal dec;
                    if (Decimal.TryParse(text, NumberStyles.Any, culture, out dec))
                    {
                        value = dec;
                        return true;
                    }
                    break;
                case CSharpType.Guid:
                    try
                    {
                        value = new Guid(text);
                        return true;
                    }
                    catch (Exception) { }
                    break;
            }
            return false;
        }

        public static bool TryParse(CSharpType type, string text, IBuilderLite builder)
        {
            object value;
            if (!TryParse(type, text, out value))
            {
                return false;
            }
            switch (type)
            {
                case CSharpType.DateTime:
                    ((CSharp.DateTime.Builder)builder).SetTicks(((DateTime)value).Ticks);
                    return true;
                case CSharpType.DateTimeOffset:
                    var dtoValue = (DateTimeOffset)value;
                    var dtoBuilder = (CSharp.DateTimeOffset.Builder)builder;
                    dtoBuilder.SetTicks(dtoValue.Ticks);
                    dtoBuilder.SetOffsetTicks(dtoValue.Offset.Ticks);
                    return true;
                case CSharpType.Decimal:
                    int[] bits = Decimal.GetBits((decimal)value);
                    var decimalBuilder = (CSharp.Decimal.Builder)builder;
                    decimalBuilder.SetI0(bits[0]);
                    decimalBuilder.SetI1(bits[1]);
                    decimalBuilder.SetI2(bits[2]);
                    decimalBuilder.SetI3(bits[3]);
                    return true;
                case CSharpType.Guid:
                    var bytes = ByteString.Unsafe.FromBytes(((Guid)value).ToByteArray());
                    ((CSharp.Guid.Builder)builder).SetBits(bytes);
                    return true;
            }
            return false;
        }
    }
}
