﻿using System;
using System.Runtime.InteropServices;


namespace Flecs
{
	public delegate void SystemAction<T>(ref Rows ids, Span<T> comp) where T : unmanaged;
	public delegate void SystemAction<T1, T2>(ref Rows ids, Span<T1> comp1, Span<T2> comp2) where T1 : unmanaged where T2 : unmanaged;
	public delegate void SystemAction<T1, T2, T3>(ref Rows ids, Span<T1> comp1, Span<T2> comp2, Span<T3> comp3) where T1 : unmanaged where T2 : unmanaged;

	public unsafe static partial class ecs
	{
		#region Imperitive Macros

		public static EntityId ecs_new(World world, TypeId typeId)
		{
			return _ecs.@new(world, typeId);
		}

		public static EntityId ecs_new(World world, Type componentType)
		{
			return _ecs.@new(world, Caches.GetComponentTypeId(world, componentType));
		}

		public static bool ecs_has(World world, EntityId entity, TypeId typeId)
		{
			return _ecs.has(world, entity, typeId);
		}

		public static bool ecs_has(World world, EntityId entity, Type componentType)
		{
			return _ecs.has(world, entity, Caches.GetComponentTypeId(world, componentType));
		}

		public static bool ecs_has<T>(World world, EntityId entity) where T : unmanaged
		{
			return _ecs.has(world, entity, Caches.GetComponentTypeId<T>(world));
		}

		public static EntityId ecs_new_w_count(World world, TypeId typeId, uint count)
		{
			return _ecs.new_w_count(world, typeId, count);
		}

		public static EntityId ecs_new_child(World world, EntityId parent, TypeId type)
		{
			return _ecs.new_child(world, parent, type);
		}

		public static EntityId ecs_new_child_w_count(World world, EntityId parent, TypeId type, uint count)
		{
			return _ecs.new_child_w_count(world, parent, type, count);
		}

		public static EntityId ecs_new_instance(World world, EntityId baseEntityId, TypeId type)
		{
			return _ecs.new_instance(world, baseEntityId, type);
		}

		public static EntityId ecs_new_instance_w_count(World world, EntityId baseEntityId, TypeId type, uint count)
		{
			return _ecs.new_instance_w_count(world, baseEntityId, type, count);
		}

		public static T* ecs_column<T>(ref Rows rows, uint columnIndex) where T : unmanaged
		{
			return (T*)_ecs.column(ref rows, (UIntPtr)Marshal.SizeOf<T>(), columnIndex);
		}

		public static void ecs_set(World world, EntityId entity)
		{
			//#define ecs_set(world, entity, component, ...)\
			//		_ecs_set_ptr(world, entity, ecs_entity(component), sizeof(component), &(component) __VA_ARGS__)
		}

		public static void ecs_set_ptr(World world)
		{
			//#define ecs_set_ptr(world, entity, component, ptr)\
			//		_ecs_set_ptr(world, entity, ecs_entity(component), sizeof(component), ptr)
		}

		public static void ecs_set_singleton(World world)
		{
			//#define ecs_set_singleton(world, component, ...)\
			//		_ecs_set_singleton_ptr(world, ecs_entity(component), sizeof(component), &(component) __VA_ARGS__)
		}

		public static void ecs_set_singleton_ptr(World world)
		{
			//#define ecs_set_singleton_ptr(world, component, ptr)\
			//    _ecs_set_singleton_ptr(world, ecs_entity(component), sizeof(component), ptr)
		}

		public static void ecs_add(World world, EntityId entity, TypeId type)
		{
			_ecs.add(world, entity, type);
		}

		public static void ecs_remove(World world, EntityId entity, TypeId type)
		{
			_ecs.remove(world, entity, type);
		}

		public static void ecs_add_remove(World world, EntityId entity, TypeId typeToAdd, TypeId typeToRemove)
		{
			_ecs.add_remove(world, entity, typeToAdd, typeToRemove);
		}

		#endregion

		#region Declarative Macros

		public static TypeId ECS_COMPONENT(World world, Type componentType) => Caches.GetComponentTypeId(world, componentType);

		public static TypeId ECS_COMPONENT<T>(World world) where T : unmanaged => Caches.GetComponentTypeId<T>(world);

		public static EntityId ECS_SYSTEM(World world, SystemActionDelegate method, SystemKind kind, string expr)
		{
			var systemNamePtr = Caches.AddUnmanagedString(method.Method.Name);
			var signaturePtr = Caches.AddUnmanagedString(expr);
			Caches.AddSystemAction(world, method);

			return ecs.new_system(world, systemNamePtr, kind, signaturePtr, method);
		}

