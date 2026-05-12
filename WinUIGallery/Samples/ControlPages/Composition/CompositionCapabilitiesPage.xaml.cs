// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using SamplesCommon;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;

namespace WinUIGallery.ControlPages;

public sealed partial class CompositionCapabilitiesPage : Page, INotifyPropertyChanged
{
    private readonly CompositionCapabilities _liveCapabilities;
    private CapabilityWrapper _activeCapabilityWrapper;
    private SpriteVisual? _backgroundImageVisual;
    private ManagedSurface? _circleMaskSurface;
    private SpriteVisual? _circleImageVisual;
    private string _capabilityText = string.Empty;
    private Compositor? _compositor;
    private bool _containsCircleImage;
    private ContainerVisual? _imageContainer;
    private ManagedSurface? _surface;

    public CompositionCapabilitiesPage()
    {
        InitializeComponent();

        _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        _liveCapabilities = new CompositionCapabilities();

        CapabilityWrapper fastEffectsOption = new CapabilityWrapper("EffectsFast", true, true);
        CapabilityDropdownOptions.Add(fastEffectsOption);
        CapabilityDropdownOptions.Add(new CapabilityWrapper("EffectsSupported", true, false));
        CapabilityDropdownOptions.Add(new CapabilityWrapper("None", false, false));

        _activeCapabilityWrapper = fastEffectsOption;
        SimulatorDropdown.SelectedItem = fastEffectsOption;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CapabilityWrapper> CapabilityDropdownOptions { get; } = [];

    public string CapabilityText
    {
        get
        {
            return _capabilityText;
        }

        set
        {
            _capabilityText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CapabilityText)));
        }
    }

    private async void CapabilitiesExample_Loaded(object sender, RoutedEventArgs e)
    {
        _compositor ??= ElementCompositionPreview.GetElementVisual(ImageCanvas).Compositor;
        ImageLoader.Initialize(_compositor);

        _backgroundImageVisual = _compositor.CreateSpriteVisual();
        _imageContainer = _compositor.CreateContainerVisual();
        _liveCapabilities.Changed += HandleCapabilitiesChanged;

        ElementCompositionPreview.SetElementChildVisual(ImageCanvas, _imageContainer);

        _surface = await ImageLoader.Instance.LoadFromUriAsync(new Uri("ms-appx:///Assets/SceneGraph/Landscapes/Landscape-7.jpg"));
        _surface.Brush.Stretch = CompositionStretch.Fill;

        _imageContainer.Children.InsertAtTop(_backgroundImageVisual);
        UpdateVisualSizeAndPosition();
        UpdateAlbumArt();
    }

    private void HandleCapabilitiesChanged(CompositionCapabilities sender, object args)
    {
        if (!CapabilityToggle.IsOn)
        {
            _activeCapabilityWrapper = new CapabilityWrapper(
                "Live",
                sender.AreEffectsSupported(),
                sender.AreEffectsFast());
            UpdateAlbumArt();
        }
    }

    private void UpdateAlbumArt()
    {
        if (_compositor is null || _surface is null || _backgroundImageVisual is null || _imageContainer is null)
        {
            return;
        }

        if (_activeCapabilityWrapper.EffectsSupported)
        {
            EnsureCircleImage();

            using SaturationEffect saturationEffect = new SaturationEffect
            {
                Saturation = 0.3f,
                Source = new CompositionEffectSourceParameter("SaturationSource")
            };

            if (_activeCapabilityWrapper.EffectsFast)
            {
                using GaussianBlurEffect chainedEffect = new GaussianBlurEffect
                {
                    Name = "Blur",
                    Source = saturationEffect,
                    BlurAmount = 6.0f,
                    BorderMode = EffectBorderMode.Hard,
                    Optimization = EffectOptimization.Balanced
                };

                CompositionEffectFactory chainedEffectFactory = _compositor.CreateEffectFactory(chainedEffect);
                CompositionEffectBrush effectBrush = chainedEffectFactory.CreateBrush();
                effectBrush.SetSourceParameter("SaturationSource", _surface.Brush);
                _backgroundImageVisual.Brush = effectBrush;
                CapabilityText = "Effects are supported and fast. The background is blurred and desaturated.";
            }
            else
            {
                CompositionEffectFactory saturationEffectFactory = _compositor.CreateEffectFactory(saturationEffect);
                CompositionEffectBrush saturationBrush = saturationEffectFactory.CreateBrush();
                saturationBrush.SetSourceParameter("SaturationSource", _surface.Brush);
                _backgroundImageVisual.Brush = saturationBrush;
                CapabilityText = "Effects are supported but not fast. The background is desaturated.";
            }
        }
        else
        {
            if (_containsCircleImage && _circleImageVisual is not null)
            {
                _imageContainer.Children.Remove(_circleImageVisual);
                _containsCircleImage = false;
            }

            _backgroundImageVisual.Brush = _surface.Brush;
            CapabilityText = "Effects are not supported. The source image is shown without effects.";
        }
    }

    private void EnsureCircleImage()
    {
        if (_containsCircleImage || _compositor is null || _surface is null || _imageContainer is null)
        {
            return;
        }

        _circleMaskSurface = ImageLoader.Instance.LoadCircle(200, Colors.White);
        _circleMaskSurface.Brush.Stretch = CompositionStretch.Uniform;

        _circleImageVisual = _compositor.CreateSpriteVisual();
        UpdateVisualSizeAndPosition();

        CompositionMaskBrush maskBrush = _compositor.CreateMaskBrush();
        maskBrush.Source = _surface.Brush;
        maskBrush.Mask = _circleMaskSurface.Brush;
        _circleImageVisual.Brush = maskBrush;

        _imageContainer.Children.InsertAtTop(_circleImageVisual);
        _containsCircleImage = true;
    }

    private void ImageCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateVisualSizeAndPosition();
    }

    private void UpdateVisualSizeAndPosition()
    {
        float width = (float)ImageCanvas.ActualWidth;
        float height = (float)ImageCanvas.ActualHeight;

        if (_imageContainer is not null)
        {
            _imageContainer.Size = new Vector2(width, height);
        }

        if (_backgroundImageVisual is not null)
        {
            _backgroundImageVisual.Size = new Vector2(width, height);
        }

        if (_circleImageVisual is not null)
        {
            _circleImageVisual.Size = new Vector2(width / 2, height / 2);
            _circleImageVisual.Offset = new Vector3((width - _circleImageVisual.Size.X) / 2, (height - _circleImageVisual.Size.Y) / 2, 0);
        }
    }

    private void SimulatorDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SimulatorDropdown.SelectedItem is CapabilityWrapper selectedCapability)
        {
            _activeCapabilityWrapper = selectedCapability;
        }

        UpdateAlbumArt();
    }

    private void CapabilityToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (SimulatorDropdown is null)
        {
            return;
        }

        if (CapabilityToggle.IsOn)
        {
            SimulatorDropdown.Visibility = Visibility.Visible;
            if (SimulatorDropdown.SelectedItem is CapabilityWrapper selectedCapability)
            {
                _activeCapabilityWrapper = selectedCapability;
            }
        }
        else
        {
            SimulatorDropdown.Visibility = Visibility.Collapsed;
            _activeCapabilityWrapper = new CapabilityWrapper(
                "Live",
                _liveCapabilities.AreEffectsSupported(),
                _liveCapabilities.AreEffectsFast());
        }

        UpdateAlbumArt();
    }

    private void CapabilitiesExample_Unloaded(object sender, RoutedEventArgs e)
    {
        _liveCapabilities.Changed -= HandleCapabilitiesChanged;
        ElementCompositionPreview.SetElementChildVisual(ImageCanvas, null);

        _surface?.Dispose();
        _circleMaskSurface?.Dispose();
        _surface = null;
        _circleMaskSurface = null;
        _circleImageVisual = null;
        _backgroundImageVisual = null;
        _imageContainer = null;
        _containsCircleImage = false;
    }
}

public sealed class CapabilityWrapper
{
    public CapabilityWrapper(string name, bool effectsSupported, bool effectsFast)
    {
        Name = name;
        EffectsSupported = effectsSupported;
        EffectsFast = effectsFast;
    }

    public string Name { get; }

    public bool EffectsSupported { get; }

    public bool EffectsFast { get; }
}
