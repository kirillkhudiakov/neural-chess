using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using NNLibrary;

public class AI : MonoBehaviour
{
    GameManager GM;
    NeuralNetwork Net;
    FileStream FS;
    int Side = 0;

    // Use this for initialization
    void Start()
    {
        FS = new FileStream("NeuralNetwork", FileMode.Open);
        BinaryFormatter formatter = new BinaryFormatter();
        Net = (NeuralNetwork)formatter.Deserialize(FS);
        GM = FindObjectOfType<GameManager>();
        GM.WhiteSelection += WhiteSelected;
        GM.BlackSelection += BlackSelected;
    }

    public void WhiteSelected()
    {
        Side = -1;
    }

    public void BlackSelected()
    {
        Side = 1;
        GM.UserMove = false;
        Invoke("FirstStep", 2.5F);
    }

    public void NextMove()
    {
        GM.UserMove = false;
        Invoke("_NextMove", 1.5F);
    }

    void _NextMove()
    {
        int[] ixy = null;
        switch (GM.Complexity)
        {
            case 0:
                ixy = EasyEvaluating(Side);
                break;
            case 1:
                ixy = MediumEvaluating(Side);
                break;
            case 2:
                ixy = HardEvaluating(Side);
                break;
        }
        GM.Board.MakeStep(ixy[0], ixy[1], ixy[2], true);
        GM.UserMove = true;
        GM.CheckResult();
    }

    void FirstStep()
    {
        var ixy = EasyEvaluating(Side);
        GM.Board.MakeStep(ixy[0], ixy[1], ixy[2], true);
        GM.UserMove = true;
        GM.CheckResult();
    }

    int[] GetBestStep(int side, out double bestValue)
    {
        if (side == 1)
            bestValue = double.MinValue;
        else
            bestValue = double.MaxValue;

        int x = -1, y = -1;
        int bestIndex = -1;
        int i;
        if (side == 1)
            i = 0;
        else
            i = 16;

        for (int n = 0; n < 16; n++, i++)
        {
            var moves = GM.Board.PossibleMoves(i);
            for (int j = 0; j < moves.Count; j++)
            {
                var temp = GM.Board.DeepCopy();

                GM.Board.MakeStep(i, moves[j][0], moves[j][1]);
                var vector = GM.Board.Vector;
                var value = Net.Evaluate(vector);

                GM.Board = temp;

                if (side == 1 && value >= bestValue)
                {
                    x = moves[j][0];
                    y = moves[j][1];
                    bestIndex = i;
                    bestValue = value;
                }
                if (side == -1 && value <= bestValue)
                {
                    x = moves[j][0];
                    y = moves[j][1];
                    bestIndex = i;
                    bestValue = value;
                }
            }
        }

        return new int[] { bestIndex, x, y };
    }

    int[] EasyEvaluating(int side)
    {
        double bestValue;
        if (side == 1)
            bestValue = double.MinValue;
        else
            bestValue = double.MaxValue;

        int bestIndex = -1;
        int bestX = -1;
        int bestY = -1;

        int i;
        if (side == 1)
            i = 0;
        else
            i = 16;

        for (int k = 0; k < 16; k++, i++)
        {
            var moves = GM.Board.PossibleMoves(i);
            for (int j = 0; j < moves.Count; j++)
            {
                double value;
                var temp = GM.Board.DeepCopy();
                GM.Board.MakeStep(i, moves[j][0], moves[j][1]);

                var enemyStep = GetBestStep(-side, out value);

                if (enemyStep[0] == -1)
                {
                    GM.Board = temp;
                    return new int[] { i, moves[j][0], moves[j][1] };
                }

                GM.Board.MakeStep(enemyStep[0], enemyStep[1], enemyStep[2]);

                GetBestStep(side, out value);

                if (side == 1 && value >= bestValue)
                {
                    bestValue = value;
                    bestIndex = i;
                    bestX = moves[j][0];
                    bestY = moves[j][1];
                }
                if (side == -1 && value <= bestValue)
                {
                    bestValue = value;
                    bestIndex = i;
                    bestX = moves[j][0];
                    bestY = moves[j][1];
                }

                GM.Board = temp;
            }

            if (moves.Count > 0 && ((side == 1 && bestValue == double.MinValue) || (side == 1 && bestValue == double.MaxValue)))
            {
                bestIndex = i;
                var c = moves.Count - 1;
                bestX = moves[c][0];
                bestY = moves[c][1];
            }
        }

        return new int[] { bestIndex, bestX, bestY };
    }

