// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graphics.DirectX;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Scenes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Numerics;
using WinUIGallery.Samples.SampleHelpers.SceneGraph;

namespace WinUIGallery.ControlPages;

public sealed partial class SceneNodePlaygroundPage : Page
{
    private const float SceneSize = 300;

    private ContainerVisual? _container;
    private SceneVisual? _sceneVisual;

    public SceneNodePlaygroundPage()
    {
        InitializeComponent();
    }

    private void SceneNodeExample_Loaded(object sender, RoutedEventArgs e)
    {
        Compositor compositor = ElementCompositionPreview.GetElementVisual(SceneHost).Compositor;
        _container = compositor.CreateContainerVisual();
        ElementCompositionPreview.SetElementChildVisual(SceneHost, _container);

        SceneNode rootNode = SceneNode.Create(compositor);
        SceneNode meshNode = SceneNode.Create(compositor);
        rootNode.Children.Add(meshNode);

        SceneMesh mesh = SceneMesh.Create(compositor);
        mesh.PrimitiveTopology = DirectXPrimitiveTopology.TriangleList;
#pragma warning disable CA2000 // SceneMesh consumes the buffers when the mesh attributes are filled.
        mesh.FillMeshAttribute(
            SceneAttributeSemantic.Vertex,
            DirectXPixelFormat.R32G32B32Float,
            SceneNodePlaygroundMeshData.CreatePositionBuffer());
        mesh.FillMeshAttribute(
            SceneAttributeSemantic.Normal,
            DirectXPixelFormat.R32G32B32Float,
            SceneNodePlaygroundMeshData.CreateNormalBuffer());
        mesh.FillMeshAttribute(
            SceneAttributeSemantic.Index,
            DirectXPixelFormat.R16UInt,
            SceneNodePlaygroundMeshData.CreateIndexBuffer());
#pragma warning restore CA2000

        SceneMetallicRoughnessMaterial material = SceneMetallicRoughnessMaterial.Create(compositor);
        material.BaseColorFactor = new Vector4(0.22f, 0.75f, 0.43f, 1.0f);
        material.MetallicFactor = 0.2f;
        material.RoughnessFactor = 0.5f;
        material.IsDoubleSided = true;

        SceneMeshRendererComponent rendererComponent = SceneMeshRendererComponent.Create(compositor);
        rendererComponent.Mesh = mesh;
        rendererComponent.Material = material;
        meshNode.Components.Add(rendererComponent);

        meshNode.Transform.Scale = new Vector3(90);
        meshNode.Transform.RotationAxis = new Vector3(0, 1, 0);

        ScalarKeyFrameAnimation rotationAnimation = compositor.CreateScalarKeyFrameAnimation();
        rotationAnimation.InsertKeyFrame(0, 0);
        rotationAnimation.InsertKeyFrame(1, 360);
        rotationAnimation.Duration = TimeSpan.FromSeconds(8);
        rotationAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
        meshNode.Transform.StartAnimation("RotationAngleInDegrees", rotationAnimation);

        _sceneVisual = SceneVisual.Create(compositor);
        _sceneVisual.Root = rootNode;
        _sceneVisual.Size = new Vector2(SceneSize);
        _container.Children.InsertAtTop(_sceneVisual);
        UpdateScenePosition();
    }

    private void SceneHost_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateScenePosition();
    }

    private void UpdateScenePosition()
    {
        if (_container is null || _sceneVisual is null)
        {
            return;
        }

        float width = (float)SceneHost.ActualWidth;
        float height = (float)SceneHost.ActualHeight;
        _container.Size = new Vector2(width, height);
        _sceneVisual.Offset = new Vector3((width - SceneSize) / 2, (height - SceneSize) / 2, 0);
    }

    private void SceneNodeExample_Unloaded(object sender, RoutedEventArgs e)
    {
        ElementCompositionPreview.SetElementChildVisual(SceneHost, null);
        _sceneVisual = null;
        _container = null;
    }
}
