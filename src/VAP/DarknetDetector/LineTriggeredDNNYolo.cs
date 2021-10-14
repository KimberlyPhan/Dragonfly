// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using DNNDetector.Model;
using DNNDetector.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using OpenCvSharp;
using Utils.Config;
using Wrapper.Yolo;
using Wrapper.Yolo.Model;


namespace DarknetDetector
{
    public class LineTriggeredDNNDarknet
    {
        static string YOLOCONFIG = DNNConfig.YOLO_TINY_CONFIG; // "cheap" yolo config folder name
        FrameDNNDarknet frameDNNYolo;
        FrameBuffer frameBufferLtDNNYolo;
        Dictionary<string, int> counts_prev = new Dictionary<string, int>();

        public LineTriggeredDNNDarknet(double rFactor)
        {
            frameBufferLtDNNYolo = new FrameBuffer(DNNConfig.FRAME_SEARCH_RANGE);

            frameDNNYolo = new FrameDNNDarknet(YOLOCONFIG, DNNMode.LT, rFactor);
        }

        public LineTriggeredDNNDarknet(List<(string key, (System.Drawing.Point p1, System.Drawing.Point p2) coordinates)> lines)
        {
            frameBufferLtDNNYolo = new FrameBuffer(DNNConfig.FRAME_SEARCH_RANGE);

            frameDNNYolo = new FrameDNNDarknet(YOLOCONFIG, DNNMode.LT, lines);
        }

        public List<Item> Run(Mat frame, int frameIndex, Dictionary<string, int> counts, List<(string key, (System.Drawing.Point p1, System.Drawing.Point p2) coordinates)> lines, HashSet<string> category)
        {
            // buffer frame
            frameBufferLtDNNYolo.Buffer(frame);

            if (counts_prev.Count != 0)
            {
                foreach (string lane in counts.Keys)
                {
                    int diff = Math.Abs(counts[lane] - counts_prev[lane]);
                    if (diff > 0) //object detected by BGS-based counter
                    {
                        Mat[] frameBufferArray = frameBufferLtDNNYolo.ToArray();
                        if (frameIndex >= frameBufferArray.Count())
                        {
                            // call yolo for crosscheck
                            int lineID = Array.IndexOf(counts.Keys.ToArray(), lane);
                            frameDNNYolo.SetTrackingPoint(new System.Drawing.Point((int)((lines[lineID].coordinates.p1.X + lines[lineID].coordinates.p2.X) / 2),
                                                                (int)((lines[lineID].coordinates.p1.Y + lines[lineID].coordinates.p2.Y) / 2))); //only needs to check the last line in each row

                            int frameIndexYolo = frameIndex - 1;
                            DateTime start = DateTime.Now;
                            List<YoloTrackingItem> analyzedTrackingItems = null;

                            List<Item> ltDNNItem = new List<Item>();
                            while (frameIndex - frameIndexYolo < frameBufferArray.Count())
                            {
                                Console.WriteLine($"** Calling DarkNet Cheap on {frameBufferArray.Count() - (frameIndex - frameIndexYolo)}");
                                Mat frameYolo = frameBufferArray[frameBufferArray.Count() - (frameIndex - frameIndexYolo)];
                                byte[] imgByte = Utils.Utils.ImageToByteBmp(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frameYolo));

                                analyzedTrackingItems = frameDNNYolo.Detect(imgByte, category, lineID, System.Drawing.Brushes.Pink, DNNConfig.MIN_SCORE_FOR_LINEBBOX_OVERLAP_LARGE, frameIndexYolo);

                                // object detected by cheap YOLO
                                if (analyzedTrackingItems != null)
                                {
                                    foreach (YoloTrackingItem yoloTrackingItem in analyzedTrackingItems)
                                    {
                                        Item item = Item(yoloTrackingItem);
                                        item.RawImageData = imgByte;
                                        item.TriggerLine = lane;
                                        item.TriggerLineID = lineID;
                                        item.Model = "Cheap";
                                        ltDNNItem.Add(item);

                                        // output cheap YOLO results
                                        string blobName_Cheap = $@"frame-{frameIndex}-Cheap-{yoloTrackingItem.Confidence}.jpg";
                                        string fileName_Cheap = @OutputFolder.OutputFolderLtDNN + blobName_Cheap;
                                        File.WriteAllBytes(fileName_Cheap, yoloTrackingItem.TaggedImageData);
                                        File.WriteAllBytes(@OutputFolder.OutputFolderAll + blobName_Cheap, yoloTrackingItem.TaggedImageData);
                                    }
                                    updateCount(counts);
                                }
                                frameIndexYolo--;
                            }
                            return ltDNNItem;
                        }
                    }
                }
            }
            updateCount(counts);

