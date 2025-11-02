using powered_parachute.Models;

namespace powered_parachute.Services
{
    /// <summary>
    /// Service for managing checklists and their items
    /// </summary>
    public class ChecklistService
    {
        private static List<ChecklistItem>? _allChecklists;

        /// <summary>
        /// Gets all checklist items for a specific checklist type
        /// </summary>
        public List<ChecklistItem> GetChecklist(ChecklistType type)
        {
            InitializeChecklists();

            return _allChecklists!
                .Where(c => c.ChecklistType == type)
                .OrderBy(c => c.Order)
                .Select(c => c.Clone())
                .ToList();
        }

        /// <summary>
        /// Gets all available checklist types with display names
        /// </summary>
        public List<(ChecklistType Type, string DisplayName, string Description)> GetChecklistTypes()
        {
            return new List<(ChecklistType, string, string)>
            {
                (ChecklistType.PreflightDetailed, "Preflight (Detailed)", "Complete preflight inspection"),
                (ChecklistType.PreflightAbbreviated, "Preflight (Quick)", "Abbreviated preflight checklist"),
                (ChecklistType.WarmUp, "Warm Up", "Engine warm-up procedure"),
                (ChecklistType.PreStartAndTakeoff, "Pre-Start & Takeoff", "Pre-start and takeoff checklist"),
                (ChecklistType.WingLayout, "Wing Layout", "Wing deployment and setup"),
                (ChecklistType.InFlight, "In-Flight Practice", "Track landings, maneuvers, and practices"),
                (ChecklistType.PostFlight, "Post Flight", "Post-flight shutdown and securing")
            };
        }

        private void InitializeChecklists()
        {
            if (_allChecklists != null) return;

            _allChecklists = new List<ChecklistItem>();

            // Warm Up Procedure (Page 31)
            AddWarmUpChecklist();

            // Pre-Start and Takeoff (Page 32)
            AddPreStartTakeoffChecklist();

            // Wing Layout (Page 33)
            AddWingLayoutChecklist();

            // In-Flight Practice Log
            AddInFlightChecklist();

            // Post Flight (Page 34)
            AddPostFlightChecklist();

            // Preflight Abbreviated (Page 15)
            AddPreflightAbbreviatedChecklist();

            // Preflight Detailed (Pages 11-14)
            AddPreflightDetailedChecklist();
        }

        #region Warm Up Checklist

        private void AddWarmUpChecklist()
        {
            var items = new[]
            {
                "Remove prop and carb covers and exhaust plug. Store in saddle bag",
                "Check oil (Injection and Rotary Valve)",
                "Check fuel, all caps secure. Prime until fuel gets to carb and two-three more squirts",
                "Mags ON, EIS OFF",
                "Yell \"Clear Prop\" and look to make sure all is clear before starting",
                "Turn Key to start. Prime as needed",
                "Once running smoothly, turn on EIS",
                "Bring RPM up to 3000-3500rpm range and run for minimum of 5 minutes. Verify voltage is 13.5-14.5 volts",
                "Mag Check. Turn off one mag at a time to verify both mags are operational",
                "Full Power run up. Secure plane against solid object, water temp 140°F or higher"
            };

            AddItems(ChecklistType.WarmUp, "Engine Start", items);
        }

        #endregion

        #region Pre-Start and Takeoff Checklist

        private void AddPreStartTakeoffChecklist()
        {
            var items = new[]
            {
                "Conduct preflight passenger briefing and cover emergency procedures",
                "Make sure passenger seat belt and helmet are secure and connected to intercom",
                "Get in and secure your seat belt and helmet, plug into intercom",
                "Verify wind direction and speed has not changed",
                "Ready to start: Mags on, EIS off, Look back and verify prop area clear",
                "Yell \"Clear Prop\" and start",
                "Once started and running smoothly turn on EIS, verify engine is up to temp",
                "Take off into wind"
            };

            AddItems(ChecklistType.PreStartAndTakeoff, "Pre-Start", items);
        }

        #endregion

        #region Wing Layout Checklist

        private void AddWingLayoutChecklist()
        {
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

            AddItems(ChecklistType.WingLayout, "Wing Setup", items);
        }

        #endregion

        #region Post Flight Checklist

        private void AddPostFlightChecklist()
        {
            var items = new[]
            {
                "Bag chute and secure on plane",
                "Replace carb cover and exhaust plug",
                "Secure seat belts, zip saddle bags closed",
                "Replace prop covers",
                "Remove key",
                "Secure plane on trailer and strap down",
                "Verify nothing loose is left on plane"
            };

            AddItems(ChecklistType.PostFlight, "Shutdown", items);
        }

        #endregion

        #region Preflight Abbreviated Checklist

        private void AddPreflightAbbreviatedChecklist()
        {
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

            foreach (var section in sections)
            {
                AddItems(ChecklistType.PreflightAbbreviated, section.Key, section.Value);
            }
        }

        #endregion

        #region Preflight Detailed Checklist

        private void AddPreflightDetailedChecklist()
        {
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
                    "Links finger tight plus no more than ¼ turn (Mallion Rapide links only)",
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
                    "Links finger tight plus no more than ¼ turn",
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

            foreach (var section in sections)
            {
                AddItems(ChecklistType.PreflightDetailed, section.Key, section.Value);
            }
        }

        #endregion

        #region In-Flight Practice Log

        private void AddInFlightChecklist()
        {
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

            AddCounterItems(ChecklistType.InFlight, "Landings", landings);
            AddCounterItems(ChecklistType.InFlight, "Maneuvers", maneuvers);
            AddCounterItems(ChecklistType.InFlight, "Airwork", airwork);
            AddCounterItems(ChecklistType.InFlight, "Emergency Procedures", emergency);
        }

        #endregion

        private void AddItems(ChecklistType type, string section, string[] descriptions)
        {
            var startOrder = _allChecklists!.Count(c => c.ChecklistType == type);

            for (int i = 0; i < descriptions.Length; i++)
            {
                _allChecklists.Add(new ChecklistItem
                {
                    Id = _allChecklists.Count + 1,
                    ChecklistType = type,
                    Section = section,
                    Description = descriptions[i],
                    Order = startOrder + i,
                    IsChecked = false,
                    HasCounter = false
                });
            }
        }

        private void AddCounterItems(ChecklistType type, string section, string[] descriptions)
        {
            var startOrder = _allChecklists!.Count(c => c.ChecklistType == type);

            for (int i = 0; i < descriptions.Length; i++)
            {
                _allChecklists.Add(new ChecklistItem
                {
                    Id = _allChecklists.Count + 1,
                    ChecklistType = type,
                    Section = section,
                    Description = descriptions[i],
                    Order = startOrder + i,
                    IsChecked = false,
                    HasCounter = true,
                    Count = 0
                });
            }
        }
    }
}
