using UnityEngine;

public static class ScriptableLevelManager
{
    private const string path = "Levels/Level_";

    public static LevelSO LoadLevel(int level)
    {
        var levelScriptable = Resources.Load<LevelSO>(path + level);
        return levelScriptable;
    }
}
