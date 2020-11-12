using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Console = Colorful.Console;


namespace Snake
{

    struct Position
    {
        public int row;
        public int col;
        public Position(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
    }

    class Program
    {
        private static int index = 0;
        public const int console = 0x00000000;
        public const int maximizeButton = 0xF030; //maximize button
        public const int consoleBorder = 0xF000; //console border

        // function to disable the functionality of menu items
        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        // function to enable the access to system menu
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        // function to retrieve the console window handle 
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        public void Direction(Position[] directions)
        {

            directions[0] = new Position(0, 1);
            directions[1] = new Position(0, -1);
            directions[2] = new Position(1, 0);
            directions[3] = new Position(-1, 0);

        }

        public void BackgroundMusic()
        {
            //Create SoundPlayer objbect to control sound playback
            SoundPlayer backgroundMusic = new SoundPlayer();

            //Locate the SoundPlayer to the correct sound directory
            backgroundMusic.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "/SnakeBGM_1.wav";

            //Play the background music at the beginning
            backgroundMusic.PlayLooping();
        }

        public void LoseSoundEffect()
        {
            SoundPlayer playerLose = new SoundPlayer();
            playerLose.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "/SnakeLose_1.wav";
            playerLose.Play(); //Play the lose sound effect after player died
        }
        public void WinSoundEffect()
        {
            SoundPlayer playerWin = new SoundPlayer();
            playerWin.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "/SnakeWin_1.wav";
            playerWin.Play();
        }

        public void DieSoundEffect()
        {
            SoundPlayer playerDie = new SoundPlayer();
            playerDie.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "/SnakeDie_1.wav";
            playerDie.Play();
        }

