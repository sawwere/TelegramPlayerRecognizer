using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNeuralNetwork
{
    public class Layer
    {
        public static double Sigmoid(double x) => 1 / (1 + Math.Exp(-x));
        public static double _Sigmoid(double x) => x * (1 - x);

        public double learningRate;
        public Neuron[] values;
        public int Length { get => values.Length; }

        public Func<double, double> activationFunction { get; private set; }
        public Func<double, double> backPropFunction { get; private set; }
        private Layer prevLayer;

        public Layer(double lr, int length, Func<double, double> activate, Func<double, double> backprop)
        {
            prevLayer = null;
            activationFunction = activate;
            backPropFunction = backprop;
            learningRate = lr;
            values = new Neuron[length];
            for (int j = 0; j < length; j++)
                values[j] = new Neuron();
        }

        public Layer(double lr, int length, Layer prev, Func<double, double> activate, Func<double, double> backprop)
        {
            prevLayer = prev;
            activationFunction = activate;
            backPropFunction = backprop;
            learningRate = lr;
            values = new Neuron[length];
            for (int j = 0; j < length; j++)
            {
                values[j] = new Neuron(prev.values);
            }
        }

        public Neuron this[int i]
        {
            get { return values[i]; }
            private set
            {
                //if (value is null)
                //    throw new ArgumentNullException();
                values[i] = value;
            }
        }

        public void Forward(bool parallel)
        {
            if (parallel)
            {
                Parallel.For(0, values.Length, i =>
                {
                    values[i].Activate(activationFunction);
                });
            }
            else
            {
                foreach (Neuron n in values)
                {
                    n.Activate(activationFunction);
                }
            }

        }

        public void ResetError()
        {
            foreach (Neuron n in values)
                n.Error = 0;
        }

        public static int THREADS = Environment.ProcessorCount;

        public void Backward(bool parallel)
        {
            if (parallel)
            {
                int perThread = prevLayer.Length / THREADS;
                prevLayer.ResetError();

                for (int j = 0; j < values.Length; j++)
                {
                    values[j].SetError(backPropFunction, learningRate);
                }

                Parallel.For(0, THREADS, i =>
                {
                    for (int j = 0; j < values.Length; j++)
                        values[j].BackPropParallel(learningRate, perThread * i, i == THREADS ? prevLayer.Length : perThread * (i + 1));
                });
            }
            else
            {
                for (int j = 0; j < values.Length; j++)
                    values[j].BackProp(backPropFunction, learningRate);
            }

        }

        public double[] Result()
        {
            return values.Select(x => x.Result).ToArray();
        }
    }
}
