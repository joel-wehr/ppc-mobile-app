using powered_parachute.Models;
using powered_parachute.Models.Enums;

namespace powered_parachute.Services
{
    /// <summary>
    /// Provides default checklist template data for DB seeding and "Reset to Defaults".
    /// NOT used at runtime for serving checklists - that's the database now.
    /// </summary>
    public class DefaultChecklistDataService
    {
        public class TemplateDefinition
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int DisplayOrder { get; set; }
            public List<ItemDefinition> Items { get; set; } = new();
        }

        public class ItemDefinition
        {
            public string Section { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int DisplayOrder { get; set; }
            public ChecklistItemType ItemType { get; set; }
        }

        public List<TemplateDefinition> GetDefaultTemplates()
        {
            return new List<TemplateDefinition>
            {
                GetPreflightDetailed(),
                GetPreflightAbbreviated(),
                GetEngineStart(),
                GetWarmUpRunUp(),
                GetWingLayout(),
                GetBeforeTakeoff(),
                GetInFlightChecks(),
                GetPreLanding(),
                GetLanding(),
                GetPostFlight(),
                GetInFlightPractice()
            };
        }

        private TemplateDefinition GetPreflightDetailed()
        {
            var template = new TemplateDefinition
            {
                Name = "Pre-Flight (Detailed)",
                Description = "Complete preflight inspection - all items",
                DisplayOrder = 0
            };

            var sections = new Dictionary<string, string[]>
            {
                ["Nose Wheel & Front"] = new[]
                {
                    "Check nose wheel security on axle with nut secure",
                    "Wheel fork and pivot bolt secure",
                    "Brake mechanism secure and operating",
                    "Centering springs OK, steering arm secure",
                    "Wheel and steering turn freely"
                },
                ["Left Steering"] = new[]
                {
                    "Foot bar is secure and pivots freely",
                    "Steering line in good order",
                    "Pulleys in good order",
                    "Trim system secure, moves freely, in proper takeoff position"
                },
                ["Instruments"] = new[]
                {
                    "Instrument pod is secure",
                    "All instruments and switches secure",
                    "Switches in safe position before engine startup"
                },
                ["Ground Steering"] = new[]
                {
                    "Check for proper range of steering",
                    "Check all nuts and bolts"
                },
                ["Front Seats"] = new[]
                {
                    "Front seat securely fastened to airframe",
                    "Seat belts securely fastened to airframe"
                },
                ["Rear Seats"] = new[]
                {
                    "Rear seat securely fastened to airframe",
                    "Seat belts securely fastened to airframe"
                },
                ["Left Riser"] = new[]
                {
                    "Riser connections to airframe secure, brackets secure",
                    "Links finger tight plus no more than 1/4 turn (Mallion Rapide links only)",
                    "Steering line properly routed and in good condition",
                    "Pulley in good condition",
                    "Riser in good condition and properly connected (no twists)"
                },
                ["Left Suspension"] = new[]
                {
                    "All suspension pivots smooth and free from excessive wear",
                    "Springs, shocks and tubes in good order"
                },
                ["Left Wheel"] = new[]
                {
                    "Tire in good condition and properly inflated",
                    "Axle secure, axle nut secure"
                },
                ["Left Side Components"] = new[]
                {
                    "Battery mount secure, battery secure and free of leaks",
                    "Radiator mounts secure, radiator free of leaks, drain cock closed",
                    "Left side exhaust, exhaust springs, sparkplugs, plug caps and plug wires OK",
                    "Left side prop cage tubes secure, bolts secure"
                },
                ["Propeller & Gearbox"] = new[]
                {
                    "Check gearbox for leaks",
                    "Move prop to check for normal gearbox backlash",
                    "Examine prop for damage, cleanliness, distortion"
                },
                ["Right Engine"] = new[]
                {
                    "Carburetor and mounting secure",
                    "Check carb boots for cracks",
                    "Throttle and choke linkages secure",
                    "Fuel lines secure and in good condition"
                },
                ["Right Suspension"] = new[]
                {
                    "All suspension pivots smooth and free from excessive wear",
                    "Springs, shocks and tubes in good order"
                },
                ["Right Wheel"] = new[]
                {
                    "Tire in good condition and properly inflated",
                    "Axle secure, axle nut secure"
                },
                ["Coolant System"] = new[]
                {
                    "Coolant bottle secure, filled to proper level",
                    "Lines secure, no leaks, cap secure",
                    "Header tank secure, cap secure",
                    "Lines and clamps secure, no leaks"
                },
                ["Oil & Accessories"] = new[]
                {
                    "Oil bottle secure, filled to proper level",
                    "Lines secure, no leaks, cap secure",
                    "Strobe light securely mounted, wiring in good order, lens secure"
                },
                ["Fuel System"] = new[]
                {
                    "Fuel tank - check for leaks, secure mountings",
                    "Check fuel gage for fuel level and any leaks",
                    "Check fuel tank vent for security and lack of blockage or leaks",
                    "Check fuel filter for any contamination"
                },
                ["Gascolator"] = new[]
                {
                    "Visually check gascolator securely connected to airframe and fuel lines",
                    "Draw sample, check for water, correct fuel color, dirt or contamination"
                },
                ["Controls"] = new[]
                {
                    "Rear throttle control and linkage - proper action and security",
                    "Throttle controls assembly secure to airframe",
                    "Linkages secure, cables and housings in good condition",
                    "Friction set correctly, throttle levers move smoothly through full range"
                },
                ["Right Riser"] = new[]
                {
                    "Riser connections to airframe secure, brackets secure",
                    "Links finger tight plus no more than 1/4 turn",
                    "Steering line properly routed and in good condition",
                    "Pulley in good condition",
                    "Riser in good condition and properly connected (no twists)"
                },
                ["Right Steering"] = new[]
                {
                    "Foot bar is secure and pivots freely",
                    "Steering line in good order",
                    "Pulleys in good order",
                    "Trim system secure, moves freely, in proper takeoff position"
                }
            };

            AddSections(template, sections, ChecklistItemType.Checkbox);
            return template;
        }

