# SAM
[![Build Status](https://hldigitalinnovation.visualstudio.com/HLApps/_apis/build/status/SAM-Deploy-SAM?branchName=master)](https://hldigitalinnovation.visualstudio.com/HLApps/_build/latest?definitionId=18&branchName=master)


Currently tested on Rhino 6 (6.32.20340.21001), Rhino 7 and Revit 2020, 2021

To install SAM from .exe just down load run latest version 
https://github.com/HoareLea/SAM_Deploy/releases


SAM  Core

Panel-> | PlanarBoundary3D-> | Face3D-> | Polygon3D 
------------ | ------------- | ------------- | -------------
*SAM.Analytical* | *SAM.Analytical* | *SAM.Geometry* | *SAM.Geometry*
PlanarBoundary3D  | BoundaryEdges2DLoop | Edge IClosed2D+Plane | Closed
N/A  | BoundaryEdge2D | ExternalEdge IClosed2D+Plane | Planar
N/A  | N/A| InternalEdge IClosed2D+Plane | Segmentable

Test
