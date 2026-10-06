// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Deciding which systems meet an Approved Document F requirement, and taking the one a supplier's own
    /// preference order puts first.
    /// <para>
    /// <b>Only the analytical half is here, and that is the design.</b> <c>SAM.Analytical</c> owns the
    /// capability vocabulary, the rule that reads a requirement off a Part F assessment, and the rule that
    /// decides <b>suitability</b>. It does not own preference: which of several suitable systems is the
    /// right answer is a judgement about a particular library of templates, and the assembly that ships
    /// them is the one that knows. So nothing here names a template file, and the descriptors below are a
    /// <b>test fixture</b> standing in for the <c>SAM_Systems</c> catalogue, not a copy of it.
    /// </para>
    /// </summary>
    public class SystemCapabilitySelectionTests
    {
        // ---------------------------------------------------------------------------------------------
        // Suitability
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Every system that meets the requirement comes back, in the supplied preference order - so a
        /// caller with a different policy has the whole suitable set to apply it to rather than only this
        /// assembly's answer.
        /// </summary>
        [Fact]
        public void CapableSystems_ReturnsEverySuitableSystemInPreferenceOrder()
        {
            Assert.Equal(["NV", "MV", "MVRE"], Ventilations(Descriptors().CapableSystems(Requirement(SystemCapability.ContinuousVentilation))));
            Assert.Equal(["MV", "MVRE"], Ventilations(Descriptors().CapableSystems(Requirement(SystemCapability.ContinuousVentilation | SystemCapability.Boost))));
            Assert.Equal(["MVRE"], Ventilations(Descriptors().CapableSystems(Requirement(SystemCapability.ContinuousVentilation | SystemCapability.SummerBypass))));

            //A system that can do nothing is offered and is never suitable.
            Assert.DoesNotContain("UV", Ventilations(Descriptors().CapableSystems(Requirement(SystemCapability.ContinuousVentilation))));

            //Nothing required, nothing suitable - an empty requirement is not met by everything.
            Assert.Empty(Descriptors().CapableSystems(new SystemCapabilityRequirement()));
            Assert.Empty(Descriptors().CapableSystems(null));
            Assert.Empty(((List<SystemCapabilityDescriptor>)null).CapableSystems(Requirement(SystemCapability.ContinuousVentilation)));
        }

        /// <summary>
        /// Requiring boost removes every system that cannot boost; requiring summer bypass leaves only the
        /// heat-recovery system, because bypass is a state of a heat exchanger and there is nothing to
        /// bypass without one.
        /// </summary>
        [Fact]
        public void RequiredCapabilities_ExcludeSystemsThatLackThem()
        {
            Assert.Equal("MV", Descriptors().SelectPreferredCapableSystem(Requirement(SystemCapability.ContinuousVentilation | SystemCapability.Boost)).SystemTemplate?.Ventilation);
            Assert.Equal("MVRE", Descriptors().SelectPreferredCapableSystem(Requirement(SystemCapability.ContinuousVentilation | SystemCapability.SummerBypass)).SystemTemplate?.Ventilation);
        }

        // ---------------------------------------------------------------------------------------------
        // Preference
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// The preferred system is the suitable one the catalogue ranked first.
        /// </summary>
        [Fact]
        public void PreferredSystem_IsTheLowestRankedSuitableOne()
        {
            SystemCapabilitySelection systemCapabilitySelection = Descriptors().SelectPreferredCapableSystem(Requirement(SystemCapability.ContinuousVentilation));

            Assert.True(systemCapabilitySelection.IsSelected);
            Assert.Equal("NV", systemCapabilitySelection.SystemTemplate?.Ventilation);
            Assert.Null(systemCapabilitySelection.Reason);
        }

        /// <summary>
        /// <b>Rank decides, and extra capability does not.</b> An earlier revision chose the system with
        /// the fewest capabilities, on the reasoning that anything more implies plant nobody required -
        /// which is a policy about a particular library, not something that follows from Part F, and a
        /// capability a system happens to have may cost nothing to specify. Here the <b>more</b> capable
        /// system is ranked first and it wins, which the old rule could not have produced.
        /// </summary>
        [Fact]
        public void ExtraCapability_DoesNotMakeASystemLessPreferred()
        {
            List<SystemCapabilityDescriptor> systemCapabilityDescriptors =
            [
                new(Template("MVRE"), SystemCapability.ContinuousVentilation | SystemCapability.Boost | SystemCapability.SummerBypass | SystemCapability.HeatRecovery, 10),
                new(Template("NV"), SystemCapability.ContinuousVentilation, 20)
            ];

            Assert.Equal("MVRE", systemCapabilityDescriptors.SelectPreferredCapableSystem(Requirement(SystemCapability.ContinuousVentilation)).SystemTemplate?.Ventilation);
            Assert.Equal(["MVRE", "NV"], Ventilations(systemCapabilityDescriptors.CapableSystems(Requirement(SystemCapability.ContinuousVentilation))));
        }

        /// <summary>
        /// <b>The order the systems arrived in cannot change the answer.</b> A caller that built its list
        /// by enumerating a directory would otherwise let the file system decide an engineering question,
        /// and the answer would differ between two machines with the same library. Asserted over every
        /// rotation and the reverse of each.
        /// </summary>
        [Fact]
        public void SystemOrdering_CannotChangeTheSelection()
        {
            SystemCapabilityRequirement systemCapabilityRequirement = Requirement(SystemCapability.ContinuousVentilation | SystemCapability.Boost);

            List<SystemCapabilityDescriptor> systemCapabilityDescriptors = Descriptors();

            Assert.Equal("MV", systemCapabilityDescriptors.SelectPreferredCapableSystem(systemCapabilityRequirement).SystemTemplate?.Ventilation);

            for (int i = 0; i < systemCapabilityDescriptors.Count; i++)
            {
                List<SystemCapabilityDescriptor> systemCapabilityDescriptors_Rotated = [];

                for (int j = 0; j < systemCapabilityDescriptors.Count; j++)
                {
                    systemCapabilityDescriptors_Rotated.Add(systemCapabilityDescriptors[(i + j) % systemCapabilityDescriptors.Count]);
                }

                Assert.Equal("MV", systemCapabilityDescriptors_Rotated.SelectPreferredCapableSystem(systemCapabilityRequirement).SystemTemplate?.Ventilation);
                Assert.Equal(["MV", "MVRE"], Ventilations(systemCapabilityDescriptors_Rotated.CapableSystems(systemCapabilityRequirement)));

                systemCapabilityDescriptors_Rotated.Reverse();

                Assert.Equal("MV", systemCapabilityDescriptors_Rotated.SelectPreferredCapableSystem(systemCapabilityRequirement).SystemTemplate?.Ventilation);
                Assert.Equal(["MV", "MVRE"], Ventilations(systemCapabilityDescriptors_Rotated.CapableSystems(systemCapabilityRequirement)));
            }
        }

        /// <summary>
        /// <b>Two suitable systems at the same rank are a refusal, not a coin toss.</b> The catalogue has
        /// not said which is preferred, and breaking the tie on a name would let an alphabetical accident
        /// pick a building's plant. This is the same rule <c>SimulationSpaceMap</c> follows: refuse on
        /// ambiguity rather than resolve it by something nobody chose.
        /// </summary>
        [Fact]
        public void SuitableSystemsAtTheSameRank_AreRefusedNotGuessedAt()
        {
            List<SystemCapabilityDescriptor> systemCapabilityDescriptors =
            [
                new(Template("MV"), SystemCapability.ContinuousVentilation, 10),
                new(Template("NV"), SystemCapability.ContinuousVentilation, 10)
            ];

            SystemCapabilitySelection systemCapabilitySelection = systemCapabilityDescriptors.SelectPreferredCapableSystem(Requirement(SystemCapability.ContinuousVentilation));

            Assert.False(systemCapabilitySelection.IsSelected);
            Assert.Contains("has not said which is preferred", systemCapabilitySelection.Reason);

            //Both are still reported as suitable - the ambiguity is about preference, not about fitness,
            //and a caller that wants to choose differently can.
            Assert.Equal(2, systemCapabilityDescriptors.CapableSystems(Requirement(SystemCapability.ContinuousVentilation)).Count);

            //A tie at a rank that is not the lowest changes nothing.
            systemCapabilityDescriptors.Add(new SystemCapabilityDescriptor(Template("MVRE"), SystemCapability.ContinuousVentilation, 5));

            Assert.Equal("MVRE", systemCapabilityDescriptors.SelectPreferredCapableSystem(Requirement(SystemCapability.ContinuousVentilation)).SystemTemplate?.Ventilation);
        }

        /// <summary>
        /// <b>One system listed twice is not an ambiguity.</b> A duplicated entry has a single answer, and
        /// reporting it as "the catalogue has not said which is preferred" would send somebody looking for a
        /// preference defect instead of a duplicate one.
        /// </summary>
        [Fact]
        public void OneSystemListedTwice_IsNotAmbiguous()
        {
            List<SystemCapabilityDescriptor> systemCapabilityDescriptors =
            [
                new(Template("MVRE"), SystemCapability.ContinuousVentilation, 50),
                new(Template("MVRE"), SystemCapability.ContinuousVentilation, 50)
            ];

            SystemCapabilitySelection systemCapabilitySelection = systemCapabilityDescriptors.SelectPreferredCapableSystem(Requirement(SystemCapability.ContinuousVentilation));

            Assert.True(systemCapabilitySelection.IsSelected);
            Assert.Equal("MVRE", systemCapabilitySelection.SystemTemplate?.Ventilation);
        }

        /// <summary>
        /// <b>Ranks are compared, not subtracted.</b> Ranks come from a file, and <c>int.MaxValue</c> minus
        /// <c>int.MinValue</c> overflows to the wrong sign - which both mis-orders the list and can make
        /// <c>List.Sort</c> throw on an inconsistent comparator. A review found the lowest-ranked system
        /// finishing last, silently.
        /// </summary>
        [Fact]
        public void ExtremeRanks_AreOrderedCorrectly()
        {
            List<SystemCapabilityDescriptor> systemCapabilityDescriptors =
            [
                new(Template("AAA"), SystemCapability.ContinuousVentilation, int.MaxValue),
                new(Template("BBB"), SystemCapability.ContinuousVentilation, int.MinValue + 1),
                new(Template("CCC"), SystemCapability.ContinuousVentilation, 0)
            ];

            Assert.Equal(["BBB", "CCC", "AAA"], Ventilations(systemCapabilityDescriptors.CapableSystems(Requirement(SystemCapability.ContinuousVentilation))));
            Assert.Equal("BBB", systemCapabilityDescriptors.SelectPreferredCapableSystem(Requirement(SystemCapability.ContinuousVentilation)).SystemTemplate?.Ventilation);
        }

        /// <summary>
        /// <b>Mechanical supply is required exactly when the assessment sized supply terminals</b>, and a
        /// dwelling that has them is never offered an extract-only system. The defect a review found: a
        /// balanced design - paragraph 1.67, a supply terminal in every habitable room - was met by
        /// <c>Local Extract Only</c>, because extract-only does run continuously and can boost.
        /// </summary>
        [Fact]
        public void MechanicalSupply_IsRequiredWhenSupplyTerminalsWereSized()
        {
            PartFDwellingResult partFDwellingResult = new("Flat 1") { ContinuousDesignSystemRate_Lps = 21.0, TotalHighExtract_Lps = 39.0 };

            //Extract only - no supply terminals, so no mechanical supply is required.
            Assert.False(partFDwellingResult.PartFSystemCapabilityRequirement().Requires(SystemCapability.MechanicalSupply));

            partFDwellingResult.TotalSupply_Lps = 21.0;

            Assert.True(partFDwellingResult.PartFSystemCapabilityRequirement().Requires(SystemCapability.MechanicalSupply));

            //The high-rate supply on its own is enough, for a design recorded only at the boost condition.
            PartFDwellingResult partFDwellingResult_High = new("Flat 2") { ContinuousDesignSystemRate_Lps = 21.0, TotalHighSupply_Lps = 21.0 };

            Assert.True(partFDwellingResult_High.PartFSystemCapabilityRequirement().Requires(SystemCapability.MechanicalSupply));

            //And an extract-only system is then not even suitable.
            List<SystemCapabilityDescriptor> systemCapabilityDescriptors =
            [
                new(Template("EOL"), SystemCapability.ContinuousVentilation | SystemCapability.Boost, 20),
                new(Template("MV"), SystemCapability.ContinuousVentilation | SystemCapability.Boost | SystemCapability.MechanicalSupply, 40)
            ];

            Assert.Equal(["MV"], Ventilations(systemCapabilityDescriptors.CapableSystems(partFDwellingResult.PartFSystemCapabilityRequirement())));
        }

        // ---------------------------------------------------------------------------------------------
        // Refusal
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>Nothing capable means a refusal, never the nearest thing.</b> The result names the capability
        /// no system offered, so a report can say what the library was short of rather than that something
        /// unspecified went wrong.
        /// </summary>
        [Fact]
        public void NoCapableSystem_IsRefusedAndSaysWhatWasMissing()
        {
            List<SystemCapabilityDescriptor> systemCapabilityDescriptors =
            [
                new(Template("NV"), SystemCapability.ContinuousVentilation, 10),
                new(Template("MV"), SystemCapability.ContinuousVentilation | SystemCapability.Boost, 20)
            ];

            SystemCapabilitySelection systemCapabilitySelection = systemCapabilityDescriptors.SelectPreferredCapableSystem(Requirement(SystemCapability.ContinuousVentilation | SystemCapability.SummerBypass));

            Assert.False(systemCapabilitySelection.IsSelected);
            Assert.Null(systemCapabilitySelection.Descriptor);
            Assert.Null(systemCapabilitySelection.SystemTemplate);

            Assert.Equal(SystemCapability.SummerBypass, systemCapabilitySelection.Missing);
            Assert.Contains("SummerBypass", systemCapabilitySelection.Reason);
        }

        /// <summary>
        /// The harder refusal: every capability exists somewhere, just never together on one system. Saying
        /// nothing was missing would read as a contradiction, so the reason says the real thing.
        /// </summary>
        [Fact]
        public void CapabilitiesSplitAcrossSystems_AreRefusedWithATruthfulReason()
        {
            List<SystemCapabilityDescriptor> systemCapabilityDescriptors =
            [
                new(Template("NV"), SystemCapability.ContinuousVentilation, 10),
                new(Template("XX"), SystemCapability.SummerBypass, 20)
            ];

            SystemCapabilitySelection systemCapabilitySelection = systemCapabilityDescriptors.SelectPreferredCapableSystem(Requirement(SystemCapability.ContinuousVentilation | SystemCapability.SummerBypass));

            Assert.False(systemCapabilitySelection.IsSelected);
            Assert.Equal(SystemCapability.None, systemCapabilitySelection.Missing);
            Assert.Contains("No single system", systemCapabilitySelection.Reason);
        }

        /// <summary>
        /// An empty requirement is refused rather than answered with whatever is ranked first. Choosing a
        /// system nobody asked for is inventing a requirement, and the same goes for an empty or absent
        /// library.
        /// </summary>
        [Fact]
        public void NothingRequiredOrNothingOffered_IsRefused()
        {
            Assert.False(Descriptors().SelectPreferredCapableSystem(new SystemCapabilityRequirement()).IsSelected);
            Assert.False(Descriptors().SelectPreferredCapableSystem(null).IsSelected);

            SystemCapabilityRequirement systemCapabilityRequirement = Requirement(SystemCapability.ContinuousVentilation);

            Assert.False(new List<SystemCapabilityDescriptor>().SelectPreferredCapableSystem(systemCapabilityRequirement).IsSelected);
            Assert.False(((List<SystemCapabilityDescriptor>)null).SelectPreferredCapableSystem(systemCapabilityRequirement).IsSelected);

            //A descriptor naming no system is not a system, and a list of them is an empty library.
            List<SystemCapabilityDescriptor> systemCapabilityDescriptors = [null, new SystemCapabilityDescriptor(), new SystemCapabilityDescriptor(null, SystemCapability.ContinuousVentilation)];

            Assert.False(systemCapabilityDescriptors.SelectPreferredCapableSystem(systemCapabilityRequirement).IsSelected);
        }

        // ---------------------------------------------------------------------------------------------
        // The Part F rule
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// A sized dwelling requires continuous ventilation, and requires boost exactly when the balanced
        /// high-rate extract exceeds the continuous design rate.
        /// </summary>
        [Fact]
        public void PartFRequirement_IsContinuousAndBoostWhenTheHighRateExceedsIt()
        {
            PartFDwellingResult partFDwellingResult = new("Flat 1") { ContinuousDesignSystemRate_Lps = 21.0, TotalHighExtract_Lps = 19.0 };

            Assert.Equal(SystemCapability.ContinuousVentilation, partFDwellingResult.PartFSystemCapabilityRequirement().Capabilities);

            partFDwellingResult.TotalHighExtract_Lps = 39.0;

            Assert.Equal(SystemCapability.ContinuousVentilation | SystemCapability.Boost, partFDwellingResult.PartFSystemCapabilityRequirement().Capabilities);

            //Equal rates are not a demand for extra plant.
            partFDwellingResult.TotalHighExtract_Lps = 21.0;

            Assert.False(partFDwellingResult.PartFSystemCapabilityRequirement().Requires(SystemCapability.Boost));
        }

        /// <summary>
        /// <b>Part F never asks for summer bypass or heat recovery, whatever the dwelling looks like.</b>
        /// Those are mitigation an Approved Document O scenario states, and a dwelling credited with
        /// mitigation its design does not have would pass an overheating assessment it should fail. Swept
        /// over a range of dwellings so it cannot pass on one convenient case.
        /// </summary>
        [Fact]
        public void PartFRequirement_NeverAsksForMitigation()
        {
            foreach (double continuous in new[] { 0.0, 0.0005, 13.0, 21.0, 250.0 })
            {
                foreach (double high in new[] { 0.0, 13.0, 21.0, 39.0, 600.0 })
                {
                    SystemCapabilityRequirement systemCapabilityRequirement = new PartFDwellingResult("Flat 1") { ContinuousDesignSystemRate_Lps = continuous, TotalHighExtract_Lps = high }.PartFSystemCapabilityRequirement();

                    Assert.False(systemCapabilityRequirement.Requires(SystemCapability.SummerBypass));
                    Assert.False(systemCapabilityRequirement.Requires(SystemCapability.HeatRecovery));
                }
            }
        }

        /// <summary>
        /// An unsized dwelling requires nothing, and a null one does not throw. A requirement of nothing is
        /// then refused by the selector rather than answered - so an unsized dwelling cannot acquire a
        /// system by accident.
        /// </summary>
        [Fact]
        public void UnsizedDwelling_RequiresNothingAndSelectsNothing()
        {
            SystemCapabilityRequirement systemCapabilityRequirement = new PartFDwellingResult("Flat 1").PartFSystemCapabilityRequirement();

            Assert.False(systemCapabilityRequirement.IsValid);
            Assert.Equal(SystemCapability.None, systemCapabilityRequirement.Capabilities);
            Assert.False(Descriptors().SelectPreferredCapableSystem(systemCapabilityRequirement).IsSelected);

            Assert.False(((PartFDwellingResult)null).PartFSystemCapabilityRequirement().IsValid);
        }

        /// <summary>End to end: a Part F assessment picks a system, through the two rules and nothing else.</summary>
        [Fact]
        public void PartFAssessment_SelectsASystemEndToEnd()
        {
            PartFDwellingResult partFDwellingResult = new("Flat 1") { ContinuousDesignSystemRate_Lps = 21.0, TotalHighExtract_Lps = 39.0 };

            Assert.Equal("MV", Descriptors().SelectPreferredCapableSystem(partFDwellingResult.PartFSystemCapabilityRequirement()).SystemTemplate?.Ventilation);
        }

        // ---------------------------------------------------------------------------------------------
        // The boundary
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>Nothing here can open a template.</b> Choosing a system must not cost the megabyte and a half
        /// a <c>SystemEnergyCentre</c> weighs, and the way that is guaranteed is that this side of the
        /// boundary has no way to read one: no member of the selection types names a file, a path, a
        /// directory or an energy centre, and the systems available are an argument rather than something
        /// looked up.
        /// </summary>
        [Fact]
        public void SelectionTypes_CannotReachATemplate()
        {
            string[] forbidden = ["file", "path", "directory", "load", "energycentre", "resource", "stream", "read"];

            foreach (Type type in new[] { typeof(SystemCapabilityDescriptor), typeof(SystemCapabilityRequirement), typeof(SystemCapabilitySelection) })
            {
                foreach (MemberInfo memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    foreach (string text in forbidden)
                    {
                        Assert.False(memberInfo.Name.ToLowerInvariant().Contains(text), string.Format("{0} declares '{1}', which names {2}. Choosing a system must not be able to open a template.", type.Name, memberInfo.Name, text));
                    }
                }
            }
        }

        /// <summary>
        /// <b>And the capability values do not live in this assembly.</b> Which of the shipped templates
        /// provides what is a fact about <c>SAM_Systems</c>' resources; a default catalog here would put
        /// the names of another repository's files in the core library and would have to be kept in step
        /// with them by hand. Asserted as the absence of any static member handing out descriptors.
        /// </summary>
        [Fact]
        public void NoDefaultCatalog_LivesInTheAnalyticalAssembly()
        {
            foreach (Type type in typeof(SystemCapabilityDescriptor).Assembly.GetTypes())
            {
                foreach (MemberInfo memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    Type type_Returned = (memberInfo as PropertyInfo)?.PropertyType ?? (memberInfo as FieldInfo)?.FieldType;

                    if (type_Returned == null)
                    {
                        continue;
                    }

                    bool descriptors = type_Returned == typeof(SystemCapabilityDescriptor)
                        || type_Returned == typeof(SystemCapabilityDescriptor[])
                        || type_Returned == typeof(List<SystemCapabilityDescriptor>)
                        || type_Returned == typeof(IEnumerable<SystemCapabilityDescriptor>);

                    Assert.False(descriptors, string.Format("{0}.{1} is a static system-capability catalog. The values belong beside the templates in SAM_Systems, not here.", type.FullName, memberInfo.Name));
                }
            }
        }

        /// <summary>
        /// <c>MVRE</c> stays the heat-recovery identity and nothing is added beside it. Heat recovery is a
        /// <b>capability</b> of that system, so the distinction between it and <c>MV</c> is made without a
        /// second name for one concept.
        /// </summary>
        [Fact]
        public void HeatRecovery_IsACapabilityOfMvreAndNotASecondIdentity()
        {
            Assert.DoesNotContain("MVHR", Enum.GetNames(typeof(SystemCapability)));

            Assert.Equal("MVRE", Descriptors().SelectPreferredCapableSystem(Requirement(SystemCapability.ContinuousVentilation | SystemCapability.Boost | SystemCapability.HeatRecovery)).SystemTemplate?.Ventilation);
        }

        // ---------------------------------------------------------------------------------------------
        // Serialisation
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// The requirement round-trips, because it crosses an assembly boundary. Capabilities are written by
        /// <b>name</b>, so adding a member cannot renumber a stored file, and a name a build does not know is
        /// ignored rather than turned into a bit nothing understands.
        /// <para>
        /// <b>The descriptor deliberately does not round-trip.</b> It used to, as a <c>Capabilities</c>
        /// string array, while the shipped catalogue is written as named booleans a person can audit - two
        /// formats for one concept, and a review showed the descriptor's own reader turning a real index
        /// entry into a confident, empty descriptor. The index is the wire format and the assembly that owns
        /// it parses it.
        /// </para>
        /// </summary>
        [Fact]
        public void Requirement_RoundTrips()
        {
            SystemCapabilityRequirement systemCapabilityRequirement = Requirement(SystemCapability.ContinuousVentilation | SystemCapability.MechanicalSupply | SystemCapability.SummerBypass);

            SystemCapabilityRequirement systemCapabilityRequirement_RoundTrip = Core.Create.IJSAMObject<SystemCapabilityRequirement>(systemCapabilityRequirement.ToJsonObject().ToJsonString());

            Assert.NotNull(systemCapabilityRequirement_RoundTrip);
            Assert.Equal(systemCapabilityRequirement.Capabilities, systemCapabilityRequirement_RoundTrip.Capabilities);

            Assert.Contains("SummerBypass", systemCapabilityRequirement.ToJsonObject().ToJsonString());
            Assert.Contains("MechanicalSupply", systemCapabilityRequirement.ToJsonObject().ToJsonString());

            //A name from a later version is ignored rather than misread.
            System.Text.Json.Nodes.JsonObject jsonObject = systemCapabilityRequirement.ToJsonObject();
            (jsonObject["Capabilities"] as System.Text.Json.Nodes.JsonArray)?.Add("SomethingFromALaterVersion");

            Assert.Equal(systemCapabilityRequirement.Capabilities, new SystemCapabilityRequirement(jsonObject).Capabilities);

            //And the descriptor has no serialisation to reach for.
            Assert.False(typeof(SystemCapabilityDescriptor).GetInterfaces().Length > 0, "SystemCapabilityDescriptor implements an interface again - it is a plain value and the index is the wire format.");
        }

        /// <summary>
        /// A requirement handed to a selector cannot change underneath it: <c>With</c> returns a new
        /// instance, and a descriptor copies the mutable <c>SystemTemplate</c> it was given.
        /// </summary>
        [Fact]
        public void RequirementAndDescriptor_DoNotChangeUnderneathACaller()
        {
            SystemCapabilityRequirement systemCapabilityRequirement = Requirement(SystemCapability.ContinuousVentilation);

            systemCapabilityRequirement.With(SystemCapability.SummerBypass);

            Assert.Equal(SystemCapability.ContinuousVentilation, systemCapabilityRequirement.Capabilities);

            SystemTemplate systemTemplate = Template("MV");

            SystemCapabilityDescriptor systemCapabilityDescriptor = new(systemTemplate, SystemCapability.ContinuousVentilation);

            systemTemplate.Ventilation = "NV";
            systemCapabilityDescriptor.SystemTemplate.Ventilation = "NV";

            Assert.Equal("MV", systemCapabilityDescriptor.SystemTemplate?.Ventilation);
        }

        // ---------------------------------------------------------------------------------------------
        // Fixture - a stand-in for the SAM_Systems catalogue, NOT a copy of it
        // ---------------------------------------------------------------------------------------------

        private static List<SystemCapabilityDescriptor> Descriptors()
        {
            return
            [
                new(Template("UV"), SystemCapability.None, 40),
                new(Template("NV"), SystemCapability.ContinuousVentilation, 10),
                new(Template("MV"), SystemCapability.ContinuousVentilation | SystemCapability.Boost, 20),
                new(Template("MVRE"), SystemCapability.ContinuousVentilation | SystemCapability.Boost | SystemCapability.SummerBypass | SystemCapability.HeatRecovery, 30)
            ];
        }

        private static SystemTemplate Template(string ventilation)
        {
            return new SystemTemplate(ventilation, "RAD1", "UC1", "PR1", "CTL1", "1");
        }

        private static SystemCapabilityRequirement Requirement(SystemCapability systemCapability)
        {
            return new SystemCapabilityRequirement(systemCapability);
        }

        private static List<string> Ventilations(IEnumerable<SystemCapabilityDescriptor> systemCapabilityDescriptors)
        {
            List<string> result = [];

            foreach (SystemCapabilityDescriptor systemCapabilityDescriptor in systemCapabilityDescriptors)
            {
                result.Add(systemCapabilityDescriptor.SystemTemplate?.Ventilation);
            }

            return result;
        }
    }
}
