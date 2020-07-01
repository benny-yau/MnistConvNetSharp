using System;
using System.Linq;
using ConvNetSharp.Core;
using ConvNetSharp.Core.Layers.Double;
using ConvNetSharp.Core.Training;
using ConvNetSharp.Volume;
using ConvNetSharp.Core.Serialization;
using System.IO;
using System.Diagnostics;
using Volume = ConvNetSharp.Volume.Volume<double>;
using System.Drawing;

namespace MnistDemo
{
    class Program
    {
        private readonly CircularBuffer<double> _testAccWindow = new CircularBuffer<double>(100);
        private readonly CircularBuffer<double> _trainAccWindow = new CircularBuffer<double>(100);
        private Net<double> _net;
        private int _stepCount;
        private SgdTrainer<double> _trainer;

        private static void Main()
        {
            var program = new Program();

            //Load previous model
            //var jsonText = File.ReadAllText(@"ConvNetModel.json");
            //program._net = SerializationExtensions.FromJson<double>(jsonText);

            //Run neural network training
            program.Train();

            //Test all samples
            //program.TestSamples();
        }

        private void Train(int epochs = 1)
        {
            var datasets = new DataSets();
            if (!datasets.Load())
                return;

            if (this._net == null)
            {
                this._net = new Net<double>();
                this._net.AddLayer(new InputLayer(28, 28, 1));
                this._net.AddLayer(new LeakyReluLayer(0.05));
                this._net.AddLayer(new FullyConnLayer(200));
                this._net.AddLayer(new LeakyReluLayer(0.05));
                this._net.AddLayer(new FullyConnLayer(10));
                this._net.AddLayer(new SoftmaxLayer(10));
            }
            this._trainer = new SgdTrainer<double>(this._net)
            {
                LearningRate = 0.01, //start with higher learning rate and decrease gradually
                BatchSize = 20,
            };

            int iterations = datasets.Train._trainImages.Count / this._trainer.BatchSize * epochs;

            for (int iteration = 1; iteration < iterations; iteration++)
            {
                var trainSample = datasets.Train.NextBatch(this._trainer.BatchSize);
                Train(trainSample.Item1, trainSample.Item2, trainSample.Item3);

                var testSample = datasets.Test.NextBatch(this._trainer.BatchSize);
                Test(testSample.Item1, testSample.Item3, this._testAccWindow);

                Console.WriteLine("Loss: {0} Train accuracy: {1}% Test accuracy: {2}%", this._trainer.Loss.ToString("0.00"),
                    Math.Round(this._trainAccWindow.Items.Average() * 100.0, 2),
                    Math.Round(this._testAccWindow.Items.Average() * 100.0, 2));

                Console.WriteLine("Iteration: {0} StepCount: {1} Fwd: {2}ms Bckw: {3}ms", iteration, this._stepCount,
                    Math.Round(this._trainer.ForwardTimeMs, 2),
                    Math.Round(this._trainer.BackwardTimeMs, 2));
            }

            // Serialize to json and save to file
            var fileName = @"ConvNetModel_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
            File.WriteAllText(fileName, this._net.ToJson());
        }

        public void TestSamples()
        {
            var datasets = new DataSets();
            if (!datasets.Load())
                return;

            double match = 0d;
            int batchSize = 1;
            DataSet dataSet = datasets.Test;
            for (int x = 0; x <= dataSet._trainImages.Count / batchSize - 1; x++)
            {
                int batchStart = x * batchSize;
                Tuple<Volume, Volume, int[]> testSample = dataSet.NextBatch(batchSize);
                Volume<double> batch = testSample.Item1;
                int[] labels = testSample.Item3;
                this._net.Forward(batch);
                var prediction = this._net.GetPrediction();

                for (var i = 0; i < labels.Length; i++)
                {
                    if (labels[i] == prediction[i])
                        match++;

                    int count = (i + 1);
                    if (count % batchSize == 0)
                        Debug.WriteLine("Testing " + (count + batchStart) + " Matches: " + match + " Accuracy: " + match / (count + batchStart));
                }
            }
            Debug.WriteLine("FINAL - Matches: " + match + " Accuracy: " + match / dataSet._trainImages.Count);
        }

        private void Test(Volume x, int[] labels, CircularBuffer<double> accuracy, bool forward = true)
        {
            if (forward)
                this._net.Forward(x);

            var prediction = this._net.GetPrediction();

            for (var i = 0; i < labels.Length; i++)
            {
                accuracy.Add(labels[i] == prediction[i] ? 1.0 : 0.0);
            }
        }

        private void Train(Volume x, Volume y, int[] labels)
        {
            this._trainer.Train(x, y);
            Test(x, labels, this._trainAccWindow, false);
            this._stepCount += labels.Length;
        }

        public static void ShowImageVisualizer(Image img, Image original = null)
        {
            ImageVisualizer frm = new ImageVisualizer(img, original);
            frm.ShowDialog();
        }

    }
}
