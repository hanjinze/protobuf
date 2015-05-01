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

using System;
using Google.ProtocolBuffers.Descriptors;
using Google.ProtocolBuffers.TestProtos;
using Xunit;

#pragma warning disable 618 // Disable warning about obsolete use miss-matched assert arguments

namespace Google.ProtocolBuffers
{
    /// <summary>
    /// Performs the same things that the methods of TestUtil do, but
    /// via the reflection interface.  This is its own class because it needs
    /// to know what descriptor to use.
    /// </summary>
    internal class ReflectionTester
    {
        private readonly MessageDescriptor baseDescriptor;
        private readonly ExtensionRegistry extensionRegistry;

        private readonly FileDescriptor file;
        private readonly FileDescriptor importFile;

        private readonly MessageDescriptor optionalGroup;
        private readonly MessageDescriptor repeatedGroup;
        private readonly MessageDescriptor nestedMessage;
        private readonly MessageDescriptor foreignMessage;
        private readonly MessageDescriptor importMessage;

        private readonly FieldDescriptor groupA;
        private readonly FieldDescriptor repeatedGroupA;
        private readonly FieldDescriptor nestedB;
        private readonly FieldDescriptor foreignC;
        private readonly FieldDescriptor importD;

        private readonly EnumDescriptor nestedEnum;
        private readonly EnumDescriptor foreignEnum;
        private readonly EnumDescriptor importEnum;

        private readonly EnumValueDescriptor nestedFoo;
        private readonly EnumValueDescriptor nestedBar;
        private readonly EnumValueDescriptor nestedBaz;
        private readonly EnumValueDescriptor foreignFoo;
        private readonly EnumValueDescriptor foreignBar;
        private readonly EnumValueDescriptor foreignBaz;
        private readonly EnumValueDescriptor importFoo;
        private readonly EnumValueDescriptor importBar;
        private readonly EnumValueDescriptor importBaz;

        /// <summary>
        /// Constructs an instance that will expect messages using the given
        /// descriptor. Normally <paramref name="baseDescriptor"/> should be
        /// a descriptor for TestAllTypes. However, if extensionRegistry is non-null,
        /// then baseDescriptor should be for TestAllExtensions instead, and instead of
        /// reading and writing normal fields, the tester will read and write extensions.
        /// All of the TestAllExtensions extensions must be registered in the registry.
        /// </summary>
        private ReflectionTester(MessageDescriptor baseDescriptor,
                                 ExtensionRegistry extensionRegistry)
        {
            this.baseDescriptor = baseDescriptor;
            this.extensionRegistry = extensionRegistry;

            this.file = baseDescriptor.File;
            Assert.Equal(1, file.Dependencies.Count);
            this.importFile = file.Dependencies[0];

            MessageDescriptor testAllTypes;
            if (baseDescriptor.Name == "TestAllTypes")
            {
                testAllTypes = baseDescriptor;
            }
            else
            {
                testAllTypes = file.FindTypeByName<MessageDescriptor>("TestAllTypes");
                Assert.NotNull(testAllTypes);
            }

            if (extensionRegistry == null)
            {
                // Use testAllTypes, rather than baseDescriptor, to allow
                // initialization using TestPackedTypes descriptors. These objects
                // won't be used by the methods for packed fields.
                this.optionalGroup =
                    testAllTypes.FindDescriptor<MessageDescriptor>("OptionalGroup");
                this.repeatedGroup =
                    testAllTypes.FindDescriptor<MessageDescriptor>("RepeatedGroup");
            }
            else
            {
                this.optionalGroup =
                    file.FindTypeByName<MessageDescriptor>("OptionalGroup_extension");
                this.repeatedGroup =
                    file.FindTypeByName<MessageDescriptor>("RepeatedGroup_extension");
            }
            this.nestedMessage = testAllTypes.FindDescriptor<MessageDescriptor>("NestedMessage");
            this.foreignMessage = file.FindTypeByName<MessageDescriptor>("ForeignMessage");
            this.importMessage = importFile.FindTypeByName<MessageDescriptor>("ImportMessage");

            this.nestedEnum = testAllTypes.FindDescriptor<EnumDescriptor>("NestedEnum");
            this.foreignEnum = file.FindTypeByName<EnumDescriptor>("ForeignEnum");
            this.importEnum = importFile.FindTypeByName<EnumDescriptor>("ImportEnum");

            Assert.NotNull(optionalGroup);
            Assert.NotNull(repeatedGroup);
            Assert.NotNull(nestedMessage);
            Assert.NotNull(foreignMessage);
            Assert.NotNull(importMessage);
            Assert.NotNull(nestedEnum);
            Assert.NotNull(foreignEnum);
            Assert.NotNull(importEnum);

            this.nestedB = nestedMessage.FindDescriptor<FieldDescriptor>("bb");
            this.foreignC = foreignMessage.FindDescriptor<FieldDescriptor>("c");
            this.importD = importMessage.FindDescriptor<FieldDescriptor>("d");
            this.nestedFoo = nestedEnum.FindValueByName("FOO");
            this.nestedBar = nestedEnum.FindValueByName("BAR");
            this.nestedBaz = nestedEnum.FindValueByName("BAZ");
            this.foreignFoo = foreignEnum.FindValueByName("FOREIGN_FOO");
            this.foreignBar = foreignEnum.FindValueByName("FOREIGN_BAR");
            this.foreignBaz = foreignEnum.FindValueByName("FOREIGN_BAZ");
            this.importFoo = importEnum.FindValueByName("IMPORT_FOO");
            this.importBar = importEnum.FindValueByName("IMPORT_BAR");
            this.importBaz = importEnum.FindValueByName("IMPORT_BAZ");

            this.groupA = optionalGroup.FindDescriptor<FieldDescriptor>("a");
            this.repeatedGroupA = repeatedGroup.FindDescriptor<FieldDescriptor>("a");

            Assert.NotNull(groupA);
            Assert.NotNull(repeatedGroupA);
            Assert.NotNull(nestedB);
            Assert.NotNull(foreignC);
            Assert.NotNull(importD);
            Assert.NotNull(nestedFoo);
            Assert.NotNull(nestedBar);
            Assert.NotNull(nestedBaz);
            Assert.NotNull(foreignFoo);
            Assert.NotNull(foreignBar);
            Assert.NotNull(foreignBaz);
            Assert.NotNull(importFoo);
            Assert.NotNull(importBar);
            Assert.NotNull(importBaz);
        }

