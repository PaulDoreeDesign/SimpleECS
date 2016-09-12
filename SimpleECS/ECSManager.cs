﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ECS.Internal
{
	// Keeps track of ECS Collections and handles spawning entites
	public static class ECSManager
	{
		//
		//
		// CONSTRUCTOR
		//
		//

		static ECSManager()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();	// Get a list of all IComponents
			foreach(var assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach( Type type in types)
				{
					if (type.IsSubclassOf(typeof(EntityComponent)))
					{
						_componentTypes.Add(type, _componentTypes.Count);
						_componentTypeIndex.Add(type);
					}
				}
			}
			_componentLookUPs = new ComponentPool[_componentTypes.Count];

			EntityLink[] links = GameObject.FindObjectsOfType<EntityLink>();	// make sure all enitity links are set up
			foreach( var link in links)
			{
				if (link.entity == null)
					link.SetEntity(CreateEntity());
				link.SetUpComponents();
			}
		}

		//
		//
		//	PROPERTIES
		//
		//

		// Entity component index lookup
		public static List<ushort[]> EntityLookup = new List<ushort[]>();	// short to save some space, max 53k ish components

		// reference to obtain component ID's by type
		static Dictionary<Type, int> _componentTypes = new Dictionary<Type, int>();

		// fast lookups to component pools by index, used by destory method
		public static ComponentPool[] _componentLookUPs;

		// fast lookup to component type by index
		static List<Type> _componentTypeIndex = new List <Type>();

		// entity pool
		static Queue<Entity> _pooledEntities = new Queue<Entity>();

		// Keeps track of all active entities // TODO list might be better, must experiment
		static HashSet<Entity> _activeEntities = new HashSet<Entity>();

		// list of all entities, used to fetch Entities by ID
		static List<Entity> _entities = new List<Entity>();		// List of all current entities

		// Reference to get group by type
		public static Dictionary<Type, Groups> _groups = new Dictionary<Type, Groups>();

		// Reference to get systems by group name
		public static Dictionary <string, SystemGroup> _systemGroups = new Dictionary<string, SystemGroup>();

		//
		//
		// METHODS & GETTERS
		//
		//

		public static HashSet<Entity> ActiveEntities
		{
			get {return _activeEntities;}
		}
			
		public static int PooledEntitiesCount()
		{
			return _pooledEntities.Count;

		}
		public static int ActiveEntitiesCount()
		{
			return _activeEntities.Count;
		}

		public static int TotalEntitiesCount()
		{
			return _entities.Count;
		}

		public static int UniqueComponentCount()
		{
			return _componentTypeIndex.Count;
		}

		/// <summary>
		/// Returns component type with component ID
		/// </summary>
		public static Type GetComponentType (int ID)
		{
			if (ID < 0 || ID > _componentTypeIndex.Count)
				return null;
			return _componentTypeIndex[ID];
		}

		/// <summary>
		/// Returns Component ID of Component
		/// </summary>
		public static int GetComponentID<C>() where C: EntityComponent
		{
			int id = -1;
			if (_componentTypes.TryGetValue(typeof(C), out id))
			{
				id = _componentTypes[typeof(C)];
			}
			return id;
		}

		public static Entity GetEntity(int ID)	// returns entity with ID
		{
			if (ID < 0 || ID >= _entities.Count)
				return null;
			return (_entities[ID]);
		}

		public static Entity CreateEntity()
		{
			Entity e;
			if (_pooledEntities.Count > 0)
			{
				e = _pooledEntities.Dequeue();
			}
			else
			{
				e = new Entity(EntityLookup.Count);
				EntityLookup.Add(new ushort[_componentTypes.Count]);
			}
			_entities.Add(e);
			_activeEntities.Add(e);
			return e;
		}

		//
		//
		// EVENTS
		//
		//

		public static void DestroyEntity(Entity e)
		{
			if (_activeEntities.Contains(e))
			{
				_activeEntities.Remove(e);
				_pooledEntities.Enqueue(e);

				for(int i=0; i < _componentTypes.Count; ++i)
				{
					if (EntityLookup[e.ID][i] > 0)
					{
						_componentLookUPs[i].BaseRemoveComponent(e);	// remove component
						//EntityLookup[e.ID][i] = -1;					// set lookup value
					}
				}
			}
		}
	}
}

