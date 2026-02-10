using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Models.Enums;
using powered_parachute.Services;

namespace powered_parachute.ViewModels
{
    [QueryProperty(nameof(FrameId), "FrameId")]
    public partial class EquipmentDetailViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private int frameId;

        [ObservableProperty]
        private bool isNewFrame = true;

        // Frame fields
        [ObservableProperty]
        private string manufacturer = string.Empty;

        [ObservableProperty]
        private string model = string.Empty;

        [ObservableProperty]
        private string serialNumber = string.Empty;

        [ObservableProperty]
        private string nNumber = string.Empty;

        [ObservableProperty]
        private string year = string.Empty;

        [ObservableProperty]
        private string emptyWeight = string.Empty;

        [ObservableProperty]
        private SeatConfig seatConfig;

        public List<SeatConfig> SeatConfigs { get; } = Enum.GetValues<SeatConfig>().ToList();

        [ObservableProperty]
        private bool isActive = true;

        [ObservableProperty]
        private string frameNotes = string.Empty;

        // Engine fields
        [ObservableProperty]
        private string engineManufacturer = string.Empty;

        [ObservableProperty]
        private string engineModel = string.Empty;

        [ObservableProperty]
        private string engineSerial = string.Empty;

        [ObservableProperty]
        private EngineType engineType;

        public List<EngineType> EngineTypes { get; } = Enum.GetValues<EngineType>().ToList();

        [ObservableProperty]
        private CoolingType coolingType;

        public List<CoolingType> CoolingTypes { get; } = Enum.GetValues<CoolingType>().ToList();

        [ObservableProperty]
        private string totalEngineHours = string.Empty;

        [ObservableProperty]
        private string tboHours = string.Empty;

        // Wing fields
        [ObservableProperty]
        private string wingManufacturer = string.Empty;

        [ObservableProperty]
        private string wingModel = string.Empty;

        [ObservableProperty]
        private string wingSizeSqFt = string.Empty;

        [ObservableProperty]
        private string cellCount = string.Empty;

        [ObservableProperty]
        private WingType wingType;

        public List<WingType> WingTypes { get; } = Enum.GetValues<WingType>().ToList();

        [ObservableProperty]
        private string wingHours = string.Empty;

        // Propeller fields
        [ObservableProperty]
        private string propManufacturer = string.Empty;

        [ObservableProperty]
        private string propModel = string.Empty;

        [ObservableProperty]
        private string propDiameter = string.Empty;

        [ObservableProperty]
        private string propPitch = string.Empty;

        [ObservableProperty]
        private string propMaterial = string.Empty;

        private PpcFrame? _frame;
        private Engine? _engine;
        private Wing? _wing;
        private Propeller? _propeller;

