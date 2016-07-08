using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    static void Main(string[] args)
    {
        // game loop
        while (true)
        {
            int maxH = -1;
            int res = 0;
            for (int i = 0; i < 8; i++)
            {
                int mountainH = int.Parse(Console.ReadLine()); // represents the height of one mountain, from 9 to 0.
                if (mountainH > maxH) {
                    maxH = mountainH;
                    res = i;
                }
            }

            Console.WriteLine(res); // The number of the mountain to fire on.
        }
    }
}