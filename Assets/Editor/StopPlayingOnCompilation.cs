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
             EditorApplication.playModeStateChanged
                  += PlaymodeChanged;
             waitingForStop = true;
        }
    }

    static void PlaymodeChanged(PlayModeStateChange state)
    {
        // В 6000.6 у PlayModeStateChange нет ExitedPlayMode: выход из режима игры
        // завершается переходом в EnteredEditMode (эквивалент прежней проверки !isPlaying).
        if( state != PlayModeStateChange.EnteredEditMode )
             return;

        EditorApplication.UnlockReloadAssemblies();
        EditorApplication.playModeStateChanged
             -= PlaymodeChanged;
        waitingForStop = false;
    }
}