using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class StopPlayingOnCompilation
{
    static bool waitingForStop = false;
 
    static StopPlayingOnCompilation()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    static void OnEditorUpdate()
    {
        if( ! waitingForStop 
            && EditorApplication.isCompiling 
            && EditorApplication.isPlaying )
        {
             EditorApplication.LockReloadAssemblies();
            EditorApplication.isPlaying = false;
             EditorApplication.playmodeStateChanged
                  += PlaymodeChanged;
             waitingForStop = true;
        }
    }

    static void PlaymodeChanged()
    {
        if( EditorApplication.isPlaying )
             return;
        
        EditorApplication.UnlockReloadAssemblies();
        EditorApplication.playmodeStateChanged
             -= PlaymodeChanged;
        waitingForStop = false;
    }
}