        private TemplateDefinition GetPreflightAbbreviated()
        {
            var template = new TemplateDefinition
            {
                Name = "Pre-Flight (Quick)",
                Description = "Abbreviated preflight checklist",
                DisplayOrder = 1
            };

            var sections = new Dictionary<string, string[]>
            {
                ["Front"] = new[]
                {
                    "Nose wheel, brake and ground steering",
                    "Left hand flight steering foot bar, steering line, pulley and trim system",
                    "Instrument pod, instruments, switches, displays",
                    "Ground steering controls",
                    "Front seat & belts left hand side"
                },
                ["Left Side"] = new[]
                {
                    "Rear seat and belts left hand side",
                    "Left side riser connections, steering line, pulley, links and riser",
                    "Rear suspension and pivots left side",
                    "Wheel, tire and axle left side",
                    "Battery, radiator and radiator mounts",
                    "Left side of engine",
                    "Left side prop cage"
                },
                ["Center"] = new[]
                {
                    "Propeller and gearbox",
                    "Right hand side of engine"
                },
                ["Right Side"] = new[]
                {
                    "Right hand suspension",
                    "Wheel, tire and axle right side",
                    "Coolant overflow bottle and header tank",
                    "Strobe light",
                    "Oil tank",
                    "Rear throttle",
                    "Rear seat and belts right hand side",
                    "Right side riser connections, steering line, pulley, links and riser",
                    "Front seat & belts right hand side",
                    "Throttle and choke controls",
                    "Right hand flight steering foot bar, steering line, pulley and trim system",
                    "Nose wheel, right side"
                }
            };

            AddSections(template, sections, ChecklistItemType.Checkbox);
            return template;
        }

