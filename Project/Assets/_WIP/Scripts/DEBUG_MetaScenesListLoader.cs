using NaughtyAttributes;
using UnityEngine;


[CreateAssetMenu(fileName = "DEBUG_MetaScenesListLoader", menuName = "WIP/DEBUG/Meta Scenes List Loader", order = -1000)]
public class DEBUG_MetaScenesListLoader : ScriptableObject
{
    [ReorderableList]
    public MetaScene[] MetaScenes;


    private int _lastLoadedMetaScene = 0;


    public void LoadNext()
    {
        if (MetaScenes.Length == 0)
            return;

        _lastLoadedMetaScene = _lastLoadedMetaScene >= MetaScenes.Length - 1 ? 0 : _lastLoadedMetaScene + 1;

        MetaScenes[_lastLoadedMetaScene].Load(false);
    }
}
