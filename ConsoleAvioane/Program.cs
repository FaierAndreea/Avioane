using Polly;
using Polly.Retry;
using System.Text.RegularExpressions;

#region variables
int[,] Game;
string[,] GameDisplay;
int PlainIndex;
string square = '\u25A0'.ToString();
int Turns = 0;
int GameOver = 0;
RetryPolicy retry = Policy.Handle<FormatException>().RetryForever();
#endregion

#region initialize
int[,] InitializeGame() {
    var game = new int[10, 10];
    try {
        Random random = new Random();
        for (int i = 0; i < 2; i++) {
            PlainIndex = i * 2 + 1;
            retry.Execute(() => {
                int headI = random.Next(0, 9);
                int headJ = random.Next(0, 9);
                int headDirection = random.Next(0, 3);
                if (CanFitPlain(headI, headJ, headDirection)) {
                    var temporary = (int[,])game.Clone();
                    AddPlainInGrid(temporary, headI, headJ, headDirection);
                    game = temporary;
                }
                else throw new FormatException();
            });
        }
    }
    catch (Exception) { throw; }

    return game;
}
string[,] InitializeDisplay() {
    var display = new string[11, 11];
    display[0, 0] = "  ";

    for (int i = 1; i <= 10; i++) {
        display[i, 0] = i.ToString().PadLeft(2);
        display[0, i] = ((char)(64 + i)).ToString();
        for (int j = 1; j <= 10; j++) {
            display[i, j] = square;
        }
    }

    return display;
}

void AddPlainInGrid(int[,] grid, int headI, int headJ, int headDirection) {
    SetOrThrow(grid, headI, headJ, PlainIndex + 1);
    switch (headDirection) {
        case 0:
            for (int j = headJ - 2; j <= headJ + 2; j++) {
                SetOrThrow(grid, headI + 1, j, PlainIndex);
            }
            SetOrThrow(grid, headI + 2, headJ, PlainIndex);
            for (int j = headJ - 1; j <= headJ + 1; j++) {
                SetOrThrow(grid, headI + 3, j, PlainIndex);
            }
            break;
        case 1:
            for (int i = headI - 2; i <= headI + 2; i++) {
                SetOrThrow(grid, i, headJ + 1, PlainIndex);
            }
            SetOrThrow(grid, headI, headJ + 2, PlainIndex);
            for (int i = headI - 1; i <= headI + 1; i++) {
                SetOrThrow(grid, i, headJ + 3, PlainIndex);
            }
            break;
        case 2:
            for (int j = headJ - 2; j <= headJ + 2; j++) {
                SetOrThrow(grid, headI - 1, j, PlainIndex);
            }
            SetOrThrow(grid, headI - 2, headJ, PlainIndex);
            for (int j = headJ - 1; j <= headJ + 1; j++) {
                SetOrThrow(grid, headI - 3, j, PlainIndex);
            }
            break;
        case 3:
            for (int i = headI - 2; i <= headI + 2; i++) {
                SetOrThrow(grid, i, headJ - 1, PlainIndex);
            }
            SetOrThrow(grid, headI, headJ - 2, PlainIndex);
            for (int i = headI - 1; i <= headI + 1; i++) {
                SetOrThrow(grid, i, headJ - 3, PlainIndex);
            }
            break;
    }
}
void SetOrThrow(int[,] array, int i, int j, int value) {
    if (array[i, j] == 0)
        array[i, j] = value;
    else throw new FormatException();
}

bool CanFitPlain(int headI, int headJ, int headDirection) {
    switch (headDirection) {
        case 0:
            if (headI > 6 || headJ < 2 || headJ > 7)
                return false;
            break;
        case 1:
            if (headI < 2 || headI > 7 || headJ > 6)
                return false;
            break;
        case 2:
            if (headI < 3 || headJ < 2 || headJ > 7)
                return false;
            break;
        case 3:
            if (headI < 2 || headI > 7 || headJ < 3)
                return false;
            break;
    }
    return true;
}
#endregion

#region print
void printGame(int[,] grid) {
    for (int i = 0; i <= 9; i++) {
        for (int j = 0; j <= 9; j++)
            Console.Write($"{grid[i, j]} ");
        Console.WriteLine();
    }
}
void printDisplayGame() {
    Console.OutputEncoding = System.Text.Encoding.UTF8;
    for (int i = 0; i <= 10; i++) {
        for (int j = 0; j <= 10; j++)
            Console.Write($"{GameDisplay[i, j]} ");
        Console.WriteLine();
    }
}
#endregion

#region play
void Play() {
    Game = InitializeGame();
    GameDisplay = InitializeDisplay();
    //printGame(Game);
    Console.WriteLine("Welcome! Try destroying the two airplains ");
    printDisplayGame();
    while (GameOver < 20) {
        var result = MakeAMove();
        printDisplayGame();
        Console.WriteLine(result);
        Console.WriteLine(GameOver);
    }
    EndGame();
}

string MakeAMove() {
    string move = "";
    retry.Execute(() => {
        Console.WriteLine("Row and Column : ");
        move = Console.ReadLine().ToUpper().Trim();
        IsValidMove(move);
    });

    int col = move.First() - 'A';
    int row = (int)Char.GetNumericValue(move.Last()) - 1;

    Turns++;
    switch (Game[row, col]) {
        case 0:
            GameDisplay[row + 1, col + 1] = "O";
            return "Miss";
        case 1:
        case 3:
            GameDisplay[row + 1, col + 1] = "X";
            GameOver++;
            return "Hit";
        case 2:
            GameDisplay[row + 1, col + 1] = "X";
            DestroyPlain(1);
            return "HEAD !";
        case 4:
            GameDisplay[row + 1, col + 1] = "X";
            DestroyPlain(3);
            return "HEAD !";
        default:
            throw new Exception();
    }
}

void IsValidMove(string move) {
    if (move == string.Empty)
        throw new FormatException();

    if (!Regex.IsMatch(move, @"^[A-J]([1-9]|10)$")) {
        Console.WriteLine("Please enter a letter (A-J) followed by a number (1-10). ex: G4");
        throw new FormatException();
    }

    int col = move.First() - 'A' + 1;
    int row = (int)Char.GetNumericValue(move.Last());
    if (GameDisplay[row, col] != square) {
        Console.WriteLine("You already know what's there, choose something else.");
        throw new FormatException();
    }
}

void DestroyPlain(int index) {
    GameOver++;
    for (int i = 1; i <= 10; i++) {
        for (int j = 1; j <= 10; j++) {

            if (Game[i - 1, j - 1] == index) {
                if (GameDisplay[i, j] != "X") {
                    GameDisplay[i, j] = "X";
                    GameOver++;
                }
            }

        }
    }
}
void EndGame() {
    Console.WriteLine("Good job, you WON !");
    Console.WriteLine($"It took you {Turns} turns.");
    if (Turns < 4)
        Console.WriteLine("Are you a hacker?");
    if (Turns > 20)
        Console.WriteLine("I admire your perseverance.");
}
#endregion

Play();
