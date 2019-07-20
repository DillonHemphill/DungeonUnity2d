using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Tile structure
public enum Tile
{
    Unused = ' ',
    Floor = '.',
    Corridor = ',',
    Wall = '#',
    ClosedDoor = '+',
    OpenDoor = '-',
    UpStairs = '<',
    DownStairs = '>'
};

//Directions + count
public enum Direction
{
    North,
    East,
    South,
    West,
    DirectionCount
};

//Dungeon Generator class
public class DungeonGen : MonoBehaviour
{
    //A public rectange structure
    private struct Rect
    {
        public int x { get; set; }
        public int y { get; set; }
        public int width, height;
    }

    //The Tile Object
    [SerializeField]
    private GameObject tileObject;

    //Width of the tile generated structure
    [SerializeField]
    private int width;
    //Height of the tile generated structure
    [SerializeField]
    private int height;
    //Number of structures
    [SerializeField]
    private int numbOfStructures;
    //Minum amount of rooms
    [SerializeField]
    private int minRoomSize;
    //Max amount of rooms
    [SerializeField]
    private int maxRoomSize;
    //Minum corridor length
    [SerializeField]
    private int minCorridorLength;
    //Maxium corridor length
    [SerializeField]
    private int maxCorridorLength;
    [SerializeField]
    private int roomChance;
    //Tiles list structure
    private List<Tile> tiles;
    //Rooms list structure
    private List<Rect> rooms;
    //Rect list structure
    private List<Rect> exits;

    //Start of execution
    private void Start()
    {
        //Create an width * height size tile structure and fill with Unused tiles
        tiles = new List<Tile>();
        for (int i = 0; i < width*height; i++)
        {
            tiles.Add(Tile.Unused);
        }
        //Create empty list of rooms for filling
        rooms = new List<Rect>(width * height);
        //Create empty list of height for filling
        exits = new List<Rect>(width * height);
        //Generate map
        Generate(numbOfStructures);
        //Instantiate objects in the Scene
        CreateObjects();
    }

