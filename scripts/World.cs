using Godot;
using System;

public partial class World : Node2D
{
	private TileMap tilemap;
	private Player player;
	
	public override void _Ready() {
		tilemap = GetNode<TileMap>("TileMap");
		player = GetNode<Player>("Player");
		
		SetCameraLimits();
	}

	private void SetCameraLimits() {
		var mapLimits = tilemap.GetUsedRect();
		var mapCellSize = tilemap.TileSet.TileSize;
		var mapScale = tilemap.Scale.X;
		
		player.GetNode<Camera2D>("Camera2D").LimitLeft = (int)(mapLimits.Position.X * mapCellSize.X * mapScale);
		player.GetNode<Camera2D>("Camera2D").LimitRight = (int)(mapLimits.End.X * mapCellSize.X * mapScale);
		player.GetNode<Camera2D>("Camera2D").LimitTop = (int)(mapLimits.Position.Y * mapCellSize.Y * mapScale);
		player.GetNode<Camera2D>("Camera2D").LimitBottom = (int)(mapLimits.End.Y * mapCellSize.Y * mapScale);
	}
	
	private void OnCliffsideEntranceBodyEntered(Node2D body) {
		if (body.Name == "Player") {
			GetTree().ChangeSceneToFile("res://scenes/cliffside.tscn");
		}
	}
	
	private void TestFunc() {
		// Empty func for git test
	}
}
