using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

enum Direction
{
    North,
    East,
    South,
    West
};

public class DungeonGen : MonoBehaviour
{

    //Max size of dungeon
    int xMax = 80;
    int yMax = 25;

    //Size of the map
    int xSize;
    int ySize;

    //Number of objects
    int objects;

    //Room chance
    int roomChance = 75;

    //2d tile array
    GameObject[] dungeonMap = { };

    public int Corridors
    {
        get;
        private set;
    }

    public static bool IsWall(int x, int y, int xlen, int ylen, int xt, int yt, Direction d)
    {
        Func<int, int, int> a = GetFeatureLowerBound;

        Func<int, int, int> b = IsFeatureWallBound;
        switch (d)
        {
            case Direction.North:
                return xt == a(x, xlen) || xt == b(x, xlen) || yt == y || yt == y - ylen + 1;
            case Direction.East:
                return xt == x || xt == x + xlen - 1 || yt == a(y, ylen) || yt == b(y, ylen);
            case Direction.South:
                return xt == a(x, xlen) || xt == b(x, xlen) || yt == y || yt == y + ylen - 1;
            case Direction.West:
                return xt == x || xt == x - xlen + 1 || yt == a(y, ylen) || yt == b(y, ylen);
        }
        throw new InvalidOperationException();
    }

    public static int GetFeatureLowerBound(int c, int len)
    {
        return c - len / 2;
    }

    public static int IsFeatureWallBound(int c, int len)
    {
        return c + (len - 1) / 2;
    }

    public static int GetFeatureUpperBound(int c, int len)
    {
        return c + (len + 1) / 2;
    }

    public static IEnumerable<PointI> GetRoomPoints(int x, int y, int xlen, int ylen, Direction d)
    {
        // north and south share the same x strategy
        // east and west share the same y strategy
        Func<int, int, int> a = GetFeatureLowerBound;
        Func<int, int, int> b = GetFeatureUpperBound;

        switch (d)
        {
            case Direction.North:
                for (var xt = a(x, xlen); xt < b(x, xlen); xt++) for (var yt = y; yt > y - ylen; yt--) yield return new PointI { X = xt, Y = yt };
                break;
            case Direction.East:
                for (var xt = x; xt < x + xlen; xt++) for (var yt = a(y, ylen); yt < b(y, ylen); yt++) yield return new PointI { X = xt, Y = yt };
                break;
            case Direction.South:
                for (var xt = a(x, xlen); xt < b(x, xlen); xt++) for (var yt = y; yt < y + ylen; yt++) yield return new PointI { X = xt, Y = yt };
                break;
            case Direction.West:
                for (var xt = x; xt > x - xlen; xt--) for (var yt = a(y, ylen); yt < b(y, ylen); yt++) yield return new PointI { X = xt, Y = yt };
                break;
            default:
                yield break;
        }
    }

    public GameObject GetCellType(int x, int y)
    {
        try
        {
            return this.dungeonMap[x + this.xSize * y];
        }
        catch (IndexOutOfRangeException)
        {
            new { x, y }.Dump("exceptional");
            throw;
        }
    }

    public int GetRand(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

}