        /// <summary>
        /// Creates an instance for the TestAllTypes message, with no extension registry.
        /// </summary>
        public static ReflectionTester CreateTestAllTypesInstance()
        {
            return new ReflectionTester(TestAllTypes.Descriptor, null);
        }

        /// <summary>
        /// Creates an instance for the TestAllExtensions message, with an
        /// extension registry from TestUtil.CreateExtensionRegistry.
        /// </summary>
        public static ReflectionTester CreateTestAllExtensionsInstance()
        {
            return new ReflectionTester(TestAllExtensions.Descriptor, TestUtil.CreateExtensionRegistry());
        }

        /// <summary>
        /// Creates an instance for the TestPackedTypes message, with no extensions.
        /// </summary>
        public static ReflectionTester CreateTestPackedTypesInstance()
        {
            return new ReflectionTester(TestPackedTypes.Descriptor, null);
        }

        /// <summary>
        /// Shorthand to get a FieldDescriptor for a field of unittest::TestAllTypes.
        /// </summary>
        private FieldDescriptor f(String name)
        {
            FieldDescriptor result;
            if (extensionRegistry == null)
            {
                result = baseDescriptor.FindDescriptor<FieldDescriptor>(name);
            }
            else
            {
                result = file.FindTypeByName<FieldDescriptor>(name + "_extension");
            }
            Assert.NotNull(result);
            return result;
        }

        /// <summary>
        /// Calls parent.CreateBuilderForField() or uses the extension registry
        /// to find an appropriate builder, depending on what type is being tested.
        /// </summary>
        private IBuilder CreateBuilderForField(IBuilder parent, FieldDescriptor field)
        {
            if (extensionRegistry == null)
            {
                return parent.CreateBuilderForField(field);
            }
            else
            {
                ExtensionInfo extension = extensionRegistry[field.ContainingType, field.FieldNumber];
                Assert.NotNull(extension);
                Assert.NotNull(extension.DefaultInstance);
                return (IBuilder) extension.DefaultInstance.WeakCreateBuilderForType();
            }
        }

