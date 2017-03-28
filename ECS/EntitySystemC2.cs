﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class EntitySystem<C1, C2>: EntitySystem where C1 : EntityComponent<C1> where C2 : EntityComponent<C2>
{

	// TODO Add and Remove component Events
	class Processor
	{
		public Processor(Entity e)
		{
				id = e.ID;
				c1 = e.Get<C1>();
				c2 = e.Get<C2>();
		}
		public int id;
		public C1 c1;
		public C2 c2;
	}

	public int PoolCount;

	List<Processor> processor = new List<Processor>();
	HashSet<int> id = new HashSet<int>();

	void OnEnable()
	{
		if (EntityManager.instance != null)
		{
			SetUP();
			if (this is IUpdate) EntityManager.instance.UpdateCallback += _ProcessUpdate;
			if (this is IFixedUpdate) EntityManager.instance.FixedUpdateCallback += _ProcessFixedUpdate;

			Group<C1>.instance.AddComponentCallback += AddEntity;
			Group<C2>.instance.AddComponentCallback += AddEntity;

			Group<C1>.instance.RemoveComponentCallback += RemoveEntity;
			Group<C2>.instance.RemoveComponentCallback += RemoveEntity;

			SubscribeEvents();
		}

	}

	void OnDisable()
	{
		if (EntityManager.instance != null)
		{
			if (this is IUpdate) EntityManager.instance.UpdateCallback -= _ProcessUpdate;
			if (this is IFixedUpdate) EntityManager.instance.FixedUpdateCallback -= _ProcessFixedUpdate;

			Group<C1>.instance.AddComponentCallback -= AddEntity;
			Group<C2>.instance.AddComponentCallback -= AddEntity;
			Group<C1>.instance.RemoveComponentCallback -= RemoveEntity;
			Group<C2>.instance.RemoveComponentCallback -= RemoveEntity;
			UnSubscribeEvents();
		}
	}

	public virtual void SubscribeEvents()
	{}

	public virtual void UnSubscribeEvents()
	{}


	void AddEntity(Entity e)
	{
		if (id.Contains(e.ID))
			return;
		
		if (e.HasEnabled<C1>() && e.HasEnabled<C2>())
		{
			processor.Add(new Processor(e));
			id.Add(e.ID);
		}
	}

	void RemoveEntity(Entity e)
	{
		if (!id.Contains(e.ID))
			return;
		
		id.Remove(e.ID);

		for (int i = 0; i < processor.Count; ++i)
		{
			if (processor[i].id == e.ID)
			{
				processor.RemoveAt(i);
				return;
			}
		}
	}



	void SetUP()
	{
		processor.Clear();

		List<C1> c1 = Group<C1>.instance.componentList;
		List<C2> c2 = Group<C2>.instance.componentList;

		var entities1 = from c in c1
			select c.entity;
		var entities2 = from c in c2
			select c.entity;
		var entities = entities1.Intersect(entities2);//.ToList();

		foreach(var entity in entities)
		{
			processor.Add(new Processor(entity));
		}

	}

	void _ProcessUpdate()
	{
		PoolCount = processor.Count;
		for (int i = 0; i < processor.Count; ++i)
		{
			var process = processor[i];
			UpdateSystem(process.c1, process.c2);
		}
	}

	void _ProcessFixedUpdate()
	{
		PoolCount = processor.Count;
		for (int i = 0; i < processor.Count; ++i)
		{
			var process = processor[i];
			FixedUpdateSystem(process.c1, process.c2);
		}
	}

	public virtual void UpdateSystem(C1 c1, C2 c2)
	{}

	public virtual void FixedUpdateSystem(C1 c1, C2 c2)
	{}

	public void ProcessComponents(Action<C1, C2> callback)
	{
		for (int i = 0; i < processor.Count; ++i)
		{
			callback(processor[i].c1, processor[i].c2);
		}
	}

	public void AddEvent<E>(EntityEvent<E> callback)
	{
		EntityManager.instance.AddEvent(callback);
	}

	public void RemoveEvent<E>(EntityEvent<E> callback)
	{
		EntityManager.instance.RemoveEvent(callback);
	}
}
