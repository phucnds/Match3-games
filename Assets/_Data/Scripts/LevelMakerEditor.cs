using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class LevelMakerEditor : EditorWindow
{
    private LevelSO selectedLevel;

    [MenuItem("Match3/Level Maker")]
    public static void ShowWindow()
    {
        GetWindow<LevelMakerEditor>("Level Maker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Editor", EditorStyles.boldLabel);

        selectedLevel = (LevelSO)EditorGUILayout.ObjectField("Level Data", selectedLevel, typeof(LevelSO), false);

        if (selectedLevel != null)
        {
            EditorGUILayout.Space();
            selectedLevel.width = EditorGUILayout.IntField("Width", selectedLevel.width);
            selectedLevel.height = EditorGUILayout.IntField("Height", selectedLevel.height);

            GUILayout.Space(10);

            selectedLevel.moveAmount = EditorGUILayout.IntField("Move amount", selectedLevel.moveAmount);
            selectedLevel.targetScore = EditorGUILayout.IntField("Target score", selectedLevel.targetScore);

            GUIGameField();

            if (GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(selectedLevel);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please select a LevelSO asset.", MessageType.Warning);
        }
    }

    private void GUIGameField()
    {
        selectedLevel.Initialize();

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Field board: ", EditorStyles.label, GUILayout.Width(100));
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        for (int x = 0; x < selectedLevel.width; x++)
        {
            GUILayout.BeginVertical();
            for (int y = selectedLevel.height - 1; y >= 0; y--)
            {
                int index = y * selectedLevel.width + x;
                int state = (int)selectedLevel.levelGridPositionList[index].type;

                Texture2D texture = selectedLevel.levelGridPositionList[index].texture;

                if (GUILayout.Button(texture, GUILayout.Width(50), GUILayout.Height(50)))
                {
                    int n = 1 - state;

                    if (System.Enum.TryParse(n.ToString(), out SquareTypes types))
                    {
                        selectedLevel.levelGridPositionList[index].type = types;
                        selectedLevel.levelGridPositionList[index].texture = selectedLevel.textures[n];
                    }
                }
            }

            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(20);
    }
}