            return null;
        }

        public List<Item> Run(Mat frame, int frameIndex, List<(string key, (System.Drawing.Point p1, System.Drawing.Point p2) coordinates)> lines, HashSet<string> category)
        {

            List<Item> ltDNNItem = new List<Item>();
            if (lines == null)
            {
                List<YoloTrackingItem> analyzedTrackingItems = null;

                Console.WriteLine($"** Calling DarkNet Cheap on {frameIndex}");
                byte[] imgByte = Utils.Utils.ImageToByteBmp(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame));

                analyzedTrackingItems = frameDNNYolo.Detect(imgByte, category, 0, System.Drawing.Brushes.Pink, DNNConfig.MIN_SCORE_FOR_LINEBBOX_OVERLAP_LARGE, 0);

                // object detected by cheap YOLO
                if (analyzedTrackingItems != null)
                {
                    foreach (YoloTrackingItem yoloTrackingItem in analyzedTrackingItems)
                    {
                        Item item = Item(yoloTrackingItem);
                        item.RawImageData = imgByte;
                        item.TriggerLine = "notSet" ;
                        item.TriggerLineID = 0;
                        item.Model = "Cheap";
                        ltDNNItem.Add(item);

                        // output cheap YOLO results
                        string blobName_Cheap = $@"frame-{frameIndex}-Cheap-{yoloTrackingItem.Confidence}.jpg";
                        string fileName_Cheap = @OutputFolder.OutputFolderLtDNN + blobName_Cheap;
                        File.WriteAllBytes(fileName_Cheap, yoloTrackingItem.TaggedImageData);
                        File.WriteAllBytes(@OutputFolder.OutputFolderAll + blobName_Cheap, yoloTrackingItem.TaggedImageData);
                    }
                }
            }
            else
            {
                for (int lineID = 0; lineID < lines.Count; lineID++)
                {
                    frameDNNYolo.SetTrackingPoint(new System.Drawing.Point((int)((lines[lineID].coordinates.p1.X + lines[lineID].coordinates.p2.X) / 2),
                                                           (int)((lines[lineID].coordinates.p1.Y + lines[lineID].coordinates.p2.Y) / 2)));


                    List<YoloTrackingItem> analyzedTrackingItems = null;

                    Console.WriteLine($"** Calling DarkNet Cheap on {frameIndex}");
                    byte[] imgByte = Utils.Utils.ImageToByteBmp(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame));

                    analyzedTrackingItems = frameDNNYolo.Detect(imgByte, category, lineID, System.Drawing.Brushes.Pink, DNNConfig.MIN_SCORE_FOR_LINEBBOX_OVERLAP_LARGE, 0);

                    // object detected by cheap YOLO
                    if (analyzedTrackingItems != null)
                    {
                        foreach (YoloTrackingItem yoloTrackingItem in analyzedTrackingItems)
                        {
                            Item item = Item(yoloTrackingItem);
                            item.RawImageData = imgByte;
                            item.TriggerLine = lines[lineID].key; ;
                            item.TriggerLineID = lineID;
                            item.Model = "Cheap";
                            ltDNNItem.Add(item);

                            // output cheap YOLO results
                            string blobName_Cheap = $@"frame-{frameIndex}-Cheap-{yoloTrackingItem.Confidence}.jpg";
                            string fileName_Cheap = @OutputFolder.OutputFolderLtDNN + blobName_Cheap;
                            File.WriteAllBytes(fileName_Cheap, yoloTrackingItem.TaggedImageData);
                            File.WriteAllBytes(@OutputFolder.OutputFolderAll + blobName_Cheap, yoloTrackingItem.TaggedImageData);
                        }
                    }
                }
            }
            return ltDNNItem;
        }

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

        void updateCount(Dictionary<string, int> counts)
        {
            foreach (string dir in counts.Keys)
            {
                counts_prev[dir] = counts[dir];
            }
        }
    }
}
