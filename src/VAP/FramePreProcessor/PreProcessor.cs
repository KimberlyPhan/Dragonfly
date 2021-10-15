// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;

using OpenCvSharp;

namespace FramePreProcessor
{
    public class PreProcessor
    {
        public static Mat returnFrame(Mat sourceMat, int frameIndex, int SAMPLING_FACTOR, double RESOLUTION_FACTOR, bool display)
        {
            Mat resizedFrame = null;
            if (frameIndex % SAMPLING_FACTOR != 0) return resizedFrame;

            try
            {
                resizedFrame = sourceMat.Resize(new OpenCvSharp.Size((int)(sourceMat.Size().Width * RESOLUTION_FACTOR), (int)(sourceMat.Size().Height * RESOLUTION_FACTOR)));
                int desireWidth = 1600;
                Mat resizedFrameForDisplay = resizedFrame.Resize(new OpenCvSharp.Size((int)(desireWidth), (int)(sourceMat.Size().Height * desireWidth / sourceMat.Size().Width)));
                if (display)
                    FrameDisplay.display(resizedFrameForDisplay);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("********RESET RESIZE*****");
                throw;
            }
            return resizedFrame;
        }
    }
}
