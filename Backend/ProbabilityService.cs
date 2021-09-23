using Backend.Model;
using Common;
using Common.Model;
using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backend
{
    public class ProbabilityService
    {
        private readonly string[] FirstPossibleNames = PossibleNames.FirstNames(false);
        private readonly string[] SecondPossibleNames = PossibleNames.SecondNames(false);
        private readonly string[] ThirdPossibleNames = PossibleNames.ThirdNames(false);

        public ConcurrentBag<Name> AllNamesThatMatch { get; private set; }

        public async Task<Probability> GetProbabilityAsync(List<Pattern> patterns, Pool pool, Options options, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                AllNamesThatMatch = new ConcurrentBag<Name>();

                ParallelOptions o = new ParallelOptions() { CancellationToken = token };

                var matchingService = new MatchingService(patterns, pool, options);
                int matchCount = 0;

                Parallel.ForEach(FirstPossibleNames, o, firstname =>
                {
                    Parallel.ForEach(SecondPossibleNames, o, secondname =>
                    {
                        foreach (var thirdname in ThirdPossibleNames)
                        {
                            var nameToTest = new Name(firstname, secondname, thirdname);
                            var testresult = matchingService.Test(nameToTest);
                            if (testresult.IsMatching())
                            {
                                lock (this)
                                {
                                    matchCount++;
                                    AllNamesThatMatch.Add(nameToTest);
                                }
                            }
                        }
                    }
                    );
                });


                double totalOptions = FirstPossibleNames.Length * SecondPossibleNames.Length * ThirdPossibleNames.Length;

                return new Probability(matchCount, totalOptions);
            });
        }
    }
}