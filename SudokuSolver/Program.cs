using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace SudokuSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            //Set test Sudoku field
            var sudokuBoard = new SudokuBoard();
            SetTest(sudokuBoard);
            sudokuBoard.Print();

            //Solve
            sudokuBoard.Solve();
            sudokuBoard.Print();
        }

        static void SetTest(SudokuBoard sudokuBoard)
        {
            sudokuBoard.SetValue(1, 0, 4);
            sudokuBoard.SetValue(2, 0, 8);
            sudokuBoard.SetValue(3, 0, 3);
            sudokuBoard.SetValue(5, 0, 2);
            sudokuBoard.SetValue(6, 0, 5);

            sudokuBoard.SetValue(0, 1, 7);
            sudokuBoard.SetValue(4, 1, 5);
            sudokuBoard.SetValue(5, 1, 9);

            sudokuBoard.SetValue(1, 2, 9);
            sudokuBoard.SetValue(4, 2, 4);

            sudokuBoard.SetValue(0, 3, 5);
            sudokuBoard.SetValue(1, 3, 3);
            sudokuBoard.SetValue(3, 3, 7);
            sudokuBoard.SetValue(8, 3, 1);

            sudokuBoard.SetValue(0, 4, 9);
            sudokuBoard.SetValue(8, 4, 3);

            sudokuBoard.SetValue(0, 5, 8);
            sudokuBoard.SetValue(7, 5, 5);
            sudokuBoard.SetValue(8, 5, 6);

            sudokuBoard.SetValue(4, 6, 3);
            sudokuBoard.SetValue(7, 6, 2);

            sudokuBoard.SetValue(3, 7, 6);
            sudokuBoard.SetValue(4, 7, 8);
            sudokuBoard.SetValue(8, 7, 7);

            sudokuBoard.SetValue(2, 8, 7);
            sudokuBoard.SetValue(3, 8, 4);
            sudokuBoard.SetValue(5, 8, 5);
            sudokuBoard.SetValue(6, 8, 6);
            sudokuBoard.SetValue(7, 8, 9);
        }
    }

    class SudokuBoard
    {
        public Block[,] Blocks { get; private set; }

        public SudokuBoard()
        {
            Blocks = new Block[3, 3];
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                    Blocks[x, y] = new Block();
        }

        public void Solve()
        {
            MakeStep();
        }

        private void MakeStep()
        {
            //Resolve all definite possibilities
            while (ContainsDefiniteFields())
                foreach ((int x, int y, int value) p in GetDefiniteFields())
                    SetValue(p.x, p.y, p.value);

            //Resolve definite fields in a block
            while (ContainsDefiniteFieldInBlocks())
                foreach ((int x, int y, int value) p in GetDefiniteFieldsInBlocks())
                    SetValue(p.x, p.y, p.value);
        }

        public int GetValue(int x, int y)
        {
            return Blocks[x / 3, y / 3].GetValue(x % 3, y % 3);
        }
        public void SetValue(int x, int y, int value)
        {
            Blocks[x / 3, y / 3].SetValue(x % 3, y % 3, value);

            //Process according Possibilities
            for (int x2 = 0; x2 < 9; x2++)
                if (x2 != x)
                    Blocks[x2 / 3, y / 3].RemovePossibility(x2 % 3, y % 3, value);
            for (int y2 = 0; y2 < 9; y2++)
                if (y2 != y)
                    Blocks[x / 3, y2 / 3].RemovePossibility(x % 3, y2 % 3, value);
            for (int y2 = 0; y2 < 3; y2++)
                for (int x2 = 0; x2 < 3; x2++)
                    if (y2 != y || x2 != x)
                        Blocks[x / 3, y / 3].RemovePossibility(x2, y2, value);
        }
        public void RemoveValue(int x, int y)
        {
            var bk = Blocks[x / 3, y / 3].GetValue(x % 3, y % 3);

            Blocks[x / 3, y / 3].RemoveValue(x % 3, y % 3);

            //Update possibilities
            for (int y2 = 0; y2 < 9; y2++)
                for (int x2 = 0; x2 < 9; x2++)
                    if ((x2 / 3) != (x / 3) || (y2 / 3) != (y / 3))
                        if (x2 == x || y2 == y)
                            Blocks[x2 / 3, y2 / 3].AddPossibility(x2 % 3, y2 % 3, bk);
        }

        public int[] GetPossibilities(int x, int y)
        {
            return Blocks[x / 3, y / 3].GetPossibilities(x % 3, y % 3);
        }
        public int PossibilityCount(int x, int y)
        {
            return Blocks[x / 3, y / 3].PossibilityCount(x % 3, y % 3);
        }

        public int[] GetRow(int y)
        {
            int[] res = new int[9];
            for (int x = 0; x < 9; x++)
                res[x] = GetValue(x, y);
            return res;
        }
        public int[] GetColumn(int x)
        {
            int[] res = new int[9];
            for (int y = 0; y < 9; y++)
                res[y] = GetValue(x, y);
            return res;
        }

        (int, int, int)[] GetDefiniteFields()
        {
            List<(int, int, int)> res = new List<(int, int, int)>();
            for (int y = 0; y < 9; y++)
                for (int x = 0; x < 9; x++)
                    if (PossibilityCount(x, y) == 1)
                        res.Add((x, y, GetPossibilities(x, y).First()));
            return res.ToArray();
        }
        bool ContainsDefiniteFields()
        {
            for (int y = 0; y < 9; y++)
                for (int x = 0; x < 9; x++)
                    if (PossibilityCount(x, y) == 1)
                        return true;

            return false;
        }

        (int, int, int)[] GetDefiniteFieldsInBlocks()
        {
            List<(int, int, int)> res = new List<(int, int, int)>();
            for (int y = 0; y < 9; y += 3)
                for (int x = 0; x < 9; x += 3)
                {
                    List<(int x, int y, int value)> possibsInBlock = new List<(int, int, int)>();
                    for (int yi = 0; yi < 3; yi++)
                        for (int xi = 0; xi < 3; xi++)
                            possibsInBlock.AddRange(GetPossibilities(x + xi, y + yi).Select(p => (x + xi, y + yi, p)));
                    for (int i = 1; i < 10; i++)
                        if (possibsInBlock.Count(p => p.value == i) == 1)
                            res.Add(possibsInBlock.Where(p => p.value == i).First());
                }

            return res.ToArray();
        }
        bool ContainsDefiniteFieldInBlocks()
        {
            for (int y = 0; y < 9; y += 3)
                for (int x = 0; x < 9; x += 3)
                {
                    var possibsInBlock = new List<int>();
                    for (int yi = 0; yi < 3; yi++)
                        for (int xi = 0; xi < 3; xi++)
                            possibsInBlock.AddRange(GetPossibilities(x + xi, y + yi));
                    for (int i = 1; i < 10; i++)
                        if (possibsInBlock.Count(p => p == i) == 1)
                            return true;
                }

            return false;
        }

        public void Print()
        {
            for (int y = 0; y < 9; y++)
            {
                if (y % 3 == 0) Console.WriteLine("+------+------+------+");
                for (int x = 0; x < 9; x++)
                {
                    if (x % 3 == 0) Console.Write("|");
                    var val = Blocks[x / 3, y / 3].GetValue(x % 3, y % 3);
                    Console.Write(" " + ((val == -1) ? " " : val.ToString()));
                }
                Console.WriteLine("|");
            }
            Console.WriteLine("+------+------+------+");
        }
    }

    class Block
    {
        public Field[,] Fields { get; private set; }

        public Block()
        {
            Fields = new Field[3, 3];
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                    Fields[x, y] = new Field();
        }

        public int GetValue(int x, int y)
        {
            return Fields[x, y].GetValue();
        }
        public void SetValue(int x, int y, int value)
        {
            var bk = GetValue(x, y);

            Fields[x, y].SetValue(value);

            //Update possibilities
            for (int y2 = 0; y2 < 3; y2++)
                for (int x2 = 0; x2 < 3; x2++)
                    if (x2 != x || y2 != y)
                    {
                        RemovePossibility(x2, y2, value);
                        if (bk != -1)
                            AddPossibility(x2, y2, bk);
                    }
        }
        public int[] GetValues()
        {
            var res = new int[9];
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                    res[x + y * 3] = GetValue(x, y);
            return res;
        }
        public void RemoveValue(int x, int y)
        {
            var bk = GetValue(x, y);

            Fields[x, y].RemoveValue();

            //Update possibilities
            if (bk != -1)
                for (int y2 = 0; y2 < 3; y2++)
                    for (int x2 = 0; x2 < 3; x2++)
                        if (y2 != y || x2 != x)
                            AddPossibility(x2, y2, bk);
            SetPossibilities(x, y, GetPossibilities(x, y).Except(GetValues()).ToArray());
        }

        public int[] GetPossibilities(int x, int y)
        {
            return Fields[x, y].GetPossibilities();
        }
        public void SetPossibilities(int x, int y, int[] possib)
        {
            Fields[x, y].SetPossibilities(possib);
        }
        public void AddPossibility(int x, int y, int value)
        {
            Fields[x, y].AddPossibility(value);
        }
        public void RemovePossibility(int x, int y, int value)
        {
            Fields[x, y].RemovePossibility(value);
        }

        public int PossibilityCount(int x, int y)
        {
            return Fields[x, y].PossibilityCount;
        }

        public bool Contains(int value)
        {
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                    if (GetValue(x, y) == value)
                        return true;

            return false;
        }
    }

    [DebuggerDisplay("{Value}")]
    class Field
    {
        sbyte Value = -1;
        int[] Possibilities;
        public int PossibilityCount { get => Possibilities.Length; }

        public Field()
        {
            Possibilities = Enumerable.Range(1, 9).ToArray();
        }

        public int GetValue()
        {
            return Value;
        }
        public void SetValue(int value)
        {
            Value = (sbyte)value;
            RemovePossibilities();
        }
        public void RemoveValue()
        {
            Value = -1;
            SetPossibilities(Enumerable.Range(1, 9).ToArray());
        }

        public int[] GetPossibilities()
        {
            return Possibilities;
        }
        public void SetPossibilities(int[] possib)
        {
            Possibilities = possib;
        }
        public void AddPossibility(int possib)
        {
            if (possib != -1 && Value == -1)
            {
                var local = Possibilities.ToList();
                local.Add(possib);
                Possibilities = local.ToArray();
            }
        }
        public void RemovePossibility(int value)
        {
            if (value != -1 && Value == -1)
            {
                var local = Possibilities.ToList();
                local.Remove(value);
                Possibilities = local.ToArray();
            }
        }
        public void RemovePossibilities()
        {
            Possibilities = new int[0];
        }
    }
}
