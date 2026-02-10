using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Models.Enums;
using powered_parachute.Services;
using System.Collections.ObjectModel;

namespace powered_parachute.ViewModels
{
    [QueryProperty(nameof(FlightId), "FlightId")]
    public partial class FlightEditorViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private int flightId;

        [ObservableProperty]
        private Flight? flight;

        // Date/Time
        [ObservableProperty]
        private DateTime flightDate = DateTime.Today;

        [ObservableProperty]
        private TimeSpan departureTime;

        [ObservableProperty]
        private TimeSpan landingTime;

        // Location
        [ObservableProperty]
        private string departureLocation = string.Empty;

        [ObservableProperty]
        private string landingLocation = string.Empty;

        // Aircraft
        [ObservableProperty]
        private ObservableCollection<PpcFrame> ppcFrames = new();

        [ObservableProperty]
        private PpcFrame? selectedPpcFrame;

        // Flight type
        [ObservableProperty]
        private FlightType selectedFlightType;

        public List<FlightType> FlightTypes { get; } = Enum.GetValues<FlightType>().ToList();

        // Operations
        [ObservableProperty]
        private int takeoffCount = 1;

        [ObservableProperty]
        private int landingCount = 1;

        // Fuel
        [ObservableProperty]
        private string fuelStart = string.Empty;

        [ObservableProperty]
        private string fuelEnd = string.Empty;

        // Hobbs
        [ObservableProperty]
        private string hobbsStart = string.Empty;

        [ObservableProperty]
        private string hobbsEnd = string.Empty;

        // Altitude
        [ObservableProperty]
        private string cruiseAltitude = string.Empty;

        [ObservableProperty]
        private string maxAltitude = string.Empty;

        // Weather
        [ObservableProperty]
        private string windSpeed = string.Empty;

        [ObservableProperty]
        private string windDirection = string.Empty;

        [ObservableProperty]
        private string windGusts = string.Empty;

        [ObservableProperty]
        private string temperature = string.Empty;

        [ObservableProperty]
        private string visibility = string.Empty;

        [ObservableProperty]
        private string ceiling = string.Empty;

        [ObservableProperty]
        private string altimeterSetting = string.Empty;

        [ObservableProperty]
        private string densityAltitude = string.Empty;

        [ObservableProperty]
        private string weatherNotes = string.Empty;

        // Training
        [ObservableProperty]
        private string instructorName = string.Empty;

        [ObservableProperty]
        private bool isTrainingFlight;

        // Notes
        [ObservableProperty]
        private string notes = string.Empty;

        [ObservableProperty]
        private string lessonsLearned = string.Empty;

