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

using Google.ProtocolBuffers.CSharp;
using Google.ProtocolBuffers.Descriptors;

namespace Google.ProtocolBuffers.ProtoGen.Plugins
{
    internal class RepeatedCSharpFieldGenerator : CSharpFieldGenerator, IFieldSourceGenerator
    {
        internal RepeatedCSharpFieldGenerator(FieldDescriptor descriptor, int fieldOrdinal)
            : base(descriptor, fieldOrdinal) { }

        public override void GenerateMembers(TextGenerator writer)
        {
            writer.WriteLine("private pbc::PopsicleList<{0}> {1}_ = new pbc::PopsicleList<{0}>();", ExposedTypeName, Name);
            AddPublicMemberAttributes(writer);
            writer.WriteLine("public scg::IList<{0}> {1}List {{", ExposedTypeName, PropertyName);
            writer.WriteLine("  get {{ return pbc::Lists.AsReadOnly({0}_); }}", Name);
            writer.WriteLine("}");

            // TODO(jonskeet): Redundant API calls? Possibly - include for portability though. Maybe create an option.
            AddDeprecatedFlag(writer);
            writer.WriteLine("public int {0}Count {{", PropertyName);
            writer.WriteLine("  get {{ return {0}_.Count; }}", Name);
            writer.WriteLine("}");

            AddPublicMemberAttributes(writer);
            writer.WriteLine("public {0} Get{1}(int index) {{", ExposedTypeName, PropertyName);
            writer.WriteLine("  return {0}_[index];", Name);
            writer.WriteLine("}");

            if (Descriptor.FieldType == FieldType.Message)
            {
                AddPublicMemberAttributes(writer);
                writer.WriteLine("public {0} Get{1}Proto(int index) {{", TypeName, PropertyName);
                writer.WriteLine("  {0}.Builder proto = {0}.CreateBuilder();", TypeName);
                switch (Descriptor.GetCsType())
                {
                    case CSharpType.kDateTime:
                        writer.WriteLine("  proto.SetTicks({0}_[index].Ticks);", Name);
                        break;
                    case CSharpType.kDateTimeOffset:
                        writer.WriteLine("  var item = {0}_[index];", Name);
                        writer.WriteLine("  proto.SetTicks(item.Ticks);", Name);
                        writer.WriteLine("  proto.SetOffsetTicks(item.Offset.Ticks);", Name);
                        break;
                    case CSharpType.kDecimal:
                        writer.WriteLine("  var bits = {0}.GetBits({1}_[index]);", ExposedTypeName, Name);
                        writer.WriteLine("  proto.SetI0(bits[0]).SetI1(bits[1]).SetI2(bits[2]).SetI3(bits[3]);");
                        break;
                    case CSharpType.kGuid:
                        writer.WriteLine("  var bytes = {0}_[index].ToByteArray();", Name);
                        writer.WriteLine("  proto.SetBits(pb::ByteString.Unsafe.FromBytes(bytes));");
                        break;
                }
                writer.WriteLine("  return proto.BuildPartial();");
                writer.WriteLine("}");
            }
        }

        public override void GenerateBuilderMembers(TextGenerator writer)
        {
            // Note:  We can return the original list here, because we make it unmodifiable when we build
            // We return it via IPopsicleList so that collection initializers work more pleasantly.
            AddPublicMemberAttributes(writer);
            writer.WriteLine("public pbc::IPopsicleList<{0}> {1}List {{", ExposedTypeName, PropertyName);
            writer.WriteLine("  get {{ return PrepareBuilder().{0}_; }}", Name);
            writer.WriteLine("}");
            AddDeprecatedFlag(writer);
            writer.WriteLine("public int {0}Count {{", PropertyName);
            writer.WriteLine("  get {{ return result.{0}Count; }}", PropertyName);
            writer.WriteLine("}");
            AddPublicMemberAttributes(writer);
            writer.WriteLine("public {0} Get{1}(int index) {{", ExposedTypeName, PropertyName);
            writer.WriteLine("  return result.Get{0}(index);", PropertyName);
            writer.WriteLine("}");
            AddPublicMemberAttributes(writer);
            writer.WriteLine("public Builder Set{0}(int index, {1} value) {{", PropertyName, ExposedTypeName);
            AddNullCheck(writer);
            writer.WriteLine("  PrepareBuilder();");
            writer.WriteLine("  result.{0}_[index] = value;", Name);
            writer.WriteLine("  return this;");
            writer.WriteLine("}");
            AddPublicMemberAttributes(writer);
            writer.WriteLine("public Builder Add{0}({1} value) {{", PropertyName, ExposedTypeName);
            AddNullCheck(writer);
            writer.WriteLine("  PrepareBuilder();");
            writer.WriteLine("  result.{0}_.Add(value);", Name, TypeName);
            writer.WriteLine("  return this;");
            writer.WriteLine("}");
            if (Descriptor.FieldType == FieldType.Message)
            {
                AddPublicMemberAttributes(writer);
                writer.WriteLine("public Builder Add{0}Proto({1} value) {{", PropertyName, TypeName);
                AddNullCheck(writer);
                switch (Descriptor.GetCsType())
                {
                    case CSharpType.kDateTime:
                        writer.WriteLine("  Add{0}(new {1}(value.Ticks));", PropertyName, ExposedTypeName);
                        break;
                    case CSharpType.kDateTimeOffset:
                        writer.WriteLine("  Add{0}(new {1}(value.Ticks, {2}));", PropertyName, ExposedTypeName,
                            "new System.TimeSpan(value.OffsetTicks)");
                        break;
                    case CSharpType.kDecimal:
                        writer.WriteLine("  Add{0}(new {1}({2}));", PropertyName, ExposedTypeName,
                            "new int[] {value.I0, value.I1, value.I2, value.I3}");
                        break;
                    case CSharpType.kGuid:
                        writer.WriteLine("  Add{0}(new {1}({2}));", PropertyName, ExposedTypeName,
                            "pb::ByteString.Unsafe.GetBuffer(value.Bits)");
                        break;
                }
                writer.WriteLine("  return this;");
                writer.WriteLine("}");
            }
            AddPublicMemberAttributes(writer);
            writer.WriteLine("public Builder AddRange{0}(scg::IEnumerable<{1}> values) {{", PropertyName, ExposedTypeName);
            writer.WriteLine("  PrepareBuilder();");
            writer.WriteLine("  result.{0}_.Add(values);", Name);
            writer.WriteLine("  return this;");
            writer.WriteLine("}");
            AddDeprecatedFlag(writer);
            writer.WriteLine("public Builder Clear{0}() {{", PropertyName);
            writer.WriteLine("  PrepareBuilder();");
            writer.WriteLine("  result.{0}_.Clear();", Name);
            writer.WriteLine("  return this;");
            writer.WriteLine("}");
        }

