// TecVooDoo Games - Tests
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TecVooDoo.Games.Tests
{
    [TestFixture]
    public class SerializableTypeTests
    {
        [Serializable]
        sealed class Holder
        {
            public SerializableType type;
        }

        interface IMarker { }
        abstract class AbstractBase : IMarker { }
        sealed class Concrete : AbstractBase { }
        sealed class Unrelated { }
        sealed class GenericImpl<T> : IMarker { }

        [Test]
        public void ImplicitConversions_RoundTripType()
        {
            SerializableType wrapped = typeof(Concrete);
            Type unwrapped = wrapped;

            Assert.That(wrapped.Type, Is.EqualTo(typeof(Concrete)));
            Assert.That(unwrapped, Is.EqualTo(typeof(Concrete)));
        }

        [Test]
        public void JsonRoundTrip_RestoresType()
        {
            Holder source = new Holder { type = typeof(Concrete) };

            string json = JsonUtility.ToJson(source);
            Holder restored = JsonUtility.FromJson<Holder>(json);

            StringAssert.Contains(typeof(Concrete).AssemblyQualifiedName, json);
            Assert.That(restored.type.Type, Is.EqualTo(typeof(Concrete)));
        }

        [Test]
        public void Deserialize_UnknownTypeName_LogsError()
        {
            string json = "{\"type\":{\"assemblyQualifiedName\":\"No.Such.Type, NoSuchAssembly\"}}";

            LogAssert.Expect(LogType.Error, "Type No.Such.Type, NoSuchAssembly not found");
            Holder restored = JsonUtility.FromJson<Holder>(json);

            Assert.That(restored.type.Type, Is.Null);
        }

        [Test]
        public void Deserialize_EmptyTypeName_IsSilentAndNull()
        {
            string json = "{\"type\":{\"assemblyQualifiedName\":\"\"}}";

            Holder restored = JsonUtility.FromJson<Holder>(json);

            Assert.That(restored.type.Type, Is.Null);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void InheritsOrImplements_SelfBaseAndInterface()
        {
            Assert.That(typeof(Concrete).InheritsOrImplements(typeof(Concrete)), Is.True);
            Assert.That(typeof(Concrete).InheritsOrImplements(typeof(AbstractBase)), Is.True);
            Assert.That(typeof(Concrete).InheritsOrImplements(typeof(IMarker)), Is.True);
            Assert.That(typeof(Unrelated).InheritsOrImplements(typeof(IMarker)), Is.False);
            Assert.That(typeof(Unrelated).InheritsOrImplements(typeof(AbstractBase)), Is.False);
        }

        [Test]
        public void InheritsOrImplements_OpenGenerics()
        {
            Assert.That(typeof(List<int>).InheritsOrImplements(typeof(IEnumerable<>)), Is.True);
            Assert.That(typeof(List<int>).InheritsOrImplements(typeof(List<>)), Is.True);
            Assert.That(typeof(List<int>).InheritsOrImplements(typeof(IDictionary<,>)), Is.False);
        }

        [Test]
        public void TypeFilter_ExcludesAbstractInterfaceAndGeneric()
        {
            TypeFilterAttribute filter = new TypeFilterAttribute(typeof(IMarker));

            Assert.That(filter.Filter(typeof(Concrete)), Is.True);
            Assert.That(filter.Filter(typeof(AbstractBase)), Is.False);
            Assert.That(filter.Filter(typeof(IMarker)), Is.False);
            Assert.That(filter.Filter(typeof(GenericImpl<int>)), Is.False);
            Assert.That(filter.Filter(typeof(Unrelated)), Is.False);
        }
    }
}
