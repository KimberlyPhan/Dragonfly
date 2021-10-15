// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using AML.Client;
using DarknetDetector;
using DNNDetector.Config;
using DNNDetector.Model;
using OpenCvSharp;
using PostProcessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VideoPipelineCore
{
    class Program
    {
        static void Main(string[] args)
        {
            int? maxItemFound = null;
            Console.Clear();

            //parse arguments
            if (args.Length < 3)
            {
                Console.WriteLine(args.Length);
                Console.WriteLine("Usage: <exe> <video url> <samplingFactor> <resolutionFactor>");
                return;
            }

            string videoUrl = args[0];
            bool isVideoStream;
            if (videoUrl.Substring(0, 4) == "rtmp" || videoUrl.Substring(0, 4) == "http" || videoUrl.Substring(0, 3) == "mms" || videoUrl.Substring(0, 4) == "rtsp")
            {
                isVideoStream = true;
            }
            else
            {
                isVideoStream = false;
                videoUrl = @"..\..\..\..\..\..\media\" + args[0];
            }
            int SAMPLING_FACTOR = int.Parse(args[1]);
            double RESOLUTION_FACTOR = double.Parse(args[2]);

            HashSet<string> categories = null;
            // In case we want to be more precise and only search for certain categories
            // categories = new List<string>() { "person", "bicycle", "car", "motorcycle", 
            //    "truck", "boat", "dog", "horse", "backpack", "umbrella", "handbag", "tie", 
            //    "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", 
            //    "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "" +
            //    "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", 
            //    "carrot", "hot dog", "pizza", "donut", "cake", "chair", "couch", "potted plant", "bed", 
            //    "dining table", "toilet", "tv", "laptop", "mouse", "keyboard", "cell phone", "microwave", 
            //    "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", 
            //    "hair drier", "toothbrush" }.ToHashSet<string>();

            //initialize pipeline settings
            bool loop = false;
            bool displayRawVideo = false;
            Utils.Utils.cleanFolderAll();

            //create pipeline components (initialization based on pplConfig)

            //-----Decoder-----
            Decoder.Decoder decoder = new Decoder.Decoder(videoUrl, loop);

            //-----DNN on every frame (Darknet)-----
            FrameDNNDarknet frameDNNDarknet = new FrameDNNDarknet(DNNConfig.YOLO_CONFIG, Wrapper.Yolo.DNNMode.Frame);
            List<Item> frameDNNDarknetItemList = null;

            //-----Call ML models deployed on Azure Machine Learning Workspace-----
            AMLCaller amlCaller = null;
            List<bool> amlConfirmed;
           // if (new int[] { 6 }.Contains(pplConfig))
           // {
                //amlCaller = new AMLCaller(ConfigurationManager.AppSettings["AMLHost"],
                //Convert.ToBoolean(ConfigurationManager.AppSettings["AMLSSL"]),
                //ConfigurationManager.AppSettings["AMLAuthKey"],
                //ConfigurationManager.AppSettings["AMLServiceID"]);
           // }

            //-----Write to DB-----
            List<Item> ItemList = new List<Item>();

            int frameIndex = 0;
            int videoTotalFrame = 0;
            double videoFramePerSecond = decoder.getVideoFPS();
            if (!isVideoStream)
                videoTotalFrame = decoder.getTotalFrameNum() - 1; //skip the last frame which could be wrongly encoded from vlc capture

            //RUN PIPELINE 
            DateTime startTime = DateTime.Now;
            DateTime prevTime = DateTime.Now;
            TimeSpan elsapsedTime = TimeSpan.Zero;
            List<Item> items = new List<Item>();

            while (true)
            {
                if (!loop)
                {
                    if (!isVideoStream && frameIndex >= videoTotalFrame)
                    {
                        break;
                    }
                }

                //decoder
                Mat frame = decoder.getNextFrame();

                //frame pre-processor
                frame = FramePreProcessor.PreProcessor.returnFrame(frame, frameIndex, SAMPLING_FACTOR, RESOLUTION_FACTOR, displayRawVideo);
                frameIndex++;
                if (frame == null) continue;

                elsapsedTime = videoFramePerSecond > 0 ? TimeSpan.FromMilliseconds(frameIndex * 1000 / videoFramePerSecond) : DateTime.Now - startTime;

                //frameDNN with Darknet Yolo
                frameDNNDarknetItemList = frameDNNDarknet.Run(Utils.Utils.ImageToByteBmp(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame)), frameIndex, categories, System.Drawing.Brushes.Pink);
                ItemList = frameDNNDarknetItemList;

                //Azure Machine Learning
                //amlConfirmed = AMLCaller.Run(frameIndex, ItemList, categories).Result;

                //Fill results with calculated elapsed time
                for (int i = 0; i < ItemList?.Count; i++)
                {
                    ItemList[i].ElapsedTime = elsapsedTime;
                }

                ////DB Write
                Position[] dir = { Position.Unknown, Position.Unknown }; // direction detection is not included
                DataPersistence.PersistResult("test", videoUrl, 0, frameIndex, ItemList, dir, "Cheap", "Heavy", // ArangoDB database
                                                            "dragonfly"); // Azure blob

                //display counts
                if (ItemList != null)
                {
                    Dictionary<string, string> kvpairs = new Dictionary<string, string>();
                    foreach (Item it in ItemList)
                    {
                        if (!kvpairs.ContainsKey(it.TriggerLine))
                            kvpairs.Add(it.TriggerLine, "1");
                        it.ElapsedTime = elsapsedTime;
                        items.Add(it);
                        it.Print();
                    }

                    FramePreProcessor.FrameDisplay.updateKVPairs(kvpairs);
                }

                //print out stats
                double fps = 1000 * (double)(1) / (DateTime.Now - prevTime).TotalMilliseconds;
                double avgFps = 1000 * (long)frameIndex / (DateTime.Now - startTime).TotalMilliseconds;
                Console.WriteLine($"sFactor:{SAMPLING_FACTOR} rFactor:{RESOLUTION_FACTOR} frameID:{frameIndex} FPS:{fps} avgFPS:{avgFps} time:{elsapsedTime}");
                prevTime = DateTime.Now;
                ItemList?.Clear();

                if (items?.Count() >= maxItemFound)
                {
                    break;
                }
            }

            // print out results
            foreach (var item in items)
            {
                item.Print();
            }

            Console.WriteLine($"Done! Took: {(DateTime.Now - startTime)}");
        }
    }
}