        public FlightEditorViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Flight Details";
        }

        partial void OnFlightIdChanged(int value)
        {
            if (value > 0)
            {
                LoadFlightAsync().ConfigureAwait(false);
            }
        }

        partial void OnSelectedFlightTypeChanged(FlightType value)
        {
            IsTrainingFlight = value == FlightType.TrainingDual || value == FlightType.TrainingSolo;
        }

        private async Task LoadFlightAsync()
        {
            Flight = await _databaseService.GetFlightAsync(FlightId);
            var frames = await _databaseService.GetPpcFramesAsync();
            PpcFrames = new ObservableCollection<PpcFrame>(frames);

            if (Flight == null) return;

            FlightDate = Flight.FlightDate;
            DepartureTime = Flight.DepartureTime?.TimeOfDay ?? Flight.StartTime?.TimeOfDay ?? TimeSpan.Zero;
            LandingTime = Flight.LandingTime?.TimeOfDay ?? Flight.EndTime?.TimeOfDay ?? TimeSpan.Zero;
            DepartureLocation = Flight.DepartureLocation ?? Flight.Location ?? string.Empty;
            LandingLocation = Flight.LandingLocation ?? string.Empty;
            SelectedFlightType = Flight.FlightType;
            TakeoffCount = Flight.TakeoffCount ?? 1;
            LandingCount = Flight.LandingCount ?? 1;
            FuelStart = Flight.FuelStartGallons?.ToString("F1") ?? string.Empty;
            FuelEnd = Flight.FuelEndGallons?.ToString("F1") ?? string.Empty;
            HobbsStart = Flight.HobbsStart?.ToString("F1") ?? string.Empty;
            HobbsEnd = Flight.HobbsEnd?.ToString("F1") ?? string.Empty;
            CruiseAltitude = Flight.CruiseAltitudeAGL?.ToString() ?? string.Empty;
            MaxAltitude = Flight.MaxAltitude?.ToString() ?? string.Empty;
            WindSpeed = Flight.WindSpeed?.ToString() ?? string.Empty;
            WindDirection = Flight.WindDirection ?? string.Empty;
            WindGusts = Flight.WindGusts?.ToString() ?? string.Empty;
            Temperature = Flight.Temperature?.ToString() ?? string.Empty;
            Visibility = Flight.Visibility?.ToString() ?? string.Empty;
            Ceiling = Flight.Ceiling?.ToString() ?? string.Empty;
            AltimeterSetting = Flight.AltimeterSetting?.ToString("F2") ?? string.Empty;
            DensityAltitude = Flight.DensityAltitude?.ToString() ?? string.Empty;
            WeatherNotes = Flight.WeatherNotes ?? string.Empty;
            InstructorName = Flight.InstructorName ?? string.Empty;
            Notes = Flight.Notes ?? string.Empty;
            LessonsLearned = Flight.LessonsLearned ?? string.Empty;

            if (Flight.PpcFrameId.HasValue)
            {
                SelectedPpcFrame = frames.FirstOrDefault(f => f.Id == Flight.PpcFrameId.Value);
            }
        }

        [RelayCommand]
        async Task SaveFlight()
        {
            if (Flight == null)
            {
                Flight = new Flight { FlightDate = FlightDate };
            }

            Flight.FlightDate = FlightDate;
            Flight.DepartureTime = FlightDate.Add(DepartureTime);
            Flight.StartTime = Flight.DepartureTime;
            Flight.LandingTime = FlightDate.Add(LandingTime);
            Flight.EndTime = Flight.LandingTime;
            Flight.DepartureLocation = DepartureLocation;
            Flight.Location = DepartureLocation;
            Flight.LandingLocation = LandingLocation;
            Flight.PpcFrameId = SelectedPpcFrame?.Id;
            Flight.FlightType = SelectedFlightType;
            Flight.TakeoffCount = TakeoffCount;
            Flight.LandingCount = LandingCount;

            if (double.TryParse(FuelStart, out var fs)) Flight.FuelStartGallons = fs;
            if (double.TryParse(FuelEnd, out var fe)) Flight.FuelEndGallons = fe;
            if (double.TryParse(HobbsStart, out var hs)) Flight.HobbsStart = hs;
            if (double.TryParse(HobbsEnd, out var he)) Flight.HobbsEnd = he;
            if (int.TryParse(CruiseAltitude, out var ca)) Flight.CruiseAltitudeAGL = ca;
            if (int.TryParse(MaxAltitude, out var ma)) Flight.MaxAltitude = ma;
            if (double.TryParse(WindSpeed, out var ws)) Flight.WindSpeed = ws;
            Flight.WindDirection = WindDirection;
            if (double.TryParse(WindGusts, out var wg)) Flight.WindGusts = wg;
            if (double.TryParse(Temperature, out var temp)) Flight.Temperature = temp;
            if (double.TryParse(Visibility, out var vis)) Flight.Visibility = vis;
            if (int.TryParse(Ceiling, out var ceil)) Flight.Ceiling = ceil;
            if (double.TryParse(AltimeterSetting, out var alt)) Flight.AltimeterSetting = alt;
            if (int.TryParse(DensityAltitude, out var da)) Flight.DensityAltitude = da;
            Flight.WeatherNotes = WeatherNotes;
            Flight.InstructorName = InstructorName;
            Flight.Notes = Notes;
            Flight.LessonsLearned = LessonsLearned;

            Flight.CalculateDuration();
            Flight.CalculateFuelConsumed();
            Flight.CalculateHoursFlown();

            // Update engine hours if Hobbs changed
            if (Flight.PpcFrameId.HasValue && Flight.HobbsEnd.HasValue)
            {
                var engine = await _databaseService.GetEngineByFrameAsync(Flight.PpcFrameId.Value);
                if (engine != null)
                {
                    engine.TotalHours = Flight.HobbsEnd;
                    await _databaseService.SaveEngineAsync(engine);
                }
            }

            await _databaseService.SaveFlightAsync(Flight);

            await Shell.Current.DisplayAlertAsync("Saved", "Flight details saved.", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}
