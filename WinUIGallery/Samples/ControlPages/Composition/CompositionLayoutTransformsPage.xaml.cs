// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace WinUIGallery.ControlPages;

public sealed partial class CompositionLayoutTransformsPage : Page
{
    private const float ImageWidth = 400;
    private const float ImageHeight = 267;

    private LoadedImageSurface? _imageSurface;
    private SpriteVisual? _mainImage;
    private SpriteVisual? _anchorPointIndicator;
    private SpriteVisual? _centerPointIndicator;
    private ContainerVisual? _indicatorContainer;

    public CompositionLayoutTransformsPage()
    {
        InitializeComponent();
    }

    private void LayoutTransformsExample_Loaded(object sender, RoutedEventArgs e)
    {
        Visual contentVisual = ElementCompositionPreview.GetElementVisual(ContentGrid);
        Compositor compositor = contentVisual.Compositor;
        contentVisual.Clip = compositor.CreateInsetClip();

        ContainerVisual root = compositor.CreateContainerVisual();
        ElementCompositionPreview.SetElementChildVisual(ImageContainer, root);

        _imageSurface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Assets/SceneGraph/Landscapes/Landscape-1.jpg"));
        CompositionSurfaceBrush imageBrush = compositor.CreateSurfaceBrush(_imageSurface);
        imageBrush.Stretch = CompositionStretch.Fill;

        _mainImage = compositor.CreateSpriteVisual();
        _mainImage.Brush = imageBrush;
        _mainImage.Size = new Vector2(ImageWidth, ImageHeight);
        _mainImage.CenterPoint = new Vector3(ImageWidth / 2, ImageHeight / 2, 0);
        root.Children.InsertAtBottom(_mainImage);

        _indicatorContainer = compositor.CreateContainerVisual();
        root.Children.InsertAtTop(_indicatorContainer);

        _centerPointIndicator = CreateIndicator(compositor, Colors.Green);
        _anchorPointIndicator = CreateIndicator(compositor, Colors.Red);
        _indicatorContainer.Children.InsertAtTop(_centerPointIndicator);
        _indicatorContainer.Children.InsertAtTop(_anchorPointIndicator);
        UpdateIndicators();

        TransformItemsControl.ItemsSource = CreateTransformOptions();
    }

    private static SpriteVisual CreateIndicator(Compositor compositor, Windows.UI.Color color)
    {
        SpriteVisual indicator = compositor.CreateSpriteVisual();
        indicator.Size = new Vector2(12, 12);
        indicator.AnchorPoint = new Vector2(0.5f, 0.5f);
        indicator.Brush = compositor.CreateColorBrush(color);

        return indicator;
    }

    private List<CompositionTransformPropertyModel> CreateTransformOptions()
    {
        if (_mainImage is null)
        {
            return [];
        }

        return
        [
            new CompositionTransformPropertyModel(AnchorPointXAction) { PropertyName = "AnchorPoint - X (red)", MinValue = -1, MaxValue = 2, StepFrequency = 0.01f, Value = _mainImage.AnchorPoint.X },
            new CompositionTransformPropertyModel(AnchorPointYAction) { PropertyName = "AnchorPoint - Y (red)", MinValue = -1, MaxValue = 2, StepFrequency = 0.01f, Value = _mainImage.AnchorPoint.Y },
            new CompositionTransformPropertyModel(CenterPointXAction) { PropertyName = "CenterPoint - X (green)", MinValue = -600, MaxValue = 600, StepFrequency = 1f, Value = _mainImage.CenterPoint.X },
            new CompositionTransformPropertyModel(CenterPointYAction) { PropertyName = "CenterPoint - Y (green)", MinValue = -600, MaxValue = 600, StepFrequency = 1f, Value = _mainImage.CenterPoint.Y },
            new CompositionTransformPropertyModel(RotationAction) { PropertyName = "Rotation (degrees)", MinValue = 0, MaxValue = 360, StepFrequency = 1f, Value = _mainImage.RotationAngleInDegrees },
            new CompositionTransformPropertyModel(ScaleXAction) { PropertyName = "Scale - X", MinValue = 0, MaxValue = 3, StepFrequency = 0.01f, Value = _mainImage.Scale.X },
            new CompositionTransformPropertyModel(ScaleYAction) { PropertyName = "Scale - Y", MinValue = 0, MaxValue = 3, StepFrequency = 0.01f, Value = _mainImage.Scale.Y },
            new CompositionTransformPropertyModel(OffsetXAction) { PropertyName = "Offset - X", MinValue = -200, MaxValue = 200, StepFrequency = 1f, Value = _mainImage.Offset.X },
            new CompositionTransformPropertyModel(OffsetYAction) { PropertyName = "Offset - Y", MinValue = -200, MaxValue = 200, StepFrequency = 1f, Value = _mainImage.Offset.Y },
        ];
    }

