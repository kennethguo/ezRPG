using Godot;
using System;

public partial class Cliffside : Node2D
{
	private TileMap tilemap;
	private Player player;
	
	public override void _Ready() {
		tilemap = GetNode<TileMap>("TileMap");
		player = GetNode<Player>("Player");
		
		player.SetCameraLimits(tilemap);
		player.GetNode<AnimationPlayer>("AnimationPlayer").Play("back_idle");
	}

	public override void _Process(double delta) {
		
	}
	
	private void OnWorldEntranceBodyEntered(Node2D body) {
		if (body.Name == "Player") {
			GetTree().ChangeSceneToFile("res://scenes/world.tscn");
		}
	}
}



