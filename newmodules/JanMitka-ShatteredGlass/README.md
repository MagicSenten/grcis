# Extension: ShatteredGlass/ShatteredTexture
### Autor: Jan Mitka
### Category: RT extension

Deterministické generování popraskáné textury. Chtěl jsem dosáhnout simulace popraskání skla,bohužel se mi nepodařilo nastavit materiál TriangleMesh na sklo. Tudíž výsledná verze je zobrazena na žluté krychli. 

Jak jsem docílil determinismu - Bod úderu (přesněji x  a y souřadnice) je seed pro dvě random funkce, které generují random doubly - r1 a r2. Ty poté považuji za bod [r1, r2] ve čtverci 1x1. Nasbírám takto 40 bodů (přičemž se snažím udělat větší zahuštění těchto random bodů okolo bodu úderu, aby roztříštění vypadalo co nejvěrohodněji), podle kterých vypočítám Voronoiovy křivky. Ty poté vnáším pomocí TraingleMesh do 3D scény, přesněji řečeno do krychle. 

Vygenerování random bodů a následných Voronoiových křivek se stará třída ``ShatteredGlass`` , inicializuje se pomocí "bodu úderu" (místo, kde má být "pavučina" nejhustší). Třída poté umožňuje přistup k vygenerovaným Voronoivým křivkám.

Příklad inicializace třídy
```
Point ah = new Point();
ah.setPoint(0.6, 0.45);
ShatteredGlass tryMe = new ShatteredGlass(ah);
```

Příklad následného přistupování k jendotlivým Voronoiovým hranám
```
foreach (var vorLine in tryMe.ge)
{
  triMesh = new TriangleMesh(Generate3DLine((float)vorLine.x1, (float)vorLine.x2, (float)vorLine.y1, (float)vorLine.y2));
  root.InsertChild(triMesh, Matrix4d.CreateTranslation(0.0, 0.0, 0.0));
  triMesh.SetAttribute(PropertyName.MATERIAL, pm);
}
```
