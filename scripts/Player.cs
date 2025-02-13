using Godot;
using System;

public partial class Player : CharacterBody2D
{
	// Speed
	public static float defaultSpeed = 300.0f;
	public float speed = defaultSpeed;
	private float sprintModifier = 1.8f;
	private Vector2 prevDirection = Vector2.Zero;
	
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
	public int damage = 20;
	private Timer damageCooldownTimer;
	private bool inEnemyAttackRange = false;
	private bool enemyAttackCooldown = false;
	
	// Health
	private static int health = 100;
	private ProgressBar healthBar;
	private Timer regenTimer;
	private bool isAlive = true;
	private Timer respawnTimer;
	
	// Animations
	private AnimationPlayer anim;
	private Sprite2D sprite;
	
	public override void _Ready() {
		anim = GetNode<AnimationPlayer>("AnimationPlayer");
		sprite = GetNode<Sprite2D>("PlayerSprite");
		
		attackRange = GetNode<Area2D>("AttackRange");
		attackCooldownTimer = GetNode<Timer>("AttackCooldown");
		damageCooldownTimer = GetNode<Timer>("DamageCooldown");
		
		healthBar = GetNode<ProgressBar>("HealthBar");
		regenTimer = GetNode<Timer>("RegenTimer");
		respawnTimer = GetNode<Timer>("RespawnTimer");
		
		anim.Play("side_idle");
	}

	public override void _PhysicsProcess(double delta) {
		Vector2 velocity = Velocity;
		
		if (isAlive) {
			// Gets normalized vector for x and y movement
			Vector2 direction = Input.GetVector("left", "right", "up", "down");
			if (Input.IsActionPressed("sprint")) {
				speed = defaultSpeed * sprintModifier;
			}
			else {
				speed = defaultSpeed;
			}
			velocity = direction * speed;
			
			bool isIdle = velocity==Vector2.Zero;
			if (!isAttacking & isAlive) { PlayMovementAnimation(prevDirection, isIdle); }
			PlayAttackAnimation(prevDirection);
			
			// Only update direction if player starts moving again
			if (!isIdle) { prevDirection = direction; }
			
			// Combat
			OnEnemyAttack();
			
			//Health
			UpdateHealth();

			Velocity = velocity;
			MoveAndSlide();
		}
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
		else if ((string)anim_name == "death") {
			respawnTimer.Start();
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
	
	private void PlayDeathAnimation() {
		anim.SpeedScale = 1;
		anim.Play("death");
	}
	
	private void OnAttackRangeBodyEntered(Node2D body) {
		if (body.IsInGroup("enemy")) {
			enemyInAttackRange = true;
		}
	}
	
	private void OnAttackRangeBodyExited(Node2D body) {
		if (body.IsInGroup("enemy")) {
			enemyInAttackRange = false;
		}
	}
	
	private void OnHitboxBodyEntered(Node2D body) {
		if (body.IsInGroup("enemy")) {
			inEnemyAttackRange = true;
		}
	}
	
	private void OnHitboxBodyExited(Node2D body) {
		if (body.IsInGroup("enemy")) {
			inEnemyAttackRange = false;
		}
	}
	
	private void OnEnemyAttack() {
		if (inEnemyAttackRange & !enemyAttackCooldown) { 
			health -= 30;
			enemyAttackCooldown = true; // Cooldown should be for each enemy but I can't be bothered fixing
			damageCooldownTimer.Start();
			// Restart regen when taking damage
			regenTimer.Start();
		}
	}
	
	private void OnAttackCooldownTimeout() { attackCooldown = false; }
	private void OnDamageCooldownTimeout() { enemyAttackCooldown = false; }
	
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
		else if (health <= 0) {
			isAlive = false;
			PlayDeathAnimation();
		}
	}
	
	private void OnRegenTimerTimeout() {
		if (health > 0 & health < 100) {
			health += 10; 
		}
	}
	
	private void OnRespawnTimerTimeout() {
		isAlive = true;
		health = 100;
		GetTree().CallDeferred("change_scene_to_file","res://scenes/world.tscn");
	}
}
