using Godot;
using System;

public partial class ButtonLogic : Button
{
    [Export] private GameLogic pauseMenu;

    private CanvasLayer pauseMenuLayer;
    private Node grandParentNode;
    private Node currentScene;

    public override void _Ready()
    {
        currentScene = GetTree().GetCurrentScene();
    }
    
    private void OnVisibilityChanged()
    {
        if (Visible && Name == "Main Menu")
        {
            GrabFocus();
        }
    }

    private void _on_pressed()
    {
        var t = ResourceLoader.Load<PackedScene>("res://LEVEL/level_1.tscn");
        GetTree().Paused = false;
        GetTree().ChangeSceneToPacked(t);
    }

    private void _on_exit_pressed()
    {
        GetTree().Quit();
    }

    private void _on_main_menu_pressed()
    {
        var t = ResourceLoader.Load<PackedScene>("res://LEVEL/Main Menu.tscn");
        GetTree().ChangeSceneToPacked(t);
    }

    private void _on_resume_pressed()
    {
        if (currentScene.Name != "MainMenu")
        {
            var parentNode = GetParent();
            grandParentNode = parentNode.GetParent<CanvasLayer>();
            pauseMenuLayer = (CanvasLayer)grandParentNode;
            pauseMenuLayer.Hide();
        }

        GetTree().Paused = false;
    }
}