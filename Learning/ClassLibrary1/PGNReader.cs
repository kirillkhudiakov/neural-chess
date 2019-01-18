using System;
using System.IO;
using System.Collections.Generic;

namespace DataHandlingLib
{
    public class PGNReader
    {
        // Game хранит список ходов каждой партии
        public List<string[]> Game = new List<string[]>();

        public List<int> Result = new List<int>();

        Position Board;

        public PGNReader(string path)
        {
            // В input содержатся все строки входного pgn-файла
            var input = File.ReadAllLines(path);
            for (int i = 0; i < input.Length; i++)
            {
                // Из входного файла необходимо обрабатывать только строки, которые содержат список ходов,
                // они начианаются с символа '1'
                if (input[i] != string.Empty && input[i][0] == '1'
                    && (input[i - 2].Contains("1-0") || input[i - 2].Contains("0-1")))
                {
                    if (input[i].Contains("=R") || input[i].Contains("=N") || input[i].Contains("=B"))
                        continue;

                    if (input[i - 2].Contains("1-0"))
                        Result.Add(1);
                    else if (input[i - 2].Contains("0-1"))
                        Result.Add(-1);
                    else Result.Add(0);

                    // steps создает массив строк, которые являются подстроками строки ходов, разделенной пробелами
                    var steps = input[i].Split(' ');

                    // l является индексом подстроки, в которой заканчивается описание ходов
                    int l;
                    for (l = steps.Length - 1; steps[l][0] != '{'; l--) ;
                    // Будем добавлять данные о партии, только когда сделан хотя бы 1 ход
                    if (l > 1)
                    {
                        // В list будем добавлять каждый ход
                        var list = new List<string>();
                        for (int j = 0; j < l; j++)
                            if (j % 3 != 0)
                                list.Add(steps[j]);
                        Game.Add(list.ToArray());
                    }
                }
            }
        }

        public List<int[]> GetSteps(int index)
        {
            var record = Game[index];
            var list = new List<int[]>();
            var board = new Position();
            int isWhite = 1;

            for (int i = 0; i < record.Length; i++)
            {
                // Удалим символы шаха, мата и взятия
                int t;
                if (record[i].Contains("x"))
                {
                    t = record[i].IndexOf('x');
                    record[i] = record[i].Remove(t, 1);
                }
                if (record[i].Contains("+"))
                {
                    t = record[i].IndexOf('+');
                    record[i] = record[i].Remove(t, 1);
                }
                if (record[i].Contains("#"))
                {
                    t = record[i].IndexOf('#');
                    record[i] = record[i].Remove(t, 1);
                }

                // Проведем рокировку, если она совершилась
                if (record[i] == "O-O")
                {
                    if (isWhite == 1)
                    {
                        list.Add(new int[] { 5, 1, 7, 1 });
                        board.MakeStep(5, 1, 7, 1);
                        isWhite *= -1;
                        continue;
                    }
                    else
                    {
                        list.Add(new int[] { 5, 8, 7, 8 });
                        board.MakeStep(5, 8, 7, 8);
                        isWhite *= -1;
                        continue;
                    }
                }
                if (record[i] == "O-O-O")
                {
                    if (isWhite == 1)
                    {
                        list.Add(new int[] { 5, 1, 3, 1 });
                        board.MakeStep(5, 1, 3, 1);
                        isWhite *= -1;
                        continue;
                    }
                    else
                    {
                        list.Add(new int[] { 5, 8, 3, 8 });
                        board.MakeStep(5, 8, 3, 8);
                        isWhite *= -1;
                        continue;
                    }
                }

                var len = record[i].Length;
                int[] xy;

                // Переменные, необходимые в том случае, если произошло превращение пешки
                bool promotion = false;
                string newType = null;

                if (record[i][len - 2] != '=')
                {
                    xy = GetCell(record[i].Substring(len - 2));
                    record[i] = record[i].Remove(len - 2);
                }
                else
                {
                    promotion = true;
                    newType = FullName(record[i][len - 1]);
                    xy = GetCell(record[i].Substring(len - 4, 2));
                    record[i] = record[i].Remove(len - 4);
                }

                var indexes = board.AvailablePieces(xy[0], xy[1], isWhite == 1);

                if (indexes.Count > 1)
                {
                    string name;
                    if (record[i] != String.Empty && Char.IsUpper(record[i][0]))
                    {
                        name = FullName(record[i][0]);
                        record[i] = record[i].Substring(1);
                    }
                    else
                    {
                        name = "Pawn";
                    }

                    for (int j = 0; j < indexes.Count; j++)
                    {
                        if (board[indexes[j]].Name != name)
                        {
                            indexes.Remove(indexes[j]);
                            j--;
                        }
                    }
                }
                if (indexes.Count > 1 && Char.IsLower(record[i][0]))
                {
                    var col = LetterToNumber(record[i][0]);
                    record[i] = record[i].Substring(1);
                    for (int j = 0; j < indexes.Count; j++)
                    {
                        if (board[indexes[j]].PositionX != col)
                        {
                            indexes.Remove(indexes[j]);
                            j--;
                        }
                    }
                }
                if (indexes.Count > 1 && Char.IsDigit(record[i][0]))
                {
                    var row = int.Parse(record[i][0].ToString());
                    record[i] = record[i].Substring(1);
                    for (int j = 0; j < indexes.Count; j++)
                    {
                        if (board[indexes[j]].PositionY != row)
                        {
                            indexes.Remove(indexes[j]);
                            j--;
                        }
                    }
                }

                var x0 = board[indexes[0]].PositionX;
                var y0 = board[indexes[0]].PositionY;
                list.Add(new int[] { x0, y0, xy[0], xy[1] });
                board.MakeStep(x0, y0, xy[0], xy[1]);
                if (promotion)
                    board[board.GetIndex(xy[0], xy[1])].Name = newType;

                isWhite *= -1;
            }
            return list;
        }

        int[] GetCell(string str)
        {
            var coord = new int[2];
            coord[0] = LetterToNumber(str[0]);
            coord[1] = int.Parse(str[1].ToString());
            return coord;
        }

        string FullName(char n)
        {
            string name = null;
            switch (n)
            {
                case 'K':
                    name = "King";
                    break;
                case 'Q':
                    name = "Queen";
                    break;
                case 'R':
                    name = "Rook";
                    break;
                case 'N':
                    name = "Knight";
                    break;
                case 'B':
                    name = "Bishop";
                    break;
            }
            return name;
        }

        int LetterToNumber(char l)
        {
            int n = 0;
            switch (l)
            {
                case 'a':
                    n = 1;
                    break;
                case 'b':
                    n = 2;
                    break;
                case 'c':
                    n = 3;
                    break;
                case 'd':
                    n = 4;
                    break;
                case 'e':
                    n = 5;
                    break;
                case 'f':
                    n = 6;
                    break;
                case 'g':
                    n = 7;
                    break;
                case 'h':
                    n = 8;
                    break;
            }
            return n;
        }

        public List<List<int[]>> GetAllMatches()
        {
            var AllMatches = new List<List<int[]>>();
            for (int i = 0; i < Game.Count; i++)
            {
                AllMatches.Add(GetSteps(i));
            }
            return AllMatches;
        }
    }
}