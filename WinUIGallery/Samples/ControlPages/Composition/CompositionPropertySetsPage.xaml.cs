// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using ExpressionBuilder;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using SamplesCommon;
using System;
using System.Numerics;
using EF = ExpressionBuilder.ExpressionFunctions;

namespace WinUIGallery.ControlPages;

public sealed partial class CompositionPropertySetsPage : Page
{
    private ManagedSurface? _blueBallSurface;
    private CompositionPropertySet? _propertySet;
    private ManagedSurface? _redBallSurface;

    public CompositionPropertySetsPage()
    {
        InitializeComponent();
    }

    private void PropertySetsExample_Loaded(object sender, RoutedEventArgs e)
    {
        Compositor compositor = ElementCompositionPreview.GetElementVisual(OrbitHost).Compositor;
        ImageLoader.Initialize(compositor);

        ContainerVisual container = compositor.CreateContainerVisual();
        ElementCompositionPreview.SetElementChildVisual(OrbitHost, container);

        _redBallSurface = ImageLoader.Instance.LoadFromUri(new Uri("ms-appx:///Assets/SceneGraph/Other/RedBall.png"));
        _blueBallSurface = ImageLoader.Instance.LoadFromUri(new Uri("ms-appx:///Assets/SceneGraph/Other/BlueBall.png"));

        float hostWidth = Math.Max((float)OrbitHost.ActualWidth, 480);

        SpriteVisual redSprite = compositor.CreateSpriteVisual();
        redSprite.Brush = _redBallSurface.Brush;
        redSprite.Size = new Vector2(100, 100);
        redSprite.Offset = new Vector3((hostWidth / 2) - (redSprite.Size.X / 2), 150, 0);
        container.Children.InsertAtTop(redSprite);

        SpriteVisual blueSprite = compositor.CreateSpriteVisual();
        blueSprite.Brush = _blueBallSurface.Brush;
        blueSprite.Size = new Vector2(25, 25);
        blueSprite.Offset = new Vector3((hostWidth / 2) - (redSprite.Size.X / 2), 50, 0);
        container.Children.InsertAtTop(blueSprite);

        _propertySet = compositor.CreatePropertySet();
        _propertySet.InsertScalar("Rotation", 0);
        _propertySet.InsertVector3(
            "CenterPointOffset",
            new Vector3((redSprite.Size.X / 2) - (blueSprite.Size.X / 2), (redSprite.Size.Y / 2) - (blueSprite.Size.Y / 2), 0));

        Vector3Node propSetCenterPoint = _propertySet.GetReference().GetVector3Property("CenterPointOffset");
        ScalarNode propSetRotation = _propertySet.GetReference().GetScalarProperty("Rotation");
        Vector3Node orbitExpression = redSprite.GetReference().Offset + propSetCenterPoint +
            EF.Vector3(
                EF.Cos(EF.ToRadians(propSetRotation)) * 150,
                EF.Sin(EF.ToRadians(propSetRotation)) * 75,
                0);

        blueSprite.StartAnimation("Offset", orbitExpression);

        LinearEasingFunction linear = compositor.CreateLinearEasingFunction();
        ScalarKeyFrameAnimation rotationAnimation = compositor.CreateScalarKeyFrameAnimation();
        rotationAnimation.InsertKeyFrame(1.0f, 360, linear);
        rotationAnimation.Duration = TimeSpan.FromSeconds(4);
        rotationAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
        _propertySet.StartAnimation("Rotation", rotationAnimation);

        ScalarKeyFrameAnimation offsetAnimation = compositor.CreateScalarKeyFrameAnimation();
        offsetAnimation.InsertKeyFrame(0, 50);
        offsetAnimation.InsertKeyFrame(0.5f, 150);
        offsetAnimation.InsertKeyFrame(1, 50);
        offsetAnimation.Duration = TimeSpan.FromSeconds(4);
        offsetAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
        redSprite.StartAnimation("Offset.Y", offsetAnimation);
    }

    private void PropertySetsExample_Unloaded(object sender, RoutedEventArgs e)
    {
        _propertySet?.StopAnimation("Rotation");
        ElementCompositionPreview.SetElementChildVisual(OrbitHost, null);

        _redBallSurface?.Dispose();
        _blueBallSurface?.Dispose();
        _redBallSurface = null;
        _blueBallSurface = null;
        _propertySet = null;
    }
}
