﻿using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureContainer : NativeOwnedStructure, IStructureContainer
    {
        public IStore Store => new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public NativeStructureContainer(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"StructureContainer({Id}, {Position})";
    }
}
