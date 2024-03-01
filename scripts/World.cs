using Godot;
using System;

public partial class World : Node2D
{
	private TileMap tilemap;
	private Player player;
	
	public override void _Ready() {
		tilemap = GetNode<TileMap>("TileMap");
		player = GetNode<Player>("Player");
		
		player.SetCameraLimits(tilemap);
	}
	
	private void OnCliffsideEntranceBodyEntered(Node2D body) {
		if (body.Name == "Player") {
			GetTree().ChangeSceneToFile("res://scenes/cliffside.tscn");
		}
	}
}