        public EquipmentDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Add PPC";
        }

        partial void OnFrameIdChanged(int value)
        {
            if (value > 0)
            {
                IsNewFrame = false;
                LoadFrameAsync().ConfigureAwait(false);
            }
        }

        private async Task LoadFrameAsync()
        {
            _frame = await _databaseService.GetPpcFrameAsync(FrameId);
            if (_frame == null) return;

            Title = _frame.DisplayName;
            Manufacturer = _frame.Manufacturer ?? string.Empty;
            Model = _frame.Model ?? string.Empty;
            SerialNumber = _frame.SerialNumber ?? string.Empty;
            NNumber = _frame.NNumber ?? string.Empty;
            Year = _frame.Year?.ToString() ?? string.Empty;
            EmptyWeight = _frame.EmptyWeight?.ToString() ?? string.Empty;
            SeatConfig = _frame.SeatConfig;
            IsActive = _frame.IsActive;
            FrameNotes = _frame.Notes ?? string.Empty;

            // Load sub-components
            _engine = await _databaseService.GetEngineByFrameAsync(FrameId);
            if (_engine != null)
            {
                EngineManufacturer = _engine.Manufacturer ?? string.Empty;
                EngineModel = _engine.Model ?? string.Empty;
                EngineSerial = _engine.SerialNumber ?? string.Empty;
                EngineType = _engine.EngineType;
                CoolingType = _engine.CoolingType;
                TotalEngineHours = _engine.TotalHours?.ToString("F1") ?? string.Empty;
                TboHours = _engine.TBOHours?.ToString("F0") ?? string.Empty;
            }

            _wing = await _databaseService.GetWingByFrameAsync(FrameId);
            if (_wing != null)
            {
                WingManufacturer = _wing.Manufacturer ?? string.Empty;
                WingModel = _wing.Model ?? string.Empty;
                WingSizeSqFt = _wing.SizeSqFt?.ToString() ?? string.Empty;
                CellCount = _wing.CellCount?.ToString() ?? string.Empty;
                WingType = _wing.WingType;
                WingHours = _wing.TotalHours?.ToString("F1") ?? string.Empty;
            }

            _propeller = await _databaseService.GetPropellerByFrameAsync(FrameId);
            if (_propeller != null)
            {
                PropManufacturer = _propeller.Manufacturer ?? string.Empty;
                PropModel = _propeller.Model ?? string.Empty;
                PropDiameter = _propeller.Diameter?.ToString() ?? string.Empty;
                PropPitch = _propeller.Pitch?.ToString() ?? string.Empty;
                PropMaterial = _propeller.Material ?? string.Empty;
            }
        }

        [RelayCommand]
        async Task SaveEquipment()
        {
            // Save frame
            _frame ??= new PpcFrame();
            _frame.Manufacturer = Manufacturer;
            _frame.Model = Model;
            _frame.SerialNumber = SerialNumber;
            _frame.NNumber = NNumber;
            if (int.TryParse(Year, out var y)) _frame.Year = y;
            if (double.TryParse(EmptyWeight, out var w)) _frame.EmptyWeight = w;
            _frame.SeatConfig = SeatConfig;
            _frame.IsActive = IsActive;
            _frame.Notes = FrameNotes;
            await _databaseService.SavePpcFrameAsync(_frame);

            // Save engine
            _engine ??= new Engine { PpcFrameId = _frame.Id };
            _engine.PpcFrameId = _frame.Id;
            _engine.Manufacturer = EngineManufacturer;
            _engine.Model = EngineModel;
            _engine.SerialNumber = EngineSerial;
            _engine.EngineType = EngineType;
            _engine.CoolingType = CoolingType;
            if (double.TryParse(TotalEngineHours, out var eh)) _engine.TotalHours = eh;
            if (double.TryParse(TboHours, out var tbo)) _engine.TBOHours = tbo;
            await _databaseService.SaveEngineAsync(_engine);

            // Save wing
            _wing ??= new Wing { PpcFrameId = _frame.Id };
            _wing.PpcFrameId = _frame.Id;
            _wing.Manufacturer = WingManufacturer;
            _wing.Model = WingModel;
            if (double.TryParse(WingSizeSqFt, out var ws)) _wing.SizeSqFt = ws;
            if (int.TryParse(CellCount, out var cc)) _wing.CellCount = cc;
            _wing.WingType = WingType;
            if (double.TryParse(WingHours, out var wh)) _wing.TotalHours = wh;
            await _databaseService.SaveWingAsync(_wing);

            // Save propeller
            _propeller ??= new Propeller { PpcFrameId = _frame.Id };
            _propeller.PpcFrameId = _frame.Id;
            _propeller.Manufacturer = PropManufacturer;
            _propeller.Model = PropModel;
            if (double.TryParse(PropDiameter, out var pd)) _propeller.Diameter = pd;
            if (double.TryParse(PropPitch, out var pp)) _propeller.Pitch = pp;
            _propeller.Material = PropMaterial;
            await _databaseService.SavePropellerAsync(_propeller);

            await Shell.Current.DisplayAlertAsync("Saved", "Equipment saved.", "OK");
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        async Task DeleteFrame()
        {
            if (_frame == null) return;
            bool confirm = await Shell.Current.DisplayAlertAsync("Delete", "Delete this PPC and all its components?", "Delete", "Cancel");
            if (!confirm) return;

            await _databaseService.DeletePpcFrameAsync(_frame);
            await Shell.Current.GoToAsync("..");
        }
    }
}
