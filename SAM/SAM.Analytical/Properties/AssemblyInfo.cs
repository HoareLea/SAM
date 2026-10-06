// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Runtime.CompilerServices;

//Lets SAM.Tests reach the internal-only test seam on VentilationUnitPerformanceTable that makes its
//reload/cache generation race deterministically reproducible without sleep-based timing.
[assembly: InternalsVisibleTo("SAM.Tests")]
