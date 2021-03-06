﻿//Created by Max Whitby

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "PerlinNoise"), to deal
//in the PerlinNoise without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

using System;
using System.Collections.Generic;

namespace PerlinNoiseGenerator
{
    public static class PerlinNoise
    {
        /// <summary>
        /// returns an image(width, height) with generated perlin noise. Persistance is the decay in amplitude per octave.
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="octaveCount">number of octaves (think fourier composition) which represent number of images that will be blended</param>
        /// <param name="persistance">Persistance is used for how long the amplitude will resonate between octaves</param>
        /// <param name="amplitude">The amplitude of the noise. A low value means not very noisy, a high value means pretty noisy</param>
        /// <returns></returns>
        public static float[,] Noise(int width, int height, int octaveCount, float persistance, float amplitude) => Blend(GenerateNoise(width, height), octaveCount, width, height, new float[] { }, persistance, amplitude);



        /// <summary>
        /// returns an image(width, height) with generated perlin noise. Prominance is an array which can be seen as a 'tuner' for each octave of the noise.
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="octaveCount">number of octaves (think fourier composition) which represent number of images that will be blended</param>
        /// <param name="prominence">values that dictate the amplitude of a given octave. A more fine grained approach. </param>
        /// <returns></returns>
        public static float[,] Noise(int width, int height, int octaveCount, float[] prominence) => Blend(GenerateNoise(width, height), octaveCount, width, height, prominence);




        // Produces a float array of .NET noise based on a width/height between 0,1. Sample size controls the fine grainness of output
        private static float[,] GenerateNoise(int width, int height, int samplesize=8192)
        {
            float[,] noise = new float[width, height];
            Random randomNumber = new Random();
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    noise[i, j] = Convert.ToSingle(randomNumber.Next(0, samplesize) ) / samplesize ;
                }
            }
            return noise;
        }

        //Given a 2D-Image we iterate through every pixel.
        private static float[,] Smooth(float[,] baseNoise, int octave, int width, int height)
        {
            float[,] smoothNoise = new float[width, height];

            int smoothPeriod = 8 << octave;

            float smoothFrequency = 1.0f / smoothPeriod;

            for (int i = 0; i < width; ++i)
            {
                int firstHorizontalSample = (i / smoothPeriod) * smoothPeriod;
                int secondHorizontalSample = (firstHorizontalSample + smoothPeriod) % width;
                float horizontalBlend = (i - firstHorizontalSample) * smoothFrequency;

                for (int j = 0; j < height; ++j)
                {
                    int firstVerticalSample = ( j / smoothPeriod) * smoothPeriod;
                    int secondVerticalSample = (firstVerticalSample + smoothPeriod) % height;
                    float verticalBlend = (j - firstVerticalSample) * smoothFrequency;

                    (float x, float y) interpolateCorners = ( Lerp(baseNoise[firstHorizontalSample, firstVerticalSample], baseNoise[secondHorizontalSample, firstVerticalSample], horizontalBlend),
                                                              Lerp(baseNoise[firstHorizontalSample, secondVerticalSample], baseNoise[secondHorizontalSample, secondVerticalSample], horizontalBlend));

                    smoothNoise[i, j] = Lerp(interpolateCorners.x, interpolateCorners.y, verticalBlend);
                }
            }

            return smoothNoise;
        }

        private static float[,] Blend(float[,] baseNoise, int octaveCount, int width, int height, float[] promenance, float amplitude = 1, float persistance = 1 )
        {
            List<float[,]> smoothNoise = new List<float[,]>();

            for (int i = 0; i < octaveCount; ++i)
            {
                smoothNoise.Add(Smooth(baseNoise, i, width, height));
            }

            float[,] perlinNoise = new float[width, height];

            float totalAmplitude = 0.0f;
            //blend noise together
            for (int octave = octaveCount - 1; octave >= 0; octave--)
            {
                float promenanceToApply = (promenance.Length > 0) ? promenance[octaveCount - 1 - octave] : 1;


                amplitude *= persistance;
                 
                totalAmplitude += ( amplitude * promenanceToApply) ;

                for (int i = 0; i < width; ++i)
                {
                    for (int j = 0; j < height; ++j)
                    {
                        perlinNoise[i, j] += smoothNoise[octave][i, j] * promenanceToApply * amplitude;
                    }
                }
            }

            //normalisation
            for (int i = 0; i < width; i++) { for (int j = 0; j < height; j++) { perlinNoise[i, j] /= totalAmplitude; } }

            return perlinNoise;
        }

        //private static float[,] Blend(float[,] baseNoise, int octaveCount, int width, int height, float persistance, float amplitude)
        //{
        //    List<float[,]> smoothNoise = new List<float[,]>();

        //    for (int i = 0; i < octaveCount; i++)
        //    {
        //        smoothNoise.Add(Smooth(baseNoise, i, width, height));
        //    }

        //    float[,] perlinNoise = new float[width, height];

        //    float totalAmplitude = 0.0f;
        //    //blend noise together
        //    for (int octave = octaveCount - 1; octave >= 0; octave--)
        //    {
        //        amplitude *= persistance;
        //        totalAmplitude += amplitude;
        //        for (int i = 0; i < width; i++)
        //        {
        //            for (int j = 0; j < height; j++)
        //            {
        //                perlinNoise[i, j] += smoothNoise[octave][i, j] * amplitude;
        //            }
        //        }
        //    }
        //    //normalisation
        //    for (int i = 0; i < width; i++) { for (int j = 0; j < height; j++) { perlinNoise[i, j] /= totalAmplitude; } }

        //    return perlinNoise;
        //}

        private static float Lerp(float a, float b, float t) => (1f - t) * a + b * t;


    }
}
