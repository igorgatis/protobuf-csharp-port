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
using Google.ProtocolBuffers.Plugins;
using System;

namespace Google.ProtocolBuffers.ProtoGen.Plugins
{
    internal class CSharpFieldGenerator : FieldGeneratorBase, IFieldSourceGenerator
    {
        public static IFieldSourceGenerator CreateFieldGenerator(FieldDescriptor field, int fieldOrdinal)
        {
            if (CSharpTypes.GetCSharpType(field) != CSharpType.NotSupported)
            {
                if (field.IsRepeated)
                {
                    return new RepeatedCSharpFieldGenerator(field, fieldOrdinal);
                }
                else
                {
                    return new CSharpFieldGenerator(field, fieldOrdinal);
                }
            }
            return null;
        }

        internal CSharpFieldGenerator(FieldDescriptor descriptor, int fieldOrdinal)
            : base(descriptor, fieldOrdinal) { }

        protected string ExposedTypeName
        {
            get
            {
                switch (CSharpTypes.GetCSharpType(Descriptor))
                {
                    case CSharpType.DateTime:
                        return typeof(DateTime).FullName;
                    case CSharpType.DateTimeOffset:
                        return typeof(DateTimeOffset).FullName;
                    case CSharpType.Decimal:
                        return "decimal";
                    case CSharpType.Guid:
                        return typeof(Guid).FullName;
                }
                return "NULL";  // Should never happen.
            }
        }

        public virtual void GenerateMembers(TextGenerator writer)
        {
            writer.WriteLine("private bool has{0};", PropertyName);
            writer.WriteLine("private {0} {1}_;", ExposedTypeName, Name);
            AddDeprecatedFlag(writer);
            writer.WriteLine("public bool Has{0} {{", PropertyName);
            writer.WriteLine("  get {{ return has{0}; }}", PropertyName);
            writer.WriteLine("}");
            AddDeprecatedFlag(writer);
            writer.WriteLine("public {0} {1} {{", ExposedTypeName, PropertyName);
            writer.WriteLine("  get {{ return {0}_; }}", Name);
            writer.WriteLine("}");
            writer.WriteLine("public {0} Get{1}Proto() {{", TypeName, PropertyName);
            writer.WriteLine("  {0}.Builder proto = {0}.CreateBuilder();", TypeName);
            switch (CSharpTypes.GetCSharpType(Descriptor))
            {
                case CSharpType.DateTime:
                    writer.WriteLine("  proto.SetTicks({0}_.Ticks);", Name);
                    break;
                case CSharpType.DateTimeOffset:
                    writer.WriteLine("  proto.SetTicks({0}_.Ticks);", Name);
                    writer.WriteLine("  proto.SetOffsetTicks({0}_.Offset.Ticks);", Name);
                    break;
                case CSharpType.Decimal:
                    writer.WriteLine("  var bits = {0}.GetBits({1}_);", ExposedTypeName, Name);
                    writer.WriteLine("  proto.SetI0(bits[0]).SetI1(bits[1]).SetI2(bits[2]).SetI3(bits[3]);");
                    break;
                case CSharpType.Guid:
                    writer.WriteLine("  var bytes = {0}_.ToByteArray();", Name);
                    writer.WriteLine("  proto.SetBits(pb::ByteString.Unsafe.FromBytes(bytes));");
                    break;
            }
            writer.WriteLine("  return proto.BuildPartial();");
            writer.WriteLine("}");
        }