    int[] MediumEvaluating(int side)
    {
        double bestValue;
        if (side == 1)
            bestValue = double.MinValue;
        else
            bestValue = double.MaxValue;

        var steps = GetAllSteps(side);
        int bestIndex = steps[0][0];
        int bestX = steps[0][1];
        int bestY = steps[0][2];

        for (int k = 0; k < steps.Count; k++)
        {
            var temp = GM.Board.DeepCopy();
            GM.Board.MakeStep(steps[k][0], steps[k][1], steps[k][2]);

            double value;
            var enemy = GetBestStep(-side, out value);
            if (enemy[0] == -1)
            {
                GM.Board = temp;
                return steps[k];
            }
            GM.Board.MakeStep(enemy[0], enemy[1], enemy[2]);

            var step2 = GetBestStep(side, out value);
            if (step2[0] != -1)
            {
                GM.Board.MakeStep(step2[0], step2[1], step2[2]);
                var enemy2 = GetBestStep(-side, out value);
                if (enemy2[0] == -1)
                {
                    GM.Board = temp;
                    return steps[k];
                }
                GM.Board.MakeStep(enemy2[0], enemy2[1], enemy2[2]);

                GetBestStep(side, out value);
                if (side == 1 && value >= bestValue)
                {
                    bestValue = value;
                    bestIndex = steps[k][0];
                    bestX = steps[k][1];
                    bestY = steps[k][2];
                }
                if (side == -1 && value <= bestValue)
                {
                    bestValue = value;
                    bestIndex = steps[k][0];
                    bestX = steps[k][1];
                    bestY = steps[k][2];
                }
            }
            GM.Board = temp;
        }

        return new int[] { bestIndex, bestX, bestY };
    }

    int[] HardEvaluating(int side)
    {
        double bestValue;
        if (side == 1)
            bestValue = double.MinValue;
        else
            bestValue = double.MaxValue;

        var steps = GetAllSteps(side);
        int bestIndex = steps[0][0];
        int bestX = steps[0][1];
        int bestY = steps[0][2];

        for (int k = 0; k < steps.Count; k++)
        {
            var temp = GM.Board.DeepCopy();
            GM.Board.MakeStep(steps[k][0], steps[k][1], steps[k][2]);

            double value;
            var enemy = GetBestStep(-side, out value);
            if (enemy[0] == -1)
            {
                GM.Board = temp;
                return steps[k];
            }
            GM.Board.MakeStep(enemy[0], enemy[1], enemy[2]);

            var step2 = GetBestStep(side, out value);
            if (step2[0] != -1)
            {
                GM.Board.MakeStep(step2[0], step2[1], step2[2]);
                var enemy2 = GetBestStep(-side, out value);
                if (enemy2[0] == -1)
                {
                    GM.Board = temp;
                    return steps[k];
                }
                GM.Board.MakeStep(enemy2[0], enemy2[1], enemy2[2]);

                var step3 = GetBestStep(side, out value);
                if (step3[0] != -1)
                {
                    GM.Board.MakeStep(step3[0], step3[1], step3[2]);

                    var enemy3 = GetBestStep(-side, out value);
                    if (enemy3[0] == -1)
                    {
                        GM.Board = temp;
                        return steps[k];
                    }
                    GM.Board.MakeStep(enemy3[0], enemy3[1], enemy3[2]);

                    GetBestStep(side, out value);
                    if (side == 1 && value >= bestValue)
                    {
                        bestValue = value;
                        bestIndex = steps[k][0];
                        bestX = steps[k][1];
                        bestY = steps[k][2];
                    }
                    if (side == -1 && value <= bestValue)
                    {
                        bestValue = value;
                        bestIndex = steps[k][0];
                        bestX = steps[k][1];
                        bestY = steps[k][2];
                    }
                }
            }
            GM.Board = temp;
        }

        return new int[] { bestIndex, bestX, bestY };
    }

    List<int[]> GetAllSteps(int side)
    {
        List<int[]> steps = new List<int[]>();

        int i;
        if (side == 1)
            i = 0;
        else
            i = 16;

        for (int n = 0; n < 16; n++, i++)
        {
            var moves = GM.Board.PossibleMoves(i);
            for (int j = 0; j < moves.Count; j++)
                steps.Add(new int[] { i, moves[j][0], moves[j][1] });
        }

        return steps;
    }
}