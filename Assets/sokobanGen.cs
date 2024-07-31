using System;

public static class SokobanLevelGenerator
{
    static Random rand = new Random();

    public static int[,] GenerateLevel(int width, int height)
    {
        int[,] level = new int[width, height];

        // Initialize boundaries
        for (int i = 0; i < width; i++)
        {
            level[i, 0] = 1;
            level[i, height - 1] = 1;
        }
        for (int j = 0; j < height; j++)
        {
            level[0, j] = 1;
            level[width - 1, j] = 1;
        }

        // Place player
        PlacePlayer(level, width, height);

        // Place boxes and goals
        int numBoxes = 3;
        for (int i = 0; i < numBoxes; i++)
        {
            PlaceBox(level, width, height);
            PlaceGoal(level, width, height);
        }

        // Fill remaining spaces with ground
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (level[i, j] == 0)
                {
                    level[i, j] = 0;
                }
            }
        }

        return level;
    }

    static void PlacePlayer(int[,] level, int width, int height)
    {
        int x,
            y;
        do
        {
            x = rand.Next(1, width - 1);
            y = rand.Next(1, height - 1);
        } while (level[x, y] != 0);
        level[x, y] = 3;
    }

    static void PlaceBox(int[,] level, int width, int height)
    {
        int x,
            y;
        do
        {
            x = rand.Next(1, width - 1);
            y = rand.Next(1, height - 1);
        } while (level[x, y] != 0);
        level[x, y] = 4;
    }

    static void PlaceGoal(int[,] level, int width, int height)
    {
        int x,
            y;
        do
        {
            x = rand.Next(1, width - 1);
            y = rand.Next(1, height - 1);
        } while (level[x, y] != 0);
        level[x, y] = 2;
    }
}
