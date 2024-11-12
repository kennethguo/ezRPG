using Godot;
using System;

public partial class World : Node2D
{
	private TileMap tilemap;
	private Player player;
	
	public override void _Ready() {
		tilemap = GetNode<TileMap>("TileMap");
		player = GetNode<Player>("Player");
		
		// Set camera limits based on tilemap
		player.SetCameraLimits(tilemap);
		// Set player spawn point
		player.Position = GlobalVariables.spawnPosition;
	}
	
	private void OnCliffsideEntranceBodyEntered(Node2D body) {
		if (body.Name == "Player") {
			// Below works with warnings: https://github.com/godotengine/godot/issues/85852
			// GetTree().ChangeSceneToFile("res://scenes/cliffside.tscn");
			GetTree().CallDeferred("change_scene_to_file","res://scenes/cliffside.tscn");
		}
	}
	
	private void OnWorldExitBodyEntered(Node2D body) {
		if (body.Name == "Player") {
			GetTree().CallDeferred("change_scene_to_file","res://scenes/menu.tscn");
			GlobalVariables.spawnPosition = new Vector2 (60,250);
		}
	}
}
