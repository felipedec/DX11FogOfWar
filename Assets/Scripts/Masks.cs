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
    /// <summary>Structure that holds the Fog Of War masks.</summary>
    public class Masks
    {
        /*----------------------------------------------------------------------------
                    Masks.
        ----------------------------------------------------------------------------*/

        /// <summary>Fog of War mask before any post-processing effect.</summary>
        public readonly RenderTexture raw;

        /// <summary>The final mask, it is actually assigned to the directional light cookie.</summary>
        public readonly RenderTexture final;

        /// <summary>Holds information about the previously masks.</summary>
        public readonly RenderTexture temporal;

        /*----------------------------------------------------------------------------
                    Constructors / Destructors.
        ----------------------------------------------------------------------------*/

        public Masks(int resolution)
        {
            raw = MakeMask(resolution, "Raw", FilterMode.Point);
            final = MakeMask(resolution, "Final", FilterMode.Bilinear);
            temporal = MakeMask(resolution, "Temporal", FilterMode.Bilinear);
        }

        /*----------------------------------------------------------------------------
                    Utilities.
        ----------------------------------------------------------------------------*/

        private static RenderTexture MakeMask(int resolution, string name, FilterMode filterMode)
        {
            RenderTexture result = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            result.name = name + "MaskTexture"; 
            result.enableRandomWrite = true;
            result.autoGenerateMips = false;
            result.filterMode = filterMode;

            result.Create();

            return result;
        }

        /*----------------------------------------------------------------------------
                    Public Methods.
        ----------------------------------------------------------------------------*/

        public void ReleaseBuffers()
        {   
            raw.Release();
            final.Release();
            temporal.Release();
        }
    }
}