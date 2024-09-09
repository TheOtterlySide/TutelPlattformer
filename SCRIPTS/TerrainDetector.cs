using Godot;

namespace Tutel.SCRIPTS;

public partial class TerrainDetector : Area2D
{
    [Signal]
    public delegate void BackgroundChangeEventHandler(int currentTerrain);

    private TileMapLayer currentTilemap;

    private int currentTerrain;

    private int previousTerrain;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void OnBodyShapeEntered(Rid bodyRid, Node2D body, int bodyshapeIndex, int localshapeIndex)
    {
        if (body is TileMapLayer tilemapLayer)
        {
            currentTilemap = tilemapLayer;

            var collided_title = currentTilemap.GetCoordsForBodyRid(bodyRid);
            var tile_Data = currentTilemap.GetCellTileData(collided_title);
            var terrainName = tile_Data.GetCustomDataByLayerId(0);

            SetTerrain(terrainName.AsInt32());
        }
    }

    private void SetTerrain(int id)
    {
        currentTerrain = id;

        //Background Change happened
        if (currentTerrain != previousTerrain)
        {
            previousTerrain = currentTerrain;
            EmitSignal(SignalName.BackgroundChange, currentTerrain);
        }
    }
}