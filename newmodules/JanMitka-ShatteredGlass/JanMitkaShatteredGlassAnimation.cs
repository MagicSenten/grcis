//////////////////////////////////////////////////
// Rendering params.
using System;
using System.Collections.Generic;
using Scene3D;
using Voronoi2;


Debug.Assert(scene != null);
Debug.Assert(context != null);

// Override image resolution and supersampling.
context[PropertyName.CTX_WIDTH] = 640; // 1800, 1920
context[PropertyName.CTX_HEIGHT] = 480; // 1200, 1080
//context[PropertyName.CTX_SUPERSAMPLING] =   4; //  400,   64
context[PropertyName.CTX_START_ANIM] = 0.0;
context[PropertyName.CTX_END_ANIM] = 20.0;
context[PropertyName.CTX_FPS] = 25.0;


// Params dictionary.
Dictionary<string, string> p = Util.ParseKeyValueList(param);

// schlick = <schlick-coeff>
double sch = 1.0;

// n = <index-of-refraction>
double n = 1.6;

//glass material
PhongMaterial pm = new PhongMaterial(new double[] {0.0, 0.2, 0.1}, 0.05, 0.05, 0.1, 128, sch);
pm.n = n;
pm.Kt = 0.9;


/// <summary>
/// Summary description for ShatteredGlass class
/// General idea - generate random points in 2D space (for now 40, could be variable depending on dimensions of glass
/// and "strenght" (which could have been how you hold the mouse, but thats to much time consuming for rn)). Three different areas of points:
/// first - closest to the point of hit, should be 20 points, second - midrange of the hit, should be 10 points, third - the rest of the glass to
/// make gradual pattern. After points being generated, we use these points as anchors for Voronoi diagram, which in my opinion pretty much resembles
/// how glass could be shattered
/// </summary>
class ShatteredGlass
{
  public List<GraphEdge> ge = new List<GraphEdge>();
  /// <summary>
  /// generates so far 40 points, could be variable as well
  /// </summary>
  /// <param name="x"></param>
  /// <param name="minX"></param>
  /// <param name="maxX"></param>
  /// <param name="minY"></param>
  /// <param name="maxY"></param>
  private List<Point> GeneratePoints (Point x, double minX, double maxX, double minY, double maxY)
  {
    List<Point> randomPoints = new List<Point>();
    Random rndX = new Random((int)(x.x * 100));
    Random rndY = new Random((int)(x.y * 100));
    //thresholds for three areas, the last one is using min&max from inicialization of the function,
    //although in this case its not going to be used, because I am working with simplification, where
    //the dimeinsions of the glass is 1x1, which makes all the random double values fall into this range
    double firstLx = x.x - 0.1;
    if (firstLx < 0.0)
      firstLx = 0.0;
    double firstHx = x.x + 0.1;
    if (firstHx > 1.0)
      firstHx = 1.0;
    double firstLy = x.y - 0.1;
    if (firstLy < 0.0)
      firstLy = 0.0;
    double firstHy = x.y + 0.1;
    if (firstHy > 1.0)
      firstHy = 1.0;
    double secondLx = x.x - 0.3;
    if (secondLx < 0.0)
      secondLx = 0.0;
    double secondHx = x.x + 0.3;
    if (secondHx > 1.0)
      secondHx = 1.0;
    double secondLy = x.y - 0.3;
    if (secondLy < 0.0)
      secondLy = 0.0;
    double secondHy = x.y + 0.3;
    if (secondHy > 1.0)
      secondHy = 1.0;
    int first = 0;
    int second = 0;
    int third = 0;
    while (randomPoints.Count < 40)
    {
      Voronoi2.Point help = new Point();
      help.setPoint(rndX.NextDouble(), rndY.NextDouble());
      if (firstLx < help.x && help.x < firstHx && firstLx < help.y && help.y < firstHy && first < 22)
      {
        randomPoints.Add(help);
        first += 1;
      }
      else if (secondLx < help.x && help.x < secondHx && secondLx < help.y && help.y < secondHy && second < 11)
      {
        randomPoints.Add(help);
        second += 1;
      }
      else if (third < 11)
      {
        randomPoints.Add(help);
        third += 1;
      }
    }
    return randomPoints;
  }

