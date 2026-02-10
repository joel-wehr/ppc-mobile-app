using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Models.Enums;
using powered_parachute.Services;

namespace powered_parachute.ViewModels
{
    public partial class PilotProfileViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private PilotProfile? _profile;

        [ObservableProperty]
        private string fullName = string.Empty;

        [ObservableProperty]
        private CertificateType certificateType;

        public List<CertificateType> CertificateTypes { get; } = Enum.GetValues<CertificateType>().ToList();

        [ObservableProperty]
        private string certificateNumber = string.Empty;

        [ObservableProperty]
        private MedicalType medicalType;

        public List<MedicalType> MedicalTypes { get; } = Enum.GetValues<MedicalType>().ToList();

        [ObservableProperty]
        private DateTime? medicalExpiration;

        [ObservableProperty]
        private string maxWindSpeed = string.Empty;

        [ObservableProperty]
        private string maxCrosswind = string.Empty;

        [ObservableProperty]
        private string minVisibility = string.Empty;

        [ObservableProperty]
        private string minCeiling = string.Empty;

        [ObservableProperty]
        private string emergencyContactName = string.Empty;

        [ObservableProperty]
        private string emergencyContactPhone = string.Empty;

        public PilotProfileViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Pilot Profile";
        }

        public async Task LoadProfileAsync()
        {
            _profile = await _databaseService.GetPilotProfileAsync();
            if (_profile == null) return;

            FullName = _profile.FullName ?? string.Empty;
            CertificateType = _profile.CertificateType;
            CertificateNumber = _profile.CertificateNumber ?? string.Empty;
            MedicalType = _profile.MedicalType;
            MedicalExpiration = _profile.MedicalExpiration;
            MaxWindSpeed = _profile.MaxWindSpeed?.ToString() ?? string.Empty;
            MaxCrosswind = _profile.MaxCrosswind?.ToString() ?? string.Empty;
            MinVisibility = _profile.MinVisibility?.ToString() ?? string.Empty;
            MinCeiling = _profile.MinCeiling?.ToString() ?? string.Empty;
            EmergencyContactName = _profile.EmergencyContactName ?? string.Empty;
            EmergencyContactPhone = _profile.EmergencyContactPhone ?? string.Empty;
        }

        [RelayCommand]
        async Task SaveProfile()
        {
            _profile ??= new PilotProfile();

            _profile.FullName = FullName;
            _profile.CertificateType = CertificateType;
            _profile.CertificateNumber = CertificateNumber;
            _profile.MedicalType = MedicalType;
            _profile.MedicalExpiration = MedicalExpiration;
            if (double.TryParse(MaxWindSpeed, out var mw)) _profile.MaxWindSpeed = mw;
            if (double.TryParse(MaxCrosswind, out var mc)) _profile.MaxCrosswind = mc;
            if (double.TryParse(MinVisibility, out var mv)) _profile.MinVisibility = mv;
            if (double.TryParse(MinCeiling, out var mce)) _profile.MinCeiling = mce;
            _profile.EmergencyContactName = EmergencyContactName;
            _profile.EmergencyContactPhone = EmergencyContactPhone;

            await _databaseService.SavePilotProfileAsync(_profile);
            await Shell.Current.DisplayAlertAsync("Saved", "Pilot profile saved.", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}