        private TemplateDefinition GetEngineStart()
        {
            var template = new TemplateDefinition
            {
                Name = "Engine Start",
                Description = "Engine start procedure",
                DisplayOrder = 2
            };

            var items = new[]
            {
                "Remove prop and carb covers and exhaust plug. Store in saddle bag",
                "Check oil (Injection and Rotary Valve)",
                "Check fuel, all caps secure. Prime until fuel gets to carb and two-three more squirts",
                "Mags ON, EIS OFF",
                "Yell \"Clear Prop\" and look to make sure all is clear before starting",
                "Turn Key to start. Prime as needed",
                "Once running smoothly, turn on EIS"
            };

            AddItems(template, "Engine Start", items, ChecklistItemType.Checkbox);
            return template;
        }

        private TemplateDefinition GetWarmUpRunUp()
        {
            var template = new TemplateDefinition
            {
                Name = "Warm-Up / Run-Up",
                Description = "Engine warm-up and run-up checks",
                DisplayOrder = 3
            };

            var items = new[]
            {
                "Bring RPM up to 3000-3500rpm range and run for minimum of 5 minutes",
                "Verify voltage is 13.5-14.5 volts",
                "Check water temperature gauge",
                "Mag Check - Turn off one mag at a time to verify both mags are operational",
                "Full Power run up - Secure plane against solid object, water temp 140F or higher",
                "Check all engine instruments in green range"
            };

            AddItems(template, "Warm-Up", items, ChecklistItemType.Checkbox);
            return template;
        }

        private TemplateDefinition GetWingLayout()
        {
            var template = new TemplateDefinition
            {
                Name = "Wing Layout",
                Description = "Wing deployment and setup",
                DisplayOrder = 4
            };

            var items = new[]
            {
                "Set up into wind based on runway orientation",
                "Remove bag from plane, place bag behind plane label facing away from rear",
                "Remove chute from bag and lay out using inverted or stacked method",
                "After removing line socks, check for tangled line or line overs. Clean lines",
                "Going back to riser attachment points make sure there are no twists in the risers",
                "Check that steering line and risers are taut, not wrapped over anything",
                "Steering line should be above the cleat not hanging below",
                "Once cleared repeat for other side. Stow line socks and chute bag",
                "Double check to make sure all lines are clear on both sides",
                "Before getting into plane make sure there is no slack in steering lines and risers"
            };

            AddItems(template, "Wing Setup", items, ChecklistItemType.Checkbox);
            return template;
        }

        private TemplateDefinition GetBeforeTakeoff()
        {
            var template = new TemplateDefinition
            {
                Name = "Before Takeoff",
                Description = "Pre-takeoff final checks",
                DisplayOrder = 5
            };

            var items = new[]
            {
                "Conduct preflight passenger briefing and cover emergency procedures",
                "Make sure passenger seat belt and helmet are secure and connected to intercom",
                "Get in and secure your seat belt and helmet, plug into intercom",
                "Verify wind direction and speed has not changed",
                "Ready to start: Mags on, EIS off, Look back and verify prop area clear",
                "Yell \"Clear Prop\" and start",
                "Once started and running smoothly turn on EIS, verify engine is up to temp",
                "Final check: all instruments normal, controls free and correct",
                "Take off into wind"
            };

            AddItems(template, "Before Takeoff", items, ChecklistItemType.Checkbox);
            return template;
        }

        private TemplateDefinition GetInFlightChecks()
        {
            var template = new TemplateDefinition
            {
                Name = "In-Flight Checks",
                Description = "Periodic in-flight monitoring",
                DisplayOrder = 6
            };

            var items = new[]
            {
                "Engine instruments all in green range",
                "Water temperature normal",
                "Oil pressure normal",
                "RPM set for cruise",
                "Fuel level check",
                "Altitude and heading check",
                "Wind conditions assessment",
                "Traffic scan - look for other aircraft",
                "Ground reference check - know your position"
            };

            AddItems(template, "In-Flight", items, ChecklistItemType.Checkbox);
            return template;
        }

