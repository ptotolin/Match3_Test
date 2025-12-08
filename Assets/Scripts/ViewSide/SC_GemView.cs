using System;
using System.Threading.Tasks;
using UnityEngine;

public class SC_GemView : MonoBehaviour
{    
    [SerializeField] private GlobalEnums.GemType type;
    [SerializeField] private GameObject destroyEffectPrefab;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    // locals
    private Transform cachedTransform;

    // properties
    public GlobalEnums.GemType Type => type;
    
    private void Awake()
    {
        cachedTransform = transform;
    }
    
    public void SetMatchColor(GlobalEnums.GemType matchColor)
    {
        if (spriteRenderer == null) {
            return;
        }
        
        spriteRenderer.color = GetColorForGemType(matchColor);
    }
    
    private Color GetColorForGemType(GlobalEnums.GemType gemType)
    {
        switch (gemType)
        {
            case GlobalEnums.GemType.red: return Color.red;
            case GlobalEnums.GemType.blue: return Color.blue;
            case GlobalEnums.GemType.green: return Color.green;
            case GlobalEnums.GemType.yellow: return Color.yellow;
            case GlobalEnums.GemType.purple: return new Color(0.5f, 0f, 0.5f); 
            default: return Color.white;
        }
    }

    public async Task ShowDestroyEffect()
    {
        var destroyEffect = Instantiate(destroyEffectPrefab, cachedTransform.position, Quaternion.identity);
        Destroy(destroyEffect.gameObject, 1);
    }
}