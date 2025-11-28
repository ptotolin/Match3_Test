using System;
using System.Threading.Tasks;
using UnityEngine;

public class SC_GemView : MonoBehaviour
{    
    [SerializeField] private GlobalEnums.GemType type;
    [SerializeField] private GameObject destroyEffectPrefab;
    
    // locals
    private Transform cachedTransform;

    // properties
    public GlobalEnums.GemType Type => type;
    
    private void Awake()
    {
        cachedTransform = transform;
    }

    public async Task ShowDestroyEffect()
    {
        var destroyEffect = Instantiate(destroyEffectPrefab, cachedTransform.position, Quaternion.identity);
        Destroy(destroyEffect.gameObject, 1);
        
        Destroy(gameObject);
    }
}