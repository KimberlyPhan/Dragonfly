// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using DNNDetector.Config;
using DNNDetector.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Utils.Config;
using Wrapper.Yolo;
using Wrapper.Yolo.Model;

namespace DarknetDetector
{
    public class FrameDNNDarknet
    {
        YoloWrapper frameYolo;
        YoloTracking frameYoloTracking;
        List<(string key, (Point p1, Point p2) coordinates)> _lines;

        //distance-based validation
        public FrameDNNDarknet(string modelConfig, DNNMode dnnMode, double rFactor)
        {
            var configurationDetector = new ConfigurationDetector(modelConfig);
            frameYolo = new YoloWrapper(configurationDetector.Detect(), dnnMode);
            frameYoloTracking = new YoloTracking(frameYolo, Convert.ToInt32(DNNConfig.ValidRange * rFactor));
        }

        //overlap ratio-based validation
        public FrameDNNDarknet(string modelConfig, DNNMode dnnMode, List<(string key, (Point p1, Point p2) coordinates)> lines)
        {
            var configurationDetector = new ConfigurationDetector(modelConfig);
            frameYolo = new YoloWrapper(configurationDetector.Detect(), dnnMode);
            frameYoloTracking = new YoloTracking(frameYolo);
            _lines = lines;
        }

        //without validation i.e., output all yolo detections
        public FrameDNNDarknet(string modelConfig, DNNMode dnnMode)
        {
            var configurationDetector = new ConfigurationDetector(modelConfig);
            frameYolo = new YoloWrapper(configurationDetector.Detect(), dnnMode);
            frameYoloTracking = new YoloTracking(frameYolo);
        }

        public void SetTrackingPoint(Point trackingObject)
        {
            frameYoloTracking.SetTrackingObject(trackingObject);
        }

        public List<YoloTrackingItem> Detect(byte[] imgByte, HashSet<string> category, int lineID, Brush bboxColor, double min_score_for_linebbox_overlap, int frameIndex = 0)
        {
            IEnumerable<YoloTrackingItem> yoloItems = frameYoloTracking.Analyse(imgByte, category, bboxColor);
            return OverlapVal(imgByte, yoloItems, lineID, bboxColor, min_score_for_linebbox_overlap);
        }

        List<YoloTrackingItem> OverlapVal(byte[] imgByte, IEnumerable<YoloTrackingItem> yoloItems, int lineID, Brush bboxColor, double min_score_for_linebbox_overlap)
        {
            var image = Image.FromStream(new MemoryStream(imgByte)); // to filter out bbox larger than the frame

            if (yoloItems == null) return null;

            //run overlap ratio-based validation
            if (_lines != null)
            {
                List<YoloTrackingItem> validObjects = new List<YoloTrackingItem>();
                
                var overlapItems = yoloItems.Select(o => new { Overlap = Utils.Utils.checkLineBboxOverlapRatio(_lines[lineID].coordinates, o.X, o.Y, o.Width, o.Height), Bbox_x = o.X + o.Width, Bbox_y = o.Y + o.Height, Distance = this.Distance(_lines[lineID].coordinates, o.Center()), Item = o })
                    .Where(o => o.Bbox_x <= image.Width && o.Bbox_y <= image.Height && o.Overlap >= min_score_for_linebbox_overlap).OrderBy(o => o.Distance);
                foreach (var item in overlapItems)
                {
                    var taggedImageData = Utils.Utils.DrawImage(imgByte, item.Item.X, item.Item.Y, item.Item.Width, item.Item.Height, bboxColor);
                    var croppedImageData = Utils.Utils.CropImage(imgByte, item.Item.X, item.Item.Y, item.Item.Width, item.Item.Height);
                    validObjects.Add(new YoloTrackingItem(item.Item, frameYoloTracking._index, taggedImageData, croppedImageData));
                    frameYoloTracking._index++;
                }
                return (validObjects.Count == 0 ? null : validObjects);
            }
            else
            {
                return yoloItems.ToList();
            }
        }

