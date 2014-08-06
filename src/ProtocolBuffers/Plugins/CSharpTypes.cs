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

    public class CSharpTypes
    {
        public static CSharpType GetCSharpType(FieldDescriptor field)
        {
            if (field.MappedType == MappedType.Message)
            {
                string name = field.MessageType.FullName;
                switch (name)
                {
                    case "csharp.DateTime":
                        return CSharpType.DateTime;
                    case "csharp.DateTimeOffset":
                        return CSharpType.DateTimeOffset;
                    case "csharp.Decimal":
                        return CSharpType.Decimal;
                    case "csharp.Guid":
                        return CSharpType.Guid;
                }
            }
            return CSharpType.NotSupported;
        }

        internal static IFieldAccessor<TMessage, TBuilder> CreateAccessor<TMessage, TBuilder>(FieldDescriptor field, string name)
            where TMessage : IMessage<TMessage, TBuilder>
            where TBuilder : IBuilder<TMessage, TBuilder>
        {
            if (GetCSharpType(field) != CSharpType.NotSupported)
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

        public static bool PrintFieldValue(FieldDescriptor field, object value, TextGenerator generator)
        {
            var type = GetCSharpType(field);
            string text;
            if (ToString(type, value, out text))
            {
                generator.Print(string.Format(": \"{0}\"\n", text));
                return true;
            }
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

        internal static bool ParseFieldValue(TextTokenizer tokenizer, FieldDescriptor field, out object value)
        {
            var culture = CultureInfo.InvariantCulture;
            string text;
            switch (GetCSharpType(field))
            {
                case CSharpType.DateTime:
                    tokenizer.Consume(":");
                    text = tokenizer.ConsumeString();
                    var style = DateTimeStyles.AssumeUniversal;
                    DateTime date;
                    if (!DateTime.TryParse(text, culture, style, out date))
                    {
                        throw tokenizer.CreateFormatExceptionPreviousToken("Invalid DateTime: '" + text + "'.");
                    }
                    value = date;
                    return true;
                case CSharpType.DateTimeOffset:
                    tokenizer.Consume(":");
                    text = tokenizer.ConsumeString();
                    DateTimeOffset dateTimeOffset;
                    if (!DateTimeOffset.TryParse(text, culture, DateTimeStyles.AssumeUniversal, out dateTimeOffset))
                    {
                        throw tokenizer.CreateFormatExceptionPreviousToken("Invalid DateTimeOffset: '" + text + "'.");
                    }
                    value = dateTimeOffset;
                    return true;
                case CSharpType.Decimal:
                    tokenizer.Consume(":");
                    text = tokenizer.ConsumeString();
                    decimal dec;
                    if (!Decimal.TryParse(text, NumberStyles.Any, culture, out dec))
                    {
                        throw tokenizer.CreateFormatExceptionPreviousToken("Invalid Decimal: '" + text + "'.");
                    }
                    value = dec;
                    return true;
                case CSharpType.Guid:
                    tokenizer.Consume(":");
                    text = tokenizer.ConsumeString();
                    try
                    {
                        value = new Guid(text);
                    }
                    catch (Exception)
                    {
                        throw tokenizer.CreateFormatExceptionPreviousToken("Invalid Guid: '" + text + "'.");
                    }
                    return true;
            }
            value = null;
            return false;
        }
    }
}
