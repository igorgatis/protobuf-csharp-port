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
    public static partial class CSharpTypes
    {
        public static CSharp.CSharpType GetType(FieldDescriptor field)
        {
            return field.Options.GetExtension(CSharp.CSharpTypesProto.Type);
        }

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

        internal static bool ParseFieldValue(TextTokenizer tokenizer, FieldDescriptor field, out object value)
        {
            var type = GetCSharpType(field);
            if (type != CSharpType.NotSupported)
            {
                tokenizer.Consume(":");
                string text = tokenizer.ConsumeString();
                return TryParse(type, text, out value);
            }
            value = null;
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
