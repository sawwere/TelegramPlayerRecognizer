using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace MyNeuralNetwork
{
    internal class Settings
    {
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
        public Size processedDesiredSize = new Size(500, 500);

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

            AForge.Imaging.Filters.Invert InvertFilter = new AForge.Imaging.Filters.Invert();
            InvertFilter.ApplyInPlace(uProcessed);
            AForge.Imaging.BlobCounterBase bc = new AForge.Imaging.BlobCounter();

            bc.FilterBlobs = true;
            bc.MinWidth = 3;
            bc.MinHeight = 3;
            // Упорядочиваем по размеру
            bc.ObjectsOrder = AForge.Imaging.ObjectsOrder.Size;
            // Обрабатываем картинку

            bc.ProcessImage(uProcessed);

            Rectangle[] rects = bc.GetObjectsRectangles();
            AForge.Imaging.Blob[] blobs = bc.GetObjectsInformation();
            var BlobCount = blobs.Length;

            if (blobs.Length > 0)
            {
                Console.WriteLine(BlobCount);
                var BiggestBlob = blobs[0];
                bc.ExtractBlobsImage(uProcessed, BiggestBlob, false);
                uProcessed = BiggestBlob.Image;
            }
            else
            {
                return false;
            }

            InvertFilter.ApplyInPlace(uProcessed);
            uProcessed = scaleFilter2.Apply(uProcessed);
            processed = uProcessed.ToManagedImage();

            return true;
        }

        /// <summary>
        /// Обработка одного сэмпла
        /// </summary>
        /// <param name="index"></param>
        private string processSample(ref AForge.Imaging.UnmanagedImage unmanaged)
        {
            string rez = "Обработка";

            ///  Инвертируем изображение
            AForge.Imaging.Filters.Invert InvertFilter = new AForge.Imaging.Filters.Invert();
            InvertFilter.ApplyInPlace(unmanaged);

            ///    Создаём BlobCounter, выдёргиваем самый большой кусок, масштабируем, пересечение и сохраняем
            ///    изображение в эксклюзивном использовании
            AForge.Imaging.BlobCounterBase bc = new AForge.Imaging.BlobCounter();

            bc.FilterBlobs = true;
            bc.MinWidth = 3;
            bc.MinHeight = 3;
            // Упорядочиваем по размеру
            bc.ObjectsOrder = AForge.Imaging.ObjectsOrder.Size;
            // Обрабатываем картинку

            bc.ProcessImage(unmanaged);

            Rectangle[] rects = bc.GetObjectsRectangles();
            AForge.Imaging.Blob[] blobs = bc.GetObjectsInformation();
            rez = "Насчитали " + rects.Length.ToString() + " прямоугольников!";
            var BlobCount = blobs.Length;

            if (blobs.Length > 0)
            {
                var BiggestBlob = blobs[0];
                bc.ExtractBlobsImage(unmanaged, BiggestBlob, false);
                unmanaged = BiggestBlob.Image;
                AForge.Point mc = BiggestBlob.CenterOfGravity;
                AForge.Point ic = new AForge.Point((float)BiggestBlob.Image.Width / 2, (float)BiggestBlob.Image.Height / 2);
                //AngleRad = (ic.Y - mc.Y) / (ic.X - mc.X);
                //Angle = (float)(Math.Atan(AngleRad) * 180 / Math.PI);
            }
            else
            {
                // TODO make arrengaments for No blobs case
                //Recongnised = false;
                //Angle = 0;
                //AngleRad = -1;
            }

            return rez;
        }

    }
}