        public virtual void GenerateBuilderMembers(TextGenerator writer)
        {
            var type = CSharpTypes.GetCSharpType(Descriptor);
            AddDeprecatedFlag(writer);
            writer.WriteLine("public bool Has{0} {{", PropertyName);
            writer.WriteLine(" get {{ return result.has{0}; }}", PropertyName);
            writer.WriteLine("}");
            AddDeprecatedFlag(writer);
            writer.WriteLine("public {0} {1} {{", ExposedTypeName, PropertyName);
            writer.WriteLine("  get {{ return result.{0}; }}", PropertyName);
            writer.WriteLine("  set {{ Set{0}(value); }}", PropertyName);
            writer.WriteLine("}");
            AddDeprecatedFlag(writer);
            writer.WriteLine("public Builder Set{0}({1} value) {{", PropertyName, ExposedTypeName);
            writer.WriteLine("  PrepareBuilder();");
            writer.WriteLine("  result.has{0} = true;", PropertyName);
            if (type == CSharpType.DateTime)
            {
                writer.WriteLine("  result.{0}_ = System.DateTime.SpecifyKind(value, System.DateTimeKind.Utc);", Name);
            }
            else
            {
                writer.WriteLine("  result.{0}_ = value;", Name);
            }
            writer.WriteLine("  return this;");
            writer.WriteLine("}");

            AddDeprecatedFlag(writer);
            writer.WriteLine("public Builder Set{0}Proto({1} proto) {{", PropertyName, TypeName);
            switch (type)
            {
                case CSharpType.DateTime:
                    writer.WriteLine("  Set{0}(new {1}(proto.Ticks));", PropertyName, ExposedTypeName);
                    break;
                case CSharpType.DateTimeOffset:
                    writer.WriteLine("  Set{0}(new {1}(proto.Ticks, {2}));", PropertyName, ExposedTypeName,
                        "new System.TimeSpan(proto.OffsetTicks)");
                    break;
                case CSharpType.Decimal:
                    writer.WriteLine("  Set{0}(new {1}({2}));", PropertyName, ExposedTypeName,
                        "new int[] {proto.I0, proto.I1, proto.I2, proto.I3}");
                    break;
                case CSharpType.Guid:
                    writer.WriteLine("  Set{0}(new {1}({2}));", PropertyName, ExposedTypeName,
                        "pb::ByteString.Unsafe.GetBuffer(proto.Bits)");
                    break;
            }
            writer.WriteLine("  return this;");
            writer.WriteLine("}");

            AddDeprecatedFlag(writer);
            writer.WriteLine("public Builder Clear{0}() {{", PropertyName);
            writer.WriteLine("  PrepareBuilder();");
            writer.WriteLine("  result.has{0} = false;", PropertyName);
            writer.WriteLine("  result.{0}_ = default({1});", Name, ExposedTypeName);
            writer.WriteLine("  return this;");
            writer.WriteLine("}");
        }

        public virtual void GenerateMergingCode(TextGenerator writer)
        {
            writer.WriteLine("if (other.Has{0}) {{", PropertyName);
            writer.WriteLine("  {0} = other.{0};", PropertyName);
            writer.WriteLine("}");
        }

        public virtual void GenerateBuildingCode(TextGenerator writer)
        {
            // Nothing to do for singular fields
        }

        public virtual void GenerateParsingCode(TextGenerator writer)
        {
            writer.WriteLine("{0}.Builder subBuilder = {0}.CreateBuilder();", TypeName);
            writer.WriteLine("input.ReadMessage(subBuilder, extensionRegistry);");
            writer.WriteLine("Set{0}Proto(subBuilder.BuildPartial());", PropertyName);
        }

        public virtual void GenerateSerializationCode(TextGenerator writer)
        {
            writer.WriteLine("if (has{0}) {{", PropertyName);
            writer.WriteLine("  output.WriteMessage({0}, field_names[{2}], Get{1}Proto());", Number, PropertyName,
                             FieldOrdinal);
            writer.WriteLine("}");
        }

        public virtual void GenerateSerializedSizeCode(TextGenerator writer)
        {
            writer.WriteLine("if (has{0}) {{", PropertyName);
            writer.WriteLine("  size += pb::CodedOutputStream.ComputeMessageSize({0}, Get{1}Proto());",
                             Number, PropertyName);
            writer.WriteLine("}");
        }

        public override void WriteHash(TextGenerator writer)
        {
            writer.WriteLine("if (has{0}) hash ^= {1}_.GetHashCode();", PropertyName, Name);
        }

        public override void WriteEquals(TextGenerator writer)
        {
            writer.WriteLine("if (has{0} != other.has{0} || (has{0} && !{1}_.Equals(other.{1}_))) return false;",
                             PropertyName, Name);
        }

        public override void WriteToString(TextGenerator writer)
        {
            writer.WriteLine("PrintField(\"{0}\", has{1}, {2}_, writer);", Descriptor.Name, PropertyName, Name);
        }
    }
}
