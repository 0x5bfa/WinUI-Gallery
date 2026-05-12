// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Linq;
using System.Numerics;

namespace WinUIGallery.ControlPages;

public sealed partial class CompositionXamlInteropPage : Page
{
    private ContainerVisual? _orderContainer;
    private DispatcherTimer? _reorderTimer;

    public CompositionXamlInteropPage()
    {
        InitializeComponent();
    }

    private void XamlInteropExample_Loaded(object sender, RoutedEventArgs e)
    {
        Visual textVisual = ElementCompositionPreview.GetElementVisual(AnimatedTextBlock);
        Compositor compositor = textVisual.Compositor;

        Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.InsertKeyFrame(0.5f, new Vector3(100, 0, 0));
        offsetAnimation.InsertKeyFrame(1.0f, Vector3.Zero);
        offsetAnimation.Duration = TimeSpan.FromSeconds(4);
        offsetAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
        textVisual.StartAnimation("Offset", offsetAnimation);

        ContainerVisual spriteContainer = compositor.CreateContainerVisual();
        ElementCompositionPreview.SetElementChildVisual(SpriteHost, spriteContainer);
        spriteContainer.Offset = new Vector3(12, 56, 0);

        AddSprite(spriteContainer, Colors.Red, new Vector3(0, 0, 0));
        AddSprite(spriteContainer, Colors.Blue, new Vector3(108, 0, 0));
        spriteContainer.StartAnimation("Offset", offsetAnimation);

        _orderContainer = compositor.CreateContainerVisual();
        ElementCompositionPreview.SetElementChildVisual(OrderHost, _orderContainer);
        _orderContainer.Offset = new Vector3(12, 56, 0);

        AddSprite(_orderContainer, Colors.Orange, new Vector3(0, 0, 0));
        AddSprite(_orderContainer, Colors.Green, new Vector3(50, 0, 0));
        AddSprite(_orderContainer, Colors.Purple, new Vector3(100, 0, 0));

        _reorderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _reorderTimer.Tick += ReorderTimer_Tick;
        _reorderTimer.Start();
    }

    private static void AddSprite(ContainerVisual container, Windows.UI.Color color, Vector3 offset)
    {
        Compositor compositor = container.Compositor;
        SpriteVisual sprite = compositor.CreateSpriteVisual();
        sprite.Brush = compositor.CreateColorBrush(color);
        sprite.Size = new Vector2(96, 80);
        sprite.Offset = offset;
        container.Children.InsertAtTop(sprite);
    }

    private void ReorderTimer_Tick(object? sender, object e)
    {
        Visual? child = _orderContainer?.Children.FirstOrDefault();
        if (child is null || _orderContainer is null)
        {
            return;
        }

        _orderContainer.Children.Remove(child);
        _orderContainer.Children.InsertAtTop(child);
    }

    private void XamlInteropExample_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_reorderTimer is not null)
        {
            _reorderTimer.Stop();
            _reorderTimer.Tick -= ReorderTimer_Tick;
            _reorderTimer = null;
        }

        ElementCompositionPreview.GetElementVisual(AnimatedTextBlock).StopAnimation("Offset");
        ElementCompositionPreview.SetElementChildVisual(SpriteHost, null);
        ElementCompositionPreview.SetElementChildVisual(OrderHost, null);
        _orderContainer = null;
    }
}
