using Godot;
using System;

public partial class Menu : Node
{
	private Button playButton;
	private Button quitButton;
	
	public override void _Ready() {
		playButton = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer/PlayButton");
		quitButton = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer/QuitButton");
	}

	public override void _Process(double delta) {
		
	}
	
	private void OnPlayButtonPressed() {
		GetTree().CallDeferred("change_scene_to_file","res://scenes/world.tscn");
	}

	
	private void OnQuitButtonPressed() {
		GetTree().Quit();
	}
}
