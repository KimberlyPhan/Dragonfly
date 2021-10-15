using BGSObjectDetector;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            string inputtPath = "C:\\stuff\\RocketVA\\media\\";
            string outputPath = "C:\\stuff\\videos\\test\\output\\";
            string filename = "SampleHighwaySideSearch";

            System.IO.DirectoryInfo di = new DirectoryInfo(outputPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            Decoder.Decoder decoder = new Decoder.Decoder($"{inputtPath}{filename}.mp4", false);

            int sampleCount = 3;
            int jumpframe = 0;
            for (int frameIndex = 1; frameIndex < sampleCount + 1; frameIndex++)
            {
                Mat frame = decoder.getNextFrame();

                BGSObjectDetector.BGSObjectDetector bgs = new BGSObjectDetector.BGSObjectDetector();
                Mat fgmask = null;
                List<Box> foregroundBoxes = bgs.DetectObjects(DateTime.Now, frame, frameIndex, out fgmask, outputPath, $"{filename}_{frameIndex}");
                if (foregroundBoxes.Count > 0)
                {
                    // do nothing
                }


                if (fgmask != null)
                {
                    File.WriteAllBytes($"{outputPath}{filename}_{frameIndex}.jpg", Utils.Utils.ImageToByteJpeg(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(fgmask)));
                }

                for (int i = 0; i < jumpframe; i++)
                {
                    decoder.getNextFrame();
                }
            }
        }
    }
}
