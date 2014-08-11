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
using Google.ProtocolBuffers.FieldAccess;
using System.Globalization;

namespace Google.ProtocolBuffers.CSharp
{
    public static partial class CSharpTypes
    {
        public static bool Inactive;

        public static CSharpType GetCsType(this FieldDescriptor field)
        {
            if (field.MappedType == MappedType.String)
            {
                return field.Options.GetExtension(CSharpTypesProto.Type);
            }
            if (field.MappedType == MappedType.Message)
            {
                switch (field.MessageType.FullName)
                {
                    case "cs.DateTime":
                        return CSharpType.kDateTime;
                    case "cs.DateTimeOffset":
                        return CSharpType.kDateTimeOffset;
                    case "cs.Decimal":
                        return CSharpType.kDecimal;
                    case "cs.Guid":
                        return CSharpType.kGuid;
                }
            }
            return CSharpType.kNone;
        }

        internal static IFieldAccessor<TMessage, TBuilder> CreateAccessor<TMessage, TBuilder>(FieldDescriptor field, string name)
            where TMessage : IMessage<TMessage, TBuilder>
            where TBuilder : IBuilder<TMessage, TBuilder>
        {
            if (!Inactive && field.GetCsType() != CSharpType.kNone)
            {
                if (field.IsRepeated)
                {
                    return new RepeatedPrimitiveAccessor<TMessage, TBuilder>(name);
                }
                else
                {
                    return new SinglePrimitiveAccessor<TMessage, TBuilder>(name);
                }
            }
            return null;
        }

        public static string ToString(System.DateTime value)
        {
            return value.ToString("O");
        }

        public static string ToString(System.DateTimeOffset value)
        {
            return value.ToString("O");
        }

        public static string ToString(System.Decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToString(System.Guid value)
        {
            return value.ToString("N");
        }

        public static string ToString(CSharpType type, object value)
        {
            switch (type)
            {
                case CSharpType.kDateTime:
                    return ToString((System.DateTime)value);
                case CSharpType.kDateTimeOffset:
                    return ToString((System.DateTimeOffset)value);
                case CSharpType.kDecimal:
                    return ToString((System.Decimal)value);
                case CSharpType.kGuid:
                    return ToString((System.Guid)value);
                default:
                    throw new System.NotImplementedException();
            }
        }

        public static bool PrintFieldValue(FieldDescriptor field, object value, TextGenerator generator)
        {
            generator.Print(string.Format(": \"{0}\"\n", ToString(field.GetCsType(), value)));
            return true;
        }

        /*public static bool TryParseProto(CSharpType type, string text, IBuilderLite builder)
        {
            object value = null;
            if (!TryParse(type, text, ref value))
            {
                return false;
            }
            switch (type)
            {
                case CSharpType.kDateTime:
                    ((DateTime.Builder)builder).SetTicks(((DateTime)value).Ticks);
                    return true;
                case CSharpType.kDateTimeOffset:
                    var dtoValue = (System.DateTimeOffset)value;
                    var dtoBuilder = (DateTimeOffset.Builder)builder;
                    dtoBuilder.SetTicks(dtoValue.Ticks);
                    dtoBuilder.SetOffsetTicks(dtoValue.Offset.Ticks);
                    return true;
                case CSharpType.kDecimal:
                    int[] bits = System.Decimal.GetBits((decimal)value);
                    var decimalBuilder = (Decimal.Builder)builder;
                    decimalBuilder.SetI0(bits[0]);
                    decimalBuilder.SetI1(bits[1]);
                    decimalBuilder.SetI2(bits[2]);
                    decimalBuilder.SetI3(bits[3]);
                    return true;
                case CSharpType.kGuid:

                    var bytes = ((System.Guid)value).ToByteArray();
                    var byteString = ByteString.Unsafe.FromBytes(bytes);
                    ((Guid.Builder)builder).SetBits(byteString);
                    return true;
            }
            return false;
        }*/

        public static bool TryParse(string text, ref System.DateTime value)
        {
            System.DateTime date;
            if (System.DateTime.TryParse(
                    text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date))
            {
                value = date;
                return true;
            }
            return false;
        }

        public static bool TryParse(string text, ref System.DateTimeOffset value)
        {
            System.DateTimeOffset date;
            if (System.DateTimeOffset.TryParse(
                    text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date))
            {
                value = date;
                return true;
            }
            return false;
        }

        public static bool TryParse(string text, ref decimal value)
        {
            decimal dec;
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out dec))
            {
                value = dec;
                return true;
            }
            return false;
        }

        public static bool TryParse(string text, ref System.Guid value)
        {
            try
            {
                value = new System.Guid(text);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static object ParseImpl(CSharpType type, string text)
        {
            switch (type)
            {
                case CSharpType.kDateTime:
                    var date = default(System.DateTime);
                    if (TryParse(text, ref date)) return date;
                    break;
                case CSharpType.kDateTimeOffset:
                    var dto = default(System.DateTimeOffset);
                    if (TryParse(text, ref dto)) return dto;
                    break;
                case CSharpType.kDecimal:
                    var dec = default(System.Decimal);
                    if (TryParse(text, ref dec)) return dec;
                    break;
                case CSharpType.kGuid:
                    var guid = default(System.Guid);
                    if (TryParse(text, ref guid)) return guid;
                    break;
                default:
                    throw new System.NotImplementedException();
            }
            return null;
        }

        public static object ParseOrThrow(CSharpType type, string text)
        {
            object value = ParseImpl(type, text);
            if (value == null)
            {
                throw new System.ArgumentException(text);
            }
            return value;
        }

        public static bool TryParse(CSharpType type, string text, ref object value)
        {
            object output = ParseImpl(type, text);
            if (output != null)
            {
                value = output;
                return true;
            }
            return false;
        }

        internal static bool ParseFieldValue(TextTokenizer tokenizer, FieldDescriptor field, ref object value)
        {
            var type = field.GetCsType();
            if (type != CSharpType.kNone)
            {
                tokenizer.Consume(":");
                string text = tokenizer.ConsumeString();
                return TryParse(type, text, ref value);
            }
            return false;
        }

    }
}
