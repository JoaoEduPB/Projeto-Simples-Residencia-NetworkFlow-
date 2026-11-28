using Godot;

public partial class Protoboard : CharacterBody2D
{
	[Export] public float Speed = 150.0f;
	private AnimatedSprite2D _sprite;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("Protoboard");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		Velocity = direction * Speed;
		MoveAndSlide();

		if (direction != Vector2.Zero)
		{
			if (direction.X != 0)
				_sprite.FlipH = direction.X > 0;

			_sprite.Play("Andando"); 
		}
		else
		{
			_sprite.Play("Parado");
			_sprite.Frame = 0;
		}
	}
}
