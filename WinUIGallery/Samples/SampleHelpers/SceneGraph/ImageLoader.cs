// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.DirectX;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Media;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;

namespace SamplesCommon;

public sealed class ManagedSurface : IDisposable
{
    private readonly IDisposable? _surface;

    internal ManagedSurface(CompositionSurfaceBrush brush, IDisposable? surface)
    {
        Brush = brush;
        _surface = surface;
    }

    public CompositionSurfaceBrush Brush { get; }

    public void Dispose()
    {
        Brush.Dispose();
        _surface?.Dispose();
    }
}

public sealed class ImageLoader
{
    private static ImageLoader? _imageLoader;

    private readonly Compositor _compositor;
    private readonly CanvasDevice _canvasDevice;
    private readonly CompositionGraphicsDevice _graphicsDevice;

    private ImageLoader(Compositor compositor)
    {
        _compositor = compositor;
        _canvasDevice = new CanvasDevice();
        _graphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(_compositor, _canvasDevice);
    }

    public static ImageLoader Instance
    {
        get
        {
            return _imageLoader ?? throw new InvalidOperationException("ImageLoader.Initialize must be called before accessing ImageLoader.Instance.");
        }
    }

    public static void Initialize(Compositor compositor)
    {
        _imageLoader ??= new ImageLoader(compositor);
    }

    public ManagedSurface LoadFromUri(Uri uri)
    {
        LoadedImageSurface surface = LoadedImageSurface.StartLoadFromUri(uri);
        CompositionSurfaceBrush brush = _compositor.CreateSurfaceBrush(surface);

        return new ManagedSurface(brush, surface);
    }

    public Task<ManagedSurface> LoadFromUriAsync(Uri uri)
    {
        return Task.FromResult(LoadFromUri(uri));
    }

    public ManagedSurface LoadCircle(float radius, Color color)
    {
        Size size = new Size(radius * 2, radius * 2);
        CompositionDrawingSurface surface = _graphicsDevice.CreateDrawingSurface(
            size,
            DirectXPixelFormat.B8G8R8A8UIntNormalized,
            DirectXAlphaMode.Premultiplied);

        using (CanvasDrawingSession drawingSession = CanvasComposition.CreateDrawingSession(surface))
        {
            drawingSession.Clear(Color.FromArgb(0, 0, 0, 0));
            drawingSession.FillCircle(new Vector2(radius), radius, color);
        }

        CompositionSurfaceBrush brush = _compositor.CreateSurfaceBrush(surface);

        return new ManagedSurface(brush, surface);
    }
}