        public List<Item> Run(byte[] imgByte, int frameIndex, HashSet<string> category, Brush bboxColor)
        {
            List<Item> frameDNNItem = new List<Item>();

            IEnumerable<YoloTrackingItem> yoloItems = frameYoloTracking.Analyse(imgByte, category, bboxColor);

            if (_lines == null)
            {
                List<YoloTrackingItem> analyzedTrackingItems = OverlapVal(imgByte, yoloItems, 0, bboxColor, DNNConfig.MIN_SCORE_FOR_LINEBBOX_OVERLAP_LARGE);
                if (analyzedTrackingItems == null) return null;

                foreach (YoloTrackingItem yoloTrackingItem in analyzedTrackingItems)
                {
                    Item it = Item(yoloTrackingItem);
                    it.RawImageData = imgByte;
                    it.TriggerLineID = 0;
                    it.TriggerLine = "notSet";
                    it.FrameIndex = frameIndex;
                    frameDNNItem.Add(it);
                    it.Model = "FramedYolo";

                    //--------------output frameDNN results - one bbox per image--------------
                    string blobName_FrameDNN = $@"frame-{frameIndex}-{yoloTrackingItem.Type}-{yoloTrackingItem.Confidence}.jpg";
                    string fileName_FrameDNN = @OutputFolder.OutputFolderFrameDNNDarknet + blobName_FrameDNN;
                    //File.WriteAllBytes(fileName_FrameDNN, yoloTrackingItem.TaggedImageData);
                    //--------------output frameDNN results - one bbox per image--------------
                }
                DrawAllBb(frameIndex, imgByte, analyzedTrackingItems, bboxColor);
            }
            else
            {
                for (int lineID = 0; lineID < _lines.Count; lineID++)
                {
                    frameYoloTracking.SetTrackingObject(new Point((int)((_lines[lineID].coordinates.p1.X + _lines[lineID].coordinates.p2.X) / 2),
                                                                    (int)((_lines[lineID].coordinates.p1.Y + _lines[lineID].coordinates.p2.Y) / 2)));
                    List<YoloTrackingItem> analyzedTrackingItems = OverlapVal(imgByte, yoloItems, lineID, bboxColor, DNNConfig.MIN_SCORE_FOR_LINEBBOX_OVERLAP_LARGE);
                    if (analyzedTrackingItems == null) continue;

                    foreach (YoloTrackingItem yoloTrackingItem in analyzedTrackingItems)
                    {
                        Item it = Item(yoloTrackingItem);
                        it.RawImageData = imgByte;
                        it.TriggerLineID = lineID;
                        it.TriggerLine = _lines[lineID].key;
                        it.FrameIndex = frameIndex;
                        frameDNNItem.Add(it);
                        it.Model = "FramedYolo";

                        //--------------output frameDNN results - one bbox per image--------------
                        string blobName_FrameDNN = $@"frame-{frameIndex}-{yoloTrackingItem.Type}-{yoloTrackingItem.Confidence}.jpg";
                        string fileName_FrameDNN = @OutputFolder.OutputFolderFrameDNNDarknet + blobName_FrameDNN;
                        File.WriteAllBytes(fileName_FrameDNN, yoloTrackingItem.TaggedImageData);
                        //--------------output frameDNN results - one bbox per image--------------
                    }
                    DrawAllBb(frameIndex, imgByte, analyzedTrackingItems, bboxColor);
                }
            }
            return frameDNNItem;
        }

        private double Distance((Point p1, Point p2) line, Point bboxCenter)
        {
            Point p1 = new Point((int)((line.p1.X + line.p2.X)/2), (int)((line.p1.Y+line.p2.Y)/2));
            return Math.Sqrt(this.Pow2(bboxCenter.X - p1.X) + Pow2(bboxCenter.Y - p1.Y));
        }

        private double Pow2(double x)
        {
            return x * x;
        }


        private void DrawAllBb(int frameIndex, byte[] imgByte, List<YoloTrackingItem> items, Brush bboxColor)
        {
            if (items == null || items.Count == 0) return;
            byte[] canvas = new byte[imgByte.Length];
            canvas = imgByte;
            foreach (var item in items)
            {
                canvas = Utils.Utils.DrawImage(canvas, item.X, item.Y, item.Width, item.Height, bboxColor, item.Type);
            }

            // saving bitmap as jpg because it's soooooooo much smaller and human eyes can't tell the difference
            string path = @OutputFolder.OutputFolderAll + $"frame-{frameIndex}";
            File.WriteAllBytes(path + ".bmp", canvas);
            //Image image = Image.FromFile(path + ".bmp");
            //image.Save(path + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            var image = ResizeImage(Image.FromFile(path + ".bmp"));
            Bitmap bm = (Bitmap)image;
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo ici = null;
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == "image/jpeg")
                    ici = codec;
            }
            EncoderParameters ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)80);
            bm.Save(path + ".jpg", ici, ep);
            bm.Dispose();
            image.Dispose();
            File.Delete(path + ".bmp");
        }

        public static Image ResizeImage(Image imgToResize)
        {
            int desireWidth = 1600;
            int calculatedHeight = imgToResize.Height * desireWidth / imgToResize.Width;
            if (imgToResize.Width > desireWidth)
            {
                var x = (Image)(new Bitmap(imgToResize, new Size(desireWidth, calculatedHeight)));
                imgToResize.Dispose();
                return x;
            }
            else
            {
                return imgToResize;
            }
        }


        //public byte[] RoundTripImage(byte[] bytes)
        //{
        //    using (var ms = new MemoryStream(bytes))
        //    {
        //        var image =  Image.FromStream(ms);
        //        image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
        //        return ms.ToArray();
        //    }
        //}

        Item Item(YoloTrackingItem yoloTrackingItem)
        {
            Item item = new Item(yoloTrackingItem.X, yoloTrackingItem.Y, yoloTrackingItem.Width, yoloTrackingItem.Height,
                yoloTrackingItem.ObjId, yoloTrackingItem.Type, yoloTrackingItem.Confidence, 0, "");

            item.TrackId = yoloTrackingItem.TrackId;
            item.Index = yoloTrackingItem.Index;
            item.TaggedImageData = yoloTrackingItem.TaggedImageData;
            item.CroppedImageData = yoloTrackingItem.CroppedImageData;

            return item;
        }
    }
}
