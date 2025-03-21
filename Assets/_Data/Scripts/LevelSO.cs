using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LevelSO : ScriptableObject
{
    public int width;
    public int height;

    public int moveAmount;
    public int targetScore;

    public Texture2D[] textures;

    public List<GemSO> gemList;

    public LevelGridPosition[] levelGridPositionList;


    public void Initialize()
    {
        if (levelGridPositionList == null || levelGridPositionList.Length != width * height)
        {
            levelGridPositionList = new LevelGridPosition[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    levelGridPositionList[index] = new LevelGridPosition { gemSO = null, x = x, y = y, type = SquareTypes.EmptySquare, texture = textures[0] };
                }
            }

        }
    }

    [System.Serializable]
    public class LevelGridPosition
    {
        public GemSO gemSO;
        public int x;
        public int y;
        public SquareTypes type;
        public Texture2D texture;
    }
}
