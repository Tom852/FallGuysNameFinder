using Backend.Model;
using Common.Model;
using Serilog;
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

        public async Task<Probability> GetProbabilityAsync(List<Pattern> patterns, Options options, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                ParallelOptions o = new ParallelOptions() { CancellationToken = token };

                var matchingService = new MatchingService(patterns, options);
                int matchCount = 0;

                Parallel.ForEach(FirstPossibleNames, o, firstname =>
                {
                    Parallel.ForEach(SecondPossibleNames, o, secondname =>
                    {
                        foreach (var thirdname in ThirdPossibleNames)
                        {
                            var testresult = matchingService.Test(new Name(firstname, secondname, thirdname));
                            if (testresult.IsMatching())
                            {
                                lock (this)
                                {
                                    matchCount++;
                                }
                            }
                        }
                    }
                    );
                });


                double totalOptions = FirstPossibleNames.Length * SecondPossibleNames.Length * ThirdPossibleNames.Length;

                return new Probability(matchCount / totalOptions);
            });
        }
    }
}