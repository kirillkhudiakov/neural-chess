using System;

namespace NNLibrary
{
    /// <summary>
    /// Многослойный персептрон
    /// </summary>
    [Serializable]
    public class NeuralNetwork
    {
        static Random Rnd = new Random();
        // Разменрность входного вектора
        public int DimInput { get; set; }
        // Количество скрытых слоев
        public int Layers { set; get; }
        // Количество нейронов в каждом слое
        public int Neurons { set; get; }
        // Весовые коэфициенты
        double[][,] Weight;

        public NeuralNetwork(int DimInput, int Layers, int Neurons)
        {
            this.DimInput = DimInput;
            this.Layers = Layers;
            this.Neurons = Neurons;
            Weight = new double[Layers][,];
            for (int i = 0; i < Layers; i++)
            {
                // Устанавливаем количество синаптических связей в каждом слое
                if (i == 0 && i + 1 == Layers)
                    Weight[i] = new double[DimInput + 1, 1];
                else if (i == 0)
                    Weight[i] = new double[DimInput + 1, Neurons];
                else if (i + 1 == Layers)
                    Weight[i] = new double[Neurons + 1, 1];
                else
                    Weight[i] = new double[Neurons + 1, Neurons];
                // Каждому синапсу присваиваем случайный вес в полуинтервале [-1, 1)
                for (int j = 0; j < Weight[i].GetLength(0); j++)
                    for (int k = 0; k < Weight[i].GetLength(1); k++)
                        Weight[i][j, k] = 2 * Rnd.NextDouble() - 1;
            }
        }

        // Функция активации
        double Activ(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        // Расчитать выход нейронной сети по заданному входному вектору
        public double Evaluate(double[] input)
        {
            // Массив выхода на каждом слое
            double[][] output = new double[Layers + 1][];
            output[0] = input;
            for (int i = 0; i < Layers; i++)
            {
                if (i + 1 == Layers)
                    output[i + 1] = new double[1];
                else
                    output[i + 1] = new double[Neurons];
                // Нам известен все выходные сигналы i-го слоя. Высчитаем выходы i + 1 слоя
                for (int j = 0; j < output[i + 1].Length; j++)
                {
                    // Просуммируем выходы с предыдущего слоя, умноженные на соответсвующие весовые коэффициенты
                    double v = Weight[i][0, j];
                    for (int k = 0; k < output[i].Length; k++)
                    {
                        v += output[i][k] * Weight[i][k + 1, j];
                    }
                    // На последнем слое выход считается как линейный сумматор без функции активации
                    if (i + 1 == Layers)
                        output[i + 1][j] = v;
                    else
                        output[i + 1][j] = Activ(v);
                }
            }
            return output[Layers][0];
        }

        // Произвести обучение на массиве векторов (input) с соответсвующем массивом желаемых откликов (response),
        // Количество итераций - epochs
        public void Train(double[] input, double response, double coef)
        {
            // Высчитаем выходы всех нейронов
            double[][] output = new double[Layers + 1][];
            output[0] = input;
            for (int i = 0; i < Layers; i++)
            {
                if (i + 1 == Layers)
                    output[i + 1] = new double[1];
                else
                    output[i + 1] = new double[Neurons];
                for (int j = 0; j < output[i + 1].Length; j++)
                {
                    double v = Weight[i][0, j];
                    for (int k = 0; k < output[i].Length; k++)
                    {
                        v += output[i][k] * Weight[i][k + 1, j];
                    }
                    if (i + 1 == Layers)
                        output[i + 1][j] = v;
                    else
                        output[i + 1][j] = Activ(v);
                }
            }

            // Массив новых весовых коэффициентов
            double[][,] W1 = new double[Layers][,];
            // Массив градиентов для каждого нейрона
            double[][] Grad = new double[Layers][];
            for (int i = Layers - 1; i >= 0; i--)
            {
                W1[i] = new double[Weight[i].GetLength(0), Weight[i].GetLength(1)];
                Grad[i] = new double[output[i + 1].Length];
                for (int j = 0; j < output[i + 1].Length; j++)
                {
                    // Для последнего слоя градиент считается как разница желаемого и фактического отклика
                    if (i == Layers - 1)
                    {
                        Grad[i][j] = response - output[Layers][j];
                    }
                    // Для остальных слоев - более сложно
                    else
                    {
                        double sum = 0;
                        for (int k = 0; k < output[i + 2].Length; k++)
                        {
                            sum += Grad[i + 1][k] * Weight[i + 1][j + 1, k];
                        }
                        Grad[i][j] = sum * output[i + 1][j] * (1 - output[i + 1][j]);
                    }
                    // Используя полученные градиенты высчитаем новые значения весовых коэффициентов в данном слое
                    W1[i][0, j] = Weight[i][0, j] + coef * Grad[i][j];
                    for (int m = 0; m < output[i].Length; m++)
                    {
                        W1[i][m + 1, j] = Weight[i][m + 1, j] + coef * Grad[i][j] * output[i][m];
                    }
                }
            }
            // Сохраним новые весовые коэффициенты
            Weight = (double[][,])W1.Clone();
        }
    }
}