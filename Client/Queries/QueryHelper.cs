using System.Linq;
using Client.Interface;

namespace Client.Queries
{
    public static class QueryHelper
    {
        public static void OptimizeQuery(OrQuery rootExpression)
        {
            // convert < and > into BETWEEN operator. Much more afficient

            foreach (var andQuery in rootExpression.Elements)
            {
                var multipleTests = andQuery.Elements.GroupBy(q => q.IndexName).Where(g => g.Count() > 1).ToList();

                if (multipleTests.Count > 0)
                {
                    // these ones will not be changed
                    var atomicQueries = andQuery.Elements.Where(q => multipleTests.All(mt => mt.Key != q.IndexName))
                        .ToList();

                    foreach (var multipleTest in multipleTests)
                    {
                        if (multipleTest.Count() != 2)
                            throw new CacheException($"Inconsistent query on index {multipleTest.Key}");

                        var q1 = multipleTest.First();
                        var q2 = multipleTest.Last();

                        // multiple atomic queries for the same index do not make sense
                        if (q1.Operator == QueryOperator.Eq)
                            throw new CacheException($"Inconsistent query on index {multipleTest.Key}");

                        if (q2.Operator == QueryOperator.Eq)
                            throw new CacheException($"Inconsistent query on index {multipleTest.Key}");


                        var optimized = false;

                        // a >= x && a <=y will be concverted to "a BETWEEN x, y"

                        if (q1.Operator != QueryOperator.In && q1.Operator != QueryOperator.In)
                        {
                            if (q1.Value < q2.Value)
                            {
                                if (q1.Operator == QueryOperator.Ge)
                                    if (q2.Operator == QueryOperator.Le)
                                    {
                                        var between = new AtomicQuery(q1.Value, q2.Value);
                                        atomicQueries.Add(between);
                                        optimized = true;
                                    }
                            }
                            else if (q1.Value > q2.Value)
                            {
                                if (q1.Operator == QueryOperator.Le)
                                    if (q2.Operator == QueryOperator.Ge)
                                    {
                                        var between = new AtomicQuery(q2.Value, q1.Value);
                                        atomicQueries.Add(between);
                                        optimized = true;
                                    }
                            }
                        }

                        if (!optimized)
                        {
                            // keep the original expressions 
                            atomicQueries.Add(q1);
                            atomicQueries.Add(q2);
                        }
                    }

                    andQuery.Elements.Clear();
                    andQuery.Elements.AddRange(atomicQueries);
                }
            }
        }
    }
}