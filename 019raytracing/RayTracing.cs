﻿// Author: Josef Pelikan

using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using OpenTK;
using Rendering;

namespace _019raytracing
{
  public class FormSupport
  {
    /// <summary>
    /// Initialize the ray-scene.
    /// </summary>
    public static IRayScene getScene ()
    {
      return Form1.singleton.SceneByComboBox();
    }

    /// <summary>
    /// Initialize ray-scene and image function (good enough for simple samples).
    /// </summary>
    public static IImageFunction getImageFunction ( IRayScene scene )
    {
      return new RayTracing( scene );
    }

    /// <summary>
    /// Initialize image synthesizer (responsible for raster image computation).
    /// </summary>
    public static IRenderer getRenderer ( IImageFunction imf )
    {
      SimpleImageSynthesizer sis = new SimpleImageSynthesizer();
      sis.ImageFunction = imf;
      return sis;
    }

    /// <summary>
    /// Prepare combo-box of available scenes.
    /// </summary>
    public static void InitializeScenes ()
    {
      Form1 f = Form1.singleton;
      f.sceneInitFunctions = new List<InitSceneDelegate>( Scenes.InitFunctions );

      // 1. default scenes from RayCastingScenes
      foreach ( string name in Scenes.Names )
        f.ComboScene.Items.Add( name );

      // 2. eventually add custom scenes
      f.sceneInitFunctions.Add( new InitSceneDelegate( CustomScene.TestScene ) );
      f.ComboScene.Items.Add( "Test scene" );

      // .. and set your favorite scene here:
      f.ComboScene.SelectedIndex = f.ComboScene.Items.IndexOf( "Test scene" );
    }
  }
}

namespace Rendering
{
  /// <summary>
  /// Custom scene for ray-based rendering.
  /// </summary>
  public class CustomScene
  {
    public static void TestScene ( IRayScene sc )
    {
      Debug.Assert( sc != null );

      // CSG scene:
      CSGInnerNode root = new CSGInnerNode( SetOperation.Union );
      root.SetAttribute( PropertyName.REFLECTANCE_MODEL, new PhongModel() );
      root.SetAttribute( PropertyName.MATERIAL, new PhongMaterial( new double[] { 0.6, 0.0, 0.0 }, 0.15, 0.8, 0.15, 16 ) );
      sc.Intersectable = root;

      // Background color:
      sc.BackgroundColor = new double[] { 0.0, 0.05, 0.07 };

      // Camera:
      sc.Camera = new StaticCamera( new Vector3d( 0.7, 3.0, -10.0 ),
                                    new Vector3d( 0.0, -0.2, 1.0 ),
                                    50.0 );

      // Light sources:
      sc.Sources = new LinkedList<ILightSource>();
      sc.Sources.Add( new AmbientLightSource( 0.8 ) );
      sc.Sources.Add( new PointLightSource( new Vector3d( -5.0, 3.0, -3.0 ), 1.0 ) );

      // --- NODE DEFINITIONS ----------------------------------------------------

      // Base plane
      Plane pl = new Plane();
      pl.SetAttribute( PropertyName.COLOR, new double[] { 0.0, 0.2, 0.0 } );
      pl.SetAttribute( PropertyName.TEXTURE, new CheckerTexture( 0.5, 0.5, new double[] { 1.0, 1.0, 1.0 } ) );
      root.InsertChild( pl, Matrix4d.RotateX( -MathHelper.PiOver2 ) * Matrix4d.CreateTranslation( 0.0, -1.0, 0.0 ) );

      // Cylinders
      Cylinder c = new Cylinder();
      root.InsertChild( c, Matrix4d.RotateX( MathHelper.PiOver2 ) * Matrix4d.CreateTranslation( -2.1, 0.0, 1.0 ) );
      c = new Cylinder();
      c.SetAttribute( PropertyName.COLOR, new double[] { 0.2, 0.0, 0.7 } );
      c.SetAttribute( PropertyName.TEXTURE, new CheckerTexture( 12.0, 1.0, new double[] { 0.0, 0.0, 0.3 } ) );
      root.InsertChild( c, Matrix4d.RotateY( -0.4 ) * Matrix4d.CreateTranslation( 1.0, 0.0, 1.0 ) );
      c = new Cylinder( 0.0, 100.0 );
      c.SetAttribute( PropertyName.COLOR, new double[] { 0.1, 0.7, 0.0 } );
      root.InsertChild( c, Matrix4d.RotateY( 0.2 ) * Matrix4d.CreateTranslation( 5.0, 0.3, 4.0 ) );
      c = new Cylinder( -0.5, 0.5 );
      c.SetAttribute( PropertyName.COLOR, new double[] { 0.8, 0.6, 0.0 } );
      root.InsertChild( c, Matrix4d.Scale( 2.0 ) * Matrix4d.RotateX( 1.2 ) * Matrix4d.CreateTranslation( 2.0, 1.8, 16.0 ) );
    }
  }
}
