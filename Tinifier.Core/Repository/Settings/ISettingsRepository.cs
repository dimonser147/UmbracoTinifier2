using Tinifier.Core.Models.Db;

namespace Tinifier.Core.Repository.Settings
{
    /// <summary>
    /// Repository for settings with custom methods
    /// </summary>
    /// <typeparam name="TSetting">class</typeparam>
    public interface ISettingsRepository
    {
        TSetting GetSettings();

        void Update(int currentMonthRequests);

        /// <summary>
        /// Create settings
        /// </summary>
        /// <param name="entity">TSetting</param>
        void Create(TSetting entity);
    }
}
