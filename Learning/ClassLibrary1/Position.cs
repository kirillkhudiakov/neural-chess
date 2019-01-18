using System;
using System.Collections.Generic;

namespace DataHandlingLib
{
    public class Position
    {
        Figure[] Pieces = new Figure[32];

        public Position()
        {
            // Заполним белые фигуры
            Pieces[0] = new Figure("King", 1, 1, 5, 1);
            Pieces[1] = new Figure("Queen", 1, 1, 4, 1);
            Pieces[2] = new Figure("Rook", 1, 1, 1, 1);
            Pieces[3] = new Figure("Rook", 1, 1, 8, 1);
            Pieces[4] = new Figure("Knight", 1, 1, 2, 1);
            Pieces[5] = new Figure("Knight", 1, 1, 7, 1);
            Pieces[6] = new Figure("Bishop", 1, 1, 3, 1);
            Pieces[7] = new Figure("Bishop", 1, 1, 6, 1);
            for (int i = 8; i < 16; i++)
                Pieces[i] = new Figure("Pawn", 1, 1, i - 7, 2);

            // Черные
            Pieces[16] = new Figure("King", -1, 1, 5, 8);
            Pieces[17] = new Figure("Queen", -1, 1, 4, 8);
            Pieces[18] = new Figure("Rook", -1, 1, 1, 8);
            Pieces[19] = new Figure("Rook", -1, 1, 8, 8);
            Pieces[20] = new Figure("Knight", -1, 1, 2, 8);
            Pieces[21] = new Figure("Knight", -1, 1, 7, 8);
            Pieces[22] = new Figure("Bishop", -1, 1, 3, 8);
            Pieces[23] = new Figure("Bishop", -1, 1, 6, 8);
            for (int i = 24; i < 32; i++)
                Pieces[i] = new Figure("Pawn", -1, 1, i - 23, 7);
        }

        /// <summary>
        /// Возвращает конкретную фигуру по заданному индексу
        /// </summary>
        /// <param name="index">Индекс фигуры</param>
        /// <returns>Объект класса Figure</returns>
        public Figure this[int index]
        {
            get
            {
                return Pieces[index];
            }
        }

        /// <summary>
        /// Представляет текущую шахматную позицию в векторном виде
        /// </summary>
        /// <returns>Одномерный массив из 128 элементов</returns>
        public double[] Vector
        {
            get
            {
                var vector = new List<double>();
                for (int i = 0; i <= 16; i += 16)
                {
                    vector.Add(Pieces[i].Exist);
                    vector.Add(Pieces[i + 1].Exist);
                    vector.Add(Pieces[i + 2].Exist + Pieces[i + 3].Exist);
                    vector.Add(Pieces[i + 4].Exist + Pieces[i + 5].Exist);
                    vector.Add(Pieces[i + 6].Exist + Pieces[i + 7].Exist);
                    vector.Add(Pieces[i + 8].Exist + Pieces[i + 9].Exist + Pieces[i + 10].Exist + Pieces[i + 11].Exist
                         + Pieces[i + 12].Exist + Pieces[i + 13].Exist + Pieces[i + 14].Exist + Pieces[i + 15].Exist);

                }
                return vector.ToArray();
            }
        }

        /// <summary>
        /// Определяет, есть ли на данной клетке какая-либо фигура
        /// </summary>
        /// <param name="x">Координаты клетки по вертикали</param>
        /// <param name="y">Координаты клетки по горизонтали</param>
        /// <returns>1, если в клетке белая фигура. -1, если черная. 0, если клетка пуста</returns>
        int IsFigure(int x, int y)
        {
            foreach (var p in Pieces)
                if (p.PositionX == x && p.PositionY == y && p.Exist == 1)
                    return p.Side;
            return 0;
        }

