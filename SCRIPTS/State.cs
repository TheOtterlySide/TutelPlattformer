using Godot;
using System;

public partial class State : Node
{
	public Tutel.SCRIPTS.StateMachine fsm;
	
	public virtual void Enter() {}
	public virtual void Exit() {}
	
	public virtual void MakeReady() {}
	public virtual void Update(float delta) {}
	public virtual void PhysicsUpdate(float delta) {}
	public virtual void HandleInput(InputEvent @event) {}
}
