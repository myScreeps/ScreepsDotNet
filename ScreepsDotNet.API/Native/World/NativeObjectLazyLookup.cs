﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.JavaScript;
using System.Linq;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeObjectLazyLookup<TConcrete, TInterface> : IReadOnlyDictionary<string, TInterface> where TInterface : class where TConcrete : class, INativeObject, TInterface
    {
        #region Imports

        [JSImport("getKeysOf", "object")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.String>>]
        internal static partial string[] Native_GetKeysOf([JSMarshalAs<JSType.Object>] JSObject obj);

        #endregion

        private readonly Func<JSObject> proxyObjectReacquireFn;
        private readonly Func<TConcrete, string> getObjectKeyFn;
        private readonly Func<string, JSObject, TConcrete?> constructObjectFn;

        private JSObject proxyObject;

        private ISet<string>? keysCache;
        private readonly IDictionary<string, TConcrete> mapCache = new Dictionary<string, TConcrete>();

        private ISet<string> KeySet => keysCache ??= new HashSet<string>(Native_GetKeysOf(proxyObject));

        public TInterface this[string key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();

        public IEnumerable<string> Keys => KeySet;

        public IEnumerable<TInterface> Values => this.Select(pair => pair.Value);

        public int Count => KeySet.Count;

        public NativeObjectLazyLookup(Func<JSObject> proxyObjectReacquireFn, Func<TConcrete, string> getObjectKeyFn, Func<string, JSObject, TConcrete?> constructObjectFn)
        {
            this.proxyObjectReacquireFn = proxyObjectReacquireFn;
            this.getObjectKeyFn = getObjectKeyFn;
            this.constructObjectFn = constructObjectFn;
            proxyObject = proxyObjectReacquireFn();
        }

        public bool ContainsKey(string key)
            => KeySet.Contains(key);

        public IEnumerator<KeyValuePair<string, TInterface>> GetEnumerator()
#pragma warning disable CS8604 // Possible null reference argument.
            => KeySet
                .Select(key => new KeyValuePair<string, TInterface>(key, TryGetValue(key, out var value) ? value : null))
                .Where(pair => pair.Value != null)
                .GetEnumerator();
#pragma warning restore CS8604 // Possible null reference argument.

        public void InvalidateProxyObject()
        {
            proxyObject = proxyObjectReacquireFn();
            keysCache = null;
            // TODO: Instead of clearing the whole cache, go through and remove any where Exists == false
            mapCache.Clear();
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TInterface value)
        {
            if (!KeySet.Contains(key))
            {
                value = null;
                return false;
            }
            if (mapCache.TryGetValue(key, out var cachedObj))
            {
                value = cachedObj;
                return true;
            }
            var obj = proxyObject.GetPropertyAsJSObject(key);
            if (obj == null)
            {
                value = null;
                return false;
            }
            var newObj = constructObjectFn(key, obj);
            if (newObj == null)
            {
                value = null;
                return false;
            }
            mapCache.Add(key, newObj);
            value = newObj;
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}