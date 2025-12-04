using UnityEngine;

// When we add some visual position to our gameboard we could enhance it with details
public class CellModelToWorldConverter : ICellModelToWorldConverter
{
    public Vector2 Convert(Vector2Int modelPos)
    {
        return new Vector2(modelPos.x, modelPos.y);
    }
}