    //Instantiate all the tiles in the last by using getTile, and then changing color based on type of tile
    private void CreateObjects()
    {
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                GameObject newTile = Instantiate(tileObject, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                Tile newTileEnum = getTile(x, y);
                //Closed Door Structure Tile Color = blue
                if(newTileEnum== Tile.ClosedDoor)
                {
                    newTile.GetComponent<SpriteRenderer>().color = Color.blue;
                }
                //Corridor Tile Color = black
                else if (newTileEnum == Tile.Corridor)
                {
                    newTile.GetComponent<SpriteRenderer>().color = Color.black;
                }
                //Downstairs Structure Tile Color = green
                else if (newTileEnum == Tile.DownStairs)
                {
                    newTile.GetComponent<SpriteRenderer>().color = Color.red;
                }
                //OpenDoor Structure Tile Color = yellow
                else if (newTileEnum == Tile.OpenDoor)
                {
                    newTile.GetComponent<SpriteRenderer>().color = Color.yellow;
                }
                //UpStairs Structure Tile Color = green
                else if (newTileEnum == Tile.UpStairs)
                {
                    newTile.GetComponent<SpriteRenderer>().color = Color.green;
                }
                //Wall tile Color = gray
                else if (newTileEnum == Tile.Wall)
                {
                    newTile.GetComponent<SpriteRenderer>().color = Color.gray;
                }
            }
                
        }
    }

    //Generate the duegon based on Version 3 of "C++ Example of Dungeon-Building Algorithm" by MindControlDx
    private void Generate(int maxFeatures)
    {
        // place the first room in the center
        if (!makeRoom(width / 2, height / 2, (Direction)randomInt(4), true))
        {
            return;
        }

        // we already placed 1 feature (the first room) so start at 1, then create remaining features
        for (int i = 1; i < maxFeatures; ++i)
        {
            if (!createFeature())
            {
                break;
            }
        }

        //Place an UpStairs structure on the map
        if (!placeObject(Tile.UpStairs))
        {
            return;
        }

        //Place an DownStairs structure on the map
        if (!placeObject(Tile.DownStairs))
        {
            return;
        }

        //Set all unused and non structure tiles to floor. Corridors are not used in this case
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] == Tile.Unused)
                tiles[i] = Tile.Floor;
            else if (tiles[i] == Tile.Floor || tiles[i] == Tile.Corridor)
                tiles[i] = Tile.Unused;
        }
    }

    //Get tile from list based on x & y
    private Tile getTile(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height)
            return Tile.Unused;
        return tiles[x + y * width];
    }

    //Set tile from list based on x & y
    private void setTile(int x, int y, Tile tile)
    {
        tiles[x + y * width] = tile;
    }

    //Default initalized createFeature
    private bool createFeature()
    {
        for (int i = 0; i < 1000; ++i)
        {
            if (exits.Count == 0)
                break;

            // choose a random side of a random room or corridor
            int r = randomInt(exits.Count);
            int x = randomInt(exits[r].x, exits[r].x + exits[r].width - 1);
            int y = randomInt(exits[r].y, exits[r].y + exits[r].height - 1);

            // north, south, west, east
            for (int j = 0; j < 4; ++j)
            {
                if (createFeature(x, y, (Direction)j))
                {
                    exits.RemoveAt(0 + r);
                    return true;
                }
            }
        }
        return false;
    }

    //Create feature with direction
    private bool createFeature(int x, int y, Direction dir)
    {
       

        int dx = 0;
        int dy = 0;
        //Directions
        if (dir == Direction.North)
            dy = 1;
        else if (dir == Direction.South)
            dy = -1;
        else if (dir == Direction.West)
            dx = 1;
        else if (dir == Direction.East)
            dx = -1;

        //Check if tile has structure
        if (getTile(x + dx, y + dy) != Tile.Floor && getTile(x + dx, y + dy) != Tile.Corridor)
            return false;
        
        //Roll a 100 chance to decide if to create room or make corridor
        if (randomInt(100) < roomChance)
        {
            if (makeRoom(x, y, dir))
            {
                setTile(x, y, Tile.ClosedDoor);
                return true;
            }
        }
        else
        {
            if (makeCorridor(x, y, dir))
            {
                if (getTile(x + dx, y + dy) == Tile.Floor)
                    setTile(x, y, Tile.ClosedDoor);
                else // don't place a door between corridors
                    setTile(x, y, Tile.Corridor);

                return true;
            }
        }
        return false;
    }

    //Make room at x & y
    private bool makeRoom(int x, int y, Direction dir, bool firstRoom = false)
    {

        Rect room = new Rect();
        room.width = randomInt(minRoomSize, maxRoomSize);
        room.height = randomInt(minRoomSize, maxRoomSize);

        if (dir == Direction.North)
        {
            room.x = x - room.width / 2;
            room.y = y - room.height;
        }

        else if (dir == Direction.South)
        {
            room.x = x - room.width / 2;
            room.y = y + 1;
        }

        else if (dir == Direction.West)
        {
            room.x = x - room.width;
            room.y = y - room.height / 2;
        }

        else if (dir == Direction.East)
        {
            room.x = x + 1;
            room.y = y - room.height / 2;
        }

        if (placeRect(room, Tile.Floor))
        {
            rooms.Add(room);

            if (dir != Direction.South || firstRoom)
            {
                Rect newRect = new Rect();
                newRect.x = room.x;
                newRect.y = room.y - 1;
                newRect.width = room.width;
                newRect.height = 1;
                exits.Add(newRect);
            }
            if (dir != Direction.North || firstRoom)
            {
                Rect newRect = new Rect();
                newRect.x = room.x;
                newRect.y = room.y + room.height;
                newRect.width = room.width;
                newRect.height = 1;
                exits.Add(newRect);
            }
            if (dir != Direction.East || firstRoom)
            {
                Rect newRect = new Rect();
                newRect.x = room.x - 1;
                newRect.y = room.y;
                newRect.width = 1;
                newRect.height = room.height;
                exits.Add(newRect);
            }
            if (dir != Direction.West || firstRoom)
            {
                Rect newRect = new Rect();
                newRect.x = room.x+room.width;
                newRect.y = room.y;
                newRect.width = 1;
                newRect.height = room.height;
                exits.Add(newRect);
            }
            return true;
        }
        return false;
    }

    //Make Corridor at x & y
    private bool makeCorridor(int x, int y, Direction dir)
    {
        Rect corridor = new Rect();
        corridor.x = x;
        corridor.y = y;

        if (randomBool()) //Vertical corridor
        {
            corridor.width = randomInt(minCorridorLength, maxCorridorLength);
            corridor.height = 1;

            if (dir == Direction.North)
            {
                corridor.y = y - 1;
                if (randomBool()) 
                    corridor.x = x - corridor.width + 1;
            }
            else if (dir == Direction.South)
            {
                corridor.y = y + 1;
                if (randomBool()) 
                    corridor.x = x - corridor.width + 1;
            }
            else if (dir == Direction.West)
                corridor.x = x - corridor.width;
            else if (dir == Direction.East)
                corridor.x = x + 1;
        }

        else //Horrizontal corridor
        {
            corridor.width = 1;
            corridor.height = randomInt(minCorridorLength, maxCorridorLength);

            if (dir == Direction.North)
                corridor.y = y - corridor.height;
            else if (dir == Direction.South)
                corridor.y = y + 1;
            else if (dir == Direction.West)
            {
                corridor.x = x - 1;
                if (randomBool()) 
                    //Gen South
                    corridor.y = y - corridor.height + 1;
            }

            else if (dir == Direction.East)
            {
                corridor.x = x + 1;
                if (randomBool()) 
                    //Gen North 
                    corridor.y = y - corridor.height + 1;
            }
        }

        if (placeRect(corridor, Tile.Corridor))
        {
            // north side
            if (dir != Direction.South && corridor.width != 1)
            {
                Rect newRect = new Rect();
                newRect.x = corridor.x;
                newRect.y = corridor.y - 1;
                newRect.width = corridor.width;
                newRect.height = 1;
                exits.Add(newRect);
            }
            // south side
            if (dir != Direction.North && corridor.width != 1)
            {
                Rect newRect = new Rect();
                newRect.x = corridor.x;
                newRect.y = corridor.y + corridor.height;
                newRect.width = corridor.width;
                newRect.height = 1;
                exits.Add(newRect);
            }
            // west side
            if (dir != Direction.East && corridor.height != 1)
            {
                Rect newRect = new Rect();
                newRect.x = corridor.x-1;
                newRect.y = corridor.y;
                newRect.width = 1;
                newRect.height = corridor.height;
                exits.Add(newRect);
            }
            // east side
            if (dir != Direction.West && corridor.height != 1)
            {
                Rect newRect = new Rect();
                newRect.x = corridor.x+corridor.width;
                newRect.y = corridor.y;
                newRect.width = 1;
                newRect.height = corridor.height;
                exits.Add(newRect);
            }
            return true;
        }
        return false;
    }

    //Place Rect with a certain tile type
    private bool placeRect(Rect rect, Tile tile)
	{
		if (rect.x< 1 || rect.y< 1 || rect.x + rect.width> width - 1 || rect.y + rect.height> height - 1)
			return false;
 
		for (int y = rect.y; y<rect.y + rect.height; ++y)
        {
            for (int x = rect.x; x < rect.x + rect.width; ++x)
            {
                if (getTile(x, y) != Tile.Unused)
                    return false; // the area already used
            }
        }
			
 
		for (int y = rect.y - 1; y<rect.y + rect.height + 1; ++y)
        {
            for (int x = rect.x - 1; x < rect.x + rect.width + 1; ++x)
            {
                if (x == rect.x - 1 || y == rect.y - 1 || x == rect.x + rect.width || y == rect.y + rect.height)
                    setTile(x, y, Tile.Wall);
                else
                    setTile(x, y, tile);
            }
        }
		return true;
	}

    //Place tile object randomly on the map
    private bool placeObject(Tile tile)
    {
        //Check that user has set room amount
        if (rooms.Count > 0)
            return false;

        // choose a random room
        int r = randomInt(rooms.Count); 
        int x = randomInt(rooms[r].x + 1, rooms[r].x + rooms[r].width - 2);
        int y = randomInt(rooms[r].y + 1, rooms[r].y + rooms[r].height - 2);

        if (getTile(x, y) == Tile.Floor)
        {
            setTile(x, y, tile);
            // place one object in one room (optional)
            rooms.RemoveAt(0 + r);
            return true;
        }
        return false;
    }

    //Random int helpers
    private int randomInt(int exclusiveMax)
    {
        System.Random r = new System.Random();
        int rInt = r.Next(0, exclusiveMax);
        return rInt;
    }
    private int randomInt(int min, int max)
    {
        System.Random r = new System.Random();
        int rInt = r.Next(min,max);
        return rInt;

    }

    //Random bool healpers
    private bool randomBool()
    {
        bool Boolean = (UnityEngine.Random.value > 0.5f);
        return Boolean;
    }




}
