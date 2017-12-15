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
using UnityEngine.Assertions;

namespace NonHumanProgrammer.DX11FogOfWar
{
    /// <summary>Fog of war component with fixed size and position.</summary>
    [RequireComponent(typeof(Light)), DisallowMultipleComponent]
    public class StaticFogOfWar : MonoBehaviour
    {
        /*----------------------------------------------------------------------------
                    Constants.
        ----------------------------------------------------------------------------*/

        /// <summary>The number max of sights.</summary>
        public const int kMaxSights = 128;

        /*----------------------------------------------------------------------------
                    Public Properties.
        ----------------------------------------------------------------------------*/

        /// <summary>Resolution of masks in pixels.</summary>
        public int resolution { get { return m_Resolution; } }

        /// <summary>The size of fog of war.</summary>
        public float size { get { return m_Size; } }

        /// <summary>For each update an amount of data from the current mask is added up to the temporal mask.</summary>
        public float temporalAmount { get { return m_TemporalAmount; } }

        /// <summary>Distance in seconds between each update.</summary>
        public float timeStep { get { return m_TimeStep; } }

        /*----------------------------------------------------------------------------
                    Protected Fields.
        ----------------------------------------------------------------------------*/

        /// <summary>Holds fog of war masks (RenderTextures).</summary>
        protected Masks masks;

        /// <summary>Sights buffers.</summary>
        protected Eyesights eyesights;

        /// <summary>Compute shader used to render and clear the masks.</summary>
        protected ComputeShader computeShader;

        /// <summary>Shaders properties and kernels ids.</summary>
        protected int renderSightKernelId, clearMaskKernelId, temporalAmountId, scaleId, offsetId;

        /*----------------------------------------------------------------------------
                    Private Fields.
        ----------------------------------------------------------------------------*/

        [SerializeField] Texture navigationMaskTexture;
        [SerializeField] int m_Resolution = 512;
        [SerializeField] float m_Size = 128;
        [SerializeField] float m_TemporalAmount = 0.5f;
        [SerializeField] float m_TimeStep = 0.02f;

        // Initially It's assigned to lowest value, so when the first update is called the fog of war already should render.
        float m_LastUpdateTime = float.MinValue;

        /*----------------------------------------------------------------------------
                    Public Methods.
        ----------------------------------------------------------------------------*/

        /// <summary>
        /// Checks if there's enough time between the current time and the last
        /// update, if so returns true and update the time of the last update.
        /// </summary>
        /// <returns>Returns true if there's enouth time between the current time and the last update.</returns>
        public bool ShouldUpdate()
        {
            if (m_TimeStep < Time.time - m_LastUpdateTime)
            {
                m_LastUpdateTime = Time.time;
                return true;
            }
            return false;
        }

        /*----------------------------------------------------------------------------
                    Protected Methods.
        ----------------------------------------------------------------------------*/

        protected virtual void Awake()
        {
            Assert.AreEqual(navigationMaskTexture.width, navigationMaskTexture.height);
            Assert.AreEqual(navigationMaskTexture.width, resolution);

            computeShader = Resources.Load<ComputeShader>("ComputeShaders/FogOfWarComputeShader");

            AssignKernelAndPropertyIds();
            PopulateFields();
            AssignTextures();

            PrepareGameObject();
            eyesights.SetRange(0, 32);
        }
        public Transform t;
        protected virtual void Update()
        {
            if (!ShouldUpdate())
                return;

            eyesights.SetPosition(0, new Vector2(t.position.x, t.position.z));

            eyesights.UpdateBuffersData();

            Render();
        }

        protected virtual void OnDestroy()
        {
            eyesights.ReleaseBuffers();
            masks.ReleaseBuffers();
        }

        protected virtual void Render()
        {
            computeShader.SetFloat(scaleId, size / resolution);
            computeShader.SetFloat(temporalAmountId, temporalAmount);
            computeShader.SetVector(offsetId, transform.position - new Vector3(size, 0, size) * 0.5f);

            computeShader.Dispatch(clearMaskKernelId, resolution / 8, resolution / 8, 1);
            computeShader.Dispatch(renderSightKernelId, kMaxSights / 64, 1, 1);

            //Graphics.Blit(masks.raw, masks.temporal);
        }

        /// <summary>Resize every mask and repopulate the navigation mask.</summary>
        /// <param name="newResolution"></param>
        protected void ResizeResolution(int newResolution)
        {
            if (newResolution == resolution)
                return;

            masks = new Masks(newResolution);           
            m_Resolution = newResolution;

            Assert.AreEqual(navigationMaskTexture.width, navigationMaskTexture.height);
            Assert.AreEqual(navigationMaskTexture.width, newResolution);
        }

        /*----------------------------------------------------------------------------
                    Private Methods.
        ----------------------------------------------------------------------------*/

        private void PopulateFields()
        {         
            masks = new Masks(resolution);
            eyesights = new Eyesights(renderSightKernelId, computeShader, kMaxSights);
        }

        private void PrepareGameObject()
        {
            Light light = GetComponent<Light>();

            Assert.IsTrue(light.type == LightType.Directional, "Define the light type as directional.");
            Assert.AreEqual(transform.rotation, Quaternion.LookRotation(Vector3.down), "The light direction has to be looking straight down.");   

            light.cookieSize = size;
            light.cookie = masks.raw;
        }

        private void AssignTextures()
        {     
            computeShader.SetTexture(renderSightKernelId, masks.raw.name, masks.raw);
            computeShader.SetTexture(clearMaskKernelId, masks.raw.name, masks.raw);
            computeShader.SetTexture(clearMaskKernelId, masks.temporal.name, masks.temporal);
            computeShader.SetTexture(renderSightKernelId, "NavigationMaskTexture", navigationMaskTexture);
        }

        private void AssignKernelAndPropertyIds()
        {
            renderSightKernelId = computeShader.FindKernel("RenderSightMain");
            clearMaskKernelId = computeShader.FindKernel("ClearMaskMain");
            temporalAmountId = Shader.PropertyToID("TemporalAmount");
            scaleId = Shader.PropertyToID("Scale");
            offsetId = Shader.PropertyToID("Offset");
        }
    }
} 