        /// <summary>
        /// Sets every field of the message to the values expected by
        /// AssertAllFieldsSet, using the reflection interface.
        /// </summary>
        /// <param name="message"></param>
        internal void SetAllFieldsViaReflection(IBuilder message)
        {
            message[f("optional_int32")] = 101;
            message[f("optional_int64")] = 102L;
            message[f("optional_uint32")] = 103U;
            message[f("optional_uint64")] = 104UL;
            message[f("optional_sint32")] = 105;
            message[f("optional_sint64")] = 106L;
            message[f("optional_fixed32")] = 107U;
            message[f("optional_fixed64")] = 108UL;
            message[f("optional_sfixed32")] = 109;
            message[f("optional_sfixed64")] = 110L;
            message[f("optional_float")] = 111F;
            message[f("optional_double")] = 112D;
            message[f("optional_bool")] = true;
            message[f("optional_string")] = "115";
            message[f("optional_bytes")] = TestUtil.ToBytes("116");

            message[f("optionalgroup")] =
                CreateBuilderForField(message, f("optionalgroup")).SetField(groupA, 117).WeakBuild();
            message[f("optional_nested_message")] =
                CreateBuilderForField(message, f("optional_nested_message")).SetField(nestedB, 118).WeakBuild();
            message[f("optional_foreign_message")] =
                CreateBuilderForField(message, f("optional_foreign_message")).SetField(foreignC, 119).WeakBuild();
            message[f("optional_import_message")] =
                CreateBuilderForField(message, f("optional_import_message")).SetField(importD, 120).WeakBuild();

            message[f("optional_nested_enum")] = nestedBaz;
            message[f("optional_foreign_enum")] = foreignBaz;
            message[f("optional_import_enum")] = importBaz;

            message[f("optional_string_piece")] = "124";
            message[f("optional_cord")] = "125";

            // -----------------------------------------------------------------

            message.WeakAddRepeatedField(f("repeated_int32"), 201);
            message.WeakAddRepeatedField(f("repeated_int64"), 202L);
            message.WeakAddRepeatedField(f("repeated_uint32"), 203U);
            message.WeakAddRepeatedField(f("repeated_uint64"), 204UL);
            message.WeakAddRepeatedField(f("repeated_sint32"), 205);
            message.WeakAddRepeatedField(f("repeated_sint64"), 206L);
            message.WeakAddRepeatedField(f("repeated_fixed32"), 207U);
            message.WeakAddRepeatedField(f("repeated_fixed64"), 208UL);
            message.WeakAddRepeatedField(f("repeated_sfixed32"), 209);
            message.WeakAddRepeatedField(f("repeated_sfixed64"), 210L);
            message.WeakAddRepeatedField(f("repeated_float"), 211F);
            message.WeakAddRepeatedField(f("repeated_double"), 212D);
            message.WeakAddRepeatedField(f("repeated_bool"), true);
            message.WeakAddRepeatedField(f("repeated_string"), "215");
            message.WeakAddRepeatedField(f("repeated_bytes"), TestUtil.ToBytes("216"));


            message.WeakAddRepeatedField(f("repeatedgroup"),
                                         CreateBuilderForField(message, f("repeatedgroup")).SetField(repeatedGroupA, 217)
                                             .WeakBuild());
            message.WeakAddRepeatedField(f("repeated_nested_message"),
                                         CreateBuilderForField(message, f("repeated_nested_message")).SetField(nestedB,
                                                                                                               218).
                                             WeakBuild());
            message.WeakAddRepeatedField(f("repeated_foreign_message"),
                                         CreateBuilderForField(message, f("repeated_foreign_message")).SetField(
                                             foreignC, 219).WeakBuild());
            message.WeakAddRepeatedField(f("repeated_import_message"),
                                         CreateBuilderForField(message, f("repeated_import_message")).SetField(importD,
                                                                                                               220).
                                             WeakBuild());

            message.WeakAddRepeatedField(f("repeated_nested_enum"), nestedBar);
            message.WeakAddRepeatedField(f("repeated_foreign_enum"), foreignBar);
            message.WeakAddRepeatedField(f("repeated_import_enum"), importBar);

            message.WeakAddRepeatedField(f("repeated_string_piece"), "224");
            message.WeakAddRepeatedField(f("repeated_cord"), "225");

            // Add a second one of each field.
            message.WeakAddRepeatedField(f("repeated_int32"), 301);
            message.WeakAddRepeatedField(f("repeated_int64"), 302L);
            message.WeakAddRepeatedField(f("repeated_uint32"), 303U);
            message.WeakAddRepeatedField(f("repeated_uint64"), 304UL);
            message.WeakAddRepeatedField(f("repeated_sint32"), 305);
            message.WeakAddRepeatedField(f("repeated_sint64"), 306L);
            message.WeakAddRepeatedField(f("repeated_fixed32"), 307U);
            message.WeakAddRepeatedField(f("repeated_fixed64"), 308UL);
            message.WeakAddRepeatedField(f("repeated_sfixed32"), 309);
            message.WeakAddRepeatedField(f("repeated_sfixed64"), 310L);
            message.WeakAddRepeatedField(f("repeated_float"), 311F);
            message.WeakAddRepeatedField(f("repeated_double"), 312D);
            message.WeakAddRepeatedField(f("repeated_bool"), false);
            message.WeakAddRepeatedField(f("repeated_string"), "315");
            message.WeakAddRepeatedField(f("repeated_bytes"), TestUtil.ToBytes("316"));

            message.WeakAddRepeatedField(f("repeatedgroup"),
                                         CreateBuilderForField(message, f("repeatedgroup"))
                                             .SetField(repeatedGroupA, 317).WeakBuild());
            message.WeakAddRepeatedField(f("repeated_nested_message"),
                                         CreateBuilderForField(message, f("repeated_nested_message"))
                                             .SetField(nestedB, 318).WeakBuild());
            message.WeakAddRepeatedField(f("repeated_foreign_message"),
                                         CreateBuilderForField(message, f("repeated_foreign_message"))
                                             .SetField(foreignC, 319).WeakBuild());
            message.WeakAddRepeatedField(f("repeated_import_message"),
                                         CreateBuilderForField(message, f("repeated_import_message"))
                                             .SetField(importD, 320).WeakBuild());

            message.WeakAddRepeatedField(f("repeated_nested_enum"), nestedBaz);
            message.WeakAddRepeatedField(f("repeated_foreign_enum"), foreignBaz);
            message.WeakAddRepeatedField(f("repeated_import_enum"), importBaz);

            message.WeakAddRepeatedField(f("repeated_string_piece"), "324");
            message.WeakAddRepeatedField(f("repeated_cord"), "325");

            // -----------------------------------------------------------------

            message[f("default_int32")] = 401;
            message[f("default_int64")] = 402L;
            message[f("default_uint32")] = 403U;
            message[f("default_uint64")] = 404UL;
            message[f("default_sint32")] = 405;
            message[f("default_sint64")] = 406L;
            message[f("default_fixed32")] = 407U;
            message[f("default_fixed64")] = 408UL;
            message[f("default_sfixed32")] = 409;
            message[f("default_sfixed64")] = 410L;
            message[f("default_float")] = 411F;
            message[f("default_double")] = 412D;
            message[f("default_bool")] = false;
            message[f("default_string")] = "415";
            message[f("default_bytes")] = TestUtil.ToBytes("416");

            message[f("default_nested_enum")] = nestedFoo;
            message[f("default_foreign_enum")] = foreignFoo;
            message[f("default_import_enum")] = importFoo;

            message[f("default_string_piece")] = "424";
            message[f("default_cord")] = "425";
        }

        /// <summary>
        /// Clears every field of the message, using the reflection interface.
        /// </summary>
        /// <param name="message"></param>
        internal void ClearAllFieldsViaReflection(IBuilder message)
        {
            foreach (FieldDescriptor field in message.AllFields.Keys)
            {
                message.WeakClearField(field);
            }
        }

        // -------------------------------------------------------------------

        /// <summary>
        /// Modify the repeated fields of the specified message to contain the
        /// values expected by AssertRepeatedFieldsModified, using the IBuilder
        /// reflection interface.
        /// </summary>
        internal void ModifyRepeatedFieldsViaReflection(IBuilder message)
        {
            message[f("repeated_int32"), 1] = 501;
            message[f("repeated_int64"), 1] = 502L;
            message[f("repeated_uint32"), 1] = 503U;
            message[f("repeated_uint64"), 1] = 504UL;
            message[f("repeated_sint32"), 1] = 505;
            message[f("repeated_sint64"), 1] = 506L;
            message[f("repeated_fixed32"), 1] = 507U;
            message[f("repeated_fixed64"), 1] = 508UL;
            message[f("repeated_sfixed32"), 1] = 509;
            message[f("repeated_sfixed64"), 1] = 510L;
            message[f("repeated_float"), 1] = 511F;
            message[f("repeated_double"), 1] = 512D;
            message[f("repeated_bool"), 1] = true;
            message[f("repeated_string"), 1] = "515";
            message.SetRepeatedField(f("repeated_bytes"), 1, TestUtil.ToBytes("516"));

            message.SetRepeatedField(f("repeatedgroup"), 1,
                                     CreateBuilderForField(message, f("repeatedgroup")).SetField(repeatedGroupA, 517).
                                         WeakBuild());
            message.SetRepeatedField(f("repeated_nested_message"), 1,
                                     CreateBuilderForField(message, f("repeated_nested_message")).SetField(nestedB, 518)
                                         .WeakBuild());
            message.SetRepeatedField(f("repeated_foreign_message"), 1,
                                     CreateBuilderForField(message, f("repeated_foreign_message")).SetField(foreignC,
                                                                                                            519).
                                         WeakBuild());
            message.SetRepeatedField(f("repeated_import_message"), 1,
                                     CreateBuilderForField(message, f("repeated_import_message")).SetField(importD, 520)
                                         .WeakBuild());

            message[f("repeated_nested_enum"), 1] = nestedFoo;
            message[f("repeated_foreign_enum"), 1] = foreignFoo;
            message[f("repeated_import_enum"), 1] = importFoo;

            message[f("repeated_string_piece"), 1] = "524";
            message[f("repeated_cord"), 1] = "525";
        }

