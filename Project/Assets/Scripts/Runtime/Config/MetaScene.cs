using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "MyMetaScene", menuName = "WIP/Config/Meta Scene", order = -1000)]
public class MetaScene : ScriptableObject
{
    [System.Serializable]
    public class SceneLoadMode
    {
        public SceneReference Scene;

        public LocalPhysicsMode PhysicsMode;

        public bool ForceReload = true;
        
        // TODO: BUDGET FOR THE PHYSICS SCENES AND OTHER STUFF

    }


    [Space]
    [Tooltip("The active scene will not be unloaded if you don't provide a Main Scene. Unity needs to have one active scene at all times.")]
    public bool UnloadOtherScenes = true;

    public SceneLoadMode MainScene;

    [Space]
    [ReorderableList]
    public SceneLoadMode[] AdditiveScenes;


    public void Load(bool forceReloadOverride)
    {
        Debug.Log("TODO: MetaScene loading is still WORK IN PROGRESS");

        // LEAVING IT LIKE THIS FOR THE MOMENT. WILL COME BACK SOME OTHER TIME.
        SceneManager.LoadScene(MainScene.Scene, LoadSceneMode.Single);
        foreach (var slm in AdditiveScenes)
            if (slm.Scene != null)
                SceneManager.LoadScene(slm.Scene, LoadSceneMode.Additive);

        LoadMainScene(forceReloadOverride);

        // if no mainscene defined, we still need to unloadotherscenes

        // do not unload the other scenes if they are included in the array and they are not marked as forceReload

        LoadAdditiveScenes(forceReloadOverride);
    }


    private void LoadMainScene(bool forceReloadOverride)
    {
        if (MainScene.Scene == null)
            return;

        var reload = forceReloadOverride || MainScene.ForceReload;

        // TODO: figure this out 
        // if unloadOtherScenes && reload -> Load single
        // if !unloadOtherScenes && reload -> Load additive, set active, ¿should we ask if we should unload the old active scene?
        // if unloadOtherScenes && !reload -> ..............?
        // if !unloadOtherScenes && !reload -> ..............?
    }


    private void LoadAdditiveScenes(bool forceReloadOverride)
    {
        // TODO

        foreach (var slm in AdditiveScenes)
        {
            if (slm.Scene == null)
                continue;

            // TODO
        }
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MetaScene.SceneLoadMode))]
public class SceneLoadModePropertyDrawer : PropertyDrawer
{
    static readonly RectOffset kBoxPadding = EditorStyles.helpBox.padding;


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var sceneProp = property.FindPropertyRelative("Scene");
        var physicsModeProp = property.FindPropertyRelative("PhysicsMode");
        var forceReloadProp = property.FindPropertyRelative("ForceReload");

        EditorGUI.PropertyField(position, sceneProp);

        // Compensate the Box that SceneReference draws. We want to draw our stuff inside that box without drawing a new one.
        position = kBoxPadding.Remove(position);
        position.y += EditorGUI.GetPropertyHeight(sceneProp);
        position.y -= EditorGUIUtility.singleLineHeight;
        position.y += kBoxPadding.top;

        position.height = EditorGUI.GetPropertyHeight(physicsModeProp);
        EditorGUI.PropertyField(position, physicsModeProp);
        position.y += position.height;

        position.height = EditorGUI.GetPropertyHeight(forceReloadProp);
        EditorGUI.PropertyField(position, forceReloadProp);
    }


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var sceneProp = property.FindPropertyRelative("Scene");
        var physicsModeProp = property.FindPropertyRelative("PhysicsMode");
        var forceReloadProp = property.FindPropertyRelative("ForceReload");

        return EditorGUI.GetPropertyHeight(sceneProp) + EditorGUI.GetPropertyHeight(physicsModeProp) + EditorGUI.GetPropertyHeight(forceReloadProp);
    }

}
#endif