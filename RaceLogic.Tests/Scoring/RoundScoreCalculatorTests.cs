using System;
using System.Collections.Generic;
using System.Linq;
using RaceLogic.Checkpoints;
using RaceLogic.Model;
using RaceLogic.Scoring;
using Shouldly;
using Xunit;

namespace RaceLogic.Tests.Scoring
{
    public class RoundScoreCalculatorTests
    {
        [Fact]
        public void Static_scoring_should_work()
        {
            var calc = RoundScoringStrategy<int>.FromStaticScores(new []{3, 1, 5});
            calc.FirstPlacePoints.ShouldBe(0);
            calc.SubstractBy.ShouldBe(0);
            calc.GetScoreForPosition(0).ShouldBe(0);
            calc.GetScoreForPosition(1).ShouldBe(3);
            calc.GetScoreForPosition(2).ShouldBe(1);
            calc.GetScoreForPosition(3).ShouldBe(5);
            calc.GetScoreForPosition(4).ShouldBe(0);

            var scores = calc.Calculate(GetRating(5, 2)).ToList();
            scores[0].Position.ShouldBe(1);
            scores[0].Points.ShouldBe(3);
            scores[1].Position.ShouldBe(2);
            scores[1].Points.ShouldBe(1);
            scores[2].Position.ShouldBe(3);
            scores[2].Points.ShouldBe(5);
            scores.Skip(3).ShouldAllBe(x => x.Points == 0);
        }

        [Fact]
        public void Finishers_scoring_should_work()
        {
            var rating = GetRating(3, 2).ToList();
            var calc = RoundScoringStrategy<int>.FromFinishers(rating);
            calc.FirstPlacePoints.ShouldBe(3);
            calc.SubstractBy.ShouldBe(1);
            var scores = calc.Calculate(rating).ToList();
            scores[0].Points.ShouldBe(3);
            scores[1].Points.ShouldBe(2);
            scores[2].Points.ShouldBe(1);
            scores[3].Points.ShouldBe(0);
            scores[4].Points.ShouldBe(0);
        }
        
        [Fact]
        public void Expected_riders_should_be_append_with_zero_score()
        {
            var rating = GetRating(3, 0).ToList();
            var calc = RoundScoringStrategy<int>.FromFirstPlacePoints(9, 3);
            calc.FirstPlacePoints.ShouldBe(9);
            calc.SubstractBy.ShouldBe(3);
            var scores = calc.Calculate(rating, new []{21, 22}).ToList();
            scores.Count.ShouldBe(5);
            scores[0].Points.ShouldBe(9);
            scores[1].Points.ShouldBe(6);
            scores[2].Points.ShouldBe(3);
            scores[3].RiderId.ShouldBe(21);
            scores[3].Points.ShouldBe(0);
            scores[4].RiderId.ShouldBe(22);
            scores[4].Points.ShouldBe(0);
        }
        
        [Fact]
        public void Expected_riders_should_be_merged_with_actual_riders()
        {
            var rating = GetRating(3, 0).ToList();
            var calc = RoundScoringStrategy<int>.FromFirstPlacePoints(9, 3);
            var scores = calc.Calculate(rating, new []{11, 12, 13, 21, 22}).ToList();
            scores.Count.ShouldBe(5);
            scores[0].Points.ShouldBe(9);
            scores[0].RiderId.ShouldBe(11);
            scores[1].Points.ShouldBe(6);
            scores[1].RiderId.ShouldBe(12);
            scores[2].Points.ShouldBe(3);
            scores[2].RiderId.ShouldBe(13);
            scores[3].RiderId.ShouldBe(21);
            scores[3].Points.ShouldBe(0);
            scores[4].RiderId.ShouldBe(22);
            scores[4].Points.ShouldBe(0);
        }
        
        [Fact]
        public void Rate_dnfs_should_be_respected()
        {
            var rating = GetRating(2, 1).ToList();
            var scores = RoundScoringStrategy<int>.FromFirstPlacePoints(10, rateDnfs: false)
                .Calculate(rating, new []{21, 22}).ToList();
            scores.Count.ShouldBe(5);
            scores[0].Points.ShouldBe(10);
            scores[1].Points.ShouldBe(9);
            scores[2].Points.ShouldBe(0);
            scores[3].Points.ShouldBe(0);
            scores[4].Points.ShouldBe(0);
            
            scores = RoundScoringStrategy<int>.FromFirstPlacePoints(10, rateDnfs: true)
                .Calculate(rating, new []{21, 22}).ToList();
            scores.Count.ShouldBe(5);
            scores[0].Points.ShouldBe(10);
            scores[1].Points.ShouldBe(9);
            scores[2].Points.ShouldBe(8);
            scores[3].Points.ShouldBe(0);
            scores[4].Points.ShouldBe(0);
        }

        IEnumerable<RoundPosition<int>> GetRating(int finishers, int starters)
        {
            for (var i = 0; i < finishers; i++)
                yield return RoundPosition<int>.FromLaps(11 + i, new List<Lap<int>>{
                    new Lap<int>(new Checkpoint<int>(11 + i), DateTime.Now)
                }, true);
            for (var i = 0; i < starters; i++)
                yield return RoundPosition<int>.FromLaps(11 + i, new List<Lap<int>>{
                    new Lap<int>(new Checkpoint<int>(11 + i), DateTime.Now)
                }, false);
        }
    }
}