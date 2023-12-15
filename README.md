# SAM

[![Build Status](https://hldigitalinnovation.visualstudio.com/HLApps/_apis/build/status/SAM-Deploy-SAM?branchName=master)](https://hldigitalinnovation.visualstudio.com/HLApps/_build/latest?definitionId=18&branchName=master)

<p align="center">
  <img src="https://github.com/HoareLea/SAM/blob/master/Grasshopper/SAM.Core.Grasshopper/Resources/SAM_Small.png" alt="SAM Logo" align="left" hspace="10" vspace="6">
</p>

**Sustainable Analytical Model (SAM)** is an open-source software designed to help engineers create analytical models of energy-efficient buildings. It enables the creation and analysis of energy models of buildings, including heating, cooling, ventilation, and lighting.

SAM allows users to create energy models in a variety of ways, including importing building geometry from common architectural software such as Revit or gbXML, manually inputting building geometry, or generating geometry using SAM's built-in tools. Once the geometry is defined, users can assign construction properties, internal loads, and system configurations to the model.

SAM uses an energy simulation engine to predict the energy performance of the building. The simulation results can be used to compare different design scenarios, assess compliance with building codes and standards, and evaluate the effectiveness of energy-saving strategies.

SAM also includes several tools for post-processing and visualizing the simulation results. Users can view the results in various formats, including graphs, tables, and 3D models, and export the data for further analysis.

SAM is designed to be extensible, and users can add new features and functionalities by developing custom plugins. It is integrated with several third-party tools and frameworks, including Ladybug Tools, BHoM, and Topologic, which enhance its capabilities and enable more advanced workflows.

## Getting Started

To install **SAM**, download and run the latest installer from the [releases](https://github.com/HoareLea/SAM_Deploy/releases) page or rebuild using Visual Studio from the [SAM repository](https://github.com/HoareLea/SAM).

## Contributing

We welcome contributions from anyone who would like to help improve SAM. For more information on how to contribute, please see the [contributing guidelines](CONTRIBUTING.md).

## Resources

For more information about SAM, please visit our [wiki](https://github.com/HoareLea/SAM/wiki).

## License

SAM is free software licensed under the GNU Lesser General Public License. Each contributor holds a copyright over their respective contributions. The project versioning (Git) records all such contribution source information. See [LICENSE](https://github.com/HoareLea/SAM_gbXML/blob/master/LICENSE) and [COPYRIGHT_HEADER](https://github.com/HoareLea/SAM/blob/master/COPYRIGHT_HEADER.txt) for more details.

## Included Repositories that link with SAM

1. https://github.com/HoareLea/SAM
2. https://github.com/HoareLea/SAM_BHoM
3. https://github.com/HoareLea/SAM_Excel
4. https://github.com/HoareLea/SAM_gbXML
5. https://github.com/HoareLea/SAM_GEM
6. https://github.com/HoareLea/SAM_IFC
7. https://github.com/HoareLea/SAM_LadybugTools
8. https://github.com/HoareLea/SAM_Mollier
9. https://github.com/HoareLea/SAM_OpenStudio
10. https://github.com/HoareLea/SAM_Psychrometrics
11. https://github.com/HoareLea/SAM_Revit
12. https://github.com/HoareLea/SAM_Revit_UI
12. https://github.com/HoareLea/SAM_Rhino_UI
14. https://github.com/HoareLea/SAM_SQLite
15. https://github.com/HoareLea/SAM_SolarCalculator
16. https://github.com/HoareLea/SAM_Systems
17. https://github.com/HoareLea/SAM_Tas
18. https://github.com/HoareLea/SAM_Topologic
19. https://github.com/HoareLea/SAM_UI
20. https://github.com/HoareLea/SAM_Windows


## Build order

1. SAM.sln
2. SAM_Systems
3. SAM_Psychrometrics.sln
4. SAM_Mollier.sln
5. SAM_Windows.sln
6. SAM_IFC
7. SAM_Topologic.sln
8. SAM_Acoustic.sln
9. SAM_BHom.sln
10. SAM_gbXML.sln
11. SAM_GEM.sln
12. SAM_LadybugTools.sln
13. SAM_Solver.sln
14. SAM_SolarCalculator.sln
15. SAM_Tas.sln
16. SAM_Excel.sln
17. SAM_SQLite.sln
18. SAM_OpenStudio.sln
19. SAM_Origin.sln
20. SAM_Revit.sln (Release2020)
21. SAM_Revit.sln (Release2021)
22. SAM_Revit.sln (Release2022)
23. SAM_Revit.sln (Release2023)
24. SAM_Revit.sln (Release2024)
25. SAM_UI.sln
26. SAM_Rhino_UI.sln
27. SAM_Revit_UI.sln (Release2020)
28. SAM_Revit_UI.sln (Release2021)
29. SAM_Revit_UI.sln (Release2022)
30. SAM_Revit_UI.sln (Release2023)
31. SAM_Revit_UI.sln (Release2024)