  public ShatteredGlass (Point pointOfTheHit)
  {
    List<Point> pointsForVoronoi = GeneratePoints(pointOfTheHit, 0.0, 1.0, 0.0, 1.0);
    Voronoi voroObject = new Voronoi(0.1);
    double[] xVal = new double[40];
    double[] yVal = new double[40];
    for (int i = 0; i < 40; i++)
    {
      xVal[i] = pointsForVoronoi[i].x;
      yVal[i] = pointsForVoronoi[i].y;
    }
    ge = voroObject.generateVoronoi(xVal, yVal, 0, 1.0, 0, 1.0);
  }
}

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
scene.Camera = new StaticCamera(new Vector3d(0.7, -0.5, -5.0),
                                new Vector3d(0.0, -0.18, 1.0),
                                50.0);

// Light sources.
scene.Sources = new System.Collections.Generic.LinkedList<ILightSource>();
scene.Sources.Add(new AmbientLightSource(0.8));
scene.Sources.Add(new PointLightSource(new Vector3d(-5.0, 4.0, -3.0), 1.2));

// --- NODE DEFINITIONS ----------------------------------------------------
/*PhongMaterial sds  = new PhongMaterial(new double[] { 0.5, 0.5, 0.5 }, 0.1, 0.6, 0.3, 16, sch);

ISolid c;

// glass cube, if the TriangleMesh material starts working
c = new Cube();
root.InsertChild(c, Matrix4d.RotateY(0.0) * Matrix4d.CreateTranslation(0.5, -1.0, -2.0));
c.SetAttribute(PropertyName.MATERIAL, sds);
//c.SetAttribute(PropertyName.MATERIAL, pm);*/

// Infinite plane with checker.
Plane pl = new Plane();
pl.SetAttribute(PropertyName.COLOR, new double[] { 0.3, 0.0, 0.0 });
pl.SetAttribute(PropertyName.TEXTURE, new CheckerTexture(0.6, 0.6, new double[] { 1.0, 1.0, 1.0 }));
root.InsertChild(pl, Matrix4d.RotateX(-MathHelper.PiOver2) * Matrix4d.CreateTranslation(0.0, -1.0, 0.0));

ISolid triMesh;


//if the material of the TriangleMesh worked, I would have used this function for the lines for
//the shattering effect
public SceneBrep GenerateSceneBrepForMest (float x1, float x2, float y1, float y2)
{
  SceneBrep sceneBr = new SceneBrep();
  int[] v = new int[8];
  v[0] = sceneBr.AddVertex(new Vector3(0.5f + x2, -1.0f + y2, -0.9999f));
  v[1] = sceneBr.AddVertex(new Vector3(0.5f + x1, -1.0f + y1, -0.9999f));
  v[2] = sceneBr.AddVertex(new Vector3(0.505f + x1, -1.0f + y1, -0.9999f));
  v[3] = sceneBr.AddVertex(new Vector3(0.505f + x2, -1.0f + y2, -0.9999f));
  v[4] = sceneBr.AddVertex(new Vector3(0.5f + x2, -1.0f + y2, -2.0001f));
  v[5] = sceneBr.AddVertex(new Vector3(0.5f + x1, -1.0f + y1, -2.0001f));
  v[6] = sceneBr.AddVertex(new Vector3(0.505f + x1, -1.0f + y1, -2.0001f));
  v[7] = sceneBr.AddVertex(new Vector3(0.505f + x2, -1.0f + y2, -2.0001f));
  //predni cast
  sceneBr.AddTriangle(v[0], v[1], v[2]);
  sceneBr.AddTriangle(v[0], v[2], v[3]);
  //pravy bok
  sceneBr.AddTriangle(v[2], v[3], v[6]);
  sceneBr.AddTriangle(v[6], v[3], v[7]);
  //zadni cast
  sceneBr.AddTriangle(v[6], v[7], v[5]);
  sceneBr.AddTriangle(v[7], v[5], v[4]);
  //levy bok
  sceneBr.AddTriangle(v[5], v[4], v[1]);
  sceneBr.AddTriangle(v[4], v[1], v[0]);
  //vrch
  sceneBr.AddTriangle(v[0], v[4], v[3]);
  sceneBr.AddTriangle(v[4], v[3], v[7]);
  //spodek
  sceneBr.AddTriangle(v[1], v[2], v[5]);
  sceneBr.AddTriangle(v[2], v[5], v[6]);

  return sceneBr;
}