        public void InitialRandomObstacles(List<Position> obstacles)
        {
            Random randomNumbersGenerator = new Random();
            obstacles.Add(new Position(randomNumbersGenerator.Next(3, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            obstacles.Add(new Position(randomNumbersGenerator.Next(3, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            obstacles.Add(new Position(randomNumbersGenerator.Next(3, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            obstacles.Add(new Position(randomNumbersGenerator.Next(3, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            obstacles.Add(new Position(randomNumbersGenerator.Next(3, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));

            foreach (Position obstacle in obstacles)
            {
                Console.SetCursorPosition(obstacle.col, obstacle.row);

                Console.ForegroundColor = Color.Cyan;
                Console.OutputEncoding = Encoding.Unicode;
                Console.Write("▒");
            }
        }

        public void CheckUserInput(ref int direction, byte right, byte left, byte down, byte up)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo userInput = Console.ReadKey(true); // Disable the display of pressed key
                if (userInput.Key == ConsoleKey.LeftArrow)
                {
                    if (direction != right) direction = left;
                }
                if (userInput.Key == ConsoleKey.RightArrow)
                {
                    if (direction != left) direction = right;
                }
                if (userInput.Key == ConsoleKey.UpArrow)
                {
                    if (direction != down) direction = up;
                }
                if (userInput.Key == ConsoleKey.DownArrow)
                {
                    if (direction != up) direction = down;
                }
            }
        }

        public int GameOverCheck(int dieCountDownTime, int gameStartTime, Queue<Position> snakeElements, Position snakeNewHead, int negativePoints, List<Position> obstacles, ref int userPoints, ref int finalScore, ref int life, string userName)
        {
            if (snakeElements.Contains(snakeNewHead) || obstacles.Contains(snakeNewHead) || ((Environment.TickCount - gameStartTime) / 1000) > dieCountDownTime
                || snakeNewHead.row >= Console.WindowHeight || snakeNewHead.col >= Console.WindowWidth || snakeNewHead.row < 2 || snakeNewHead.col < 0)
            {
                life--;
                if (life == 0)
                {

                    PrintLifePoint(life);
                    finalScore += userPoints;
                    PrintAccumulatedScore(finalScore);
                    LoseSoundEffect(); //this sound effect will be play if game over
                    Console.SetCursorPosition(0, 0);
                    Console.ForegroundColor = Color.Red;//Text color for game over

                    //Display game over text and points
                    PrintLinesInCenter("Game Over!", "Your final score is: " + finalScore);
                    Console.SetCursorPosition(32, 17);
                    Console.ForegroundColor = Color.LightGreen;
                    Console.Write("Enter your name: ");
                    userName = Console.ReadLine();
                    SavePointsToFile(finalScore, userName, life);//saving points to files
                    Console.Clear();
                    PrintLinesInCenter("Press ENTER to exit the game!");

                    //close only when enter key is pressed
                    while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                    return 1;
                }
                PrintLinesInCenter("Git Gud");
                DieSoundEffect();//this sound effect will be played if the player died
                Thread.Sleep(3000);// Pause for 3 seconds to let the player get ready for next game
                finalScore += userPoints;
                return 1;
            }
            return 0;
        }

        public int WinningCheck(int numberFoodEaten, int negativePoints, ref int userPoints, ref int finalScore, int life, string userName, int lifeBonusPoint)
        {
            if (numberFoodEaten == 10)
            {
                WinSoundEffect(); 
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = Color.Green;//Text color for game won

                finalScore = finalScore + userPoints + life * lifeBonusPoint;

                PrintLinesInCenter("You Win!", "Life Left: " + life, "SCORE: " + finalScore);
                Console.SetCursorPosition(32, 19);
                Console.ForegroundColor = Color.LightCyan;
                Console.Write("Enter your name: ");
                userName = Console.ReadLine();
                SavePointsToFile(finalScore, userName, life);//saving points to files
                Console.Clear();
                PrintLinesInCenter("Press ENTER to exit the game!");

                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                return 1;
            }
            return 0;
        }

        public void GenerateFood(ref Position food, Queue<Position> snakeElements, List<Position> obstacles, Position powerUp)
        {
            Random randomNumbersGenerator = new Random();
            do
            {
                food = new Position(randomNumbersGenerator.Next(2, Console.WindowHeight), //Food generated from 3rd row to console height
                    randomNumbersGenerator.Next(0, Console.WindowWidth)); //Food generate within console width
            }
            while (snakeElements.Contains(food) || obstacles.Contains(food) || (powerUp.col == food.col && powerUp.row == food.row));
            Console.SetCursorPosition(food.col, food.row);

            Console.ForegroundColor = Color.Red;
            Console.OutputEncoding = Encoding.Unicode;
            Console.Write("♥");
        }

        public void GeneratePowerUp(ref Position powerUp, Position food, Queue<Position> snakeElements, List<Position> obstacles)
        {
            Random randomNumbersGenerator = new Random();
            do
            {
                powerUp = new Position(randomNumbersGenerator.Next(2, Console.WindowHeight), //Food generated from 3rd row to console height
                    randomNumbersGenerator.Next(0, Console.WindowWidth)); //Food generate within console width
            }          
            while (snakeElements.Contains(powerUp) || obstacles.Contains(powerUp) || (food.col == powerUp.col && food.row == powerUp.row));
            Console.SetCursorPosition(powerUp.col, powerUp.row);

            Console.ForegroundColor = Color.YellowGreen;
            Console.OutputEncoding = Encoding.Unicode;
            Console.Write("♥");
        }

        public void GenerateNewObstacle(ref Position food, Queue<Position> snakeElements, List<Position> obstacles, int numofObstacles, Position powerUp)
        {
            Random randomNumbersGenerator = new Random();

            Position obstacle = new Position();
            for (int a = 0; a < numofObstacles; a++)
            {

                do
                {
                    obstacle = new Position(randomNumbersGenerator.Next(2, Console.WindowHeight),
                        randomNumbersGenerator.Next(1, Console.WindowWidth));
                }
                while (snakeElements.Contains(obstacle) || obstacles.Contains(obstacle) || (food.row == obstacle.row && food.col == obstacle.col) || (food.row == obstacle.row && food.col + 1 == obstacle.col) || (powerUp.row == obstacle.row && powerUp.col == obstacle.col));
                obstacles.Add(obstacle);
                Console.SetCursorPosition(obstacle.col, obstacle.row);

                Console.ForegroundColor = Color.LightCyan;
                Console.OutputEncoding = Encoding.Unicode;
                Console.Write("▒");
            }
        }

        public void SavePointsToFile(int userPoints, string userName, int life)
        {
            //declare the file path
            String filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userPoints.txt");
            //var file = new StringBuilder();
            var scoreCol = userPoints.ToString();
            var nameCol = userName.ToString();
            var lifeCol = life.ToString();
            var newRecord = string.Format("{0}, {1}, {2}", scoreCol, lifeCol, nameCol);

            try
            {
                //if there is no such text file in the folder, it will be created before saving the points into it
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                    File.WriteAllText(filePath, newRecord + Environment.NewLine);
                }
                else
                {
                    //if there are points exist in the text file, new points will be saved in next line
                    File.AppendAllText(filePath, newRecord + Environment.NewLine);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("{0} Exception caught.", exception);
            }
        }

        public string ReadPointsFromFile()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userPoints.txt");
            string[] scoreBoard = File.ReadAllLines(filePath);
            var max = (from line in scoreBoard
                       let score = int.Parse(line.Split(',').First())
                       orderby score descending
                       select score).First();
            var name = (from line in scoreBoard
                        let score = int.Parse(line.Split(',').First())
                        let userMax = line.Split(',').Last()
                        orderby score descending
                        select userMax).First(); 
            string highestPoint = max.ToString();
            string userwithHighestScore = name.ToString();
            string abc = userwithHighestScore + "\t\t" + highestPoint;
            return abc;
        }

        private static void PrintLinesInCenter(params string[] lines)
        {
            int verticalStart = (Console.WindowHeight - lines.Length) / 2; //  printing the lines
            int verticalPosition = verticalStart;
            foreach (var line in lines)
            {
                int horizontalStart = (Console.WindowWidth - line.Length) / 2;
                Console.SetCursorPosition(horizontalStart, verticalPosition);
                Console.Write(line);
                ++verticalPosition;
            }
        }

        public void DisplayStartScreen()
        {
            
            Console.ForegroundColor = Color.Magenta;
            Console.SetCursorPosition(34, 10);
            Console.Write("NACOS' SNAKEGAME");
            Console.ForegroundColor = Color.Cyan; //text color for text display
            Console.SetCursorPosition(34, 19);
            Console.Write("HIGH SCORE");
            Console.SetCursorPosition(34, 20);
            Console.Write("-------------");
            Console.SetCursorPosition(30, 22);
            Console.Write(ReadPointsFromFile());
        }

        public void PrintUserPoint(ref int userPoints, Queue<Position> snakeElements, int negativePoints, int powerUpEaten)
        {
            userPoints = (snakeElements.Count - 4) * 100 - negativePoints + (powerUpEaten*200);//points calculated for player
            userPoints = Math.Max(userPoints, 0); //if (userPoints < 0) userPoints = 0;
            Console.ForegroundColor = Color.Red;
            Console.SetCursorPosition(36, 1);
            Console.WriteLine("                ");
            Console.SetCursorPosition(36, 1);
            Console.WriteLine("Score: {0}", userPoints);
        }

        public void PrintLifePoint(int life)
        {
            if (life == 0)
                Console.ForegroundColor = Color.Red;//Text color for 0 life point
            else
                Console.ForegroundColor = Color.Green;//Text color for life point more than 0
            Console.SetCursorPosition(36, 0);
            Console.WriteLine("               ");
            Console.SetCursorPosition(36, 0);
            Console.WriteLine("Life: " + life);
        }

        public void PrintDieTime(int gameStartTime, int dieCountDownTime)
        {
            //dieTime is the time left for the player if the player did not manage to finish the task in the given time
            int dieTime = dieCountDownTime - (Environment.TickCount - gameStartTime) / 1000;

            Console.ForegroundColor = Color.White;
            Console.SetCursorPosition(52, 0);
            //Console.WriteLine("                                 ");
            Console.SetCursorPosition(52, 0);
            Console.WriteLine("Die Time: " + dieTime + " Seconds");

        }
        public void PrintFoodCountDownTimer(int lastFoodTime, int foodDissapearTime)
        {
            Console.ForegroundColor = Color.White;
            int foodCountDownTimer = (foodDissapearTime / 1000 - (Environment.TickCount - lastFoodTime) / 1000);
            Console.SetCursorPosition(52, 1);
            Console.WriteLine("                               ");
            Console.SetCursorPosition(52, 1);
            Console.WriteLine("Food Count Down: " + foodCountDownTimer + " Second");
        }

        public void PrintAccumulatedScore(int finalScore)
        {
            Console.ForegroundColor = Color.White;
            Console.SetCursorPosition(1, 0);
            Console.WriteLine("                     ");
            Console.SetCursorPosition(1, 0);
            Console.WriteLine("Accumulated Score: " + finalScore);
        }

        public void PrintNumberFoodEaten(int numberFoodEaten)
        {
            Console.ForegroundColor = Color.White;
            Console.SetCursorPosition(1, 1);
            Console.WriteLine("                     ");
            Console.SetCursorPosition(1, 1);
            Console.WriteLine("Food Eaten: " + numberFoodEaten);
        }
 
        static void Main(string[] args)
        {
            byte right = 0;
            byte left = 1;
            byte down = 2;
            byte up = 3;
            int lastFoodTime = 0;
            int foodDissapearTime = 10000; //food dissappears after 10 second 
            int powerUpDissapearTime = 5000;
            int lastPowerUpDissapearTime = 0;
            int powerUpAppearTime = 2000;
            int lastPowerUpAppearTime = 0;
            

            int life = 3;
            int userPoints = 0;
            int finalScore = 0;
            int dieCountDownTime = 150; //Time limit per life in seconds
            double sleepTime = 100;
            int numofObstacles = 0;
            string userName = "-";
            int lifeBonusPoint = 0; //Life bonus point is bonus point added in winning condition with per life left
            string selectedMode;
            Console.SetWindowSize(80, 30);//reducing screen size 
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);

            Position[] directions = new Position[4];

            Program p = new Program();

            //disbale the resize of console window by disabling maximize button and border dragging
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), maximizeButton, console);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), consoleBorder, console);

            //Main menu snake console ASCII ART is here
            int DA = 255;
            int V = 255;
            int ID = 255;

            Console.WriteAscii("SNAKE", Color.FromArgb(DA, V, ID));


            //Console snake size set with BufferArea using new library
            Console.MoveBufferArea(0, 0, Console.BufferWidth, Console.BufferHeight, 22, 5);

            //Game mode menu for bound and unbound 
            List<string> modeMenu = new List<string>() {
                "Normal",
                "Free Mode"
            };


            while (true)
            {
                selectedMode = drawModeMenu(modeMenu);
                if (selectedMode == "Normal")
                {
                    if (Console.ReadKey().Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
                else if (selectedMode == "Free Mode")
                {
                    if (Console.ReadKey().Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            }
            Console.Clear();

            //Game level snake ASCII art for screen 
            Console.WriteAscii("SNAKE", Color.FromArgb(DA, V, ID));

            //Console snake size set with BufferArea using new library
            Console.MoveBufferArea(0, 0, Console.BufferWidth, Console.BufferHeight, 22, 5);

            // game menu items for level of intensity 
            List<string> menuItems = new List<string>() {
                "Easy",
                "Medium",
                "Hard"
            };

            //display start screen before background music and game start 
            p.DisplayStartScreen();
            Console.CursorVisible = false;
            while (true)
            {
                string selectedMenuItem = drawMenu(menuItems);
                if (selectedMenuItem == "Easy")
                {

                    foodDissapearTime = 20000;
                    life = 3;
                    lifeBonusPoint = 200;
                    sleepTime = 100;
                    numofObstacles = 1;
                    if (Console.ReadKey().Key == ConsoleKey.Enter)
                    {
                        break;
                    }

                }
                else if (selectedMenuItem == "Medium")
                {

                    foodDissapearTime = 14000;
                    life = 2;
                    lifeBonusPoint = 400;
                    sleepTime = 90;
                    numofObstacles = 2;
                    if (Console.ReadKey().Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }

                else if (selectedMenuItem == "Hard")
                {

                    foodDissapearTime = 10000;
                    life = 1;
                    lifeBonusPoint = 1000;
                    sleepTime = 60;
                    numofObstacles = 4;
                    if (Console.ReadKey().Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            }
            Console.Clear(); 

            p.Direction(directions);

            while (life > 0)
            {
                //Set the game start time everytime the player start a new game with new life
                int gameStartTime = Environment.TickCount;
                int powerUpEaten = 0;
                int appearFlag = 1;
                lastPowerUpAppearTime = Environment.TickCount;
                //Play background music
                //p.BackgroundMusic();
                int numberFoodEaten = 0;//Initialised number of food eaten by the snake to be 0
                int negativePoints = 0;
                //Clear the console everytime start the game in new life
                Console.Clear();
                //Print the current life point
                p.PrintLifePoint(life);

                // Initialised the obstacles location at the starting of the game
                List<Position> obstacles = new List<Position>();
                p.InitialRandomObstacles(obstacles);

                Position powerUp = new Position();
                powerUp.col = 200;
                powerUp.row = 100;

                int direction = right;
                Random randomNumbersGenerator = new Random();
                Console.BufferHeight = Console.WindowHeight;
                lastFoodTime = Environment.TickCount;

                Queue<Position> snakeElements = new Queue<Position>();
                for (int i = 0; i <= 3; i++) // Length of the snake was reduced to 3 units of *
                {
                    snakeElements.Enqueue(new Position(2, i));
                }

                //To position food randomly when the program runs first time
                Position food = new Position();
                p.GenerateFood(ref food, snakeElements, obstacles, powerUp);

                //while the game is running position snake on terminal with shape "*"
                foreach (Position position in snakeElements)
                {
                    Console.SetCursorPosition(position.col, position.row);

                    Console.ForegroundColor = Color.GreenYellow;
                    Console.Write("•");
                }

                while (true)
                {
                    //Print the food count down timer
                    p.PrintFoodCountDownTimer(lastFoodTime, foodDissapearTime);
                    //Print the time left for the game
                    p.PrintDieTime(gameStartTime, dieCountDownTime);
                    //Print the accumulated score so far from the previous life game
                    p.PrintAccumulatedScore(finalScore);
                    //Print the number of food eaten by the player
                    p.PrintNumberFoodEaten(numberFoodEaten);

                    negativePoints++;

                    //Check the user input direction
                    p.CheckUserInput(ref direction, right, left, down, up);

                    //When the game starts the snake head is towards the end of his body with face direct to start from right.
                    Position snakeHead = snakeElements.Last();
                    Position nextDirection = directions[direction];

                    //Snake position to go within the terminal window assigned.
                    Position snakeNewHead = new Position(snakeHead.row + nextDirection.row,
                        snakeHead.col + nextDirection.col);

                    // Unbound mode selected then utilise this function 
                    if (selectedMode == "Unbound Mode")
                    {
                        if (snakeNewHead.col < 0) snakeNewHead.col = Console.WindowWidth - 1;
                        if (snakeNewHead.row < 2) snakeNewHead.row = Console.WindowHeight - 1;
                        if (snakeNewHead.row >= Console.WindowHeight) snakeNewHead.row = 2;
                        if (snakeNewHead.col >= Console.WindowWidth) snakeNewHead.col = 0;
                    }



                    //print realtime user points
                    p.PrintUserPoint(ref userPoints, snakeElements, negativePoints, powerUpEaten);

                    //Check for GameOver Criteria including the Final Game Over and Die Condition
                    int gameOver = p.GameOverCheck(dieCountDownTime, gameStartTime, snakeElements, snakeNewHead, negativePoints, obstacles, ref userPoints, ref finalScore, ref life, userName);
                    if (gameOver == 1)
                        break;


                    //Check for Winning Criteria
                    int winning = p.WinningCheck(numberFoodEaten, negativePoints, ref userPoints, ref finalScore, life, userName, lifeBonusPoint);
                    if (winning == 1)
                        return;

                    //The way snake head will change as the player changes his direction
                    Console.SetCursorPosition(snakeHead.col, snakeHead.row);
                    Console.ForegroundColor = Color.GreenYellow;
                    Console.Write("•");

                    //Snake head shape when the user presses the key to change his direction
                    snakeElements.Enqueue(snakeNewHead);
                    Console.SetCursorPosition(snakeNewHead.col, snakeNewHead.row);
                    Console.ForegroundColor = Color.GreenYellow;
                    if (direction == right) Console.Write("►"); //Snake head when going right
                    if (direction == left) Console.Write("◄");//Snake head when going left
                    if (direction == up) Console.Write("▲");//Snake head when going up
                    if (direction == down) Console.Write("▼");//Snake head when going down     

                    if ((Environment.TickCount - lastPowerUpAppearTime) > powerUpAppearTime && appearFlag == 1) 
                    {
                        p.GeneratePowerUp(ref powerUp, food, snakeElements, obstacles);
                        lastPowerUpDissapearTime = Environment.TickCount;
                        appearFlag = 0;
                    }
                    if ((Environment.TickCount -lastPowerUpDissapearTime)> powerUpDissapearTime && appearFlag == 0)
                    {
                        appearFlag = 1;
                        Console.SetCursorPosition(powerUp.col, powerUp.row);
                        Console.Write(" ");
                        powerUp.col = 0;
                        powerUp.row = 0;
                        lastPowerUpAppearTime = Environment.TickCount;
                    }
                    if (snakeNewHead.col == powerUp.col && snakeNewHead.row == powerUp.row)
                    {
                        powerUpEaten++;
                        Console.Beep(600, 100);// Make a sound effect when food was eaten.
                        numberFoodEaten++;
                        appearFlag = 1;
                        powerUp.col = 0;
                        powerUp.row = 0;
                        lastPowerUpAppearTime = Environment.TickCount;
                    }

                    // food will be positioned randomly until they are not at the same row & column as snake head
                    if ((snakeNewHead.col == food.col && snakeNewHead.row == food.row) || (snakeNewHead.col == food.col + 1 && snakeNewHead.row == food.row))
                    {
                        if (snakeNewHead.col == food.col && snakeNewHead.row == food.row)
                        {
                            Console.SetCursorPosition(food.col + 1, food.row);
                        }
                        else
                        {
                            Console.SetCursorPosition(food.col, food.row);
                        }
                        Console.Write(" ");
                        Console.Beep(600, 100);// Make a sound effect when food was eaten.
                        numberFoodEaten++; //Increase the number count by 1 each time the snake eat the food
                         
                        p.GenerateFood(ref food, snakeElements, obstacles,powerUp);
                       
                        lastFoodTime = Environment.TickCount;
                        sleepTime--;

                        //Generate new obstacle
                        p.GenerateNewObstacle(ref food, snakeElements, obstacles, numofObstacles,powerUp);

                    }
                    else
                    {
                        // snake is moving
                        Position last = snakeElements.Dequeue();
                        Console.SetCursorPosition(last.col, last.row);
                        Console.Write(" ");
                    }
                    if (Environment.TickCount - lastFoodTime >= foodDissapearTime)
                    {
                        negativePoints = negativePoints + 50;
                        Console.SetCursorPosition(food.col, food.row);
                        Console.Write("  ");

                        //Generate the new food and record the system tick count
                        p.GenerateFood(ref food, snakeElements, obstacles,powerUp);
                        lastFoodTime = Environment.TickCount;
                    }

                    //snake moving speed increased 
                    sleepTime -= 0.01;

                    //pause the execution thread of snake moving speed
                    Thread.Sleep((int)sleepTime);
                }
            }

            return;
        }

        private static string drawMenu(List<string> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (i == index)
                {
                    Console.BackgroundColor = Color.Cyan;
                    Console.ForegroundColor = Color.White;

                    Console.SetCursorPosition(38, 13 + i);
                    Console.WriteLine(items[i]);

                }
                else
                {
                    Console.SetCursorPosition(38, 13 + i);
                    Console.WriteLine(items[i]);
                }
                Console.ResetColor();
            }

            ConsoleKeyInfo ckey = Console.ReadKey();

            if (ckey.Key == ConsoleKey.DownArrow)
            {
                if (index == items.Count - 1)
                {
                    //index = 0; //Remove the comment to return to the topmost item in the list
                }
                else { index++; }
            }
            else if (ckey.Key == ConsoleKey.UpArrow)
            {
                if (index <= 0)
                {
                    //index = menuItem.Count - 1; //Remove the comment to return to the item in the bottom of the list
                }
                else { index--; }
            }
            else if (ckey.Key == ConsoleKey.Enter)
            {
                return items[index];
            }
            else
            {
                return "";
            }

            return "";
        }

        private static string drawModeMenu(List<string> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (i == index)
                {
                    Console.BackgroundColor = Color.Cyan;
                    Console.ForegroundColor = Color.White;

                    Console.SetCursorPosition(32, 13 + i);
                    Console.WriteLine(items[i]);

                }
                else
                {
                    Console.SetCursorPosition(32, 13 + i);
                    Console.WriteLine(items[i]);
                }
                Console.ResetColor();
            }

            ConsoleKeyInfo ckey = Console.ReadKey();

            if (ckey.Key == ConsoleKey.DownArrow)
            {
                if (index == items.Count - 1)
                {
                    //index = 0; //Remove the comment to return to the topmost item in the list
                }
                else { index++; }
            }
            else if (ckey.Key == ConsoleKey.UpArrow)
            {
                if (index <= 0)
                {
                    //index = menuItem.Count - 1; //Remove the comment to return to the item in the bottom of the list
                }
                else { index--; }
            }
            else if (ckey.Key == ConsoleKey.Enter)
            {
                return items[index];
            }
            else
            {
                return "";
            }

            return "";
        }

    }
}
