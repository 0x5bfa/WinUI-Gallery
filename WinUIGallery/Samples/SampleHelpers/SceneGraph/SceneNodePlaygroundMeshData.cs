// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;
using Windows.Foundation;

namespace WinUIGallery.Samples.SampleHelpers.SceneGraph;

internal static class SceneNodePlaygroundMeshData
{
    private static readonly float[] Positions =
    [
        0.0f, -1.0f, 0.0f,
        -1.0f, 1.0f, -1.0f,
        1.0f, 1.0f, -1.0f,
        1.0f, 1.0f, 1.0f,
        -1.0f, 1.0f, 1.0f,
    ];

    private static readonly float[] Normals =
    [
        0.0f, -1.0f, 0.0f,
        -0.5f, 0.5f, -0.5f,
        0.5f, 0.5f, -0.5f,
        0.5f, 0.5f, 0.5f,
        -0.5f, 0.5f, 0.5f,
    ];

    private static readonly ushort[] Indices =
    [
        0, 1, 2,
        0, 2, 3,
        0, 3, 4,
        0, 4, 1,
        1, 4, 3,
        1, 3, 2,
    ];

    public static MemoryBuffer CreatePositionBuffer()
    {
        return CreateBuffer(Positions);
    }

    public static MemoryBuffer CreateNormalBuffer()
    {
        return CreateBuffer(Normals);
    }

    public static MemoryBuffer CreateIndexBuffer()
    {
        return CreateBuffer(Indices);
    }

    private static MemoryBuffer CreateBuffer<T>(T[] values)
        where T : struct
    {
        ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(values.AsSpan());
        return global::SceneNodeCommon.CopyToMemoryBuffer(bytes.ToArray());
    }
}
