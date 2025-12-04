using UnityEngine;

public class GlobalEnums : MonoBehaviour
{
    public enum GemType
    {
        blue,
        green,
        red,
        yellow,
        purple,
        bomb
    };

    public enum GameState
    {
        wait,
        move
    };

    public enum GemSpawnType
    {
        Instant,
        FallFromTop,
        Appear
    };
}
