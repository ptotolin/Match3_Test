using UnityEngine;

public interface ICellModelToWorldConverter
{
    Vector2 Convert(Vector2Int modelPos);
}