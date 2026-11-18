using Godot;

public partial class Protoboard : AnimatedSprite2D
{
	[Export] public float Speed = 150.0f;

	public override void _PhysicsProcess(double delta)
	{
		// Lê as setas do teclado (esquerda, direita, cima, baixo)
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		if (direction != Vector2.Zero)
		{
			// Move o sprite na direção das setas pressionadas
			Position += direction * Speed * (float)delta;

			// Toca a animação de andar
			Play("Andando");

			// Espelha o sprite quando andar para a direita (já que o desenho original olha para a esquerda)
			if (direction.X != 0)
			{
				FlipH = direction.X > 0;
			}
		}
		else
		{
			// Volta para a animação parado quando nenhuma seta estiver pressionada
			Play("Parado");
		}
	}
}