        public override void GenerateMergingCode(TextGenerator writer)
        {
            writer.WriteLine("if (other.{0}_.Count != 0) {{", Name);
            writer.WriteLine("  result.{0}_.Add(other.{0}_);", Name);
            writer.WriteLine("}");
        }

        public override void GenerateBuildingCode(TextGenerator writer)
        {
            writer.WriteLine("{0}_.MakeReadOnly();", Name);
        }

        public override void GenerateParsingCode(TextGenerator writer)
        {
            writer.WriteLine("var list = new System.Collections.Generic.List<{0}>();", TypeName);
            if (Descriptor.FieldType == FieldType.Message)
            {
                writer.WriteLine("input.Read{0}Array(tag, field_name, list, {1}.DefaultInstance, extensionRegistry);",
                    CapitalizedTypeName, TypeName);
                writer.WriteLine("foreach (var item in list) {");
                writer.WriteLine("  Add{0}Proto(item);", PropertyName);
                writer.WriteLine("}");
            }
            else
            {
                writer.WriteLine("input.ReadStringArray(tag, field_name, list);");
                writer.WriteLine("foreach (var text in list) {");
                writer.WriteLine("  var value = default({0});", ExposedTypeName);
                writer.WriteLine("  if (cs.CSharpTypes.TryParse(text, ref value)) {");
                writer.WriteLine("    Add{0}(value);", PropertyName);
                writer.WriteLine("  }");
                writer.WriteLine("}");
            }
        }

        public override void GenerateSerializationCode(TextGenerator writer)
        {
            writer.WriteLine("if ({0}_.Count > 0) {{", Name);
            writer.Indent();
            writer.WriteLine("for (int i = 0; i < {0}_.Count; ++i) {{", Name);
            if (Descriptor.FieldType == FieldType.Message)
            {
                writer.WriteLine("  var element = Get{0}Proto(i);", PropertyName);
                writer.WriteLine("  output.WriteMessage({0}, field_names[{1}], element);", Number, FieldOrdinal);
            }
            else
            {
                writer.WriteLine("  string text = cs.CSharpTypes.ToString(this.{0}_[i]);", Name);
                writer.WriteLine("  output.WriteString({0}, field_names[{1}], text);", Number, FieldOrdinal);
            }
            writer.WriteLine("}");
            writer.Outdent();
            writer.WriteLine("}");
        }

        public override void GenerateSerializedSizeCode(TextGenerator writer)
        {
            writer.WriteLine("{");
            writer.Indent();
            writer.WriteLine("int dataSize = 0;");
            writer.WriteLine("for (int i = 0; i < {0}_.Count; ++i) {{", Name);
            if (Descriptor.FieldType == FieldType.Message)
            {
                writer.WriteLine("  var element = Get{0}Proto(i);", PropertyName);
                writer.WriteLine("  dataSize += pb::CodedOutputStream.ComputeMessageSize({0}, element);", Number);
            }
            else
            {
                writer.WriteLine("  string text = cs.CSharpTypes.ToString(this.{0}_[i]);", Name);
                writer.WriteLine("  dataSize += pb::CodedOutputStream.ComputeStringSize({0}, text);", Number);
            }
            writer.WriteLine("}");
            writer.WriteLine("size += dataSize;");
            int tagSize = CodedOutputStream.ComputeTagSize(Descriptor.FieldNumber);
            // cache the data size for packed fields.
            writer.Outdent();
            writer.WriteLine("}");
        }

        public override void WriteHash(TextGenerator writer)
        {
            writer.WriteLine("foreach({0} i in {1}_)", ExposedTypeName, Name);
            writer.WriteLine("  hash ^= i.GetHashCode();");
        }

        public override void WriteEquals(TextGenerator writer)
        {
            writer.WriteLine("if({0}_.Count != other.{0}_.Count) return false;", Name);
            writer.WriteLine("for(int ix=0; ix < {0}_.Count; ix++)", Name);
            writer.WriteLine("  if(!{0}_[ix].Equals(other.{0}_[ix])) return false;", Name);
        }

        public override void WriteToString(TextGenerator writer)
        {
            if (Descriptor.FieldType == FieldType.Message)
            {
                writer.WriteLine("PrintField(\"{0}\", {1}_, writer);", Descriptor.Name, Name);
            }
            else
            {
                writer.WriteLine("  string text = cs.CSharpTypes.ToString(this.{0}_[i]);", Name);
                writer.WriteLine("PrintField(\"{0}\", text, writer);", Descriptor.Name);
            }
        }
    }
}