//function which generates 1x1x1 cube at position [0.5,0.0,-2.0]
//can be easily modified to any position, size and color - created this function, because
//I wasnt able to create cube without glass material
public SceneBrep GenerateSceneBrepForCube ()
{
  Vector3 yellow = new Vector3(0.5f, 0.5f, 0.5f);
  SceneBrep brepForCube = new SceneBrep();
  int[] v = new int[8];
  v[0] = brepForCube.AddVertex(new Vector3(0.5f, -1.0f, -2.0f));
  v[1] = brepForCube.AddVertex(new Vector3(0.5f, 0.0f, -2.0f));
  v[2] = brepForCube.AddVertex(new Vector3(1.5f, 0.0f, -2.0f));
  v[3] = brepForCube.AddVertex(new Vector3(1.5f, -1.0f, -2.0f));
  v[4] = brepForCube.AddVertex(new Vector3(0.5f, -1.0f, -1.0f));
  v[5] = brepForCube.AddVertex(new Vector3(0.5f, 0.0f, -1.0f));
  v[6] = brepForCube.AddVertex(new Vector3(1.5f, 0.0f, -1.0f));
  v[7] = brepForCube.AddVertex(new Vector3(1.5f, -1.0f, -1.0f));
  brepForCube.SetColor(v[0], yellow);
  brepForCube.SetColor(v[1], yellow);
  brepForCube.SetColor(v[2], yellow);
  brepForCube.SetColor(v[3], yellow);
  brepForCube.SetColor(v[4], yellow);
  brepForCube.SetColor(v[5], yellow);
  brepForCube.SetColor(v[6], yellow);
  brepForCube.SetColor(v[7], yellow);
  //front
  brepForCube.AddTriangle(v[0], v[1], v[2]);
  brepForCube.AddTriangle(v[0], v[2], v[3]);
  //right side
  brepForCube.AddTriangle(v[3], v[2], v[6]);
  brepForCube.AddTriangle(v[3], v[6], v[7]);
  //back side
  brepForCube.AddTriangle(v[7], v[6], v[5]);
  brepForCube.AddTriangle(v[7], v[5], v[4]);
  //left side
  brepForCube.AddTriangle(v[4], v[5], v[0]);
  brepForCube.AddTriangle(v[1], v[0], v[5]);
  //top
  brepForCube.AddTriangle(v[1], v[5], v[2]);
  brepForCube.AddTriangle(v[2], v[5], v[6]);
  //bottom
  brepForCube.AddTriangle(v[0], v[3], v[4]);
  brepForCube.AddTriangle(v[3], v[4], v[7]);

  return brepForCube;
}


//function creates slices from Voronoi lines, they are off 0.01, so they can be visible
//on a different material than glass
//couldn't figure out how to make TriangleMesh with glass material (setAttribude didn't work)
//still wanted to show my deterministic shattering effect, so I came up with this
public SceneBrep Generate3DLine (float x1, float x2, float y1, float y2)
{
  /*this part is to show the rupture on a different material than glass
  if (x1 == 0)
    x1 -= 0.01f;
  if (x2 == 0)
    x2 = 0.01f;
  if (x1 == 1)
    x1 += 0.01f;
  if (x2 == 1)
    x2 += 0.01f;
  if (y1 == 1)
    y1 += 0.01f;
  if (y2 == 1)
    y2 += 0.01f;
  until here*/
  SceneBrep sceneBr = new SceneBrep();
  int[] v = new int[4];
  v[0] = sceneBr.AddVertex(new Vector3(0.5f + x1, -1.0f + y1, -0.99f));
  v[1] = sceneBr.AddVertex(new Vector3(0.5f + x2, -1.0f + y2, -0.99f));
  v[2] = sceneBr.AddVertex(new Vector3(0.5f + x2, -1.0f + y2, -2.01f));
  v[3] = sceneBr.AddVertex(new Vector3(0.5f + x1, -1.0f + y1, -2.01f));
  sceneBr.AddTriangle(v[0], v[1], v[2]);
  sceneBr.AddTriangle(v[0], v[2], v[3]);
  return sceneBr;
}


//generates yellow cube with TriangleMesh
triMesh = new TriangleMesh(GenerateSceneBrepForCube());
root.InsertChild(triMesh, Matrix4d.CreateTranslation(0.0, 0.0, 0.0));



//deterministic function based on point ah, which is supposed to be
//"the point of the hit"
Point ah = new Point();
ah.setPoint(0.6, 0.45);
ShatteredGlass tryMe = new ShatteredGlass(ah);

//creating TriangleMeshes for each Voronoi line
foreach (var vorLine in tryMe.ge)
{
  triMesh = new TriangleMesh(Generate3DLine((float)vorLine.x1, (float)vorLine.x2, (float)vorLine.y1, (float)vorLine.y2));
  root.InsertChild(triMesh, Matrix4d.CreateTranslation(0.0, 0.0, 0.0));
  triMesh.SetAttribute(PropertyName.MATERIAL, pm);
}









