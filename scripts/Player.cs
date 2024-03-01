using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	private Vector2 prevDirection = Vector2.Zero;
	private int health = 100;
	private bool alive = true;
	
	private AnimationPlayer anim;
	private Sprite2D sprite;
	
	// Attack
	private Area2D attackRange;
	private Vector2 attackPos;
	private Vector2 sideAttackPos = new Vector2 (7,-4);
	private Vector2 frontAttackPos = new Vector2 (2.5f,1);
	private Vector2 backAttackPos = new Vector2 (0,-9);
	private Timer attackCooldownTimer;
	private bool attackCooldown = false;
	public bool isAttacking = false;
	public bool enemyInAttackRange = false;
	
	// Damage
	private Timer damageCooldownTimer;
	private bool inEnemyAttackRange = false;
	private bool enemyAttackCooldown = false;
	
	public override void _Ready() {
		anim = GetNode<AnimationPlayer>("AnimationPlayer");
		sprite = GetNode<Sprite2D>("PlayerSprite");
		
		attackRange = GetNode<Area2D>("AttackRange");
		attackCooldownTimer = GetNode<Timer>("AttackCooldown");
		damageCooldownTimer = GetNode<Timer>("DamageCooldown");

		anim.Play("front_idle");
	}

	public override void _PhysicsProcess(double delta) {
		Vector2 velocity = Velocity;
		
		// Gets normalized vector for x and y movement
		Vector2 direction = Input.GetVector("left", "right", "up", "down");
		velocity = direction * Speed;
		
		bool isIdle = velocity==Vector2.Zero;
		if (!isAttacking) { PlayMovementAnimation(prevDirection, isIdle); }
		PlayAttackAnimation(prevDirection);
		
		// Only update direction if player starts moving again
		if (!isIdle) { prevDirection = direction; }
		
		// Combat
		OnEnemyAttack();
		if (health <= 0) {
			alive = false;
			GD.Print("You Died!");
		}

		Velocity = velocity;
		MoveAndSlide();
	}
	
	public void SetCameraLimits(TileMap tilemap) {
		var mapLimits = tilemap.GetUsedRect();
		var mapCellSize = tilemap.TileSet.TileSize;
		var mapScale = tilemap.Scale.X;
		
		GetNode<Camera2D>("Camera2D").LimitLeft = (int)(mapLimits.Position.X * mapCellSize.X * mapScale);
		GetNode<Camera2D>("Camera2D").LimitRight = (int)(mapLimits.End.X * mapCellSize.X * mapScale);
		GetNode<Camera2D>("Camera2D").LimitTop = (int)(mapLimits.Position.Y * mapCellSize.Y * mapScale);
		GetNode<Camera2D>("Camera2D").LimitBottom = (int)(mapLimits.End.Y * mapCellSize.Y * mapScale);
	}
	
	private void PlayAttackAnimation(Vector2 direction) {
		if (Input.IsActionJustPressed("attack") & !attackCooldown) {
			isAttacking = true;
			anim.SpeedScale = 2;
			if (direction.X != 0) {
				attackPos = new Vector2 (sideAttackPos.X * Math.Sign(direction.X), sideAttackPos.Y);
				anim.Play("side_attack");
			}
			else if (direction.Y > 0) {
				attackPos = frontAttackPos;
				anim.Play("front_attack");
			}
			else if (direction.Y < 0) {
				attackPos = backAttackPos;
				anim.Play("back_attack");
			}
			attackRange.GetNode<CollisionShape2D>("AttackCollision").Position = attackPos;
		}
	}
	
	private void OnAnimationPlayerAnimationFinished(StringName anim_name) {
		if (((string)anim_name).EndsWith("attack")) { 
			isAttacking = false;
			attackCooldown = true;
			attackCooldownTimer.Start();
		}
	}
	
	private void PlayMovementAnimation(Vector2 direction, bool idle) {
		anim.SpeedScale = 1;
		if (direction.X != 0) {
			sprite.FlipH = direction.X < 0;
			if (idle) { anim.Play("side_idle"); }
			else { anim.Play("side_walk"); }
		}
		else if (direction.Y > 0) {
			sprite.FlipH = false;
			if (idle) { anim.Play("front_idle"); }
			else { anim.Play("front_walk"); }
		}
		else if (direction.Y < 0) {
			sprite.FlipH = false;
			if (idle) { anim.Play("back_idle"); }
			else { anim.Play("back_walk"); }
		}
	}
	
	private void OnAttackRangeBodyEntered(Node2D body) {
		if (body.Name == "Slime") {
			enemyInAttackRange = true;
		}
	}
	
	private void OnAttackRangeBodyExited(Node2D body) {
		if (body.Name == "Slime") {
			enemyInAttackRange = false;
		}
	}
	
	private void OnHitboxBodyEntered(Node2D body) {
		if (body.Name == "Slime") {
			inEnemyAttackRange = true;
		}
	}
	
	private void OnHitboxBodyExited(Node2D body) {
		if (body.Name == "Slime") {
			inEnemyAttackRange = false;
		}
	}
	
	private void OnEnemyAttack() {
		if (inEnemyAttackRange & !enemyAttackCooldown) {
			health -= 10;
			enemyAttackCooldown = true;
			damageCooldownTimer.Start();
		}
	}
	
	private void OnAttackCooldownTimeout() { attackCooldown = false; }
	private void OnDamageCooldownTimeout() { enemyAttackCooldown = false; }
}
