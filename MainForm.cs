using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyNeuralNetwork
{
    delegate void FormUpdateDelegate();

    public delegate void FormUpdater(double progress, double error, TimeSpan time);

    public delegate void UpdateTLGMessages(string msg);

    public partial class MainForm : Form
    {
        string pathToDataset = @"..\..\dataset";

        /// <summary>
        /// Чат-бот AIML
        /// </summary>
        AIMLBotik botik = new AIMLBotik();

        TLGBotik tlgBot;

        public BaseNetwork Net
        {
            get
            {
                var selectedItem = (string)netTypeBox.SelectedItem;
                if (!networksCache.ContainsKey(selectedItem))
                    networksCache.Add(selectedItem, CreateNetwork(selectedItem));

                return networksCache[selectedItem];
            }
        }

        private readonly Dictionary<string, Func<int[], BaseNetwork>> networksFabric;
        private Dictionary<string, BaseNetwork> networksCache = new Dictionary<string, BaseNetwork>();

        /// <summary>
        /// Класс, реализующий всю логику работы
        /// </summary>
        private Controller controller = null;

        /// <summary>
        /// Событие для синхронизации таймера
        /// </summary>
        private AutoResetEvent evnt = new AutoResetEvent(false);
                
        /// <summary>
        /// Список устройств для снятия видео (веб-камер)
        /// </summary>
        private FilterInfoCollection videoDevicesList;
        
        /// <summary>
        /// Выбранное устройство для видео
        /// </summary>
        private IVideoSource videoSource;
        
        /// <summary>
        /// Таймер для измерения производительности (времени на обработку кадра)
        /// </summary>
        private Stopwatch sw = new Stopwatch();
        
        /// <summary>
        /// Таймер для обновления объектов интерфейса
        /// </summary>
        System.Threading.Timer updateTmr;

        /// <summary>
        /// Функция обновления формы, тут же происходит анализ текущего этапа, и при необходимости переключение на следующий
        /// Вызывается автоматически - это плохо, надо по делегатам вообще-то
        /// </summary>
        private void UpdateFormFields()
        {
            //  Проверяем, вызвана ли функция из потока главной формы. Если нет - вызов через Invoke
            //  для синхронизации, и выход
            if (StatusLabel.InvokeRequired)
            {
                this.Invoke(new FormUpdateDelegate(UpdateFormFields));
                return;
            }

            sw.Stop();
            ticksLabel.Text = "Тики : " + sw.Elapsed.ToString();
            originalImageBox.Image = controller.GetOriginalImage();
            processedImgBox.Image = controller.GetProcessedImage();

            //if (Net == null) return;
            //FigureType figure = (FigureType)comboBox1.SelectedIndex;
            //var img = AForge.Imaging.UnmanagedImage.FromManagedImage(controller.GetProcessedImage());
            //Sample fig = new Sample(ImageToArray2(img), classes, figure);

            //var pred = Net.Predict(fig);
            //var names = Enum.GetNames(typeof(FigureType));
            //ResLabel.Text = $"Распознано : {fig.recognizedClass}" + Environment.NewLine;
            //for (int i = 0; i < classes; i++)
            //{
            //    ResLabel.Text += names[i] + ": " + fig.output[i].ToString("F4") + Environment.NewLine;
            //}
        }

        public void UpdateTLGInfo(string message)
        {
            if (TLGUsersMessages.InvokeRequired)
            {
                TLGUsersMessages.Invoke(new UpdateTLGMessages(UpdateTLGInfo), new Object[] { message });
                return;
            }
            TLGUsersMessages.Text += message + Environment.NewLine;
        }

        /// <summary>
        /// Обёртка для обновления формы - перерисовки картинок, изменения состояния и прочего
        /// </summary>
        /// <param name="StateInfo"></param>
        public void Tick(object StateInfo)
        {
            UpdateFormFields();
            return;
        }

        public MainForm(Dictionary<string, Func<int[], BaseNetwork>> networksFabric)
        {
            InitializeComponent();
            


            // Список камер получаем
            videoDevicesList = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo videoDevice in videoDevicesList)
            {
                cmbVideoSource.Items.Add(videoDevice.Name);
            }
            if (cmbVideoSource.Items.Count > 0)
            {
                cmbVideoSource.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("А нет у вас камеры!", "Ошибочка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            controller = new Controller(new FormUpdateDelegate(UpdateFormFields));

            this.networksFabric = networksFabric;
            foreach (string name in Enum.GetNames(typeof(FigureType)))
            {
                comboBox1.Items.Add(name);
                comboBox2.Items.Add(name);
            }
            netTypeBox.Items.AddRange(this.networksFabric.Keys.Select(s => (object)s).ToArray());

            

            netTypeBox.SelectedIndex = 0;

            tlgBot = new TLGBotik(Net, new UpdateTLGMessages(UpdateTLGInfo));
            comboBox1.SelectedIndex = comboBox2.SelectedIndex = 0;
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            //  Время засекаем
            sw.Restart();

            //  Отправляем изображение на обработку, и выводим оригинал (с раскраской) и разрезанные изображения
            if(controller.Ready)
                
                #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                controller.ProcessImage((Bitmap)eventArgs.Frame.Clone());
                #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                //  Это выкинуть в отдельный поток!
                //  И отдать делегат? Или просто проверять значение переменной?
                //  Тут хрень какая-то

                //currentState = Stage.Thinking;
                //sage.solveState(processor.currentDeskState, 16, 7);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (videoSource == null)
            {
                var vcd = new VideoCaptureDevice(videoDevicesList[cmbVideoSource.SelectedIndex].MonikerString);
                vcd.VideoResolution = vcd.VideoCapabilities[resolutionsBox.SelectedIndex];
                Debug.WriteLine(vcd.VideoCapabilities[0].FrameSize.ToString());
                Debug.WriteLine(resolutionsBox.SelectedIndex);
                videoSource = vcd;
                videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                videoSource.Start();
                StartButton.Text = "Стоп";
                controlBox.Enabled = true;
                cmbVideoSource.Enabled = false;
            }
            else
            {
                videoSource.SignalToStop();
                if (videoSource != null && videoSource.IsRunning && originalImageBox.Image != null)
                {
                    originalImageBox.Image.Dispose();
                }
                videoSource = null;
                StartButton.Text = "Старт";
                //controlBox.Enabled = false;
                cmbVideoSource.Enabled = true;
            }
        }

        private void tresholdTrackBar_ValueChanged(object sender, EventArgs e)
        {
            controller.settings.threshold = (byte)tresholdTrackBar.Value;
            controller.settings.differenceLim = (float)tresholdTrackBar.Value/tresholdTrackBar.Maximum;
        }

        private void borderTrackBar_ValueChanged(object sender, EventArgs e)
        {
            controller.settings.border = borderTrackBar.Value;
        }

        private void marginTrackBar_ValueChanged(object sender, EventArgs e)
        {
            controller.settings.margin = marginTrackBar.Value;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (updateTmr != null)
                updateTmr.Dispose();

            //  Как-то надо ещё робота подождать, если он работает

            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
            }
        }
        private void cmbVideoSource_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var vcd = new VideoCaptureDevice(videoDevicesList[cmbVideoSource.SelectedIndex].MonikerString);
            resolutionsBox.Items.Clear();
            for (int i = 0; i < vcd.VideoCapabilities.Length; i++)
                resolutionsBox.Items.Add(vcd.VideoCapabilities[i].FrameSize.ToString());
            resolutionsBox.SelectedIndex = 0;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            controller.settings.processImg = checkBox1.Checked;
        }

        public void UpdateLearningInfo(double progress, double error, TimeSpan elapsedTime)
        {
            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new TrainProgressHandler(UpdateLearningInfo), progress, error, elapsedTime);
                return;
            }

            StatusLabel.Text = "Ошибка: " + error;
            int progressPercent = (int)System.Math.Round(progress * 100);
            progressPercent = System.Math.Min(100, System.Math.Max(0, progressPercent));
            elapsedTimeLabel.Text = "Затраченное время : " + elapsedTime.Duration().ToString(@"hh\:mm\:ss\:ff");
            progressBar1.Value = progressPercent;
        }

        private int[] CurrentNetworkStructure()
        {
            return netStructureBox.Text.Split(';').Select(int.Parse).ToArray();
        }

        private BaseNetwork CreateNetwork(string networkName)
        {
            var network = networksFabric[networkName](CurrentNetworkStructure());
            network.TrainProgress += UpdateLearningInfo;
            return network;
        }

        private void recreateNetButton_Click(object sender, EventArgs e)
        {
            //  Проверяем корректность задания структуры сети
            int[] structure = CurrentNetworkStructure();
            if (structure.Length < 2 || structure[0] != Settings.SIZE * 2 ||
                structure[structure.Length - 1] != (int)FigureType.Undef)
            {
                MessageBox.Show(
                    $"В сети должно быть более двух слоёв, первый слой должен содержать XXX нейронов, последний - {(int)FigureType.Undef}",
                    "Ошибка", MessageBoxButtons.OK);
                return;
            }

            // Чистим старые подписки сетей
            foreach (var network in networksCache.Values)
                network.TrainProgress -= UpdateLearningInfo;
            // Пересоздаём все сети с новой структурой
            networksCache = networksCache.ToDictionary(oldNet => oldNet.Key, oldNet => CreateNetwork(oldNet.Key));

            tlgBot.SetNet(Net);
        }

        private void button4_Click(object sender, EventArgs e)
        {
#pragma warning disable CS4014 
            train_networkAsync((int)EpochesCounter.Value,
                (100 - AccuracyCounter.Value) / 100.0, parallelCheckBox.Checked);
#pragma warning restore CS4014
        }

        private async Task<double> train_networkAsync(int epoches, double acceptable_error,
            bool parallel = true)
        {
            //  Выключаем всё ненужное
            label12.Text = "Выполняется обучение...";
            label12.ForeColor = Color.Red;
            originalImageBox.Enabled = false;
            trainOneButton.Enabled = false;
            try
            {
                //  Обучение запускаем асинхронно, чтобы не блокировать форму
                var curNet = Net;
                double f = await Task.Run(() => curNet.TrainOnDataSet(controller.processor.CreateSamplesSet(), epoches, acceptable_error, parallel));

                originalImageBox.Enabled = true;
                trainOneButton.Enabled = true;
                StatusLabel.Text = "Ошибка: " + f;
                StatusLabel.ForeColor = Color.Green;
                return f;
            }
            catch (Exception e)
            {
                label12.Text = $"Исключение: {e.Message}";
            }

            return 0;
        }

        private void trainOneButton_Click(object sender, EventArgs e)
        {
            if (Net == null) return;
            FigureType figure = (FigureType)comboBox1.SelectedIndex;
            var img = AForge.Imaging.UnmanagedImage.FromManagedImage(controller.GetProcessedImage());
            Sample fig = new Sample(ImageToArray2(img), Settings.classes, figure);
            Net.Train(fig, 0.5, parallelCheckBox.Checked);
            set_result(fig);
        }

        private void set_result(Sample figure)
        {
            label12.ForeColor = figure.Correct() ? Color.Green : Color.Red;

            label12.Text = "Распознано : " + figure.recognizedClass;

            var names = Enum.GetNames(typeof(FigureType));
            ResLabel.Text = $"Распознано : {figure.recognizedClass}" + Environment.NewLine;
            for (int i = 0; i < Settings.classes; i++)
            {
                ResLabel.Text += names[i] + ": " + figure.output[i].ToString("F4") + Environment.NewLine;
            }
        }


        private double[] ImageToArray2(AForge.Imaging.UnmanagedImage img)
        {
            double[] res = new double[img.Width];
            
            for (int x = 0; x < img.Width; x++)
            {
                int first = -1;
                int last = -1;
                for (int y = 0; y < img.Height; y++)
                {
                    var value = img.GetPixel(x, y).GetBrightness();
                    if (value < 0.001)
                    {
                        if (first < 0)
                        {
                            first = last = y;
                        }
                        else if (y > last)
                        {
                            last = y;
                        }
                    }
                }
                res[x] = first - last;
            }
            return res;
        }

        private double[] ImageToArra(AForge.Imaging.UnmanagedImage img)
        {
            double[] res = new double[img.Width * img.Height];
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    res[i * img.Width + j] = img.GetPixel(j, i).GetBrightness();
                }
            }
            return res;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Enabled = false;

            double accuracy = controller.processor.CreateTestSamplesSet().TestNeuralNetwork(Net);

            StatusLabel.Text = $"Точность на тестовой выборке : {accuracy * 100,5:F2}%";
            StatusLabel.ForeColor = accuracy * 100 >= AccuracyCounter.Value ? Color.Green : Color.Red;

            Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Net == null) return;
            FigureType figure = (FigureType)comboBox1.SelectedIndex;
            
            var img = AForge.Imaging.UnmanagedImage.FromManagedImage(controller.GetProcessedImage());
            Sample fig = controller.processor.CreateProcessedSample(); // new Sample(ImageToArray2(img), classes, figure);

            var pred = Net.Predict(fig);
            var names = Enum.GetNames(typeof(FigureType));
            ResLabel.Text = $"Распознано : {fig.recognizedClass}" + Environment.NewLine;
            for (int i = 0; i < Settings.classes; i++)
            {
                ResLabel.Text += names[i] +": " + fig.output[i].ToString("F4") + Environment.NewLine;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var phrase = AIMLInput.Text;
            if (phrase.Length > 0)
                AIMLOutput.Text += botik.Talk(0, "default", phrase) + Environment.NewLine;
        }

        private void TLGBotOnButton_Click(object sender, EventArgs e)
        {
            tlgBot.SetNet(Net);
            tlgBot.Act();
            TLGBotOnButton.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog1.Filter = "Jpg files (*.jpg)|*.jpg|All files (*.*)|*.*";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var filename = openFileDialog1.FileName;
                        if (Net == null) return;
                        FigureType figure = (FigureType)comboBox1.SelectedIndex;
                        var bm = new Bitmap(Image.FromFile(filename));
                        var img = AForge.Imaging.UnmanagedImage.FromManagedImage(bm);
                        //controller.processor.ProcessImage(new Bitmap(Image.FromFile(filename)));

                        Sample fig = controller.processor.CreateSample(bm);

                        var pred = Net.Predict(fig);
                        var names = Enum.GetNames(typeof(FigureType));
                        ResLabel.Text = $"Распознано : {fig.recognizedClass}" + Environment.NewLine;
                        for (int i = 0; i < Settings.classes; i++)
                        {
                            ResLabel.Text += names[i] + ": " + fig.output[i].ToString("F4") + Environment.NewLine;
                        }
                    }
                    catch
                    {
                        DialogResult result = MessageBox.Show("Could not open file",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
