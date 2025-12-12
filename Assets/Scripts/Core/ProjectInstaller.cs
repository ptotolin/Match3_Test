using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Debug.Log($"Project context: Install bindings!");
        
        GameLogger.IsEnabled = false;
    }
}
