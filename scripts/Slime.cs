using Godot;
using System;

public partial class Slime : CharacterBody2D
{
	[Signal]
	public delegate void EnemyAttackEventHandler();
	
	public const float Speed = 180.0f;
	private bool chase = false;
	private int health = 100;
	
	private bool inPlayerAttackRange = false;
	
	private Timer damageCooldownTimer;
	private bool damageCooldown = false;
	
	private ProgressBar healthBar;
	
	private Player player;
	
	private AnimationPlayer anim;
	private Sprite2D sprite;
	
	public override void _Ready() {
		anim = GetNode<AnimationPlayer>("AnimationPlayer");
		sprite = GetNode<Sprite2D>("SlimeSprite");
		anim.Play("idle");
		
		player = GetNode<Player>($"/root/{GetTree().CurrentScene.Name}/Player");
		damageCooldownTimer = GetNode<Timer>("DamageCooldown");
		healthBar = GetNode<ProgressBar>("HealthBar");
	}
	
	public override void _PhysicsProcess(double delta) {
		Vector2 velocity = Velocity;
		
		if (chase) {
			velocity = (player.Position - Position).Normalized() * Speed;
			sprite.FlipH = velocity.X < 0;
			anim.Play("walk");
		}
		else {
			velocity = velocity/2;
			anim.Play("idle");
		}
		
		// Combat
		OnPlayerAttack();
		if (health <= 0) { 
			QueueFree(); 
		}
		
		UpdateHealth();
		
		Velocity = velocity;
		MoveAndSlide();
		//MoveAndCollide(Velocity * (float)delta);
	}

	private void OnDetectionAreaBodyEntered(Player body) { chase = true; }
	private void OnDetectionAreaBodyExited(Player body) { chase = false; }
	
	private void OnDamageCooldownTimeout() { damageCooldown = false; }
	
	private void OnPlayerAttack() {
		if (player.enemyInAttackRange & player.isAttacking & !damageCooldown) {
			health -= player.damage;
			damageCooldown = true;
			damageCooldownTimer.Start();
		}
	}
	
	private void UpdateHealth() {
		healthBar.Value = health;
		
		if (health >= 100) {
			healthBar.Visible = false;
		}
		else {
			healthBar.Visible = true;
		}
		
		if (health >= 50) {
			healthBar.Modulate = new Color(0x66923dff);
		}
		else if (health > 0) {
			healthBar.Modulate = new Color(0xe5652eff);
		}
	}
}
