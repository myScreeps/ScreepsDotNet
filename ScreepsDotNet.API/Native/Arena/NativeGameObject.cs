﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.CompilerServices;
using System.Linq;

using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeGameObject : IGameObject, IEquatable<NativeGameObject?>
    {
        #region Imports

        [JSImport("GameObject.findClosestByPath", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_FindClosestByPath_NoOpts([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] positions);

        [JSImport("GameObject.findClosestByPath", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_FindClosestByPath([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] positions, [JSMarshalAs<JSType.Object>] JSObject options);

        [JSImport("GameObject.findClosestByRange", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_FindClosestByRange([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] positions);

        [JSImport("GameObject.findInRange", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_FindInRange([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] positions, [JSMarshalAs<JSType.Number>] int range);

        [JSImport("GameObject.findPathTo", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_FindPathTo_NoOpts([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject pos);

        [JSImport("GameObject.findPathTo", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_FindPathTo([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject pos, [JSMarshalAs<JSType.Object>] JSObject options);

        [JSImport("GameObject.getRangeTo", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GetRangeTo([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject pos);

        #endregion

        internal readonly JSObject ProxyObject;

        public bool Exists => ProxyObject.GetPropertyAsBoolean("exists");

        public string Id => ProxyObject.GetTypeOfProperty("id") == "number" ? ProxyObject.GetPropertyAsInt32("id").ToString() : (ProxyObject.GetPropertyAsString("id") ?? ProxyObject.ToString() ?? "");

        public int? TicksToDecay => ProxyObject.GetTypeOfProperty("ticksToDecay") == "number" ? ProxyObject.GetPropertyAsInt32("ticksToDecay") : null;

        public int X => ProxyObject.GetPropertyAsInt32("x");

        public int Y => ProxyObject.GetPropertyAsInt32("y");

        public Position Position => new(X, Y);

        public NativeGameObject(JSObject wrappedJsObject)
        {
            this.ProxyObject = wrappedJsObject;
        }

        public T? FindClosestByPath<T>(IEnumerable<T> positions, FindPathOptions? options) where T : class, IPosition
            => (options != null
                ? Native_FindClosestByPath(ProxyObject, positions.Select(x => x.ToJS()).ToArray(), options.Value.ToJS())
                : Native_FindClosestByPath_NoOpts(ProxyObject, positions.Select(x => x.ToJS()).ToArray())
            ).ToGameObject<IGameObject>() as T;

        public Position? FindClosestByPath(IEnumerable<Position> positions, FindPathOptions? options)
            => (options != null
                ? Native_FindClosestByPath(ProxyObject, positions.Select(x => x.ToJS()).ToArray(), options.Value.ToJS())
                : Native_FindClosestByPath_NoOpts(ProxyObject, positions.Select(x => x.ToJS()).ToArray())
            ).ToPositionNullable();

        public T? FindClosestByRange<T>(IEnumerable<T> positions) where T : class, IPosition
            => Native_FindClosestByRange(ProxyObject, positions.Select(x => x.ToJS()).ToArray()).ToGameObject<IGameObject>() as T;

        public Position? FindClosestByRange(IEnumerable<Position> positions)
            => Native_FindClosestByRange(ProxyObject, positions.Select(x => x.ToJS()).ToArray()).ToPosition();

        public IEnumerable<T> FindInRange<T>(IEnumerable<T> positions, int range) where T : class, IPosition
            => Native_FindInRange(ProxyObject, positions.Select(x => x.ToJS()).ToArray(), range)
                .Select(NativeGameObjectUtils.CreateWrapperForObject)
                .Cast<T>()
                .ToArray();

        public IEnumerable<Position> FindInRange(IEnumerable<Position> positions, int range)
            => Native_FindInRange(ProxyObject, positions.Select(x => x.ToJS()).ToArray(), range)
                .Select(x => x.ToPosition())
                .ToArray();

        public IEnumerable<Position> FindPathTo(IPosition pos, FindPathOptions? options)
            => (options.HasValue ? Native_FindPathTo(ProxyObject, pos.ToJS(), options.Value.ToJS()) : Native_FindPathTo_NoOpts(ProxyObject, pos.ToJS()))
                .Select(x => x.ToPosition())
                .ToArray();

        public IEnumerable<Position> FindPathTo(Position pos, FindPathOptions? options)
            => (options.HasValue ? Native_FindPathTo(ProxyObject, pos.ToJS(), options.Value.ToJS()) : Native_FindPathTo_NoOpts(ProxyObject, pos.ToJS()))
                .Select(x => x.ToPosition())
                .ToArray();

        public int GetRangeTo(IPosition pos)
            => Native_GetRangeTo(ProxyObject, pos.ToJS());

        public int GetRangeTo(Position pos)
            => Native_GetRangeTo(ProxyObject, pos.ToJS());

        public override string ToString()
            => $"GameObject({Id}, {Position})";

        public override bool Equals(object? obj) => Equals(obj as NativeGameObject);

        public bool Equals(NativeGameObject? other) => other is not null && Id == other.Id;

        public override int GetHashCode() => HashCode.Combine(Id);

        public static bool operator ==(NativeGameObject? left, NativeGameObject? right) => EqualityComparer<NativeGameObject>.Default.Equals(left, right);

        public static bool operator !=(NativeGameObject? left, NativeGameObject? right) => !(left == right);
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static class NativeGameObjectPrototypes<T> where T : IGameObject
    {
        public static JSObject? ConstructorObj;

        static NativeGameObjectPrototypes()
        {
            RuntimeHelpers.RunClassConstructor(typeof(NativeGameObjectUtils).TypeHandle);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static partial class NativeGameObjectUtils
    {
        private const string TypeIdKey = "__dotnet_typeId";

        private static readonly JSObject prototypesObject;
        private static readonly IList<Type> prototypeTypeMappings = new List<Type>();
        private static readonly IDictionary<Type, string> prototypeNameMappings = new Dictionary<Type, string>();

        [JSImport("getUtils", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject GetUtilsObject();

        [JSImport("getPrototypes", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject GetPrototypesObject();

        [JSImport("getConstructorOf", "object")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject GetConstructorOf([JSMarshalAs<JSType.Object>] JSObject obj);

        [JSImport("create", "object")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject CreateObject([JSMarshalAs<JSType.Object>] JSObject? prototype);

        [JSImport("set", "object")]
        internal static partial void SetArrayOnObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string key, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] val);

        [JSImport("get", "object")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[]? GetArrayOnObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string key);

        internal static void RegisterPrototypeTypeMapping<TInterface, TConcrete>(string prototypeName)
            where TInterface : IGameObject
            where TConcrete : NativeGameObject
        {
            var constructor = prototypesObject.GetPropertyAsJSObject(prototypeName);
            if (constructor == null)
            {
                Console.WriteLine($"Failed to retrieve constructor for '{prototypeName}'");
                return;
            }
            if (constructor.HasProperty(TypeIdKey))
            {
                Console.WriteLine($"Constructor for '{prototypeName}' was already bound to {prototypeTypeMappings[constructor.GetPropertyAsInt32(TypeIdKey) - 1]}");
                return;
            }
            int typeId = prototypeTypeMappings.Count;
            constructor.SetProperty(TypeIdKey, typeId + 1);
            prototypeTypeMappings.Add(typeof(TConcrete));
            NativeGameObjectPrototypes<TInterface>.ConstructorObj = constructor;
            NativeGameObjectPrototypes<TConcrete>.ConstructorObj = constructor;
            prototypeNameMappings.Add(typeof(TInterface), prototypeName);
            prototypeNameMappings.Add(typeof(TConcrete), prototypeName);
        }

        internal static Type? GetWrapperTypeForConstructor(JSObject constructor)
        {
            int typeId = constructor.GetPropertyAsInt32(TypeIdKey) - 1;
            if (typeId < 0)
            {
                Console.WriteLine($"Failed to retrieve wrapper type for {constructor} - typeId not found");
                return null;
            }
            return prototypeTypeMappings[typeId];
        }

        internal static Type? GetWrapperTypeForObject(JSObject jsObject)
            => GetWrapperTypeForConstructor(GetConstructorOf(jsObject));

        internal static NativeGameObject? CreateWrapperForObjectNullSafe(JSObject? proxyObject)
            => proxyObject == null ? null : CreateWrapperForObject(proxyObject);

        internal static NativeGameObject CreateWrapperForObject(JSObject proxyObject)
        {
            var wrapperType = GetWrapperTypeForObject(proxyObject);
            if (wrapperType == null) { return new NativeGameObject(proxyObject); }
            return (Activator.CreateInstance(wrapperType, new object[] { proxyObject }) as NativeGameObject)!;
        }

        internal static string GetPrototypeName(Type type)
            => prototypeNameMappings[type];

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeCreep))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructure))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeOwnedStructure))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureTower))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureContainer))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureRampart))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureRoad))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureSpawn))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureWall))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeFlag))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeSource))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeConstructionSite))]
        static NativeGameObjectUtils()
        {
            prototypesObject = GetPrototypesObject();
            try
            {
                // NOTE - order matters! We must do derived classes first and base classes last
                // This is to avoid a nasty bug where the runtime gets objects and their prototype objects mixed up due to the tracking id being set as a property on the object
                RegisterPrototypeTypeMapping<IStructureTower, NativeStructureTower>("StructureTower");
                RegisterPrototypeTypeMapping<IStructureContainer, NativeStructureContainer>("StructureContainer");
                RegisterPrototypeTypeMapping<IStructureRampart, NativeStructureRampart>("StructureRampart");
                RegisterPrototypeTypeMapping<IStructureRoad, NativeStructureRoad>("StructureRoad");
                RegisterPrototypeTypeMapping<IStructureSpawn, NativeStructureSpawn>("StructureSpawn");
                RegisterPrototypeTypeMapping<IOwnedStructure, NativeOwnedStructure>("OwnedStructure");
                RegisterPrototypeTypeMapping<IStructureWall, NativeStructureWall>("StructureWall");
                RegisterPrototypeTypeMapping<IStructure, NativeStructure>("Structure");
                RegisterPrototypeTypeMapping<IFlag, NativeFlag>("Flag");
                RegisterPrototypeTypeMapping<IResource, NativeResource>("Resource");
                RegisterPrototypeTypeMapping<ISource, NativeSource>("Source");
                RegisterPrototypeTypeMapping<IConstructionSite, NativeConstructionSite>("ConstructionSite");
                RegisterPrototypeTypeMapping<ICreep, NativeCreep>("Creep");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static class NativeGameObjectExtensions
    {
        public static JSObject ToJS(this IPosition position)
        {
            if (position is NativeGameObject nativeGameObject) { return nativeGameObject.ProxyObject; }
            var obj = NativeGameObjectUtils.CreateObject(null);
            obj.SetProperty("x", position.X);
            obj.SetProperty("y", position.Y);
            return obj;
        }

        public static T? ToGameObject<T>(this JSObject? proxyObject) where T : class, IGameObject
            => NativeGameObjectUtils.CreateWrapperForObjectNullSafe(proxyObject) as T;
    }
}
