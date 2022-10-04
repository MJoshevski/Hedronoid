using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Object = UnityEngine.Object;

namespace DanmakU
{
    internal class DanmakuRenderer : IDisposable
    {
        const int kBatchSize = 4096;

        static Vector4[] colorCache = new Vector4[kBatchSize];
        static Vector3[] positionCache = new Vector3[kBatchSize];
        static float[] yawRotationCache = new float[kBatchSize];
        static float[] pitchRotationCache = new float[kBatchSize];
        static uint[] args = new uint[] { 0, 0, 0, 0, 0 };

        static int positionPropertyId = Shader.PropertyToID("positionBuffer");
        static int yawRotationPropertyId = Shader.PropertyToID("yawRotationBuffer");
        static int pitchRotationPropertyId = Shader.PropertyToID("pitchRotationBuffer");
        static int colorPropertyId = Shader.PropertyToID("colorBuffer");

        public Color Color { get; set; } = Color.white;

        public readonly Mesh Mesh;

        Material sharedMaterial;
        protected Material renderMaterial;
        public Material Material
        {
            get { return sharedMaterial; }
            set
            {
                if (renderMaterial != null) Object.DestroyImmediate(renderMaterial);
                sharedMaterial = value;
                if (sharedMaterial != null)
                {
                    renderMaterial = Object.Instantiate(sharedMaterial);
                    renderMaterial.enableInstancing = true;
                    PrepareMaterial(renderMaterial);
                }
            }
        }

        readonly MaterialPropertyBlock propertyBlock;
        readonly ComputeBufferPool.Context StructuredBuffers;
        readonly ComputeBufferPool.Context ArgBuffers;

        public unsafe DanmakuRenderer(Material material, Mesh mesh)
        {
            propertyBlock = new MaterialPropertyBlock();
            StructuredBuffers = ComputeBufferPool.GetShared().CreateContext();
            ArgBuffers = ComputeBufferPool.GetShared(ComputeBufferType.IndirectArguments).CreateContext();
            Material = material;
            Mesh = mesh;
        }

        protected virtual void PrepareMaterial(Material material) { }

        public virtual void Dispose()
        {
            if (renderMaterial != null)
            {
                Object.DestroyImmediate(renderMaterial);
            }
            FlushBuffers();
        }

        public void FlushBuffers()
        {
            StructuredBuffers.Flush();
            ArgBuffers.Flush();
        }

        public unsafe void Render(List<DanmakuSet> sets, int layer)
        {
            var mesh = Mesh;
            int batchIndex = 0;

            foreach (var set in sets)
            {
                var pool = set.Pool;
                if (pool == null || pool.ActiveCount <= 0) continue;

                var srcPositions = (Vector3*)pool.Positions.GetUnsafeReadOnlyPtr();
                var srcYawRotations = (float*)pool.Yaws.GetUnsafeReadOnlyPtr();
                var srcPitchRotations = (float*)pool.Pitches.GetUnsafeReadOnlyPtr();
                var srcColors = (Color*)pool.Colors.GetUnsafeReadOnlyPtr();

                int poolIndex = 0;
                while (poolIndex < pool.ActiveCount)
                {
                    var count = Math.Min(kBatchSize - batchIndex, pool.ActiveCount - poolIndex);
                    fixed (Vector3* positions = positionCache)
                    {
                        UnsafeUtility.MemCpy(positions + batchIndex, srcPositions + poolIndex, sizeof(Vector3) * count);
                    }
                    fixed (float* yawRotations = yawRotationCache)
                    {
                        UnsafeUtility.MemCpy(yawRotations + batchIndex, srcYawRotations + poolIndex, sizeof(float) * count);
                    }
                    fixed (float* pitchRotations = pitchRotationCache)
                    {
                        UnsafeUtility.MemCpy(pitchRotations + batchIndex, srcPitchRotations + poolIndex, sizeof(float) * count);
                    }
                    fixed (Vector4* colors = colorCache)
                    {
                        UnsafeUtility.MemCpy(colors + batchIndex, srcColors + poolIndex, sizeof(Vector4) * count);
                    }
                    batchIndex += count;
                    poolIndex += count;
                    batchIndex %= kBatchSize;

                    if (batchIndex == 0)
                    {
                        RenderBatch(mesh, kBatchSize, layer);
                    }
                }
            }
            if (batchIndex != 0)
            {
                RenderBatch(mesh, batchIndex, layer);
            }
        }

        unsafe void RenderBatch(Mesh mesh, int batchSize, int layer)
        {
            ComputeBuffer argsBuffer = ArgBuffers.Rent(1, args.Length * sizeof(uint));
            ComputeBuffer positionBuffer = StructuredBuffers.Rent(kBatchSize, sizeof(Vector3));
            ComputeBuffer colorBuffer = StructuredBuffers.Rent(kBatchSize, sizeof(Color));
            ComputeBuffer yawRotationBuffer = StructuredBuffers.Rent(kBatchSize, sizeof(float));
            ComputeBuffer pitchRotationBuffer = StructuredBuffers.Rent(kBatchSize, sizeof(float));

            colorBuffer.SetData(colorCache, 0, 0, batchSize);
            positionBuffer.SetData(positionCache, 0, 0, batchSize);
            yawRotationBuffer.SetData(yawRotationCache, 0, 0, batchSize);
            pitchRotationBuffer.SetData(pitchRotationCache, 0, 0, batchSize);

            propertyBlock.SetBuffer(positionPropertyId, positionBuffer);
            propertyBlock.SetBuffer(yawRotationPropertyId, yawRotationBuffer);
            propertyBlock.SetBuffer(pitchRotationPropertyId, pitchRotationBuffer);
            propertyBlock.SetBuffer(colorPropertyId, colorBuffer);

            args[0] = mesh.GetIndexCount(0);
            args[1] = (uint)batchSize;
            argsBuffer.SetData(args);

            Graphics.DrawMeshInstancedIndirect(mesh, 0, renderMaterial,
              bounds: new Bounds(Vector3.zero, Vector3.one * 1000f),
              bufferWithArgs: argsBuffer,
              argsOffset: 0,
              properties: propertyBlock,
              castShadows: ShadowCastingMode.Off,
              receiveShadows: false,
              layer: layer,
              camera: null);
        }
    }
}