[![Build (Windows)](https://github.com/SAM-BIM/SAM/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/SAM-BIM/SAM/actions/workflows/build.yml)
[![Installer (latest)](https://img.shields.io/github/v/release/SAM-BIM/SAM_Deploy?label=installer)](https://github.com/SAM-BIM/SAM_Deploy/releases/latest)

# SAM

<a href="https://github.com/SAM-BIM/SAM">
  <img src="https://raw.githubusercontent.com/SAM-BIM/SAM/master/Grasshopper/SAM.Core.Grasshopper/Resources/SAM_Small.png"
       align="left" hspace="10" vspace="6">
</a>

**SAM (Sustainable Analytical Model)** is the core of the **SAM Toolkit** —  
an open-source analytical-model and computational-engineering platform that
connects BIM, building physics, HVAC engineering and simulation through
deterministic, tested and traceable workflows.

SAM provides a structured analytical representation of buildings,
supporting workflows for energy modelling, systems analysis,
environmental simulation, and performance-driven design.

It is designed as a **modular and extensible platform**:
core analytical concepts are implemented in this repository,
with additional functionality provided through specialised SAM modules
and integrations. SAM is **not a Grasshopper plugin** — Grasshopper,
Rhino and Revit are hosts and adapters on top of the analytical model,
not its foundation.

**Website:** https://sambim.xyz ·
**Installer:** [SAM_Deploy releases](https://github.com/SAM-BIM/SAM_Deploy/releases/latest) ·
**Documentation:** [SAM Wiki](https://github.com/SAM-BIM/SAM/wiki)

<!--
Screenshot strip (pending evidence captures — do not uncomment until the
files exist in the website repository; see assets/evidence/README.md in
SAM-BIM/sam-bim.github.io):
<p>
  <img src="https://sambim.xyz/assets/evidence/E06_SAM_Analytical_Model_Context.png" alt="SAM analytical model in context" width="32%">
  <img src="https://sambim.xyz/assets/evidence/E01_SAM_PartF_Design_Airflow_Overlay.png" alt="Part F design airflow overlay" width="32%">
  <img src="https://sambim.xyz/assets/evidence/E04_SAM_PartO_TM59_Result.png" alt="TM59 assessment result" width="32%">
</p>
-->

---

## What SAM provides

At its core, SAM enables:

- creation and management of analytical building models  
- assignment of constructions, loads, and system definitions  
- preparation of models for simulation and analysis  
- orchestration of analytical workflows and scenarios  
- integration with external tools and simulation engines  

SAM supports both **programmatic** and **visual** workflows,
including integration with environments such as **Grasshopper**, **Rhino**, and **Revit**.

---

## Extensibility

The SAM platform is intentionally modular.
Additional repositories provide functionality such as:

- simulation engine integrations (e.g. Tas, OpenStudio)
- data exchange formats (IFC, gbXML, GEM)
- environmental and physical calculations (psychrometrics, solar, acoustics)
- UI layers and scripting interfaces (Windows UI, Rhino, Python)
- experimental and research workflows

The full ecosystem, module descriptions, and relationships
are documented in the **SAM Wiki**.

---

## Getting started

To install **SAM**, download and run the  
[latest Windows installer](https://github.com/SAM-BIM/SAM_Deploy/releases/latest).

Alternatively, the toolkit can be built from source using Visual Studio.
See the documentation in the **SAM Wiki** for setup guidance and build details.

---

## Documentation

📘 **SAM Wiki:**  
https://github.com/SAM-BIM/SAM/wiki

The Wiki contains:
- module overviews and relationships  
- build and dependency information  
- workflow examples  
- developer and contributor guidance  

---

## Licence

This repository is free software licensed under the  
**GNU Lesser General Public License v3.0 or later (LGPL-3.0-or-later)**.

Each contributor retains copyright to their respective contributions.  
The project history (Git) records authorship and provenance of all changes.

See:
- `LICENSE`
- `NOTICE`
- `COPYRIGHT_HEADER.txt`
