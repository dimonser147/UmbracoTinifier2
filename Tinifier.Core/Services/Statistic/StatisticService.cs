using Tinifier.Core.Models.API;
using Tinifier.Core.Models.Db;
using Tinifier.Core.Repository.Image;
using Tinifier.Core.Repository.Statistic;

namespace Tinifier.Core.Services.Statistic
{
    public class StatisticService : IStatisticService
    {
        private readonly IImageRepository _imageRepository;
        private readonly IStatisticRepository _statisticRepository;

        public StatisticService(IImageRepository imageRepository, IStatisticRepository statisticRepository)
        {
            _imageRepository = imageRepository;
            _statisticRepository = statisticRepository;
        }

        private TImageStatistic CreateInitialStatistic()
        {
           var newStat = new TImageStatistic
           {
               TotalNumberOfImages = _imageRepository.AmounthOfItems(),
               NumberOfOptimizedImages = _imageRepository.AmounthOfOptimizedItems(),
               TotalSavedBytes = 0
           };

           _statisticRepository.Create(newStat);
            return newStat;
        }

        public TinifyImageStatistic GetStatistic()
        {
            var statistic = _statisticRepository.GetStatistic() ?? CreateInitialStatistic();

            var tImageStatistic = new TinifyImageStatistic
            {
                TotalOptimizedImages = statistic.NumberOfOptimizedImages,
                TotalOriginalImages = statistic.TotalNumberOfImages - statistic.NumberOfOptimizedImages,
                TotalSavedBytes = statistic.TotalSavedBytes
            };

            return tImageStatistic;
        }

        public void UpdateStatistic()
        {
            var statistic = _statisticRepository.GetStatistic() ?? CreateInitialStatistic();

            statistic.TotalNumberOfImages = _imageRepository.AmounthOfItems();
            statistic.NumberOfOptimizedImages = _imageRepository.AmounthOfOptimizedItems();
            statistic.TotalSavedBytes = _statisticRepository.GetTotalSavedBytes();

            _statisticRepository.Update(statistic);
        }

        public void UpdateStatistic(int amount)
        {
            var statistic = _statisticRepository.GetStatistic() ?? CreateInitialStatistic();

            statistic.TotalNumberOfImages = _imageRepository.AmounthOfItems() - amount;
            statistic.NumberOfOptimizedImages = _imageRepository.AmounthOfOptimizedItems();
            statistic.TotalSavedBytes = _statisticRepository.GetTotalSavedBytes();

            _statisticRepository.Update(statistic);
        }
    }
}
