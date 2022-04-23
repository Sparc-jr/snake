using System;
class Program
{
    internal class GameFieldEdges                                  //объявляем класс, который будет содержать границы игрового поля
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
    internal class Apple                                          
    {
        public int X { get; set; } //координаты яблока
        public int Y { get; set; } //координаты яблока
        public byte points { get; set; } //очки за поедание
        public byte lifeTime { get; set; }  //время существования яблока
        public byte size { get; set; }     //размер
    }
    internal class Settings                                                 //объявляем класс игровых настроек
    {
        public char userSymbol = '+';                                    // символ змейки  
        public char fieldSymbol = '*';                                   // символ поля
        public char appleSymbol = 'o';                                   // символ яблока
        public char bigAppleSymbol = '@';                                // символ большого яблока
        public int fieldSize = 20;                                       // размер игрового поля
        public ConsoleColor backgroundColor = ConsoleColor.Black;       // цвет фона консоли
        public ConsoleColor fieldColor = ConsoleColor.White;            // цвет символов игрового поля
        public ConsoleColor snakeColor = ConsoleColor.Green;            // цвет змейки
        public ConsoleColor appleColor = ConsoleColor.Red;              // цвет яблок
        public ConsoleColor menuBorderColor = ConsoleColor.Blue;              // цвет рамки меню
        public int speed = 2;                                         //скорость игры
        public int[] realSpeed = { 300, 250, 200, 150, 100, 50 };
        public bool throughWalls = true;                                 //флаг прохода "сквозь" стены
    }