		public static EntityId ECS_SYSTEM<T1>(World world, SystemAction<T1> systemImpl, SystemKind kind) where T1 : unmanaged
		{
			SystemActionDelegate del = delegate(ref Rows rows)
			{
				var set1 = (T1*)_ecs.column(ref rows, (UIntPtr)Marshal.SizeOf<T1>(), 1);
				systemImpl(ref rows, new Span<T1>(set1, (int)rows.count));
			};

			// ensure our system doesnt get GCd and that our Component is registered
			Caches.AddSystemAction(world, del);
			Caches.GetComponentTypeId<T1>(world);

			var systemNamePtr = Caches.AddUnmanagedString(systemImpl.Method.Name);
			var signaturePtr = Caches.AddUnmanagedString(typeof(T1).Name);
			return ecs.new_system(world, systemNamePtr, kind, signaturePtr, del);
		}

		public static EntityId ECS_SYSTEM<T1, T2>(World world, SystemAction<T1, T2> systemImpl, SystemKind kind) where T1 : unmanaged where T2 : unmanaged
		{
			SystemActionDelegate del = delegate(ref Rows rows)
			{
				var set1 = (T1*)_ecs.column(ref rows, (UIntPtr)Marshal.SizeOf<T1>(), 1);
				var set2 = (T2*)_ecs.column(ref rows, (UIntPtr)Marshal.SizeOf<T2>(), 2);
				systemImpl(ref rows, new Span<T1>(set1, (int)rows.count), new Span<T2>(set2, (int)rows.count));
			};

			// ensure our system doesnt get GCd and that our Component is registered
			Caches.AddSystemAction(world, del);
			Caches.GetComponentTypeId<T1>(world);
			Caches.GetComponentTypeId<T2>(world);

			var systemNamePtr = Caches.AddUnmanagedString(systemImpl.Method.Name);
			var signaturePtr = Caches.AddUnmanagedString($"{typeof(T1).Name}, {typeof(T2).Name}");
			return ecs.new_system(world, systemNamePtr, kind, signaturePtr, del);
		}

		public static EntityId ECS_SYSTEM<T1, T2, T3>(World world, SystemAction<T1, T2, T3> systemImpl, SystemKind kind)
			where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
		{
			SystemActionDelegate del = delegate(ref Rows rows)
			{
				var set1 = (T1*)_ecs.column(ref rows, (UIntPtr)Marshal.SizeOf<T1>(), 1);
				var set2 = (T2*)_ecs.column(ref rows, (UIntPtr)Marshal.SizeOf<T2>(), 2);
				var set3 = (T3*)_ecs.column(ref rows, (UIntPtr)Marshal.SizeOf<T3>(), 3);
				systemImpl(ref rows, new Span<T1>(set1, (int)rows.count), new Span<T2>(set2, (int)rows.count), new Span<T3>(set3, (int)rows.count));
			};

			// ensure our system doesnt get GCd and that our Component is registered
			Caches.AddSystemAction(world, del);
			Caches.GetComponentTypeId<T1>(world);
			Caches.GetComponentTypeId<T2>(world);

			var systemNamePtr = Caches.AddUnmanagedString(systemImpl.Method.Name);
			var signaturePtr = Caches.AddUnmanagedString($"{typeof(T1).Name}, {typeof(T2).Name}");
			return ecs.new_system(world, systemNamePtr, kind, signaturePtr, del);
		}

		public static void ECS_COLUMN<T>(ref Rows rows, out Span<T> column, uint columnIndex) where T : unmanaged
		{
			var set = ecs_column<T>(ref rows, columnIndex);
			column = new Span<T>(set, (int)rows.count);
		}

		public static EntityId ECS_ENTITY(World world, string id, string expr)
		{
            var idPtr = Caches.AddUnmanagedString(id);
			return ecs.new_entity(world, idPtr, expr);
		}

		public static TypeId ECS_TAG(World world, string tag)
		{
			var idPtr = Caches.AddUnmanagedString(tag);
			var entityId = ecs.new_component(world, idPtr, (UIntPtr)0);
			return ecs.type_from_entity(world, entityId);
		}

		public static TypeId ECS_TYPE(World world, string id, string expr)
		{
			var idPtr = Caches.AddUnmanagedString(id);
			var entityId = ecs.new_type(world, idPtr, expr);
			return ecs.type_from_entity(world, entityId);
		}

		public static void ECS_PREFAB(World world)
		{
//#define ECS_PREFAB(world, id, ...) \
//			ecs_entity_t id = ecs_new_prefab(world, #id, #__VA_ARGS__);\
//    ECS_TYPE_VAR(id) = ecs_type_from_entity(world, id);\
//    (void)id;\
//    (void)ecs_type(id);\
		}

		#endregion
	}
}
