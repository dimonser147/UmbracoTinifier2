using Tinifier.Core.Infrastructure;
using Tinifier.Core.Infrastructure.Exceptions;
using Tinifier.Core.Models.Db;
using Tinifier.Core.Repository.Settings;

namespace Tinifier.Core.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public void CreateSettings(TSetting setting)
        {
            _settingsRepository.Create(setting);
        }

        public TSetting GetSettings()
        {
            return _settingsRepository.GetSettings();
        }

        public TSetting CheckIfSettingExists()
        {
            var setting = _settingsRepository.GetSettings();

            if (setting == null)
                throw new EntityNotFoundException(PackageConstants.ApiKeyNotFound);
            return setting;
        }

        public void UpdateSettings(int currentMonthRequests)
        {
            _settingsRepository.Update(currentMonthRequests);
        }
    }
}
