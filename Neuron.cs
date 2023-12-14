using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNeuralNetwork
{
    public class Neuron
    {
        public static Random rand = new Random();

        public double Result { get; set; }
        public double Error { get; set; }

        private double biasWeight = 0.01;

        private double[] weights;
        private Neuron[] prevLayer;

        private void Init()
        {
            Result = 0;
            Error = 0;
            biasWeight = 0.01;
        }

        //Минимальное и максимальное значения для начальной инициализации весов
        private static double initMinWeight = -1;
        private static double initMaxWeight = 1;

        public Neuron(Neuron[] previous)
        {
            Init();
            prevLayer = previous;
            weights = new double[prevLayer.Length];
            for (int i = 0; i < prevLayer.Length; i++)
                weights[i] = initMinWeight + rand.NextDouble() * (initMaxWeight - initMinWeight);
        }

        public Neuron()
        {
            Init();
        }

        public void Activate(Func<double, double> ActivationFunction)
        {
            double sum = 0;
            for (int i = 0; i < prevLayer.Length; i++)
            {
                sum += prevLayer[i].Result * weights[i];
            }
            Result = ActivationFunction(biasWeight + sum);
        }

        public void SetError(Func<double, double> BackPropFunction, double learningRate)
        {
            Error *= BackPropFunction(Result);

            biasWeight -= learningRate * biasWeight * Error;
        }

        public void BackProp(Func<double, double> BackPropFunction, double learningRate)
        {
            SetError(BackPropFunction, learningRate);

            for (int i = 0; i < prevLayer.Length; i++)
                prevLayer[i].Error += Error * weights[i];

            PassErrorToWeights(learningRate, 0, prevLayer.Length);
            Error = 0;
        }

        public void BackPropParallel(double learningRate, int from, int to)
        {
            for (int i = from; i < to; i++)
                prevLayer[i].Error += Error * weights[i];
            PassErrorToWeights(learningRate, from, to);
        }

        private void PassErrorToWeights(double learningRate, int from, int to)
        {
            for (int i = from; i < to; i++)
                weights[i] -= learningRate * prevLayer[i].Result * Error;
        }
    }
}
