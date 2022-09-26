//////////////////////////////////////////////////
// Rendering params.

Debug.Assert(scene != null);
Debug.Assert(context != null);

// Override image resolution and supersampling.
context[PropertyName.CTX_WIDTH] = 640; // 1800, 1920
context[PropertyName.CTX_HEIGHT] = 480; // 1200, 1080
//context[PropertyName.CTX_SUPERSAMPLING] =   4; //  400,   64

// Tooltip (if script uses values from 'param').
context[PropertyName.CTX_TOOLTIP] = "n=<double> (index of refraction)\rschlick=<double> (Schlick coeff)\rmat={mirror|glass|diffuse}}";

// Params dictionary.
Dictionary<string, string> p = Util.ParseKeyValueList(param);

// schlick = <schlick-coeff>
double sch = 1.0;
Util.TryParse(p, "schlick", ref sch);

// n = <index-of-refraction>
double n = 1.6;
Util.TryParse(p, "n", ref n);

//glass material
PhongMaterial pm = new PhongMaterial(new double[] {0.0, 0.2, 0.1}, 0.05, 0.05, 0.1, 128, sch);
pm.n = n;
pm.Kt = 0.9;


//////////////////////////////////////////////////
// CSG scene.

CSGInnerNode root = new CSGInnerNode(SetOperation.Union);
root.SetAttribute(PropertyName.REFLECTANCE_MODEL, new PhongModel());
root.SetAttribute(PropertyName.MATERIAL, new PhongMaterial(new double[] { 1.0, 0.7, 0.1 }, 0.1, 0.7, 0.3, 128, sch));
scene.Intersectable = root;

// Background color.
scene.BackgroundColor = new double[] { 0.0, 0.01, 0.03 };
scene.Background = new DefaultBackground(scene.BackgroundColor);

// Camera.
scene.Camera = new StaticCamera(new Vector3d(0.7, 0.5, -5.0),
                                new Vector3d(0.0, -0.18, 1.0),
                                50.0);

// Light sources.
scene.Sources = new System.Collections.Generic.LinkedList<ILightSource>();
scene.Sources.Add(new AmbientLightSource(0.8));
scene.Sources.Add(new PointLightSource(new Vector3d(-5.0, 4.0, -3.0), 1.2));

// --- NODE DEFINITIONS ----------------------------------------------------

// glass sphere.
Sphere s;
s = new Sphere();
s.SetAttribute(PropertyName.MATERIAL, pm);
root.InsertChild(s, Matrix4d.Identity);

ISolid c;

// The front row.
c = new Cube();
root.InsertChild(c, Matrix4d.RotateY(0.6) * Matrix4d.CreateTranslation(0.5, -0.8, 0.0));
c.SetAttribute(PropertyName.MATERIAL, pm);

// Infinite plane with checker.
Plane pl = new Plane();
pl.SetAttribute(PropertyName.COLOR, new double[] { 0.3, 0.0, 0.0 });
pl.SetAttribute(PropertyName.TEXTURE, new CheckerTexture(0.6, 0.6, new double[] { 1.0, 1.0, 1.0 }));
root.InsertChild(pl, Matrix4d.RotateX(-MathHelper.PiOver2) * Matrix4d.CreateTranslation(0.0, -1.0, 0.0));



/*using System;
using System.Collections.Generic;
//using VoronoiLib;
/// <summary>
/// Summary description for ShatteredGlass class
/// General idea - generate random points in 2D space (for now 90, could be variable depending on dimensions of glass
/// and "strenght" (which could have been how you hold the mouse, but thats to much time consuming for rn)). Three different areas of points:
/// first - closest to the point of hit, should be 45 points, second - midrange of the hit, should be 30 points, third - the rest of the glass to
/// make gradual pattern. After points being generated, we use these points as anchors for Voronoi diagram, which in my opinion pretty much resembles
/// how glass could be shattered
/// </summary>
public class ShatteredGlass
{
    
    /// <summary>
    /// generates so far 90 points, could be variable as well
    /// </summary>
    /// <param name="x"></param>
    /// <param name="minX"></param>
    /// <param name="maxX"></param>
    /// <param name="minY"></param>
    /// <param name="maxY"></param>
    private List<Point> GeneratePoints(Point x, double minX, double maxX, double minY, double maxY)
    {
      List<Point> randomPoints = new List<Point>();
      Random rndX = new Random((int)(x.X * 100));
      Random rndY = new Random((int)(x.Y * 100));
      //thresholds for three areas, the last one is using min&max from inicialization of the function,
      //although in this case its not going to be used, because I am working with simplification, where
      //the dimeinsions of the glass is 1x1, which makes all the random double values fall into this range
      double firstLx = x.X - 0.1;
      if (firstLx < 0.0) firstLx = 0.0;
      double firstHx = x.X + 0.1;
      if (firstHx > 1.0) firstHx = 1.0;
      double firstLy = x.Y - 0.1;
      if (firstLy < 0.0) firstLy = 0.0;
      double firstHy = x.Y + 0.1;
      if (firstHy > 1.0) firstHy = 1.0;
      double secondLx = x.X - 0.3;
      if(secondLx < 0.0) secondLx = 0.0;
      double secondHx = x.X + 0.3;
      if(secondHx > 1.0) secondHx = 1.0;
      double secondLy = x.Y - 0.3;
      if(secondLy < 0.0) secondLy = 0.0;
      double secondHy = x.Y + 0.3;
      if(secondHy > 1.0) secondHy = 1.0;
    int first = 0;
    int second = 0;
    int third = 0;
      while(randomPoints.len <= 150)
      {
        Point help = new Point(rndX.NextDouble(), rndY.NextDouble());
        if(firstLx < help.X < firstHx && firstLx < help.Y < firstHy && first < 46)
        {
          randomPoints.Add(help);
          first += 1;
        }
        else if(secondLx < help.X < secondHx && secondLx < help.Y < secondHy && second < 30)
        {
          randomPoints.Add(help);
          second += 1;
        }
        else if(third < 16)
        {
          randomPoints.Add(help);
          third += 1;
        }
      }
    return randomPoints;
    }
	public ShatteredGlass(self, Point pointOfTheHit)
	{
    List<Point> pointsForVoronoi = GeneratePoints(pointOfTheHit, 0.0, 1.0, 0.0, 1.0);
    FortuneSite s = new FortuneSite(0.5, 0.5);
    }
}*/



