﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("EntitySystem/MoveRigidbodyViaInput")]
public class MoveRigidbodyViaInput : EntitySystem<InputComponent, RigidbodyComponent>, IFixedUpdate
{
	public override void FixedUpdateSystem (InputComponent input, RigidbodyComponent rigidbody)
	{
		rigidbody.velocity = new Vector3(input.xAxis, input.yAxis, 0);
	}
}
