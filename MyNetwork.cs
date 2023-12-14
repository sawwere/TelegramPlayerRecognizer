using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MyNeuralNetwork
{
    public class StudentNetwork : BaseNetwork
    {
        private Layer[] Layers;

        public int Depth { get => Layers.Length; }
        public int InputSize { get => Layers[0].Length; }
        public int Classes { get => Layers[Layers.Length - 1].Length; }

        public double[] Output
        {
            get
            {
                return Layers.Last().Result();
            }
        }
        public StudentNetwork(int[] structure)
        {
            if (structure.Length < 2)
                throw new ArgumentException("Invalid structure");
            Init(structure);
        }

        public void Init(int[] structure)
        {
            Layers = new Layer[structure.Length];

            Layers[0] = new Layer(0.25, structure[0], Layer.Sigmoid, Layer._Sigmoid);
            for (int i = 1; i < Depth; i++)
            {
                Layers[i] = new Layer(0.25, structure[i], Layers[i - 1], Layer.Sigmoid, Layer._Sigmoid);
            }
        }

        private void Forward(Sample sample, bool parallel)
        {
            if (sample.input.Length != InputSize)
                throw new ArgumentException("Invalid input size");


            for (int i = 0; i < sample.input.Length; i++)
            {
                Layers[0][i].Result = sample.input[i];
            }
            for (int i = 1; i < Depth; i++)
            {
                Layers[i].Forward(parallel);
            }
            sample.ProcessPrediction(Output);
        }

        private void Backward(Sample sample, bool parallel = true)
        {
            for (int i = 0; i < Classes; i++)
            {
                Layers[Depth - 1][i].Error = sample.error[i];
            }

            for (int i = Depth - 1; i > 0; i--)
            {
                Layers[i].Backward(parallel);
            }

        }

        public override int Train(Sample sample, double acceptableError, bool parallel)
        {
            int epoch = 0;
            do
            {
                Forward(sample, parallel);
                if (sample.Correct() && sample.EstimatedError() < 0.01)
                {
                    error += sample.EstimatedError();
                    break;
                }

                Backward(sample, parallel);
                epoch++;
            } while (epoch < 50);

            //Console.WriteLine(epoch);
            return epoch;
        }

        private double error = 0;

        public override double TrainOnDataSet(SamplesSet samplesSet, int epochsCount, double acceptableError, bool parallel)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            double truePositive = 0;
            double prev_error = error;
            for (int epoch = 0; epoch < epochsCount; epoch++)
            {

                error = 0;
                truePositive = 0;
                for (int i = 0; i < samplesSet.samples.Count; i++)
                {
                    if (Train(samplesSet.samples[i], acceptableError, parallel) == 0)
                        truePositive++;
                }
                //error = error / samplesSet.samples.Count;
                OnTrainProgress((epoch * 1.0) / epochsCount, error, stopWatch.Elapsed);

                //Console.WriteLine($"{prev_error} - {error} {-prev_error + error}");
                if (error <= acceptableError /*|| (prev_error - error) < -5e-3*/)
                {
                    Console.WriteLine($"Train ended at {epoch} epoch");
                    Console.WriteLine($"Train ended with error {error}");
                    break;
                }
                prev_error = error;
            }

            stopWatch.Stop();
            OnTrainProgress(1, error, stopWatch.Elapsed);

            return error;
        }

        protected override double[] Compute(double[] input)
        {
            if (input.Length != InputSize)
                throw new ArgumentException("Invalid input size");

            for (int i = 0; i < input.Length; i++)
            {
                Layers[0][i].Result = input[i];
            }

            for (int i = 1; i < Depth; i++)
            {
                Layers[i].Forward(false);
            }
            return Output;
        }
    }
}