using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace MyNeuralNetwork
{
    internal class Settings
    {
        public static int SIZE = 100;
        public static int classes = 7;
        public string pathToDataset = @"..\..\dataset";
        private int _border = 20;
        public int border
        {
            get
            {
                return _border;
            }
            set
            {
                if ((value > 0) && (value < height / 3))
                {
                    _border = value;
                    if (top > 2 * _border) top = 2 * _border;
                    if (left > 2 * _border) left = 2 * _border;
                }
            }
        }

        public int width = 640;
        public int height = 640;

        /// <summary>
        /// Размер сетки для сенсоров по горизонтали
        /// </summary>
        public int blocksCount = 10;

        /// <summary>
        /// Желаемый размер изображения до обработки
        /// </summary>
        public Size orignalDesiredSize = new Size(500, 500);
        /// <summary>
        /// Желаемый размер изображения после обработки
        /// </summary>
        public Size processedDesiredSize = new Size(Settings.SIZE, Settings.SIZE);

        public int margin = 10;
        public int top = 40;
        public int left = 40;

        /// <summary>
        /// Второй этап обработки
        /// </summary>
        public bool processImg = false;

        /// <summary>
        /// Порог при отсечении по цвету 
        /// </summary>
        public byte threshold = 120;
        public float differenceLim = 0.15f;

        public void incTop() { if (top < 2 * _border) ++top; }
        public void decTop() { if (top > 0) --top; }
        public void incLeft() { if (left < 2 * _border) ++left; }
        public void decLeft() { if (left > 0) --left; }
    }

    internal class MagicEye
    {
        /// <summary>
        /// Обработанное изображение
        /// </summary>
        public Bitmap processed;
        /// <summary>
        /// Оригинальное изображение после обработки
        /// </summary>
        public Bitmap original;

        /// <summary>
        /// Класс настроек
        /// </summary>
        public Settings settings = new Settings();



        public MagicEye()
        {
        }

        public bool ProcessImage(Bitmap bitmap)
        {
            // На вход поступает необработанное изображение с веб-камеры

            //  Минимальная сторона изображения (обычно это высота)
            if (bitmap.Height > bitmap.Width)
                throw new Exception("К такой забавной камере меня жизнь не готовила!");
            //  Можно было, конечено, и не кидаться эксепшенами в истерике, но идите и купите себе нормальную камеру!
            int side = bitmap.Height;

            //  Отпиливаем границы, но не более половины изображения
            if (side < 4 * settings.border) settings.border = side / 4;
            side -= 2 * settings.border;

            //  Мы сейчас занимаемся тем, что красиво оформляем входной кадр, чтобы вывести его на форму
            Rectangle cropRect = new Rectangle((bitmap.Width - bitmap.Height) / 2 + settings.left + settings.border, settings.top + settings.border, side, side);

            //  Тут создаём новый битмапчик, который будет исходным изображением
            original = new Bitmap(cropRect.Width, cropRect.Height);

            //  Объект для рисования создаём
            Graphics g = Graphics.FromImage(original);

            g.DrawImage(bitmap, new Rectangle(0, 0, original.Width, original.Height), cropRect, GraphicsUnit.Pixel);
            Pen p = new Pen(Color.Red);
            p.Width = 1;

            //  Теперь всю эту муть пилим в обработанное изображение
            AForge.Imaging.Filters.Grayscale grayFilter = new AForge.Imaging.Filters.Grayscale(0.2125, 0.7154, 0.0721);
            var uProcessed = grayFilter.Apply(AForge.Imaging.UnmanagedImage.FromManagedImage(original));

            //  Масштабируем изображение
            AForge.Imaging.Filters.ResizeBilinear scaleFilter = new AForge.Imaging.Filters.ResizeBilinear(
                settings.orignalDesiredSize.Width, settings.orignalDesiredSize.Height);
            original = scaleFilter.Apply(original);
            g = Graphics.FromImage(original);

            AForge.Imaging.Filters.ResizeBilinear scaleFilter2 = new AForge.Imaging.Filters.ResizeBilinear(
                settings.processedDesiredSize.Width, settings.processedDesiredSize.Height);
            uProcessed = scaleFilter2.Apply(uProcessed);
            //  Пороговый фильтр применяем. Величина порога берётся из настроек, и меняется на форме
            AForge.Imaging.Filters.BradleyLocalThresholding threshldFilter = new AForge.Imaging.Filters.BradleyLocalThresholding();
            threshldFilter.PixelBrightnessDifferenceLimit = settings.differenceLim;
            threshldFilter.ApplyInPlace(uProcessed);

            //AForge.Imaging.Filters.Invert InvertFilter = new AForge.Imaging.Filters.Invert();
            //InvertFilter.ApplyInPlace(uProcessed);
            //AForge.Imaging.BlobCounterBase bc = new AForge.Imaging.BlobCounter();

            //bc.FilterBlobs = true;
            //bc.MinWidth = 3;
            //bc.MinHeight = 3;
            //// Упорядочиваем по размеру
            //bc.ObjectsOrder = AForge.Imaging.ObjectsOrder.Size;
            //// Обрабатываем картинку

            //bc.ProcessImage(uProcessed);

            //Rectangle[] rects = bc.GetObjectsRectangles();
            //AForge.Imaging.Blob[] blobs = bc.GetObjectsInformation();
            //var BlobCount = blobs.Length;

            //if (blobs.Length > 0)
            //{
            //    var BiggestBlob = blobs[0];
            //    bc.ExtractBlobsImage(uProcessed, BiggestBlob, false);
            //    uProcessed = BiggestBlob.Image;
            //}
            //else
            //{
            //    return false;
            //}

            //InvertFilter.ApplyInPlace(uProcessed);
            //uProcessed = scaleFilter2.Apply(uProcessed);
            processed = uProcessed.ToManagedImage();

            return true;
        }

        private Bitmap Get(Bitmap bitmap)
        {
            // На вход поступает необработанное изображение с веб-камеры

            //  Минимальная сторона изображения (обычно это высота)
            if (bitmap.Height > bitmap.Width)
                throw new Exception("К такой забавной камере меня жизнь не готовила!");
            //  Можно было, конечено, и не кидаться эксепшенами в истерике, но идите и купите себе нормальную камеру!
            int side = bitmap.Height;

            //  Отпиливаем границы, но не более половины изображения
            if (side < 4 * settings.border) settings.border = side / 4;
            side -= 2 * settings.border;

            //  Мы сейчас занимаемся тем, что красиво оформляем входной кадр, чтобы вывести его на форму
            Rectangle cropRect = new Rectangle((bitmap.Width - bitmap.Height) / 2 + settings.left + settings.border, settings.top + settings.border, side, side);

            //  Тут создаём новый битмапчик, который будет исходным изображением

            //  Теперь всю эту муть пилим в обработанное изображение
            AForge.Imaging.Filters.Grayscale grayFilter = new AForge.Imaging.Filters.Grayscale(0.2125, 0.7154, 0.0721);
            var uProcessed = grayFilter.Apply(AForge.Imaging.UnmanagedImage.FromManagedImage(bitmap));

            //  Масштабируем изображение
            AForge.Imaging.Filters.ResizeBilinear scaleFilter = new AForge.Imaging.Filters.ResizeBilinear(
                settings.orignalDesiredSize.Width, settings.orignalDesiredSize.Height);
            //original = scaleFilter.Apply(original);
            //g = Graphics.FromImage(original);

            AForge.Imaging.Filters.ResizeBilinear scaleFilter2 = new AForge.Imaging.Filters.ResizeBilinear(
                settings.processedDesiredSize.Width, settings.processedDesiredSize.Height);
            uProcessed = scaleFilter2.Apply(uProcessed);
            //  Пороговый фильтр применяем. Величина порога берётся из настроек, и меняется на форме
            AForge.Imaging.Filters.BradleyLocalThresholding threshldFilter = new AForge.Imaging.Filters.BradleyLocalThresholding();
            threshldFilter.PixelBrightnessDifferenceLimit = settings.differenceLim;
            threshldFilter.ApplyInPlace(uProcessed);

            AForge.Imaging.Filters.Invert InvertFilter = new AForge.Imaging.Filters.Invert();
            InvertFilter.ApplyInPlace(uProcessed);
            //AForge.Imaging.BlobCounterBase bc = new AForge.Imaging.BlobCounter();

            //bc.FilterBlobs = true;
            ////bc.MinWidth = 3;
            ////bc.MinHeight = 3;
            //// Упорядочиваем по размеру
            //bc.ObjectsOrder = AForge.Imaging.ObjectsOrder.Size;
            //// Обрабатываем картинку

            //bc.ProcessImage(uProcessed);

            //Rectangle[] rects = bc.GetObjectsRectangles();
            //AForge.Imaging.Blob[] blobs = bc.GetObjectsInformation();
            //var BlobCount = blobs.Length;

            //if (blobs.Length > 0)
            //{
            //    var BiggestBlob = blobs[0];
            //    bc.ExtractBlobsImage(uProcessed, BiggestBlob, false);
            //    uProcessed = BiggestBlob.Image;
            //}
            //else
            //{
            //    return bitmap;
            //}

            InvertFilter.ApplyInPlace(uProcessed);
            uProcessed = scaleFilter2.Apply(uProcessed);
            var processed = uProcessed.ToManagedImage();

            return processed;
        }

        public SamplesSet CreateTestSamplesSet()
        {
            //  Создаём новую обучающую выборку
            SamplesSet samples = new SamplesSet();
            foreach (var directory in Directory.GetDirectories(settings.pathToDataset))
            {
                int type = -1;
                switch (directory.Split('\\').Last())
                {
                    case "back":
                        type = 0;
                        break;
                    case "break":
                        type = 1;
                        break;
                    case "forward":
                        type = 2;
                        break;
                    case "next":
                        type = 3;
                        break;
                    case "pause":
                        type = 4;
                        break;
                    case "play":
                        type = 5;
                        break;
                    case "previous":
                        type = 6;
                        break;
                    default:
                        //MessageBox.Show("Нет такой папки!" + directory.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw new FileNotFoundException(directory.ToString());
                }
                var files = Directory.GetFiles(directory);
                foreach (var file in files.Skip((int)(files.Length * 0.8)))
                {
                    var img = new Bitmap(file);
                    Sample newSample = CreateSample(img, (FigureType)type);
                    samples.AddSample(newSample);
                    Console.WriteLine($"{newSample.actualClass} {file}");
                }
                Console.WriteLine(directory);

            }
            return samples;
        }

        public SamplesSet CreateSamplesSet()
        {
            //  Создаём новую обучающую выборку
            SamplesSet samples = new SamplesSet();
            foreach (var directory in Directory.GetDirectories(settings.pathToDataset))
            {
                int type = -1;
                switch (directory.Split('\\').Last())
                {
                    case "back":
                        type = 0;
                        break;
                    case "break":
                        type = 1;
                        break;
                    case "forward":
                        type = 2;
                        break;
                    case "next":
                        type = 3;
                        break;
                    case "pause":
                        type = 4;
                        break;
                    case "play":
                        type = 5;
                        break;
                    case "previous":
                        type = 6;
                        break;
                    default:
                        //MessageBox.Show("Нет такой папки!" + directory.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw new FileNotFoundException(directory.ToString());
                }
                var files = Directory.GetFiles(directory);
                foreach (var file in files.Take((int)(files.Length * 0.8)))
                {
                    var img = new Bitmap(file);
                    Sample newSample = CreateSample(img, (FigureType)type);
                    samples.AddSample(newSample);
                    Console.WriteLine($"{newSample.actualClass} {file}");
                }
                Console.WriteLine(directory);
                
            }
            return samples;
        }

        public Sample CreateSample(Bitmap img, FigureType actualType = FigureType.Undef)
        {
            var inputs = new double[Settings.SIZE * 2]; 
            var um = AForge.Imaging.UnmanagedImage.FromManagedImage(Get(img));
            var cols = GetBitmapColumn(um);
            var rows = GetBitmapRow(um);
            for (int i = 0; i < Settings.SIZE; i++)
            {
                inputs[i] = cols[i];
                inputs[i + Settings.SIZE] = rows[i];
            }

            return new Sample(inputs, 7, actualType);
        }

        public Sample CreateProcessedSample(FigureType actualType = FigureType.Undef)
        {
            var inputs = new double[Settings.SIZE * 2];
            for (int i = 0; i < Settings.SIZE; i++)
            {
                inputs[i] = CountBlackPixels(GetBitmapColumn(processed, i));
                inputs[i + Settings.SIZE] = CountBlackPixels(GetBitmapRow(processed, i));
            }

            return new Sample(inputs, Settings.classes, actualType);
        }

        public int CountBlackPixels(Color[] pixels) =>
            pixels.Count(p => p.R < 0.1 && p.G < 0.1 && p.B < 0.1);

        public Color[] GetBitmapColumn(Bitmap picture, int ind)
        {
            var result = new Color[picture.Height];
            for (int i = 0; i < picture.Height; i++)
                result[i] = picture.GetPixel(ind, i);
            return result;
        }

        public Color[] GetBitmapRow(Bitmap picture, int ind)
        {
            var result = new Color[picture.Width];
            for (int i = 0; i < picture.Width; i++)
                result[i] = picture.GetPixel(i, ind);
            return result;
        }

        private double[] GetBitmapRow(AForge.Imaging.UnmanagedImage img)
        {
            double[] res = new double[img.Width];

            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    var value = img.GetPixel(x, y).GetBrightness();
                    if (value < 0.1)
                    {
                        res[x]++;
                    }
                }
            }
            return res;
        }

        private double[] GetBitmapColumn(AForge.Imaging.UnmanagedImage img)
        {
            double[] res = new double[img.Width];

            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    var value = img.GetPixel(x, y).GetBrightness();
                    if (value < 0.1)
                    {
                        res[y]++;
                    }
                }
            }
            return res;
        }
    }
}

