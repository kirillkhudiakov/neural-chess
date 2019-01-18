using System;
using System.Collections.Generic;
using System.IO;
using NNLibrary;
using System.Runtime.Serialization.Formatters.Binary;

namespace DataHandlingLib
{
    public static class Trainer
    {
        private static Position Board;
        private static NeuralNetwork Net;
        private static Random Rnd = new Random();

        public static void LearnByData()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            string path = @"Data.pgn";
            PGNReader data = new PGNReader(path);

            Net = new NeuralNetwork(12, 3, 288);
            int winCount = 0;


            var Matches = data.GetAllMatches();
            var n = 0;

            for (int epoch = 1; true; epoch++)
            {
                for (int i = 0; i < Matches.Count; i++, n++)
                {
                    Console.Clear();
                    Console.Write(i + " / " + Matches.Count + " (" + epoch + ") " + winCount);
                    var steps = Matches[i];
                    var position = new Position();
                    int side = 1;
                    for (int s = 0; s < steps.Count; s++)
                    {
                        if (data.Result[i] == side)
                        {
                            double[] v;

                            int j;
                            if (side == 1)
                                j = 0;
                            else
                                j = 16;

                            var list = new List<int[]>();
                            do
                            {
                                var moves = position.PossibleMoves(j);
                                var x = position[j].PositionX;
                                var y = position[j].PositionY;
                                for (int k = 0; k < moves.Count; k++)
                                {
                                    if (!(x == steps[s][0] && y == steps[s][1]
                                        && moves[k][0] == steps[s][2] && moves[k][1] == steps[s][3]))
                                        list.Add(new int[] { x, y, moves[k][0], moves[k][1] });
                                }
                                j++;
                            } while (j % 16 != 0);


                            if (list.Count > 0)
                            {
                                var k = Rnd.Next(list.Count);
                                var temp = position.DeepCopy();
                                double response;
                                if (side == 1)
                                    response = 1.0;
                                else
                                    response = -1.0;

                                position.MakeStep(list[k][0], list[k][1], list[k][2], list[k][3]);
                                v = position.Vector;
                                Net.Train(v, 0, 0.01);

                                position = temp;
                                position.MakeStep(steps[s][0], steps[s][1], steps[s][2], steps[s][3]);
                                v = position.Vector;
                                Net.Train(v, response, 0.01);
                            }
                            else
                            {
                                position.MakeStep(steps[s][0], steps[s][1], steps[s][2], steps[s][3]);
                            }
                        }
                        else
                        {
                            position.MakeStep(steps[s][0], steps[s][1], steps[s][2], steps[s][3]);
                        }

                        side *= -1;
                    }
                    if (n > 0 && n % 1000 == 0)
                    {
                        var c = CheckNet();
                        if (c > winCount)
                        {
                            winCount = c;
                            string fileName = n.ToString();
                            FileStream fs = new FileStream(fileName, FileMode.Create);
                            formatter.Serialize(fs, Net);
                        }
                        else if (winCount > c)
                            return;
                    }
                }
            }
        }


        static int[] GetBestStep(NeuralNetwork net, int side)
        {
            double bestValue;
            if (side == 1)
                bestValue = double.MinValue;
            else
                bestValue = double.MaxValue;

            int x0 = -1, y0 = -1, x1 = -1, y1 = -1;

            int i;
            if (side == 1)
                i = 0;
            else
                i = 16;

            for (int n = 0; n < 16; n++, i++)
            {
                var moves = Board.PossibleMoves(i);
                for (int j = 0; j < moves.Count; j++)
                {
                    var temp = Board.DeepCopy();

                    Board.MakeStep(i, moves[j][0], moves[j][1]);
                    var vector = Board.Vector;
                    var value = net.Evaluate(vector);

                    Board = temp;

                    if (side == 1 && value >= bestValue)
                    {
                        x0 = Board[i].PositionX;
                        y0 = Board[i].PositionY;
                        x1 = moves[j][0];
                        y1 = moves[j][1];

                        bestValue = value;
                    }
                    if (side == -1 && value <= bestValue)
                    {
                        x0 = Board[i].PositionX;
                        y0 = Board[i].PositionY;
                        x1 = moves[j][0];
                        y1 = moves[j][1];

                        bestValue = value;
                    }
                }
            }

            return new int[] { x0, y0, x1, y1 };
        }

        static List<int[]> GetAllSteps(int side)
        {
            List<int[]> steps = new List<int[]>();

            int i;
            if (side == 1)
                i = 0;
            else
                i = 16;

            for (int n = 0; n < 16; n++, i++)
            {
                var moves = Board.PossibleMoves(i);
                for (int j = 0; j < moves.Count; j++)
                    steps.Add(new int[] { Board[i].PositionX, Board[i].PositionY, moves[j][0], moves[j][1] });
            }

            return steps;
        }

        public static int CheckNet()
        {
            int winCount = 0;
            int commonSide = 1;

            for (int n = 0; n < 10; n++, commonSide *= -1)
            {
                Board = new Position();
                int side = 1;
                for (int k = 1; true; k++)
                {
                    if (Board.IsMate(true))
                    {
                        if (commonSide == 1)
                            Console.WriteLine("Lose");
                        else
                        {
                            Console.WriteLine("Win");
                            winCount++;
                        }
                        break;
                    }
                    if (Board.IsMate(false))
                    {
                        if (commonSide == 1)
                        {
                            Console.WriteLine("Win");
                            winCount++;
                        }
                        else
                            Console.WriteLine("Lose");
                        break;
                    }
                    if (Board.IsStal || k > 600)
                    {
                        Console.WriteLine("Draw");
                        break;
                    }


                    if (side == commonSide)
                    {
                        var step = GetBestStep(Net, side);
                        Board.MakeStep(step[0], step[1], step[2], step[3]);
                    }
                    else
                    {
                        var steps = GetAllSteps(side);
                        var r = Rnd.Next(steps.Count);
                        Board.MakeStep(steps[r][0], steps[r][1], steps[r][2], steps[r][3]);
                    }
                    side *= -1;
                }
            }
            return winCount;
        }
    }
}