        // -------------------------------------------------------------------

        /// <summary>
        /// Asserts that all fields of the specified message are set to the values
        /// assigned by SetAllFields, using the IMessage reflection interface.
        /// </summary>
        public void AssertAllFieldsSetViaReflection(IMessage message)
        {
            Assert.True(message.HasField(f("optional_int32")));
            Assert.True(message.HasField(f("optional_int64")));
            Assert.True(message.HasField(f("optional_uint32")));
            Assert.True(message.HasField(f("optional_uint64")));
            Assert.True(message.HasField(f("optional_sint32")));
            Assert.True(message.HasField(f("optional_sint64")));
            Assert.True(message.HasField(f("optional_fixed32")));
            Assert.True(message.HasField(f("optional_fixed64")));
            Assert.True(message.HasField(f("optional_sfixed32")));
            Assert.True(message.HasField(f("optional_sfixed64")));
            Assert.True(message.HasField(f("optional_float")));
            Assert.True(message.HasField(f("optional_double")));
            Assert.True(message.HasField(f("optional_bool")));
            Assert.True(message.HasField(f("optional_string")));
            Assert.True(message.HasField(f("optional_bytes")));

            Assert.True(message.HasField(f("optionalgroup")));
            Assert.True(message.HasField(f("optional_nested_message")));
            Assert.True(message.HasField(f("optional_foreign_message")));
            Assert.True(message.HasField(f("optional_import_message")));

            Assert.True(((IMessage) message[f("optionalgroup")]).HasField(groupA));
            Assert.True(((IMessage) message[f("optional_nested_message")]).HasField(nestedB));
            Assert.True(((IMessage) message[f("optional_foreign_message")]).HasField(foreignC));
            Assert.True(((IMessage) message[f("optional_import_message")]).HasField(importD));

            Assert.True(message.HasField(f("optional_nested_enum")));
            Assert.True(message.HasField(f("optional_foreign_enum")));
            Assert.True(message.HasField(f("optional_import_enum")));

            Assert.True(message.HasField(f("optional_string_piece")));
            Assert.True(message.HasField(f("optional_cord")));

            Assert.Equal(101, message[f("optional_int32")]);
            Assert.Equal(102L, message[f("optional_int64")]);
            Assert.Equal(103u, message[f("optional_uint32")]);
            Assert.Equal(104UL, message[f("optional_uint64")]);
            Assert.Equal(105, message[f("optional_sint32")]);
            Assert.Equal(106L, message[f("optional_sint64")]);
            Assert.Equal(107U, message[f("optional_fixed32")]);
            Assert.Equal(108UL, message[f("optional_fixed64")]);
            Assert.Equal(109, message[f("optional_sfixed32")]);
            Assert.Equal(110L, message[f("optional_sfixed64")]);
            Assert.Equal(111F, message[f("optional_float")]);
            Assert.Equal(112D, message[f("optional_double")]);
            Assert.Equal(true, message[f("optional_bool")]);
            Assert.Equal("115", message[f("optional_string")]);
            Assert.Equal(TestUtil.ToBytes("116"), message[f("optional_bytes")]);

            Assert.Equal(117, ((IMessage) message[f("optionalgroup")])[groupA]);
            Assert.Equal(118, ((IMessage) message[f("optional_nested_message")])[nestedB]);
            Assert.Equal(119, ((IMessage) message[f("optional_foreign_message")])[foreignC]);
            Assert.Equal(120, ((IMessage) message[f("optional_import_message")])[importD]);

            Assert.Equal(nestedBaz, message[f("optional_nested_enum")]);
            Assert.Equal(foreignBaz, message[f("optional_foreign_enum")]);
            Assert.Equal(importBaz, message[f("optional_import_enum")]);

            Assert.Equal("124", message[f("optional_string_piece")]);
            Assert.Equal("125", message[f("optional_cord")]);

            // -----------------------------------------------------------------

            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_int32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_int64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_uint32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_uint64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_sint32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_sint64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_fixed32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_fixed64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_sfixed32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_sfixed64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_float")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_double")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_bool")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_string")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_bytes")));

            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeatedgroup")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_nested_message")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_foreign_message")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_import_message")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_nested_enum")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_foreign_enum")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_import_enum")));

            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_string_piece")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_cord")));

            Assert.Equal(201, message[f("repeated_int32"), 0]);
            Assert.Equal(202L, message[f("repeated_int64"), 0]);
            Assert.Equal(203U, message[f("repeated_uint32"), 0]);
            Assert.Equal(204UL, message[f("repeated_uint64"), 0]);
            Assert.Equal(205, message[f("repeated_sint32"), 0]);
            Assert.Equal(206L, message[f("repeated_sint64"), 0]);
            Assert.Equal(207U, message[f("repeated_fixed32"), 0]);
            Assert.Equal(208UL, message[f("repeated_fixed64"), 0]);
            Assert.Equal(209, message[f("repeated_sfixed32"), 0]);
            Assert.Equal(210L, message[f("repeated_sfixed64"), 0]);
            Assert.Equal(211F, message[f("repeated_float"), 0]);
            Assert.Equal(212D, message[f("repeated_double"), 0]);
            Assert.Equal(true, message[f("repeated_bool"), 0]);
            Assert.Equal("215", message[f("repeated_string"), 0]);
            Assert.Equal(TestUtil.ToBytes("216"), message[f("repeated_bytes"), 0]);

            Assert.Equal(217, ((IMessage) message[f("repeatedgroup"), 0])[repeatedGroupA]);
            Assert.Equal(218, ((IMessage) message[f("repeated_nested_message"), 0])[nestedB]);
            Assert.Equal(219, ((IMessage) message[f("repeated_foreign_message"), 0])[foreignC]);
            Assert.Equal(220, ((IMessage) message[f("repeated_import_message"), 0])[importD]);

            Assert.Equal(nestedBar, message[f("repeated_nested_enum"), 0]);
            Assert.Equal(foreignBar, message[f("repeated_foreign_enum"), 0]);
            Assert.Equal(importBar, message[f("repeated_import_enum"), 0]);

            Assert.Equal("224", message[f("repeated_string_piece"), 0]);
            Assert.Equal("225", message[f("repeated_cord"), 0]);

            Assert.Equal(301, message[f("repeated_int32"), 1]);
            Assert.Equal(302L, message[f("repeated_int64"), 1]);
            Assert.Equal(303U, message[f("repeated_uint32"), 1]);
            Assert.Equal(304UL, message[f("repeated_uint64"), 1]);
            Assert.Equal(305, message[f("repeated_sint32"), 1]);
            Assert.Equal(306L, message[f("repeated_sint64"), 1]);
            Assert.Equal(307U, message[f("repeated_fixed32"), 1]);
            Assert.Equal(308UL, message[f("repeated_fixed64"), 1]);
            Assert.Equal(309, message[f("repeated_sfixed32"), 1]);
            Assert.Equal(310L, message[f("repeated_sfixed64"), 1]);
            Assert.Equal(311F, message[f("repeated_float"), 1]);
            Assert.Equal(312D, message[f("repeated_double"), 1]);
            Assert.Equal(false, message[f("repeated_bool"), 1]);
            Assert.Equal("315", message[f("repeated_string"), 1]);
            Assert.Equal(TestUtil.ToBytes("316"), message[f("repeated_bytes"), 1]);

            Assert.Equal(317, ((IMessage) message[f("repeatedgroup"), 1])[repeatedGroupA]);
            Assert.Equal(318, ((IMessage) message[f("repeated_nested_message"), 1])[nestedB]);
            Assert.Equal(319, ((IMessage) message[f("repeated_foreign_message"), 1])[foreignC]);
            Assert.Equal(320, ((IMessage) message[f("repeated_import_message"), 1])[importD]);

            Assert.Equal(nestedBaz, message[f("repeated_nested_enum"), 1]);
            Assert.Equal(foreignBaz, message[f("repeated_foreign_enum"), 1]);
            Assert.Equal(importBaz, message[f("repeated_import_enum"), 1]);

            Assert.Equal("324", message[f("repeated_string_piece"), 1]);
            Assert.Equal("325", message[f("repeated_cord"), 1]);

            // -----------------------------------------------------------------

            Assert.True(message.HasField(f("default_int32")));
            Assert.True(message.HasField(f("default_int64")));
            Assert.True(message.HasField(f("default_uint32")));
            Assert.True(message.HasField(f("default_uint64")));
            Assert.True(message.HasField(f("default_sint32")));
            Assert.True(message.HasField(f("default_sint64")));
            Assert.True(message.HasField(f("default_fixed32")));
            Assert.True(message.HasField(f("default_fixed64")));
            Assert.True(message.HasField(f("default_sfixed32")));
            Assert.True(message.HasField(f("default_sfixed64")));
            Assert.True(message.HasField(f("default_float")));
            Assert.True(message.HasField(f("default_double")));
            Assert.True(message.HasField(f("default_bool")));
            Assert.True(message.HasField(f("default_string")));
            Assert.True(message.HasField(f("default_bytes")));

            Assert.True(message.HasField(f("default_nested_enum")));
            Assert.True(message.HasField(f("default_foreign_enum")));
            Assert.True(message.HasField(f("default_import_enum")));

            Assert.True(message.HasField(f("default_string_piece")));
            Assert.True(message.HasField(f("default_cord")));

            Assert.Equal(401, message[f("default_int32")]);
            Assert.Equal(402L, message[f("default_int64")]);
            Assert.Equal(403U, message[f("default_uint32")]);
            Assert.Equal(404UL, message[f("default_uint64")]);
            Assert.Equal(405, message[f("default_sint32")]);
            Assert.Equal(406L, message[f("default_sint64")]);
            Assert.Equal(407U, message[f("default_fixed32")]);
            Assert.Equal(408UL, message[f("default_fixed64")]);
            Assert.Equal(409, message[f("default_sfixed32")]);
            Assert.Equal(410L, message[f("default_sfixed64")]);
            Assert.Equal(411F, message[f("default_float")]);
            Assert.Equal(412D, message[f("default_double")]);
            Assert.Equal(false, message[f("default_bool")]);
            Assert.Equal("415", message[f("default_string")]);
            Assert.Equal(TestUtil.ToBytes("416"), message[f("default_bytes")]);

            Assert.Equal(nestedFoo, message[f("default_nested_enum")]);
            Assert.Equal(foreignFoo, message[f("default_foreign_enum")]);
            Assert.Equal(importFoo, message[f("default_import_enum")]);

            Assert.Equal("424", message[f("default_string_piece")]);
            Assert.Equal("425", message[f("default_cord")]);
        }

        /// <summary>
        /// Assert that all fields of the message are cleared, and that
        /// getting the fields returns their default values, using the reflection interface.
        /// </summary>
        public void AssertClearViaReflection(IMessage message)
        {
            // has_blah() should initially be false for all optional fields.
            Assert.False(message.HasField(f("optional_int32")));
            Assert.False(message.HasField(f("optional_int64")));
            Assert.False(message.HasField(f("optional_uint32")));
            Assert.False(message.HasField(f("optional_uint64")));
            Assert.False(message.HasField(f("optional_sint32")));
            Assert.False(message.HasField(f("optional_sint64")));
            Assert.False(message.HasField(f("optional_fixed32")));
            Assert.False(message.HasField(f("optional_fixed64")));
            Assert.False(message.HasField(f("optional_sfixed32")));
            Assert.False(message.HasField(f("optional_sfixed64")));
            Assert.False(message.HasField(f("optional_float")));
            Assert.False(message.HasField(f("optional_double")));
            Assert.False(message.HasField(f("optional_bool")));
            Assert.False(message.HasField(f("optional_string")));
            Assert.False(message.HasField(f("optional_bytes")));

            Assert.False(message.HasField(f("optionalgroup")));
            Assert.False(message.HasField(f("optional_nested_message")));
            Assert.False(message.HasField(f("optional_foreign_message")));
            Assert.False(message.HasField(f("optional_import_message")));

            Assert.False(message.HasField(f("optional_nested_enum")));
            Assert.False(message.HasField(f("optional_foreign_enum")));
            Assert.False(message.HasField(f("optional_import_enum")));

            Assert.False(message.HasField(f("optional_string_piece")));
            Assert.False(message.HasField(f("optional_cord")));

            // Optional fields without defaults are set to zero or something like it.
            Assert.Equal(0, message[f("optional_int32")]);
            Assert.Equal(0L, message[f("optional_int64")]);
            Assert.Equal(0U, message[f("optional_uint32")]);
            Assert.Equal(0UL, message[f("optional_uint64")]);
            Assert.Equal(0, message[f("optional_sint32")]);
            Assert.Equal(0L, message[f("optional_sint64")]);
            Assert.Equal(0U, message[f("optional_fixed32")]);
            Assert.Equal(0UL, message[f("optional_fixed64")]);
            Assert.Equal(0, message[f("optional_sfixed32")]);
            Assert.Equal(0L, message[f("optional_sfixed64")]);
            Assert.Equal(0F, message[f("optional_float")]);
            Assert.Equal(0D, message[f("optional_double")]);
            Assert.Equal(false, message[f("optional_bool")]);
            Assert.Equal("", message[f("optional_string")]);
            Assert.Equal(ByteString.Empty, message[f("optional_bytes")]);

            // Embedded messages should also be clear.
            Assert.False(((IMessage) message[f("optionalgroup")]).HasField(groupA));
            Assert.False(((IMessage) message[f("optional_nested_message")])
                               .HasField(nestedB));
            Assert.False(((IMessage) message[f("optional_foreign_message")])
                               .HasField(foreignC));
            Assert.False(((IMessage) message[f("optional_import_message")])
                               .HasField(importD));

            Assert.Equal(0, ((IMessage) message[f("optionalgroup")])[groupA]);
            Assert.Equal(0, ((IMessage) message[f("optional_nested_message")])[nestedB]);
            Assert.Equal(0, ((IMessage) message[f("optional_foreign_message")])[foreignC]);
            Assert.Equal(0, ((IMessage) message[f("optional_import_message")])[importD]);

            // Enums without defaults are set to the first value in the enum.
            Assert.Equal(nestedFoo, message[f("optional_nested_enum")]);
            Assert.Equal(foreignFoo, message[f("optional_foreign_enum")]);
            Assert.Equal(importFoo, message[f("optional_import_enum")]);

            Assert.Equal("", message[f("optional_string_piece")]);
            Assert.Equal("", message[f("optional_cord")]);

            // Repeated fields are empty.
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_int32")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_int64")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_uint32")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_uint64")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_sint32")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_sint64")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_fixed32")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_fixed64")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_sfixed32")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_sfixed64")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_float")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_double")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_bool")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_string")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_bytes")));

            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeatedgroup")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_nested_message")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_foreign_message")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_import_message")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_nested_enum")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_foreign_enum")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_import_enum")));

            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_string_piece")));
            Assert.Equal(0, message.GetRepeatedFieldCount(f("repeated_cord")));

            // has_blah() should also be false for all default fields.
            Assert.False(message.HasField(f("default_int32")));
            Assert.False(message.HasField(f("default_int64")));
            Assert.False(message.HasField(f("default_uint32")));
            Assert.False(message.HasField(f("default_uint64")));
            Assert.False(message.HasField(f("default_sint32")));
            Assert.False(message.HasField(f("default_sint64")));
            Assert.False(message.HasField(f("default_fixed32")));
            Assert.False(message.HasField(f("default_fixed64")));
            Assert.False(message.HasField(f("default_sfixed32")));
            Assert.False(message.HasField(f("default_sfixed64")));
            Assert.False(message.HasField(f("default_float")));
            Assert.False(message.HasField(f("default_double")));
            Assert.False(message.HasField(f("default_bool")));
            Assert.False(message.HasField(f("default_string")));
            Assert.False(message.HasField(f("default_bytes")));

            Assert.False(message.HasField(f("default_nested_enum")));
            Assert.False(message.HasField(f("default_foreign_enum")));
            Assert.False(message.HasField(f("default_import_enum")));

            Assert.False(message.HasField(f("default_string_piece")));
            Assert.False(message.HasField(f("default_cord")));

            // Fields with defaults have their default values (duh).
            Assert.Equal(41, message[f("default_int32")]);
            Assert.Equal(42L, message[f("default_int64")]);
            Assert.Equal(43U, message[f("default_uint32")]);
            Assert.Equal(44UL, message[f("default_uint64")]);
            Assert.Equal(-45, message[f("default_sint32")]);
            Assert.Equal(46L, message[f("default_sint64")]);
            Assert.Equal(47U, message[f("default_fixed32")]);
            Assert.Equal(48UL, message[f("default_fixed64")]);
            Assert.Equal(49, message[f("default_sfixed32")]);
            Assert.Equal(-50L, message[f("default_sfixed64")]);
            Assert.Equal(51.5F, message[f("default_float")]);
            Assert.Equal(52e3D, message[f("default_double")]);
            Assert.Equal(true, message[f("default_bool")]);
            Assert.Equal("hello", message[f("default_string")]);
            Assert.Equal(TestUtil.ToBytes("world"), message[f("default_bytes")]);

            Assert.Equal(nestedBar, message[f("default_nested_enum")]);
            Assert.Equal(foreignBar, message[f("default_foreign_enum")]);
            Assert.Equal(importBar, message[f("default_import_enum")]);

            Assert.Equal("abc", message[f("default_string_piece")]);
            Assert.Equal("123", message[f("default_cord")]);
        }

        // ---------------------------------------------------------------

        internal void AssertRepeatedFieldsModifiedViaReflection(IMessage message)
        {
            // ModifyRepeatedFields only sets the second repeated element of each
            // field.  In addition to verifying this, we also verify that the first
            // element and size were *not* modified.
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_int32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_int64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_uint32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_uint64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_sint32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_sint64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_fixed32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_fixed64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_sfixed32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_sfixed64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_float")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_double")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_bool")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_string")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_bytes")));

            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeatedgroup")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_nested_message")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_foreign_message")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_import_message")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_nested_enum")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_foreign_enum")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_import_enum")));

            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_string_piece")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("repeated_cord")));

            Assert.Equal(201, message[f("repeated_int32"), 0]);
            Assert.Equal(202L, message[f("repeated_int64"), 0]);
            Assert.Equal(203U, message[f("repeated_uint32"), 0]);
            Assert.Equal(204UL, message[f("repeated_uint64"), 0]);
            Assert.Equal(205, message[f("repeated_sint32"), 0]);
            Assert.Equal(206L, message[f("repeated_sint64"), 0]);
            Assert.Equal(207U, message[f("repeated_fixed32"), 0]);
            Assert.Equal(208UL, message[f("repeated_fixed64"), 0]);
            Assert.Equal(209, message[f("repeated_sfixed32"), 0]);
            Assert.Equal(210L, message[f("repeated_sfixed64"), 0]);
            Assert.Equal(211F, message[f("repeated_float"), 0]);
            Assert.Equal(212D, message[f("repeated_double"), 0]);
            Assert.Equal(true, message[f("repeated_bool"), 0]);
            Assert.Equal("215", message[f("repeated_string"), 0]);
            Assert.Equal(TestUtil.ToBytes("216"), message[f("repeated_bytes"), 0]);

            Assert.Equal(217, ((IMessage) message[f("repeatedgroup"), 0])[repeatedGroupA]);
            Assert.Equal(218, ((IMessage) message[f("repeated_nested_message"), 0])[nestedB]);
            Assert.Equal(219, ((IMessage) message[f("repeated_foreign_message"), 0])[foreignC]);
            Assert.Equal(220, ((IMessage) message[f("repeated_import_message"), 0])[importD]);

            Assert.Equal(nestedBar, message[f("repeated_nested_enum"), 0]);
            Assert.Equal(foreignBar, message[f("repeated_foreign_enum"), 0]);
            Assert.Equal(importBar, message[f("repeated_import_enum"), 0]);

            Assert.Equal("224", message[f("repeated_string_piece"), 0]);
            Assert.Equal("225", message[f("repeated_cord"), 0]);

            Assert.Equal(501, message[f("repeated_int32"), 1]);
            Assert.Equal(502L, message[f("repeated_int64"), 1]);
            Assert.Equal(503U, message[f("repeated_uint32"), 1]);
            Assert.Equal(504UL, message[f("repeated_uint64"), 1]);
            Assert.Equal(505, message[f("repeated_sint32"), 1]);
            Assert.Equal(506L, message[f("repeated_sint64"), 1]);
            Assert.Equal(507U, message[f("repeated_fixed32"), 1]);
            Assert.Equal(508UL, message[f("repeated_fixed64"), 1]);
            Assert.Equal(509, message[f("repeated_sfixed32"), 1]);
            Assert.Equal(510L, message[f("repeated_sfixed64"), 1]);
            Assert.Equal(511F, message[f("repeated_float"), 1]);
            Assert.Equal(512D, message[f("repeated_double"), 1]);
            Assert.Equal(true, message[f("repeated_bool"), 1]);
            Assert.Equal("515", message[f("repeated_string"), 1]);
            Assert.Equal(TestUtil.ToBytes("516"), message[f("repeated_bytes"), 1]);

            Assert.Equal(517, ((IMessage) message[f("repeatedgroup"), 1])[repeatedGroupA]);
            Assert.Equal(518, ((IMessage) message[f("repeated_nested_message"), 1])[nestedB]);
            Assert.Equal(519, ((IMessage) message[f("repeated_foreign_message"), 1])[foreignC]);
            Assert.Equal(520, ((IMessage) message[f("repeated_import_message"), 1])[importD]);

            Assert.Equal(nestedFoo, message[f("repeated_nested_enum"), 1]);
            Assert.Equal(foreignFoo, message[f("repeated_foreign_enum"), 1]);
            Assert.Equal(importFoo, message[f("repeated_import_enum"), 1]);

            Assert.Equal("524", message[f("repeated_string_piece"), 1]);
            Assert.Equal("525", message[f("repeated_cord"), 1]);
        }

        /// <summary>
        /// Verifies that the reflection setters for the given Builder object throw an
        /// ArgumentNullException if they are passed a null value. 
        /// </summary>
        public void AssertReflectionSettersRejectNull(IBuilder builder)
        {
            Assert.Throws<ArgumentNullException>(() => builder[f("optional_string")] = null);
            Assert.Throws<ArgumentNullException>(() => builder[f("optional_bytes")] = null);
            Assert.Throws<ArgumentNullException>(() => builder[f("optional_nested_enum")] = null);
            Assert.Throws<ArgumentNullException>(() => builder[f("optional_nested_message")] = null);
            Assert.Throws<ArgumentNullException>(() => builder[f("optional_nested_message")] = null);
            Assert.Throws<ArgumentNullException>(() => builder.WeakAddRepeatedField(f("repeated_string"), null));
            Assert.Throws<ArgumentNullException>(() => builder.WeakAddRepeatedField(f("repeated_bytes"), null));
            Assert.Throws<ArgumentNullException>(() => builder.WeakAddRepeatedField(f("repeated_nested_enum"), null));
            Assert.Throws<ArgumentNullException>(() => builder.WeakAddRepeatedField(f("repeated_nested_message"), null));
        }

        /// <summary>
        /// Verifies that the reflection repeated setters for the given Builder object throw an
        /// ArgumentNullException if they are passed a null value.
        /// </summary>
        public void AssertReflectionRepeatedSettersRejectNull(IBuilder builder)
        {
            builder.WeakAddRepeatedField(f("repeated_string"), "one");
            Assert.Throws<ArgumentNullException>(() => builder.SetRepeatedField(f("repeated_string"), 0, null));
            builder.WeakAddRepeatedField(f("repeated_bytes"), TestUtil.ToBytes("one"));
            Assert.Throws<ArgumentNullException>(() => builder.SetRepeatedField(f("repeated_bytes"), 0, null));
            builder.WeakAddRepeatedField(f("repeated_nested_enum"), nestedBaz);
            Assert.Throws<ArgumentNullException>(() => builder.SetRepeatedField(f("repeated_nested_enum"), 0, null));
            builder.WeakAddRepeatedField(f("repeated_nested_message"),
                                         new TestAllTypes.Types.NestedMessage.Builder {Bb = 218}.Build());
            Assert.Throws<ArgumentNullException>(() => builder.SetRepeatedField(f("repeated_nested_message"), 0, null));
        }

        public void SetPackedFieldsViaReflection(IBuilder message)
        {
            message.WeakAddRepeatedField(f("packed_int32"), 601);
            message.WeakAddRepeatedField(f("packed_int64"), 602L);
            message.WeakAddRepeatedField(f("packed_uint32"), 603U);
            message.WeakAddRepeatedField(f("packed_uint64"), 604UL);
            message.WeakAddRepeatedField(f("packed_sint32"), 605);
            message.WeakAddRepeatedField(f("packed_sint64"), 606L);
            message.WeakAddRepeatedField(f("packed_fixed32"), 607U);
            message.WeakAddRepeatedField(f("packed_fixed64"), 608UL);
            message.WeakAddRepeatedField(f("packed_sfixed32"), 609);
            message.WeakAddRepeatedField(f("packed_sfixed64"), 610L);
            message.WeakAddRepeatedField(f("packed_float"), 611F);
            message.WeakAddRepeatedField(f("packed_double"), 612D);
            message.WeakAddRepeatedField(f("packed_bool"), true);
            message.WeakAddRepeatedField(f("packed_enum"), foreignBar);
            // Add a second one of each field.
            message.WeakAddRepeatedField(f("packed_int32"), 701);
            message.WeakAddRepeatedField(f("packed_int64"), 702L);
            message.WeakAddRepeatedField(f("packed_uint32"), 703U);
            message.WeakAddRepeatedField(f("packed_uint64"), 704UL);
            message.WeakAddRepeatedField(f("packed_sint32"), 705);
            message.WeakAddRepeatedField(f("packed_sint64"), 706L);
            message.WeakAddRepeatedField(f("packed_fixed32"), 707U);
            message.WeakAddRepeatedField(f("packed_fixed64"), 708UL);
            message.WeakAddRepeatedField(f("packed_sfixed32"), 709);
            message.WeakAddRepeatedField(f("packed_sfixed64"), 710L);
            message.WeakAddRepeatedField(f("packed_float"), 711F);
            message.WeakAddRepeatedField(f("packed_double"), 712D);
            message.WeakAddRepeatedField(f("packed_bool"), false);
            message.WeakAddRepeatedField(f("packed_enum"), foreignBaz);
        }

        public void AssertPackedFieldsSetViaReflection(IMessage message)
        {
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_int32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_int64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_uint32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_uint64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_sint32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_sint64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_fixed32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_fixed64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_sfixed32")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_sfixed64")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_float")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_double")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_bool")));
            Assert.Equal(2, message.GetRepeatedFieldCount(f("packed_enum")));

            Assert.Equal(601, message[f("packed_int32"), 0]);
            Assert.Equal(602L, message[f("packed_int64"), 0]);
            Assert.Equal(603u, message[f("packed_uint32"), 0]);
            Assert.Equal(604uL, message[f("packed_uint64"), 0]);
            Assert.Equal(605, message[f("packed_sint32"), 0]);
            Assert.Equal(606L, message[f("packed_sint64"), 0]);
            Assert.Equal(607u, message[f("packed_fixed32"), 0]);
            Assert.Equal(608uL, message[f("packed_fixed64"), 0]);
            Assert.Equal(609, message[f("packed_sfixed32"), 0]);
            Assert.Equal(610L, message[f("packed_sfixed64"), 0]);
            Assert.Equal(611F, message[f("packed_float"), 0]);
            Assert.Equal(612D, message[f("packed_double"), 0]);
            Assert.Equal(true, message[f("packed_bool"), 0]);
            Assert.Equal(foreignBar, message[f("packed_enum"), 0]);

            Assert.Equal(701, message[f("packed_int32"), 1]);
            Assert.Equal(702L, message[f("packed_int64"), 1]);
            Assert.Equal(703u, message[f("packed_uint32"), 1]);
            Assert.Equal(704uL, message[f("packed_uint64"), 1]);
            Assert.Equal(705, message[f("packed_sint32"), 1]);
            Assert.Equal(706L, message[f("packed_sint64"), 1]);
            Assert.Equal(707u, message[f("packed_fixed32"), 1]);
            Assert.Equal(708uL, message[f("packed_fixed64"), 1]);
            Assert.Equal(709, message[f("packed_sfixed32"), 1]);
            Assert.Equal(710L, message[f("packed_sfixed64"), 1]);
            Assert.Equal(711F, message[f("packed_float"), 1]);
            Assert.Equal(712D, message[f("packed_double"), 1]);
            Assert.Equal(false, message[f("packed_bool"), 1]);
            Assert.Equal(foreignBaz, message[f("packed_enum"), 1]);
        }
    }
}