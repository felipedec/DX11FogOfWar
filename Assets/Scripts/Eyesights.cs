// MIT License
// 
// Copyright (c) 2017 Felipe Marques de Carvalho de Oliveira
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using UnityEngine;

namespace NonHumanProgrammer.DX11FogOfWar
{
    /// <summary>Holds and manages the Fog Of War Sights.</summary>
    public class Eyesights
    {
        /*----------------------------------------------------------------------------
                    Constants.
        ----------------------------------------------------------------------------*/

public const string kWorldPositionIdentifier = "EyesightWorldPositions";
        public const string kRangeIdentifier = "EyesightRanges";

        /*----------------------------------------------------------------------------
                    Public Fields.
        ----------------------------------------------------------------------------*/

        public readonly ComputeBuffer worldPositionComputeBuffer;
        public readonly ComputeBuffer rangeComputeBuffer;

        /*----------------------------------------------------------------------------
                    Protected Fields.
        ----------------------------------------------------------------------------*/

        protected readonly Vector2[] worldPositions;
        protected readonly int[] ranges;

        /*----------------------------------------------------------------------------
                    Private Fields.
        ----------------------------------------------------------------------------*/

        bool m_HasRangesChanged;

        /*----------------------------------------------------------------------------
                    Constructors / Deconstructors.
        ----------------------------------------------------------------------------*/

        public Eyesights(int kernelId, ComputeShader computeShader, int size)
        {
            worldPositions = new Vector2[size];
            ranges = new int[size];

            worldPositionComputeBuffer = new ComputeBuffer(size, 8, ComputeBufferType.GPUMemory);
            rangeComputeBuffer = new ComputeBuffer(size, 4, ComputeBufferType.GPUMemory);

            computeShader.SetBuffer(kernelId, kWorldPositionIdentifier, worldPositionComputeBuffer);
            computeShader.SetBuffer(kernelId, kRangeIdentifier, rangeComputeBuffer);
        }

        /*----------------------------------------------------------------------------
                    Public Method.
        ----------------------------------------------------------------------------*/

        public void ReleaseBuffers()
        {
            worldPositionComputeBuffer.Release();
            rangeComputeBuffer.Release();
        }

        public void SetRange(int index, int newRange)
        {
            if (newRange != ranges[index])
            {
                ranges[index] = newRange;
                m_HasRangesChanged = true;
            }
        }

        public void SetPosition(int index, Vector2 newPosition)
        {
            worldPositions[index] = newPosition;
        }

        public void UpdateBuffersData()
        {
            if (m_HasRangesChanged)
            {
                rangeComputeBuffer.SetData(ranges);
                m_HasRangesChanged = false;
            }
            worldPositionComputeBuffer.SetData(worldPositions);
        }
    }
}   