﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SearchProviderTest_Season : TestBase
    {
        [Test]
        public void SeasonSearch_season_success()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .With(s => s.Title = "Title1")
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.Series = series)
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.Ignored = false)
                .Build();

            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .TheFirst(1)
                .Has(p => p.CleanTitle = "title")
                .Has(p => p.SeasonNumber = 1)
                .Has(p => p.FullSeason = true)
                .Has(p => p.EpisodeNumbers = null)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            var indexer1 = new Mock<IndexerBase>();
            indexer1.Setup(c => c.FetchSeason(episodes[0].Series.Title, episodes[0].SeasonNumber))
                .Returns(parseResults).Verifiable();

            var indexer2 = new Mock<IndexerBase>();
            indexer2.Setup(c => c.FetchSeason(episodes[0].Series.Title, episodes[0].SeasonNumber))
                .Returns(parseResults).Verifiable();

            var indexers = new List<IndexerBase> { indexer1.Object, indexer2.Object };

            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(indexers);

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetSeries(1)).Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodeNumbersBySeason(1, 1)).Returns(episodes.Select(e => e.EpisodeNumber).ToList());

            mocker.GetMock<SceneMappingProvider>()
                .Setup(s => s.GetSceneName(1)).Returns(String.Empty);

            mocker.GetMock<InventoryProvider>()
                .Setup(s => s.IsQualityNeeded(It.IsAny<EpisodeParseResult>())).Returns(true);

            mocker.GetMock<DownloadProvider>()
                .Setup(s => s.DownloadReport(It.IsAny<EpisodeParseResult>())).Returns(true);

            //Act
            var result = mocker.Resolve<SearchProvider>().SeasonSearch(notification, 1, 1);

            //Assert
            result.Should().BeTrue();
            mocker.VerifyAllMocks();
        }

        [Test]
        public void SeasonSearch_season_failure()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .With(s => s.Title = "Title1")
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.Series = series)
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.Ignored = false)
                .Build();

            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            var indexer1 = new Mock<IndexerBase>();
            indexer1.Setup(c => c.FetchSeason(episodes[0].Series.Title, episodes[0].SeasonNumber))
                .Returns(parseResults).Verifiable();

            var indexer2 = new Mock<IndexerBase>();
            indexer2.Setup(c => c.FetchSeason(episodes[0].Series.Title, episodes[0].SeasonNumber))
                .Returns(parseResults).Verifiable();

            var indexers = new List<IndexerBase> { indexer1.Object, indexer2.Object };

            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(indexers);

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetSeries(1)).Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodeNumbersBySeason(1, 1)).Returns(episodes.Select(e => e.EpisodeNumber).ToList());

            mocker.GetMock<SceneMappingProvider>()
                .Setup(s => s.GetSceneName(1)).Returns(String.Empty);

            //Act
            var result = mocker.Resolve<SearchProvider>().SeasonSearch(notification, 1, 1);

            //Assert
            ExceptionVerification.ExcpectedWarns(1);
            result.Should().BeFalse();
            mocker.VerifyAllMocks();
        }

        [Test]
        public void ProcessSeasonSearchResults_success()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .With(s => s.Title = "Title1")
                .Build();

            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .TheFirst(1)
                .Has(p => p.CleanTitle = "title")
                .Has(p => p.SeasonNumber = 1)
                .Has(p => p.FullSeason = true)
                .Has(p => p.EpisodeNumbers = null)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            mocker.GetMock<InventoryProvider>()
                .Setup(s => s.IsQualityNeeded(It.IsAny<EpisodeParseResult>())).Returns(true);

            mocker.GetMock<DownloadProvider>()
                .Setup(s => s.DownloadReport(It.IsAny<EpisodeParseResult>())).Returns(true);

            //Act
            var result = mocker.Resolve<SearchProvider>().ProcessSeasonSearchResults(notification, series, 1, parseResults);

            //Assert
            result.Should().BeTrue();
            mocker.VerifyAllMocks();
        }

        [Test]
        public void ProcessSeasonSearchResults_failure()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .With(s => s.Title = "Title1")
                .Build();

            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .TheFirst(1)
                .Has(p => p.CleanTitle = "title")
                .Has(p => p.SeasonNumber = 1)
                .Has(p => p.FullSeason = true)
                .Has(p => p.EpisodeNumbers = null)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            mocker.GetMock<InventoryProvider>()
                .Setup(s => s.IsQualityNeeded(It.IsAny<EpisodeParseResult>())).Returns(false);

            //Act
            var result = mocker.Resolve<SearchProvider>().ProcessSeasonSearchResults(notification, series, 1, parseResults);

            //Assert
            result.Should().BeFalse();
            ExceptionVerification.ExcpectedWarns(1);
            mocker.VerifyAllMocks();
        }
    }
}