    private void AnchorPointXAction(float value)
    {
        if (_mainImage is null)
        {
            return;
        }

        _mainImage.AnchorPoint = new Vector2(value, _mainImage.AnchorPoint.Y);
        UpdateIndicators();
    }

    private void AnchorPointYAction(float value)
    {
        if (_mainImage is null)
        {
            return;
        }

        _mainImage.AnchorPoint = new Vector2(_mainImage.AnchorPoint.X, value);
        UpdateIndicators();
    }

    private void CenterPointXAction(float value)
    {
        if (_mainImage is null)
        {
            return;
        }

        _mainImage.CenterPoint = new Vector3(value, _mainImage.CenterPoint.Y, _mainImage.CenterPoint.Z);
        UpdateIndicators();
    }

    private void CenterPointYAction(float value)
    {
        if (_mainImage is null)
        {
            return;
        }

        _mainImage.CenterPoint = new Vector3(_mainImage.CenterPoint.X, value, _mainImage.CenterPoint.Z);
        UpdateIndicators();
    }

    private void RotationAction(float value)
    {
        if (_mainImage is not null)
        {
            _mainImage.RotationAngleInDegrees = value;
        }
    }

    private void ScaleXAction(float value)
    {
        if (_mainImage is not null)
        {
            _mainImage.Scale = new Vector3(value, _mainImage.Scale.Y, 1);
        }
    }

    private void ScaleYAction(float value)
    {
        if (_mainImage is not null)
        {
            _mainImage.Scale = new Vector3(_mainImage.Scale.X, value, 1);
        }
    }

    private void OffsetXAction(float value)
    {
        if (_mainImage is null)
        {
            return;
        }

        _mainImage.Offset = new Vector3(value, _mainImage.Offset.Y, _mainImage.Offset.Z);
        UpdateIndicators();
    }

    private void OffsetYAction(float value)
    {
        if (_mainImage is null)
        {
            return;
        }

        _mainImage.Offset = new Vector3(_mainImage.Offset.X, value, _mainImage.Offset.Z);
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        if (_mainImage is null || _anchorPointIndicator is null || _centerPointIndicator is null || _indicatorContainer is null)
        {
            return;
        }

        _indicatorContainer.Offset = _mainImage.Offset;
        _anchorPointIndicator.Offset = new Vector3(_mainImage.AnchorPoint.X * ImageWidth, _mainImage.AnchorPoint.Y * ImageHeight, 0);
        _centerPointIndicator.Offset = _mainImage.CenterPoint;
    }

    private void ContentGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        Visual contentVisual = ElementCompositionPreview.GetElementVisual(ContentGrid);
        contentVisual.Size = new Vector2((float)ContentGrid.ActualWidth, (float)ContentGrid.ActualHeight);
    }

    private void LayoutTransformsExample_Unloaded(object sender, RoutedEventArgs e)
    {
        TransformItemsControl.ItemsSource = null;
        ElementCompositionPreview.SetElementChildVisual(ImageContainer, null);
        _imageSurface?.Dispose();
        _imageSurface = null;
        _mainImage = null;
        _anchorPointIndicator = null;
        _centerPointIndicator = null;
        _indicatorContainer = null;
    }
}

public sealed class CompositionTransformPropertyModel : INotifyPropertyChanged
{
    private readonly Action<float> _action;
    private float _value;

    public CompositionTransformPropertyModel(Action<float> action)
    {
        _action = action;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string PropertyName { get; set; } = string.Empty;

    public float MinValue { get; set; }

    public float MaxValue { get; set; }

    public float StepFrequency { get; set; }

    public float Value
    {
        get
        {
            return _value;
        }

        set
        {
            if (_value == value)
            {
                return;
            }

            _value = value;
            _action(_value);
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
