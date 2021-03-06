using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Datastore.PetaPoco;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class SeasonProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private readonly IDatabase _database;

        [Inject]
        public SeasonProvider(IDatabase database)
        {
            _database = database;
        }

        public SeasonProvider()
        {
        }

        public virtual IList<int> GetSeasons(int seriesId)
        {
            return _database.Fetch<Int32>("SELECT DISTINCT SeasonNumber FROM Seasons WHERE SeriesId = @seriesId", new { seriesId }).OrderBy(c => c).ToList();
        }

        public virtual void SetIgnore(int seriesId, int seasonNumber, bool isIgnored)
        {
            logger.Trace("Setting ignore flag on Series:{0} Season:{1} to {2}", seriesId, seasonNumber, isIgnored);
            _database.Execute(@"UPDATE Seasons SET Ignored = @isIgnored
                                WHERE SeriesId = @seriesId AND SeasonNumber = @seasonNumber AND Ignored = @InversedIgnored",
                new { isIgnored, seriesId, seasonNumber, InversedIgnored = !isIgnored });


            logger.Trace("Setting ignore flag on all episodes of  Series:{0} Season:{1} to {2}", seriesId, seasonNumber, isIgnored);
            _database.Execute(@"UPDATE Episodes SET Ignored = @0
                                WHERE SeriesId = @1 AND SeasonNumber = @2 AND Ignored = @3",
                isIgnored, seriesId, seasonNumber, !isIgnored);

            logger.Info("Ignore flag for Series:{0} Season:{1} successfully set to {2}", seriesId, seasonNumber, isIgnored);
        }

        public virtual bool IsIgnored(int seriesId, int seasonNumber)
        {
            return _database.Single<Season>("WHERE SeriesId = @seriesId AND SeasonNumber = @seasonNumber",
                                       new { seriesId, seasonNumber }).Ignored;
        }

        public virtual List<Season> All(int seriesId)
        {
            var seasons = _database.Fetch<Season, Episode, EpisodeFile, Season>(
            new EpisodeSeasonRelator().MapIt,
            @"SELECT * FROM Seasons
                INNER JOIN Episodes ON Episodes.SeriesId = Seasons.SeriesId
                AND Episodes.SeasonNumber = Seasons.SeasonNumber
                LEFT OUTER JOIN EpisodeFiles ON EpisodeFiles.EpisodeFileId = Episodes.EpisodeFileId
                WHERE Seasons.SeriesId = @0", seriesId
            );

            return seasons;
        }

        public virtual void EnsureSeasons(int seriesId, IEnumerable<int> seasons)
        {
            var existingSeasons = GetSeasons(seriesId);

            foreach (var season in seasons)
            {
                if (!existingSeasons.Contains(season))
                {
                    var newSeason = new Season
                    {
                        SeriesId = seriesId,
                        SeasonNumber = season,
                        Ignored = season == 0
                    };

                    _database.Insert(newSeason);
                }
            }
        }
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      