        private TemplateDefinition GetPreLanding()
        {
            var template = new TemplateDefinition
            {
                Name = "Pre-Landing",
                Description = "Approach and landing preparation",
                DisplayOrder = 7
            };

            var items = new[]
            {
                "Determine wind direction and landing runway",
                "Plan approach pattern",
                "Check for traffic in pattern and on runway",
                "Reduce throttle for descent",
                "Establish stable approach speed",
                "Verify landing area is clear of obstacles",
                "Passenger briefed on landing"
            };

            AddItems(template, "Pre-Landing", items, ChecklistItemType.Checkbox);
            return template;
        }

        private TemplateDefinition GetLanding()
        {
            var template = new TemplateDefinition
            {
                Name = "Landing",
                Description = "Landing and shutdown on the ground",
                DisplayOrder = 8
            };

            var items = new[]
            {
                "Final approach aligned with runway",
                "Touchdown - flare and reduce power",
                "Roll out - maintain directional control",
                "Clear runway when safe",
                "Reduce RPM to idle",
                "EIS off, then Mags off",
                "Remove key"
            };

            AddItems(template, "Landing", items, ChecklistItemType.Checkbox);
            return template;
        }

        private TemplateDefinition GetPostFlight()
        {
            var template = new TemplateDefinition
            {
                Name = "Post Flight",
                Description = "Post-flight shutdown and securing",
                DisplayOrder = 9
            };

            var items = new[]
            {
                "Bag chute and secure on plane",
                "Replace carb cover and exhaust plug",
                "Secure seat belts, zip saddle bags closed",
                "Replace prop covers",
                "Remove key",
                "Secure plane on trailer and strap down",
                "Verify nothing loose is left on plane",
                "Record Hobbs time and fuel remaining",
                "Log flight in logbook"
            };

            AddItems(template, "Shutdown", items, ChecklistItemType.Checkbox);
            return template;
        }

        private TemplateDefinition GetInFlightPractice()
        {
            var template = new TemplateDefinition
            {
                Name = "In-Flight Practice",
                Description = "Track landings, maneuvers, and practice sessions",
                DisplayOrder = 10
            };

            var landings = new[]
            {
                "Normal Landing",
                "Crosswind Landing",
                "Short Field Landing",
                "Precision Spot Landing",
                "Touch and Go"
            };

            var maneuvers = new[]
            {
                "Slow Flight",
                "Steep Turns (Left)",
                "Steep Turns (Right)",
                "S-Turns",
                "Figure 8s",
                "Power-Off Glides",
                "Emergency Descent",
                "Go-Around / Aborted Landing"
            };

            var airwork = new[]
            {
                "Altitude Changes",
                "Coordinated Turns",
                "Traffic Pattern Work",
                "Ground Reference Maneuvers",
                "Wind Drift Correction"
            };

            var emergency = new[]
            {
                "Simulated Engine Failure",
                "Emergency Landing Practice"
            };

            AddItems(template, "Landings", landings, ChecklistItemType.Counter);
            AddItems(template, "Maneuvers", maneuvers, ChecklistItemType.Counter);
            AddItems(template, "Airwork", airwork, ChecklistItemType.Counter);
            AddItems(template, "Emergency Procedures", emergency, ChecklistItemType.Counter);

            return template;
        }

        private void AddSections(TemplateDefinition template, Dictionary<string, string[]> sections, ChecklistItemType itemType)
        {
            foreach (var section in sections)
            {
                AddItems(template, section.Key, section.Value, itemType);
            }
        }

        private void AddItems(TemplateDefinition template, string section, string[] descriptions, ChecklistItemType itemType)
        {
            int startOrder = template.Items.Count;
            for (int i = 0; i < descriptions.Length; i++)
            {
                template.Items.Add(new ItemDefinition
                {
                    Section = section,
                    Description = descriptions[i],
                    DisplayOrder = startOrder + i,
                    ItemType = itemType
                });
            }
        }
    }
}
