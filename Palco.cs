using Godot;

public partial class Palco : Control
{
	[Export] public AnimatedSprite2D Sprite;

	public override void _Ready()
	{
		Resized += Centralizar;
		Centralizar();
	}

	private void Centralizar()
	{
		if (Sprite != null)
			Sprite.Position = Size / 2;
	}
}
