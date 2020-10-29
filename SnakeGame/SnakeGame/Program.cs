using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SnakeGame
{
    class Program
    {







        static void Main(string[] args)
        {
            // start game
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            

            int snakelength = 3;
            string moving = "RIGHT";
            string moved = "";
            char ch = '*';
       


            // display this char on the console during the game

            bool gameLive = true;
            ConsoleKeyInfo consoleKey; // holds whatever key is pressed


            // location info & display
            int x = 5, y = 5; // y is 2 to allow the top row for directions & space
            int dx = 1, dy = 0;
            int consoleWidthLimit = 79;
            int consoleHeightLimit = 24;
            int prex, prey;
            // clear to color
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.Clear();

            // delay to slow down the character movement so you can see it
            int delayInMillisecs = 50;

            Random rand = new Random();
            // Set obstacle position first
            // Given max value is exclusive and min is inclusive, make sure obstacle and food is not equal to 0
            int obstacleX = rand.Next(1, consoleWidthLimit);
            int obstacleY = rand.Next(1, consoleHeightLimit);
            int foodX = rand.Next(1, consoleWidthLimit);
            int foodY = rand.Next(1, consoleHeightLimit);

            // whether to keep trails
            bool trail = false;

            do // until escape
            {
                // print directions at top, then restore position
                // save then restore current color
                ConsoleColor cc = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Black;
                // Set obstacle position
                Console.SetCursorPosition(obstacleX, obstacleY);
                Console.Write("|");
                // Set food position
                Console.SetCursorPosition(foodX, foodY);
                Console.Write("+");
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("Arrows move up/down/right/left. Press 'esc' quit.");
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = cc;

                // see if a key has been pressed
                if (Console.KeyAvailable)
                {
                    // get key and use it to set options
                    consoleKey = Console.ReadKey(true);
                    switch (consoleKey.Key)
                    {

                        case ConsoleKey.UpArrow: //UP
                            dx = 0;
                            dy = -1;
                            Console.ForegroundColor = ConsoleColor.Red;
                            moved = moving;
                            moving = "UP";
                            break;
                        case ConsoleKey.DownArrow: // DOWN
                            dx = 0;
                            dy = 1;
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            moved = moving;
                            moving = "DOWN";

                            break;
                        case ConsoleKey.LeftArrow: //LEFT
                            dx = -1;
                            dy = 0;
                            Console.ForegroundColor = ConsoleColor.Green;
                            moved = moving;
                            moving = "LEFT";

                            break;
                        case ConsoleKey.RightArrow: //RIGHT
                            dx = 1;
                            dy = 0;
                            Console.ForegroundColor = ConsoleColor.Black;
                            moved = moving;
                            moving = "RIGHT";
                            break;
                        case ConsoleKey.Escape: //END

                            gameLive = false;
                            moving = "STOP";
                            break;
                    }
                }

                // find the current position in the console grid & erase the character there if don't want to see the trail
                Console.SetCursorPosition(x, y);
               
                if (trail == false)
                {
                    for (int i = 0; i < snakelength; i++)
                    {
                        if (moving == "UP")
                        {
                            if ((y + i) < consoleHeightLimit)
                            {
                                Console.SetCursorPosition(x, y + i);
                                Console.WriteLine(' ');
                            }
                        }
                        else if (moving == "RIGHT")
                        {
                            if ((x - i) > 0)
                            {
                                Console.SetCursorPosition(x - i, y);
                                Console.WriteLine(' ');
                            }
                        }
                        else if (moving == "DOWN")
                        {
                            if ((y - i) > 0)
                            {
                                Console.SetCursorPosition(x, y - i);
                                Console.WriteLine(' ');
                            }
                        }
                        else if (moving == "LEFT")
                        {
                            if ((x + i) < consoleWidthLimit)
                            {
                                Console.SetCursorPosition(x + i, y);
                                Console.WriteLine(' ');
                            }
                        }
                    }
                }





                // calculate the new position
                // note x set to 0 because we use the whole width, but y set to 1 because we use top row for instructions
                prex = x;
                x += dx;
                
                if (x > consoleWidthLimit)
                    x = 0;
                if (x < 0)
                    x = consoleWidthLimit;

                prey = y;
                y += dy;
                if (y > consoleHeightLimit)
                    y = 2; // 2 due to top spaces used for directions
                if (y < 2)
                    y = consoleHeightLimit;

                //If snake ate the food
                if (x == foodX && y == foodY)
                {
                    //snake grows
                    snakelength++;
                    // Add point here
                    foodX = rand.Next(1, consoleWidthLimit);
                    foodY = rand.Next(1, consoleHeightLimit);
                }

              

                if (obstacleX == foodX && obstacleY == foodY)
                {
                    foodX = rand.Next(1, consoleWidthLimit);
                    foodY = rand.Next(1, consoleHeightLimit);
                }


                // If snake hit the obstacle
                if (x == obstacleX && y == obstacleY)
                {
                    gameLive = false;
                    Console.Write("You lost. Press any key to exit.");
                    // Do something here
                }




                // write the character in the new position


                for (int i = 0; i < snakelength; i++)
                {
                    if (moving == "UP" )
                    {

                        if ((y + i) < consoleHeightLimit)
                        {
                            Console.SetCursorPosition(x, y + i);
                            Console.WriteLine(ch);

                        }
                    }

                    else if (moving == "RIGHT")
                    {

                        if ((x - i) > 0)
                        {
                            Console.SetCursorPosition(x - i, y);
                            Console.WriteLine(ch);

                        }
                    }
                    else if (moving == "DOWN")
                    {

                        if ((y - i) > 0)
                        {
                            Console.SetCursorPosition(x, y - i);
                            Console.WriteLine(ch);

                        }
                    }
                    else if (moving == "LEFT")
                    {
                        if ((x + i) < consoleWidthLimit)
                        {
                            Console.SetCursorPosition(x + i, y);
                            Console.WriteLine(ch);

                        }
                    }




                }





                // pause to allow eyeballs to keep up
                System.Threading.Thread.Sleep(delayInMillisecs);

                
            } while (gameLive);
            }
    } } 
    