    public enum MoveDirection
    {
        Up, Down, Left, Right
    }
    public static GameFieldEdges gameField = new GameFieldEdges();
    public static Settings gameSettings = new Settings();
    public static bool exit = false;                                        //флаг выхода из программы
    
    
    private static void DrawMenuBorders()
    {
        Console.ForegroundColor = gameSettings.menuBorderColor;
        Console.SetCursorPosition(0, gameField.top);
        Console.WriteLine("#==========================#");
        for (int i = 0; i < 8; i++)
        {
            Console.WriteLine("|                          |");
        }
        Console.WriteLine("#==========================#");
        Console.ForegroundColor = gameSettings.fieldColor;
    }
    private static void EraseMenuLeftovers()
    {
        Console.SetCursorPosition(gameField.left, gameField.top);
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine("                            ");
        }
    }

    private static void SettingsMenu()
    {
        ConsoleKeyInfo keyPressed;
        bool menu = true;
        bool restart = false;
        DrawMenuBorders();
        while (menu)
        {
            Console.SetCursorPosition(gameField.left + 2, gameField.top + 1);
            Console.Write("Размер поля   ("+(char)27+(char)26+"): {0}", gameSettings.fieldSize);
            Console.SetCursorPosition(gameField.left + 2, gameField.top + 3);
            Console.Write("Скорость игры (" + (char)25 + (char)24 + "): {0}", gameSettings.speed);
            Console.SetCursorPosition(gameField.left + 2, gameField.top + 5);
            Console.Write("\"Сквозь стены\" (Z): {0}", gameSettings.throughWalls);
            Console.SetCursorPosition(gameField.left + 5, gameField.top + 7);
            Console.Write("Выйти из игры: \"X\"");
            keyPressed = Console.ReadKey(true);
            switch (keyPressed.Key)
            {
                case ConsoleKey.UpArrow: gameSettings.speed += gameSettings.speed < 6 ? 1 : 0; break;//увеличение скорости игры
                case ConsoleKey.DownArrow: gameSettings.speed -= gameSettings.speed > 1 ? 1 : 0; break;//уменьшение скорости игры
                case ConsoleKey.LeftArrow: gameSettings.fieldSize -= gameSettings.fieldSize > 5 ? 1 : 0; ResizeField(); restart = true; break;//уменьшение игрового поля
                case ConsoleKey.RightArrow: gameSettings.fieldSize += gameSettings.fieldSize < 50 ? 1 : 0; ResizeField(); restart = true; break;//увеличение игрового поля
                case ConsoleKey.Z: gameSettings.throughWalls = !gameSettings.throughWalls; break; //смена режима прохождения границ поля
                case ConsoleKey.X: ConfirmExit(); menu = false; break;                            //выход из игры
                case ConsoleKey.Escape: menu = false;EraseMenuLeftovers(); break;                 //возврат в игру
                default: break;
            }
        }
        if (restart) StartGame(); 
    }
    private static void ConfirmExit()
    {
        DrawMenuBorders();
        ConsoleKeyInfo keyPressed;
        Console.SetCursorPosition(gameField.left + 7, gameField.top + 3);
        Console.WriteLine("Выйти из игры?");
        Console.SetCursorPosition(gameField.left + 12, gameField.top + 5);
        Console.WriteLine("y/n");
        Thread.Sleep(300);
        keyPressed = Console.ReadKey(true);
        switch (keyPressed.Key)
        {
            case ConsoleKey.Y: case ConsoleKey.Enter: exit = true; break; 
            default: SettingsMenu(); break;                         //возврат в меню
        }
    }
    
    internal class Coords
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Coords() {}
        public Coords(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    public static void WalkThroughBorders(ref Coords newHeadPosition, GameFieldEdges fieldEdges)
    {
        if (newHeadPosition.X < fieldEdges.left) newHeadPosition.X = fieldEdges.right; else if (newHeadPosition.X > fieldEdges.right) newHeadPosition.X = fieldEdges.left;   //если координаты вышли за край - 
        if (newHeadPosition.Y < fieldEdges.top) newHeadPosition.Y = fieldEdges.bottom; else if (newHeadPosition.Y > fieldEdges.bottom) newHeadPosition.Y = fieldEdges.top;    // - присваиваем координаты противоположного края 
    }

    public static void CheckFieldBorders(ref Coords newHeadPosition, GameFieldEdges fieldEdges)
    {
        if (newHeadPosition.X < fieldEdges.left || newHeadPosition.X > fieldEdges.right)    
        {
            OutOfField();                                       
            newHeadPosition.X = Console.CursorLeft;                                     
        }
        if (newHeadPosition.Y < fieldEdges.top || newHeadPosition.Y > fieldEdges.bottom)    
        {
            OutOfField();                                       
            newHeadPosition.Y = Console.CursorTop;                                     
        }   
    }
    public static void CheckSelfCross(List<Coords> snakebody)
    {
        for(int i=1;i<snakebody.Count;i++)
        {
            if (snakebody[0].X == snakebody[i].X && snakebody[0].Y == snakebody[i].Y) SelfCross();            
        }

    }
    private static void SelfCross()
    {
        DrawMenuBorders();
        Console.SetCursorPosition(gameField.left + 4, gameField.top+3);
        Console.WriteLine("Вы врезались в себя.");
        GameOver();
    }

    private static void OutOfField()
    {
        DrawMenuBorders();
        Console.SetCursorPosition(gameField.left + 4, gameField.top+3);
        Console.WriteLine("Вы врезались в стену");
        GameOver();
    }
    private static void GameOver()
    {
        ConsoleKeyInfo keyPressed;
        Console.SetCursorPosition(gameField.left + 4, gameField.top + 5);
        Console.WriteLine("Хотите сыграть еще?");
        Console.SetCursorPosition(gameField.left + 12, gameField.top + 7);
        Console.WriteLine("y/n");
        Console.SetCursorPosition(gameField.left, gameField.top - 1);
        Thread.Sleep(300);
        keyPressed = Console.ReadKey(true);
            switch (keyPressed.Key)
            {
                case ConsoleKey.Y: case ConsoleKey.Enter: EraseMenuLeftovers(); StartGame(); break; //запуск игры заново
                default: exit = true; break;
            }
    }
    private static List<Coords> InitializeSnake(int startSize)
    {
        if (startSize > gameSettings.fieldSize / 2)
        {
            startSize = gameSettings.fieldSize / 2;
        };
        List<Coords> snake = new List<Coords>();
        for (int i=0; i<startSize;i++)
        {
            snake.Add(new Coords(gameField.left + gameSettings.fieldSize / 2 - 1, gameField.top + gameSettings.fieldSize / 2 - 1 + i));
        }
        return snake;
    }

    private static void DrawField()                                         //отрисовка игрового поля
    {
        Console.SetCursorPosition(gameField.left, gameField.top);
        for (int i = 0; i < gameSettings.fieldSize; i++)                     
        {
            for (int j = 0; j < gameSettings.fieldSize; j++)
            {
                Console.Write(gameSettings.fieldSymbol);
            }
            Console.WriteLine();
        }
    }


    private static Apple GenerateApple(List<Coords> freeArea)
    {
        var rand = new Random();
        int respawnPoint = rand.Next(freeArea.Count);
        byte bonusK = (byte)(gameSettings.speed - 2); //коэффициент бонуса за скорость игры
        Apple newApple = new Apple();
        newApple.X = freeArea[respawnPoint].X;
        newApple.Y = freeArea[respawnPoint].Y;
        newApple.size = (byte)rand.Next(10);
        switch(newApple.size)
        {
            case >7: newApple.points = (byte)(100 + 10 * bonusK); break; //очки за поедание большого яблока
            default: newApple.points = (byte)(20 + 5 * bonusK); break;  //очки за поедание яблока
        }
        newApple.lifeTime = 0;
        DrawApple(newApple);
        return newApple;
    }
    private static void DrawApple(Apple apple) 
    {
        Console.SetCursorPosition(apple.X, apple.Y);                       
        Console.ForegroundColor = gameSettings.appleColor;
        switch (apple.size)
        {
            case >7: Console.Write(gameSettings.bigAppleSymbol); break;
            default: Console.Write(gameSettings.appleSymbol); break;
        }
        Console.ForegroundColor = gameSettings.fieldColor;
    }
    private static void EraseApple(Apple apple)
    {
        Console.SetCursorPosition(apple.X, apple.Y);                        //перемещаем курсор на заданную позицию
        Console.Write(gameSettings.fieldSymbol);                             //"закрашиваем" "просроченное" яблоко
    }
    private static void EatApple(Apple apple)
    {
        Console.SetCursorPosition(apple.X, apple.Y);                        //перемещаем курсор на заданную позицию
        Console.ForegroundColor = gameSettings.snakeColor;
        Console.Write(gameSettings.userSymbol);                             //рисуем символ пользователя поверх яблока
        Console.ForegroundColor = gameSettings.fieldColor;                   
    }

    private static void DrawSnake(List<Coords> snake)                  //первоначальная отрисовка змейки
    {
        foreach(Coords XY in snake)
        {
            Console.SetCursorPosition(XY.X, XY.Y);                       //перемещаем курсор на заданную позицию
            Console.ForegroundColor = gameSettings.snakeColor;
            Console.Write(gameSettings.userSymbol);                             //рисуем символ пользователя
            Console.ForegroundColor = gameSettings.fieldColor;
        }
    }
    private static void DrawSnakeHead(Coords headCoords)
    {
        Console.SetCursorPosition(headCoords.X, headCoords.Y);                       //перемещаем курсор на заданную позицию
        Console.ForegroundColor = gameSettings.snakeColor;
        Console.Write(gameSettings.userSymbol);                             //рисуем символ пользователя
        Console.ForegroundColor = gameSettings.fieldColor; 
    }

    private static void EraseSnakeTail(Coords tailCoords)
    {
        Console.SetCursorPosition(tailCoords.X, tailCoords.Y);                        //перемещаем курсор на заданную позицию
        Console.Write(gameSettings.fieldSymbol);                             //"закрашиваем" символ пользователя символом поля
    }
    
    public static List<Coords> MoveSnake(List<Coords> snakebody, MoveDirection direction, ref bool grow, ref List<Coords> freeArea)                             //перемещаем и растим змейку
    {
        snakebody.Insert(0,new Coords(snakebody[0].X, snakebody[0].Y));
        switch(direction)
        {
            case MoveDirection.Up: snakebody[0].Y--; break;
            case MoveDirection.Down: snakebody[0].Y++; break;
            case MoveDirection.Left: snakebody[0].X--; break;
            case MoveDirection.Right: snakebody[0].X++; break;
        }
        Coords snakehead = snakebody[0];
        if(gameSettings.throughWalls)
            WalkThroughBorders(ref snakehead, gameField);
            else CheckFieldBorders(ref snakehead, gameField); //проверяем на выход за пределы поля
        CheckSelfCross(snakebody);
        DrawSnakeHead(snakebody[0]);
        freeArea.RemoveAll(point => point.X == snakebody[0].X && point.Y == snakebody[0].Y);
        if (!grow)
        {
            freeArea.Add(new Coords(snakebody[snakebody.Count - 1].X, snakebody[snakebody.Count - 1].Y));
            EraseSnakeTail(snakebody[snakebody.Count - 1]);                      //"закрашиваем" последний символа хвоста
            snakebody.RemoveAt(snakebody.Count - 1);

        }
        else grow = false;
        return snakebody;
    }
    public static void StartGame()
    {
        List<Coords> freeArea = new List<Coords>(); 
        exit = false;
        bool grow = false;
        int scores=0;
        MoveDirection direction = MoveDirection.Up;
        Console.WriteLine($"Счет: {scores}");
        for (int y = gameField.top; y <= gameField.bottom; y++)
            for (int x = gameField.left; x <= gameField.right; x++)
            {
                freeArea.Add(new Coords(x, y));
            }
        List<Coords> snake = new List<Coords>();
        snake = InitializeSnake(4);
        int a;
        foreach(Coords position in snake)
        {
            freeArea.RemoveAll(points => points.X == position.X && points.Y == position.Y);
        }
        DrawSnake(snake);
        direction = MoveDirection.Up;       
        Console.SetCursorPosition(0, gameField.bottom + 2);
        Console.WriteLine("Меню игры \"Esc\"");
        Console.BackgroundColor = gameSettings.backgroundColor;
        Console.ForegroundColor = gameSettings.fieldColor;
        Console.CursorVisible = false;                          //делаем курсор невидимым
        Console.SetCursorPosition(gameField.left, gameField.top);
        DrawField();
        DrawSnake(snake);
        Apple apple = new Apple();
        apple = GenerateApple(freeArea);
         ConsoleKeyInfo keyPressed;
        do
        {
            if (Console.KeyAvailable)
            {
                keyPressed = Console.ReadKey(true);
                switch (keyPressed.Key)
                {
                    case ConsoleKey.Escape: SettingsMenu();DrawField();DrawSnake(snake);DrawApple(apple); break;
                    case ConsoleKey.UpArrow: if(direction != MoveDirection.Down) direction = MoveDirection.Up; break;         
                    case ConsoleKey.DownArrow: if (direction != MoveDirection.Up) direction = MoveDirection.Down; break;       
                    case ConsoleKey.LeftArrow: if (direction != MoveDirection.Right) direction = MoveDirection.Left; break;       
                    case ConsoleKey.RightArrow: if (direction != MoveDirection.Left) direction = MoveDirection.Right; break;      
                }
            }
            
            snake = MoveSnake(snake, direction,ref grow, ref freeArea);
            apple.lifeTime++;

            if (apple.lifeTime>gameSettings.fieldSize*3)
            {
                EraseApple(apple);
                apple = GenerateApple(freeArea);
            }
            if (snake[0].X==apple.X && snake[0].Y == apple.Y)
            {
                grow = true;
                scores += apple.points;
                Console.SetCursorPosition(gameField.left+6, gameField.top - 1);
                Console.Write(scores);
                apple = GenerateApple(freeArea);
            }

            Thread.Sleep(gameSettings.realSpeed[gameSettings.speed-1]);
        }
        while (!exit);
    }

    public static void ResizeField()
    {
        Console.WindowHeight = gameSettings.fieldSize<10?14: gameSettings.fieldSize + 4;
        Console.WindowWidth = 80;
        gameField.right = gameField.left + gameSettings.fieldSize - 1;
        gameField.bottom = gameField.top + gameSettings.fieldSize - 1;
    }
    public static void Main()
    {
        
        
        Console.Title = "З М Е Й К А v0.02";
        
        gameField.left = Console.CursorLeft;                              
        gameField.top = Console.CursorTop+1;
        ResizeField();
        StartGame();
    }
}
