using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using maxbl4.Race.Logic;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.RoundTiming;
using maxbl4.Race.Logic.Scoring;
using Xunit;

namespace maxbl4.Race.Tests.Logic.Scoring
{
    public class RoundScoreCalculatorTests
    {
        [Fact]
        public void Static_scoring_should_work()
        {
            var calc = RoundScoringStrategy.FromStaticScores(new[] {3, 1, 5});
            calc.FirstPlacePoints.Should().Be(0);
            calc.SubstractBy.Should().Be(0);
            calc.GetScoreForPosition(0).Should().Be(0);
            calc.GetScoreForPosition(1).Should().Be(3);
            calc.GetScoreForPosition(2).Should().Be(1);
            calc.GetScoreForPosition(3).Should().Be(5);
            calc.GetScoreForPosition(4).Should().Be(0);

            var scores = calc.Calculate(GetRating(5, 2)).ToList();
            scores[0].Position.Should().Be(1);
            scores[0].Points.Should().Be(3);
            scores[1].Position.Should().Be(2);
            scores[1].Points.Should().Be(1);
            scores[2].Position.Should().Be(3);
            scores[2].Points.Should().Be(5);
            scores.Skip(3).Should().OnlyContain(x => x.Points == 0);
        }

        [Fact]
        public void Finishers_scoring_should_work()
        {
            var rating = GetRating(3, 2).ToList();
            var calc = RoundScoringStrategy.FromFinishers(rating);
            calc.FirstPlacePoints.Should().Be(3);
            calc.SubstractBy.Should().Be(1);
            var scores = calc.Calculate(rating).ToList();
            scores[0].Points.Should().Be(3);
            scores[1].Points.Should().Be(2);
            scores[2].Points.Should().Be(1);
            scores[3].Points.Should().Be(0);
            scores[4].Points.Should().Be(0);
        }

        [Fact]
        public void Expected_riders_should_be_append_with_zero_score()
        {
            var rating = GetRating(3, 0).ToList();
            var calc = RoundScoringStrategy.FromFirstPlacePoints(9, 3);
            calc.FirstPlacePoints.Should().Be(9);
            calc.SubstractBy.Should().Be(3);
            var scores = calc.Calculate(rating, new[] {"21", "22"}).ToList();
            scores.Count.Should().Be(5);
            scores[0].Points.Should().Be(9);
            scores[1].Points.Should().Be(6);
            scores[2].Points.Should().Be(3);
            scores[3].RiderId.Should().Be("21");
            scores[3].Points.Should().Be(0);
            scores[4].RiderId.Should().Be("22");
            scores[4].Points.Should().Be(0);
        }

        [Fact]
        public void Expected_riders_should_be_merged_with_actual_riders()
        {
            var rating = GetRating(3, 0).ToList();
            var calc = RoundScoringStrategy.FromFirstPlacePoints(9, 3);
            var scores = calc.Calculate(rating, new[] {"11", "12", "13", "21", "22"}).ToList();
            scores.Count.Should().Be(5);
            scores[0].Points.Should().Be(9);
            scores[0].RiderId.Should().Be("11");
            scores[1].Points.Should().Be(6);
            scores[1].RiderId.Should().Be("12");
            scores[2].Points.Should().Be(3);
            scores[2].RiderId.Should().Be("13");
            scores[3].RiderId.Should().Be("21");
            scores[3].Points.Should().Be(0);
            scores[4].RiderId.Should().Be("22");
            scores[4].Points.Should().Be(0);
        }

        [Fact]
        public void Rate_dnfs_should_be_respected()
        {
            var rating = GetRating(2, 1).ToList();
            var scores = RoundScoringStrategy.FromFirstPlacePoints(10)
                .Calculate(rating, new[] {"21", "22"}).ToList();
            scores.Count.Should().Be(5);
            scores[0].Points.Should().Be(10);
            scores[1].Points.Should().Be(9);
            scores[2].Points.Should().Be(0);
            scores[3].Points.Should().Be(0);
            scores[4].Points.Should().Be(0);

            scores = RoundScoringStrategy.FromFirstPlacePoints(10, rateDnfs: true)
                .Calculate(rating, new[] {"21", "22"}).ToList();
            scores.Count.Should().Be(5);
            scores[0].Points.Should().Be(10);
            scores[1].Points.Should().Be(9);
            scores[2].Points.Should().Be(8);
            scores[3].Points.Should().Be(0);
            scores[4].Points.Should().Be(0);
        }

        private IEnumerable<RoundPosition> GetRating(int finishers, int starters)
        {
            for (var i = 0; i < finishers; i++)
                yield return RoundPosition.FromLaps($"{11 + i}", new List<Lap>
                {
                    new(new Checkpoint($"{11 + i}", Constants.DefaultUtcDate), DateTime.UtcNow)
                }, true);
            for (var i = 0; i < starters; i++)
                yield return RoundPosition.FromLaps($"{11 + i}", new List<Lap>
                {
                    new(new Checkpoint($"{11 + i}", Constants.DefaultUtcDate), DateTime.UtcNow)
                }, false);
        }
    }
}