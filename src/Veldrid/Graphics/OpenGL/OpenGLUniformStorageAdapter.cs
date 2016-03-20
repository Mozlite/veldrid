﻿using OpenTK.Graphics.OpenGL;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLUniformStorageAdapter : ConstantBuffer
    {
        private readonly int _programID;
        private readonly int _uniformLocation;
        private readonly UniformSetter _setterFunction;

        public OpenGLUniformStorageAdapter(int programID, int uniformLocation)
        {
            _programID = programID;
            _uniformLocation = uniformLocation;

            int typeVal;
            GL.GetActiveUniforms(_programID, 1, ref uniformLocation, ActiveUniformParameter.UniformType, out typeVal);
            ActiveUniformType uniformType = (ActiveUniformType)typeVal;
            Console.WriteLine("Uniform of type " + uniformType);
            _setterFunction = GetSetterFunction(uniformType);
        }

        public unsafe void GetData(IntPtr storageLocation, int storageSizeInBytes)
        {
            if (storageSizeInBytes % sizeof(float) != 0)
            {
                throw new InvalidOperationException("Storage size must be a multiple of 4 bytes.");
            }

            float* floatPtr = (float*)storageLocation.ToPointer();
            GL.GetUniform(_programID, _uniformLocation, floatPtr);
        }

        public void GetData<T>(ref T storageLocation, int storageSizeInBytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(storageLocation, GCHandleType.Pinned);
            GetData(handle.AddrOfPinnedObject(), storageSizeInBytes);
            handle.Free();
        }

        public void GetData<T>(T[] storageLocation, int storageSizeInBytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(storageLocation, GCHandleType.Pinned);
            GetData(handle.AddrOfPinnedObject(), storageSizeInBytes);
            handle.Free();
        }

        public void SetData(IntPtr data, int dataSizeInBytes)
            => SetData(data, dataSizeInBytes, 0);
        public unsafe void SetData(IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes)
        {
            if (dataSizeInBytes % sizeof(float) != 0)
            {
                throw new InvalidOperationException($"{nameof(dataSizeInBytes)} must be a multiple of 4 bytes");
            }
            if (destinationOffsetInBytes % sizeof(float) != 0)
            {
                throw new InvalidOperationException($"{nameof(destinationOffsetInBytes)} must be a multiple of 4 bytes");
            }

            _setterFunction(_uniformLocation, data, dataSizeInBytes, destinationOffsetInBytes);
        }

        public void SetData<T>(ref T data, int dataSizeInBytes) where T : struct
            => SetData(ref data, dataSizeInBytes, 0);
        public void SetData<T>(ref T data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            SetData(handle.AddrOfPinnedObject(), dataSizeInBytes, destinationOffsetInBytes);
            handle.Free();
        }

        public void SetData<T>(T[] data, int dataSizeInBytes) where T : struct
            => SetData(data, dataSizeInBytes, 0);
        public void SetData<T>(T[] data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            SetData(handle.AddrOfPinnedObject(), dataSizeInBytes, destinationOffsetInBytes);
            handle.Free();
        }

        private delegate void UniformSetter(int uniformLocation, IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes);

        private unsafe UniformSetter GetSetterFunction(ActiveUniformType uniformType)
        {
            switch (uniformType)
            {
                case ActiveUniformType.FloatMat4:
                    return (int location, IntPtr data, int dataSizeInBytes, int offsetInBytes) =>
                    {
                        float* dataPtr = (float*)data.ToPointer();
                        int numMatrices = dataSizeInBytes / sizeof(Matrix4x4);
                        int offsetElements = offsetInBytes / sizeof(float);
                        dataPtr += offsetElements;
                        GL.UniformMatrix4(location, numMatrices, false, dataPtr);
                    };
                case ActiveUniformType.FloatVec2:
                    return (int location, IntPtr data, int dataSizeInBytes, int offsetInBytes) =>
                    {
                        float* dataPtr = (float*)data.ToPointer();
                        int numFloat2s = dataSizeInBytes / sizeof(Vector2);
                        int offsetElements = offsetInBytes / sizeof(float);
                        dataPtr += offsetElements;
                        GL.UniformMatrix4(location, numFloat2s, false, dataPtr);
                    };
                case ActiveUniformType.FloatVec3:
                    return (int location, IntPtr data, int dataSizeInBytes, int offsetInBytes) =>
                    {
                        float* dataPtr = (float*)data.ToPointer();
                        int numFloat3s = dataSizeInBytes / sizeof(Vector3);
                        int offsetElements = offsetInBytes / sizeof(float);
                        dataPtr += offsetElements;
                        GL.UniformMatrix4(location, numFloat3s, false, dataPtr);
                    };
                case ActiveUniformType.FloatVec4:
                    return (int location, IntPtr data, int dataSizeInBytes, int offsetInBytes) =>
                    {
                        float* dataPtr = (float*)data.ToPointer();
                        int numFloat4s = dataSizeInBytes / sizeof(Vector4);
                        int offsetElements = offsetInBytes / sizeof(float);
                        dataPtr += offsetElements;
                        GL.UniformMatrix4(location, numFloat4s, false, dataPtr);
                    };
                default:
                    throw new NotImplementedException($"Uniforms of type {uniformType} are not implemented.");
            }
        }

    }
}