        /// <summary>
        /// Проверяет, может ли фигура переместиться в заданную позицию
        /// </summary>
        /// <param name="index">Индекс фигуры</param>
        /// <param name="x">Коордата по вертикали</param>
        /// <param name="y">Координата по горизонтали</param>
        /// <returns>True, если можно переместить, иначе - false</returns>
        public bool CanMove(int index, int x, int y)
        {
            if (Pieces[index].Exist == 0)
                return false;
            if (TryMove(index, x, y))
            {
                var temp = DeepCopy();
                // Проверим условия рокировки, если она произошла
                if (Pieces[index].Name == "King")
                {
                    var dx = x - Pieces[index].PositionX;
                    if (dx == 2)
                    {
                        if (IsCheck(Pieces[index].Side == 1))
                            return false;
                        MakeStep(index, x + 1, y);
                        if (IsCheck(Pieces[index].Side == 1))
                        {
                            Pieces = temp.Pieces;
                            return false;
                        }
                        Pieces = temp.Pieces;
                    }
                    if (dx == -2)
                    {
                        if (IsCheck(Pieces[index].Side == 1))
                            return false;
                        MakeStep(index, x - 1, y);
                        if (IsCheck(Pieces[index].Side == 1))
                        {
                            Pieces = temp.Pieces;
                            return false;
                        }
                        Pieces = temp.Pieces;
                    }
                }

                // Проверим, будет ли находится король под шахом, если сделать данный ход
                temp = DeepCopy();
                MakeStep(index, x, y);

                if (IsCheck(Pieces[index].Side == 1))
                {
                    Pieces = temp.Pieces;
                    return false;
                }
                Pieces = temp.Pieces;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет, может ли фигура переместиться в заданную позицию, без учета шаха
        /// </summary>
        /// <param name="index">Индекс фигуры</param>
        /// <param name="x">Коордата по вертикали</param>
        /// <param name="y">Координата по горизонтали</param>
        /// <returns>True, если можно переместить, иначе - false</returns>
        bool TryMove(int index, int x, int y)
        {
            var dx = x - Pieces[index].PositionX;
            var dy = y - Pieces[index].PositionY;
            if (!InArea(x, y) || (dx == 0 && dy == 0))
                return false;
            switch (Pieces[index].Name)
            {
                case "King":
                    if (Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1 && Pieces[index].Side != IsFigure(x, y))
                        return true;
                    // Рокировка
                    if (Pieces[index].Side == 1 && Pieces[index].PositionX == 5 && Pieces[index].PositionY == 1
                        && x == 7 && y == 1 && IsFigure(6, 1) == 0 && IsFigure(7, 1) == 0
                        && Pieces[3].Exist == 1 && Pieces[3].PositionX == 8 && Pieces[3].PositionY == 1)
                        return true;
                    if (Pieces[index].Side == 1 && Pieces[index].PositionX == 5 && Pieces[index].PositionY == 1
                        && x == 3 && y == 1 && IsFigure(2, 1) == 0 && IsFigure(3, 1) == 0 && IsFigure(4, 1) == 0
                        && Pieces[2].Exist == 1 && Pieces[2].PositionX == 1 && Pieces[3].PositionY == 1)
                        return true;
                    if (Pieces[index].Side == -1 && Pieces[index].PositionX == 5 && Pieces[index].PositionY == 8
                        && x == 2 && y == 8 && IsFigure(2, 1) == 0 && IsFigure(3, 1) == 0 && IsFigure(4, 1) == 0
                        && Pieces[18].Exist == 1 && Pieces[18].PositionX == 1 && Pieces[18].PositionY == 8)
                        return true;
                    if (Pieces[index].Side == -1 && Pieces[index].PositionX == 5 && Pieces[index].PositionY == 8
                        && x == 6 && y == 8 && IsFigure(6, 1) == 0 && IsFigure(7, 1) == 0
                        && Pieces[19].Exist == 1 && Pieces[18].PositionX == 8 && Pieces[18].PositionY == 8)
                        return true;
                    break;

                case "Queen":
                    if (Pieces[index].Side != IsFigure(x, y))
                    {
                        if (dx == dy && dx > 0)
                        {
                            for (int i = Pieces[index].PositionX + 1, j = Pieces[index].PositionY + 1; i < x; i++, j++)
                                if (IsFigure(i, j) != 0)
                                    return false;
                            return true;
                        }
                        if (dx == dy && dx < 0)
                        {
                            for (int i = Pieces[index].PositionX - 1, j = Pieces[index].PositionY - 1; i > x; i--, j--)
                                if (IsFigure(i, j) != 0)
                                    return false;
                            return true;
                        }
                        if (dx == -dy && dx > 0)
                        {
                            for (int i = Pieces[index].PositionX + 1, j = Pieces[index].PositionY - 1; i < x; i++, j--)
                                if (IsFigure(i, j) != 0)
                                    return false;
                            return true;
                        }
                        if (dx == -dy && dx < 0)
                        {
                            for (int i = Pieces[index].PositionX - 1, j = Pieces[index].PositionY + 1; i > x; i--, j++)
                                if (IsFigure(i, j) != 0)
                                    return false;
                            return true;
                        }
                        if (dx == 0 && dy > 0)
                        {
                            for (int j = Pieces[index].PositionY + 1; j < y; j++)
                                if (IsFigure(x, j) != 0)
                                    return false;
                            return true;
                        }
                        if (dx == 0 && dy < 0)
                        {
                            for (int j = Pieces[index].PositionY - 1; j > y; j--)
                                if (IsFigure(x, j) != 0)
                                    return false;
                            return true;
                        }
                        if (dy == 0 && dx > 0)
                        {
                            for (int i = Pieces[index].PositionX + 1; i < x; i++)
                                if (IsFigure(i, y) != 0)
                                    return false;
                            return true;
                        }
                        if (dy == 0 && dx < 0)
                        {
                            for (int i = Pieces[index].PositionX - 1; i > x; i--)
                                if (IsFigure(i, y) != 0)
                                    return false;
                            return true;
                        }
                    }
                    break;

                case "Rook":
                    if (Pieces[index].Side != IsFigure(x, y))
                    {
                        if (dx == 0 && dy > 0)
                        {
                            for (int j = Pieces[index].PositionY + 1; j < y; j++)
                                if (IsFigure(x, j) != 0)
                                    return false;
                            return true;
                        }
                        if (dx == 0 && dy < 0)
                        {
                            for (int j = Pieces[index].PositionY - 1; j > y; j--)
                                if (IsFigure(x, j) != 0)
                                    return false;
                            return true;
                        }
                        if (dy == 0 && dx > 0)
                        {
                            for (int i = Pieces[index].PositionX + 1; i < x; i++)
                                if (IsFigure(i, y) != 0)
                                    return false;
                            return true;
                        }
                        if (dy == 0 && dx < 0)
                        {
                            for (int i = Pieces[index].PositionX - 1; i > x; i--)
                                if (IsFigure(i, y) != 0)
                                    return false;
                            return true;
                        }
                    }
                    break;

                case "Knight":
                    if (Pieces[index].Side != IsFigure(x, y) &&
                        ((Math.Abs(dx) == 1 && Math.Abs(dy) == 2) || (Math.Abs(dx) == 2 && Math.Abs(dy) == 1)))
                        return true;
                    break;

                case "Bishop":
                    if (Pieces[index].Side != IsFigure(x, y))
                    {
                        if (dx == dy && dx > 0)
                        {
                            for (int i = Pieces[index].PositionX + 1, j = Pieces[index].PositionY + 1; i < x; i++, j++)
                                if (IsFigure(i, j) != 0)
                                    return false;
                            return true;
                        }
                        if (dx == dy && dx < 0)
                        {
                            for (int i = Pieces[index].PositionX - 1, j = Pieces[index].PositionY - 1; i > x; i--, j--)
                                if (IsFigure(i, j) != 0)
                                    return false;
                            return true;
                        }
                        if (dx == -dy && dx > 0)
                        {
                            for (int i = Pieces[index].PositionX + 1, j = Pieces[index].PositionY - 1; i < x; i++, j--)
                                if (IsFigure(i, j) != 0)
                                    return false;
                            return true;
                        }
                        if (dx == -dy && dx < 0)
                        {
                            for (int i = Pieces[index].PositionX - 1, j = Pieces[index].PositionY + 1; i > x; i--, j++)
                                if (IsFigure(i, j) != 0)
                                    return false;
                            return true;
                        }
                    }
                    break;

                case "Pawn":
                    if (Pieces[index].Side != IsFigure(x, y))
                    {
                        var enemyPawn = GetIndex(x, Pieces[index].PositionY);
                        if (Pieces[index].Side == 1)
                        {
                            if ((dx == 0 && dy == 1 && IsFigure(x, y) == 0)
                                || (dx == 0 && dy == 2 && IsFigure(x, y) == 0 && IsFigure(x, y - 1) == 0
                                && Pieces[index].PositionY == 2)
                                || (Math.Abs(dx) == 1 && dy == 1 && IsFigure(x, y) == -1))
                                return true;

                            // Взятие на проходе
                            if (Pieces[index].PositionY == 5 && dy == 1 && Math.Abs(dx) == 1
                                && enemyPawn > 23 && enemyPawn < 32 && IsFigure(x, y) == 0)
                                return true;
                        }
                        if (Pieces[index].Side == -1)
                        {
                            if ((dx == 0 && dy == -1 && IsFigure(x, y) == 0)
                                || (dx == 0 && dy == -2 && IsFigure(x, y) == 0 && IsFigure(x, y + 1) == 0
                                && Pieces[index].PositionY == 7)
                                || (Math.Abs(dx) == 1 && dy == -1 && IsFigure(x, y) == 1))
                                return true;

                            // Взятие на проходе
                            if (Pieces[index].PositionY == 4 && dy == -1 && Math.Abs(dx) == 1
                                && enemyPawn > 7 && enemyPawn < 16 && IsFigure(x, y) == 0)
                                return true;
                        }
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Возвращает список всех возможных координат шахматной доски, куда может сходить фигура
        /// </summary>
        /// <param name="index">Индекс данной фигуры</param>
        /// <returns>Список из массивов координат</returns>
        public List<int[]> PossibleMoves(int index)
        {
            List<int[]> moves = AllMoves(index);
            if (Pieces[index].Exist == 0)
                return moves;

            for (int i = 0; i < moves.Count; i++)
            {
                var piece = Pieces[index];
                int x = piece.PositionX;
                var y = piece.PositionY;
                var temp = DeepCopy();


                if (piece.Name == "King")
                {
                    if (moves[i][0] - x == 2)
                    {
                        if (IsCheck(piece.Side == 1))
                        {
                            Pieces = temp.Pieces;
                            moves.Remove(moves[i]);
                            i--;
                            continue;
                        }
                        MakeStep(index, x + 1, y);
                        if (IsCheck(piece.Side == 1))
                        {
                            Pieces = temp.Pieces;
                            moves.Remove(moves[i]);
                            i--;
                            continue;
                        }
                        Pieces = temp.Pieces;
                    }
                    if (moves[i][0] - x == -2)
                    {
                        if (IsCheck(piece.Side == 1))
                        {
                            Pieces = temp.Pieces;
                            moves.Remove(moves[i]);
                            i--;
                            continue;
                        }
                        MakeStep(index, x - 1, y);
                        if (IsCheck(piece.Side == 1))
                        {
                            Pieces = temp.Pieces;
                            moves.Remove(moves[i]);
                            i--;
                            continue;
                        }
                        Pieces = temp.Pieces;
                    }
                }

                temp = DeepCopy();
                MakeStep(index, moves[i][0], moves[i][1]);
                if (IsCheck(piece.Side == 1))
                {
                    moves.Remove(moves[i]);
                    i--;
                }
                Pieces = temp.Pieces;
            }

            return moves;
        }
        public List<int[]> PossibleMoves(int x0, int y0)
        {
            var index = GetIndex(x0, y0);

            List<int[]> moves = AllMoves(index);
            if (Pieces[index].Exist == 0)
                return moves;

            for (int i = 0; i < moves.Count; i++)
            {
                var piece = Pieces[index];
                var x = piece.PositionX;
                var y = piece.PositionY;
                var temp = DeepCopy();


                if (piece.Name == "King")
                {
                    if (moves[i][0] - x == 2)
                    {
                        if (IsCheck(piece.Side == 1))
                        {
                            Pieces = temp.Pieces;
                            moves.Remove(moves[i]);
                            i--;
                            continue;
                        }
                        MakeStep(index, x + 1, y);
                        if (IsCheck(piece.Side == 1))
                        {
                            Pieces = temp.Pieces;
                            moves.Remove(moves[i]);
                            i--;
                            continue;
                        }
                        Pieces = temp.Pieces;
                    }
                    if (moves[i][0] - x == -2)
                    {
                        if (IsCheck(piece.Side == 1))
                        {
                            Pieces = temp.Pieces;
                            moves.Remove(moves[i]);
                            i--;
                            continue;
                        }
                        MakeStep(index, x - 1, y);
                        if (IsCheck(piece.Side == 1))
                        {
                            Pieces = temp.Pieces;
                            moves.Remove(moves[i]);
                            i--;
                            continue;
                        }
                        Pieces = temp.Pieces;
                    }
                }

                temp = DeepCopy();
                MakeStep(index, moves[i][0], moves[i][1]);
                if (IsCheck(piece.Side == 1))
                {
                    moves.Remove(moves[i]);
                    i--;
                }
                Pieces = temp.Pieces;
            }

            return moves;
        }

        /// <summary>
        /// Возвращает список всех возможных координат шахматной доски, куда может сходить фигура (без учета шаха).
        /// </summary>
        /// <param name="index">Индекс данной фигуры</param>
        /// <returns>Список из массивов координат</returns>
        public List<int[]> AllMoves(int index)
        {
            List<int[]> Moves = new List<int[]>();
            var piece = Pieces[index];
            var x0 = Pieces[index].PositionX;
            var y0 = Pieces[index].PositionY;
            if (piece.Exist == 0)
                return Moves;
            switch (piece.Name)
            {
                case "King":
                    if (InArea(x0, y0 + 1) && IsFigure(x0, y0 + 1) != piece.Side)
                        Moves.Add(new int[] { x0, y0 + 1 });
                    if (InArea(x0, y0 - 1) && IsFigure(x0, y0 - 1) != piece.Side)
                        Moves.Add(new int[] { x0, y0 - 1 });
                    if (InArea(x0 + 1, y0) && IsFigure(x0 + 1, y0) != piece.Side)
                        Moves.Add(new int[] { x0 + 1, y0 });
                    if (InArea(x0 - 1, y0) && IsFigure(x0 - 1, y0) != piece.Side)
                        Moves.Add(new int[] { x0 - 1, y0 });
                    if (InArea(x0 + 1, y0 + 1) && IsFigure(x0 + 1, y0 + 1) != piece.Side)
                        Moves.Add(new int[] { x0 + 1, y0 + 1 });
                    if (InArea(x0 + 1, y0 - 1) && IsFigure(x0 + 1, y0 - 1) != piece.Side)
                        Moves.Add(new int[] { x0 + 1, y0 - 1 });
                    if (InArea(x0 - 1, y0 + 1) && IsFigure(x0 - 1, y0 + 1) != piece.Side)
                        Moves.Add(new int[] { x0 - 1, y0 + 1 });
                    if (InArea(x0 - 1, y0 - 1) && IsFigure(x0 - 1, y0 - 1) != piece.Side)
                        Moves.Add(new int[] { x0 - 1, y0 - 1 });
                    if (piece.Side == 1 && x0 == 5 && y0 == 1)
                    {
                        if (Pieces[2].PositionX == 1 && Pieces[2].PositionY == 1
                            && IsFigure(2, 1) == 0 && IsFigure(3, 1) == 0 && IsFigure(4, 1) == 0)
                            Moves.Add(new int[] { 3, 1 });
                        if (Pieces[3].PositionX == 8 && Pieces[3].PositionY == 1
                            && IsFigure(6, 1) == 0 && IsFigure(7, 1) == 0)
                            Moves.Add(new int[] { 7, 1 });
                    }
                    if (piece.Side == -1 && x0 == 5 && y0 == 8)
                    {
                        if (Pieces[19].PositionX == 8 && Pieces[19].PositionY == 8
                            && IsFigure(6, 8) == 0 && IsFigure(7, 8) == 0)
                            Moves.Add(new int[] { 6, 8 });
                        if (Pieces[18].PositionX == 1 && Pieces[18].PositionY == 8
                            && IsFigure(2, 8) == 0 && IsFigure(3, 8) == 0 && IsFigure(4, 8) == 0)
                            Moves.Add(new int[] { 2, 8 });
                    }
                    break;

                case "Queen":
                    for (int i = -1; i < 2; i += 2)
                        for (int j = -1; j < 2; j += 2)
                            for (int dx = x0 + i, dy = y0 + j; InArea(dx, dy); dx += i, dy += j)
                            {
                                if (IsFigure(dx, dy) == 0)
                                {
                                    Moves.Add(new int[] { dx, dy });
                                }
                                if (IsFigure(dx, dy) == -piece.Side)
                                {
                                    Moves.Add(new int[] { dx, dy });
                                    break;
                                }
                                if (IsFigure(dx, dy) == piece.Side)
                                {
                                    break;
                                }
                            }
                    for (int i = -1; i < 2; i += 2)
                    {
                        for (int dx = x0 + i; InArea(dx, y0); dx += i)
                        {
                            if (IsFigure(dx, y0) == 0)
                            {
                                Moves.Add(new int[] { dx, y0 });
                            }
                            if (IsFigure(dx, y0) == -piece.Side)
                            {
                                Moves.Add(new int[] { dx, y0 });
                                break;
                            }
                            if (IsFigure(dx, y0) == piece.Side)
                            {
                                break;
                            }
                        }
                        for (int dy = y0 + i; InArea(x0, dy); dy += i)
                        {
                            if (IsFigure(x0, dy) == 0)
                            {
                                Moves.Add(new int[] { x0, dy });
                            }
                            if (IsFigure(x0, dy) == -piece.Side)
                            {
                                Moves.Add(new int[] { x0, dy });
                                break;
                            }
                            if (IsFigure(x0, dy) == piece.Side)
                            {
                                break;
                            }
                        }
                    }
                    break;

                case "Rook":
                    for (int i = -1; i < 2; i += 2)
                    {
                        for (int dx = x0 + i; InArea(dx, y0); dx += i)
                        {
                            if (IsFigure(dx, y0) == 0)
                            {
                                Moves.Add(new int[] { dx, y0 });
                            }
                            if (IsFigure(dx, y0) == -piece.Side)
                            {
                                Moves.Add(new int[] { dx, y0 });
                                break;
                            }
                            if (IsFigure(dx, y0) == piece.Side)
                            {
                                break;
                            }
                        }
                        for (int dy = y0 + i; InArea(x0, dy); dy += i)
                        {
                            if (IsFigure(x0, dy) == 0)
                            {
                                Moves.Add(new int[] { x0, dy });
                            }
                            if (IsFigure(x0, dy) == -piece.Side)
                            {
                                Moves.Add(new int[] { x0, dy });
                                break;
                            }
                            if (IsFigure(x0, dy) == piece.Side)
                            {
                                break;
                            }
                        }
                    }
                    break;

                case "Knight":
                    for (int i = -1; i < 2; i += 2)
                        for (int j = -1; j < 2; j += 2)
                            if (InArea(x0 + 2 * i, y0 + j) && IsFigure(x0 + 2 * i, y0 + j) != piece.Side)
                                Moves.Add(new int[] { x0 + 2 * i, y0 + j });
                    for (int i = -1; i < 2; i += 2)
                        for (int j = -1; j < 2; j += 2)
                            if (InArea(x0 + i, y0 + 2 * j) && IsFigure(x0 + i, y0 + 2 * j) != piece.Side)
                                Moves.Add(new int[] { x0 + i, y0 + 2 * j });
                    break;

                case "Bishop":
                    for (int i = -1; i < 2; i += 2)
                        for (int j = -1; j < 2; j += 2)
                            for (int dx = x0 + i, dy = y0 + j; InArea(dx, dy); dx += i, dy += j)
                            {
                                if (IsFigure(dx, dy) == 0)
                                {
                                    Moves.Add(new int[] { dx, dy });
                                }
                                if (IsFigure(dx, dy) == -piece.Side)
                                {
                                    Moves.Add(new int[] { dx, dy });
                                    break;
                                }
                                if (IsFigure(dx, dy) == piece.Side)
                                {
                                    break;
                                }
                            }
                    break;

                case "Pawn":
                    if (InArea(x0 - 1, y0 + piece.Side) && IsFigure(x0 - 1, y0 + piece.Side) == -piece.Side)
                        Moves.Add(new int[] { x0 - 1, y0 + piece.Side });
                    if (InArea(x0 + 1, y0 + piece.Side) && IsFigure(x0 + 1, y0 + piece.Side) == -piece.Side)
                        Moves.Add(new int[] { x0 + 1, y0 + piece.Side });
                    if (InArea(x0, y0 + piece.Side) && IsFigure(x0, y0 + piece.Side) == 0)
                        Moves.Add(new int[] { x0, y0 + piece.Side });
                    if (piece.Side == 1 && y0 == 1 && IsFigure(x0, y0 + 1) == 0 && IsFigure(x0, y0 + 2) == 0)
                        Moves.Add(new int[] { x0, y0 + 2 });
                    if (piece.Side == -1 && y0 == 7 && IsFigure(x0, y0 - 1) == 0 && IsFigure(x0, y0 - 2) == 0)
                        Moves.Add(new int[] { x0, y0 - 2 });
                    if (piece.Side == 1 && y0 == 5 && InArea(x0 + 1, 6) && IsFigure(x0 + 1, 5) == -1 && IsFigure(x0 + 1, 6) == 0)
                        Moves.Add(new int[] { x0 + 1, 6 });
                    if (piece.Side == 1 && y0 == 5 && InArea(x0 - 1, 6) && IsFigure(x0 - 1, 5) == -1 && IsFigure(x0 - 1, 6) == 0)
                        Moves.Add(new int[] { x0 - 1, 6 });
                    if (piece.Side == -1 && y0 == 4 && InArea(x0 + 1, 3) && IsFigure(x0 + 1, 4) == -1 && IsFigure(x0 + 1, 3) == 0)
                        Moves.Add(new int[] { x0 + 1, 3 });
                    if (piece.Side == -1 && y0 == 4 && InArea(x0 - 1, 3) && IsFigure(x0 - 1, 4) == -1 && IsFigure(x0 - 1, 3) == 0)
                        Moves.Add(new int[] { x0 - 1, 3 });
                    break;
            }
            return Moves;
        }

        /// <summary>
        /// Проверяет, находится ли данная позиция в пределах шахматной доски
        /// </summary>
        /// <param name="x">Координата по горизонтали</param>
        /// <param name="y">Координата по вертикали</param>
        /// <returns>True, если позиция находится в допустимых пределах, иначе - false</returns>
        bool InArea(int x, int y)
        {
            return (x >= 1 && x <= 8 && y >= 1 && y <= 8);
        }

        /// <summary>
        /// Возвращает индекс фигуры, находящийся в указанной клетке
        /// </summary>
        /// <param name="x">Координаты по горизонтали</param>
        /// <param name="y">Координаты по вертикали</param>
        /// <returns>Целое число от нуля до 31. -1, если фигура не обнаружена.</returns>
        public int GetIndex(int x, int y)
        {
            for (int i = 0; i < 32; i++)
                if (Pieces[i].PositionX == x && Pieces[i].PositionY == y && Pieces[i].Exist == 1)
                    return i;
            return -1;
        }

        /// <summary>
        /// Проверяет, поставлен ли шах указанной стороне
        /// </summary>
        /// <param name="isWhite">>true для проверки белых. false для проверки черных</param>
        /// <returns>true, если поставлен шах; иначе false</returns>
        public bool IsCheck(bool isWhite)
        {
            if (!isWhite)
            {
                // Координаты черного короля
                int x = Pieces[16].PositionX;
                int y = Pieces[16].PositionY;
                for (int i = 0; i < 16; i++)
                {
                    if (Pieces[i].Exist == 1 && TryMove(i, x, y))
                    {
                        return true;
                    }
                }
            }
            else
            {
                // Координаты белого короля
                int x = Pieces[0].PositionX;
                int y = Pieces[0].PositionY;
                for (int i = 16; i < 32; i++)
                {
                    if (Pieces[i].Exist == 1 && TryMove(i, x, y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Проверяет, поставлен ли мат указанной стороне
        /// </summary>
        /// <param name="isWhite">>true для проверки белых. false для проверки черных</param>
        /// <returns>>true, если поставлен мат; иначе false</returns>
        public bool IsMate(bool isWhite)
        {
            if (IsCheck(isWhite))
            {
                bool hasMove = false;
                for (int i = 0; i < 16; i++)
                    if (PossibleMoves(i).Count > 0)
                        hasMove = true;

                if (!hasMove)
                    return true;
            }

            if (IsCheck(isWhite))
            {
                bool hasMove = false;
                for (int i = 16; i < 32; i++)
                    if (PossibleMoves(i).Count > 0)
                        hasMove = true;

                if (!hasMove)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Возращает true, если возник пат. false, в обратном случае.
        /// </summary>
        public bool IsStal
        {
            get
            {
                if (!IsCheck(true) && !IsCheck(false))
                {
                    bool hasMove = false;
                    for (int i = 0; i < 16; i++)
                        if (PossibleMoves(i).Count > 0)
                            hasMove = true;
                    if (!hasMove)
                        return true;

                    hasMove = false;
                    for (int i = 16; i < 32; i++)
                        if (PossibleMoves(i).Count > 0)
                            hasMove = true;
                    if (!hasMove)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Сделать ход указанной фигурой в указанную клетку
        /// </summary>
        /// <param name="index">Индекс фигуры</param>
        /// <param name="x">Координата по горизонтали</param>
        /// <param name="y">Координата по вертикали</param>
        public void MakeStep(int index, int x, int y)
        {
            var piece = Pieces[index];
            var x0 = piece.PositionX;
            var y0 = piece.PositionY;

            // Передвинем ладью, если произошла рокировка
            if (piece.Name == "King")
            {
                if (x - x0 == 2)
                {
                    if (piece.Side == 1)
                    {
                        Pieces[3].PositionX = 6;
                    }
                    else
                    {
                        Pieces[19].PositionX = 6;
                    }
                }
                if (x - x0 == -2)
                {
                    if (piece.Side == 1)
                    {
                        Pieces[2].PositionX = 4;
                    }
                    else
                    {
                        Pieces[18].PositionX = 4;
                    }
                }
            }

            int opposite;

            // Удалим вражескую пешку, если произошло взятие на проходе
            if (piece.Name == "Pawn" && Math.Abs(x - x0) == 1 && Math.Abs(y - y0) == 1 && IsFigure(x, y) == 0)
            {
                opposite = GetIndex(x, y0);
                Pieces[opposite].Exist = 0;
            }

            // Удалим противоположную фигуру с клетки, если она там есть
            opposite = GetIndex(x, y);
            if (opposite != -1)
            {
                Pieces[opposite].Exist = 0;
            }

            // Переместим заданную фигуру в клетку
            Pieces[index].PositionX = x;
            Pieces[index].PositionY = y;

            if (Pieces[index].Name == "Pawn" && ((Pieces[index].Side == 1 && Pieces[index].PositionY == 8)
                || (Pieces[index].Side == -1 && Pieces[index].PositionY == 1)))
            {
                Pieces[index].Name = "Queen";
            }
        }
        public void MakeStep(int x0, int y0, int x1, int y1)
        {
            var index = GetIndex(x0, y0);
            var piece = Pieces[index];

            // Передвинем ладью, если произошла рокировка
            if (piece.Name == "King")
            {
                if (x1 - x0 == 2)
                {
                    if (piece.Side == 1)
                    {
                        Pieces[3].PositionX = 6;
                    }
                    else
                    {
                        Pieces[19].PositionX = 6;
                    }
                }
                if (x1 - x0 == -2)
                {
                    if (piece.Side == 1)
                    {
                        Pieces[2].PositionX = 4;
                    }
                    else
                    {
                        Pieces[18].PositionX = 4;
                    }
                }
            }

            int opposite;

            // Удалим вражескую пешку, если произошло взятие на проходе
            if (piece.Name == "Pawn" && Math.Abs(x1 - x0) == 1 && Math.Abs(y1 - y0) == 1 && IsFigure(x1, y1) == 0)
            {
                opposite = GetIndex(x1, y0);
                Pieces[opposite].Exist = 0;
            }

            // Удалим противоположную фигуру с клетки, если она там есть
            opposite = GetIndex(x1, y1);
            if (opposite != -1)
            {
                Pieces[opposite].Exist = 0;
            }

            // Переместим заданную фигуру в клетку
            Pieces[index].PositionX = x1;
            Pieces[index].PositionY = y1;

            if (Pieces[index].Name == "Pawn" && ((Pieces[index].Side == 1 && Pieces[index].PositionY == 8)
                || (Pieces[index].Side == -1 && Pieces[index].PositionY == 1)))
            {
                Pieces[index].Name = "Queen";
            }
        }

        /// <summary>
        /// Возращает полную копию данного объекта
        /// </summary>
        /// <returns>Объект класса Position</returns>
        public Position DeepCopy()
        {
            Position copy = (Position)MemberwiseClone();
            copy.Pieces = new Figure[32];
            for (int i = 0; i < 32; i++)
                copy.Pieces[i] = new Figure
                    (String.Copy(Pieces[i].Name), Pieces[i].Side, Pieces[i].Exist, Pieces[i].PositionX, Pieces[i].PositionY);

            return copy;
        }

        public List<int> AvailablePieces(int x, int y, bool isWhite)
        {
            var indexes = new List<int>();

            int i;
            if (isWhite)
                i = 0;
            else
                i = 16;

            for (int k = 0; k < 16; k++, i++)
                if (CanMove(i, x, y))
                    indexes.Add(i);

            return indexes;
        }
    }
}
