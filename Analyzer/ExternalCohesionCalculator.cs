namespace Analyzer
{
    public partial class AnalyzerAnalyzer
    {
        public class ExternalCohesionCalculator
        {
            public static int GetCohesionPenalty(string sourceNamespace, string usingNamespace)
            {
                var biggestMatch = FindBiggestMatch(sourceNamespace, usingNamespace);

                var prunedSource = sourceNamespace.Substring(biggestMatch.Length );
                var prunedUsing = usingNamespace.Substring(biggestMatch.Length);

                if (string.IsNullOrEmpty(prunedSource) || string.IsNullOrEmpty(prunedUsing))
                {
                    return 0;
                }
                return prunedSource.Split('.').Length + prunedUsing.Split('.').Length;
            }

            public static string FindBiggestMatch(string one, string two)
            {
                for (int i = 0; i < one.Length; i++)
                {
                    if (one[i] != two[i])
                    {
                        return one.Substring(0, i);
                    }
                }
                return one;
            }
        }

    }
}
